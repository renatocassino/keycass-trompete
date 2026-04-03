using KeyCassTrompete;

if (args.Length == 0)
{
    // Comportamento padrao: executar HidListener
    HidListener.Run();
    return;
}

string comando = args[0].ToLower();

switch (comando)
{
    case "":
    case "hid":
    case "listener":
        HidListener.Run();
        break;

    case "build":
    case "template":
    case "process":
        TemplateProcessor.Run();
        break;

    default:
        Console.WriteLine("Comando invalido!");
        Console.WriteLine();
        Console.WriteLine("Comandos disponiveis:");
        Console.WriteLine("  hid, listener         - Escutar eventos HID do teclado");
        Console.WriteLine("  template, process     - Processar keymap.c.template");
        Console.WriteLine();
        Console.WriteLine("Uso: dotnet run [comando]");
        Console.WriteLine("  Sem argumentos: executa o HID Listener por padrao");
        break;
}
