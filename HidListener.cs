using HidSharp;
using KeyCass.Modules.TextProcessor;
using KeyCass.Modules.Speaker;

namespace KeyCassTrompete;

public static class HidListener
{
    // Converte keycode USB HID para caractere
    // Referência: USB HID Usage Tables (padrão internacional)
    // https://www.usb.org/sites/default/files/documents/hut1_12v2.pdf
    private static char? KeycodeToChar(ushort keycode)
    {
        // Letras: USB HID 0x04-0x1D = 'a'-'z' (sequencial)
        if (keycode >= 0x04 && keycode <= 0x1D)
        {
            return (char)('a' + (keycode - 0x04));
        }

        // Números: USB HID 0x1E-0x26 = '1'-'9', 0x27 = '0'
        if (keycode >= 0x1E && keycode <= 0x26)
        {
            return (char)('1' + (keycode - 0x1E));
        }
        if (keycode == 0x27)
        {
            return '0';
        }

        // Caracteres especiais comuns
        return keycode switch
        {
            0x2C => ' ',   // Espaço
            0x28 => '\n',  // Enter
            0x2D => '-',   // Minus/Underscore
            0x2E => '=',   // Equal/Plus
            0x2F => '[',   // Left bracket
            0x30 => ']',   // Right bracket
            0x31 => '\\',  // Backslash
            0x33 => ';',   // Semicolon
            0x34 => '\'',  // Apostrophe
            0x35 => '`',   // Grave accent
            0x36 => ',',   // Comma
            0x37 => '.',   // Period
            0x38 => '/',   // Slash
            _ => null
        };
    }

    public async static void Run()
    {
        Console.WriteLine("=== HID Listener ===");
        Console.WriteLine("Procurando dispositivo HID...\n");

        TextProcessor.Start();
        Speaker.Start();

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
            Console.WriteLine("Aguardando teclas... (Ctrl+C para sair)\n");
            while (true)
            {
                stream.Read(buffer);

                // HID adiciona um Report ID no buffer[0]
                // O QMK envia: data[0] = byte alto, data[1] = byte baixo, data[2] = estado
                // Então no buffer: [0] = Report ID, [1] = byte alto, [2] = byte baixo, [3] = estado
                ushort keycode = (ushort)((buffer[1] << 8) | buffer[2]);
                byte state = buffer[3];

                // Converte keycode para caractere
                char? ch = KeycodeToChar(keycode);
                string charInfo = ch.HasValue ? $" = '{ch}'" : "";

                // Enfileira o caractere quando a tecla é pressionada
                if (state == 1 && ch.HasValue)
                {
                    Console.WriteLine($"Keycode: 0x{keycode:X4} {charInfo}");
                    await TextProcessor.EnqueueKey(ch.Value.ToString());
                }
            }
        }
        else
        {
            Console.WriteLine("Raw HID device nao encontrado");
        }
    }
}
