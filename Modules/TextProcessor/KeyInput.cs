namespace KeyCass.Modules.TextProcessor;

public enum KeyType
{
    Character,
    Enter,
    Backspace,
    Space,
    Escape
}

public class KeyInput
{
    public KeyType Type { get; private set; }
    public char Character { get; private set; }

    private KeyInput() { }

    // Factory methods genéricos
    public static KeyInput FromChar(char c) => new()
    {
        Type = KeyType.Character,
        Character = c
    };

    public static KeyInput Enter() => new()
    {
        Type = KeyType.Enter,
        Character = '\n'
    };

    public static KeyInput Backspace() => new()
    {
        Type = KeyType.Backspace,
        Character = '\0'
    };

    public static KeyInput Space() => new()
    {
        Type = KeyType.Space,
        Character = ' '
    };

    public static KeyInput Escape() => new()
    {
        Type = KeyType.Escape,
        Character = '\0'
    };

    public static KeyInput? FromConsoleKeyInfo(ConsoleKeyInfo keyInfo)
    {
        return keyInfo.Key switch
        {
            ConsoleKey.Enter => Enter(),
            ConsoleKey.Backspace => Backspace(),
            ConsoleKey.Spacebar => Space(),
            ConsoleKey.Escape => Escape(),
            _ when !char.IsControl(keyInfo.KeyChar) => FromChar(keyInfo.KeyChar),
            _ => null
        };
    }
}
