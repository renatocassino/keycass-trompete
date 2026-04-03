using HidSharp;

var devices = DeviceList.Local.GetHidDevices(vendorID: 0xFEED, productID: 0x0000);

HidDevice? rawDevice = null;
foreach (var d in devices)
{
    if (d.GetReportDescriptor().DeviceItems
        .Any(item => item.Usages.GetAllValues()
        .Any(u => (u >> 16) == 0xFF60)))
    {
        rawDevice = d;
        break;
    }
}

if (rawDevice != null)
{
    var stream = rawDevice.Open();
    stream.ReadTimeout = Timeout.Infinite;

    var buffer = new byte[32];
    Console.WriteLine("Aguardando teclas...");
    while (true)
    {
        stream.Read(buffer);
        ushort keycode = (ushort)((buffer[1] << 8) | buffer[2]);
byte estado = buffer[3];
Console.WriteLine($"Tecla: {keycode}, Estado: {(estado == 1 ? "pressionada" : "solta")}");
    }
}
else
{
    Console.WriteLine("Raw HID device nao encontrado");
}
