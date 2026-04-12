using System.Text;
using System.Threading.Channels;
using KeyCass.Modules.Speaker;

namespace KeyCass.Modules.TextProcessor;

public static class TextProcessor
{
    private static Channel<string>? _channel;
    private static Task? _processingTask;
    private static CancellationTokenSource? _cancellationTokenSource;

    private static StringBuilder _letters = new();
    private static Timer? _debounceTimer;
    private static readonly object timerLock = new object();
    private static readonly HashSet<char> CharsToBreak = new()
    {
        ' ',
        ',',
        '.',
        '!',
        '?',
        '\n'
    };

    // Configs
    private const int MaxCharsBeforeSpeak = 20;
    private const int DebounceDelayMs = 1500;

    public static void Start()
    {
        if (_channel is not null)
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _channel = Channel.CreateUnbounded<string>();
        _processingTask = Task.Run(() => ProcessQueue(_cancellationTokenSource.Token));
    }

    public static async Task Stop()
    {
        if (_channel is null)
        {
            Console.WriteLine("TextProcessor is not running");
            return;
        }

        // Para o timer de debounce
        lock (timerLock)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = null;
        }

        _channel.Writer.Complete();

        if (_processingTask is not null)
        {
            try
            {
                await _processingTask;
                Console.WriteLine("TextProcessor stopped gracefully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping TextProcessor: {ex.Message}");
            }
        }

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _channel = null;
        _processingTask = null;
    }

    private static async Task ProcessQueue(CancellationToken cancellationToken)
    {
        if (_channel is null)
        {
            Console.WriteLine("Channel is not started. Ignore message");
            return;
        }

        try
        {
            await foreach (var text in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                await ProcessText(text, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Processing queue cancelled");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in processing queue: {ex.Message}");
        }
    }

    private static async Task ProcessText(string text, CancellationToken cancellationToken)
    {
        try
        {
            // Se for string vazia, ignora
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            // Pega o primeiro caractere (assumindo que vem 1 char por vez)
            char ch = text[0];

            // Se for Enter (\n), processa tudo imediatamente
            if (ch == '\n')
            {
                StopDebounceTimer();
                Console.WriteLine($"Text: {_letters}");
                // TODO: Descomentar quando tiver Speaker implementado
                // await Speaker.Speaker.EnqueueText(_letters.ToString());
                _letters.Clear();
                return;
            }

            // Adiciona o caractere ao buffer
            _letters.Append(ch);
            ResetDebounceTimer();

            // Se atingiu o limite de caracteres, fala parte da expressão
            if (MustSpeak())
            {
                ResetDebounceTimer();
                var expression = GetExpression();
                Console.WriteLine($"Text expression: {expression}");
                Console.WriteLine($"Text: {_letters}");
                // TODO: Descomentar quando tiver Speaker implementado
                // await Speaker.Speaker.EnqueueText(expression);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Speech cancelled");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error speaking: {ex.Message}");
        }
    }

    private static void ResetDebounceTimer()
    {
        lock (timerLock)
        {
            // Cancela o timer anterior
            _debounceTimer?.Dispose();

            // Cria um novo timer que dispara após DebounceDelayMs
            _debounceTimer = new Timer(
                callback: _ => OnDebounceTimeout(),
                state: null,
                dueTime: DebounceDelayMs,
                period: Timeout.Infinite
            );
        }
    }

    private static void StopDebounceTimer()
    {
        lock (timerLock)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = null;
        }
    }

    private static void OnDebounceTimeout()
    {
        // Callback do timer - usuário parou de digitar
        if (_letters.Length <= 0)
        {
            return;
        }

        var textToSpeak = _letters.ToString();
        _letters.Clear();

        Console.WriteLine($"[Debounce] Speaking remaining text: '{textToSpeak}'");

        // TODO: Descomentar quando tiver Speaker implementado
        // Enfileira o texto para falar (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await Speaker.Speaker.EnqueueText(textToSpeak);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in debounce speech: {ex.Message}");
            }
        });
    }

    private static bool MustSpeak()
    {
        return (_letters.Length > MaxCharsBeforeSpeak);
    }

    public static string GetExpression()
    {
        var fullText = _letters.ToString();

        // Procura do final para o início pelo último caractere de quebra
        for (int i = fullText.Length - 1; i >= 0; i--)
        {
            if (!CharsToBreak.Contains(fullText[i]))
            {
                continue;
            }

            // Encontrou um caractere de quebra
            var textToReturn = fullText.Substring(0, i + 1); // Até o caractere (incluindo)
            var remaining = fullText.Substring(i); // Depois do caractere

            _letters.Clear();
            _letters.Append(remaining);

            return textToReturn;
        }

        // Não encontrou nenhum caractere de quebra, retorna tudo e limpa
        _letters.Clear();
        return fullText;
    }

    public static async Task EnqueueKey(string text)
    {
        if (_channel is null)
        {
            Console.WriteLine("Channel is not started. Ignore message");
            return;
        }

        try
        {
            await _channel.Writer.WriteAsync(text);
        }
        catch (ChannelClosedException)
        {
            Console.WriteLine("Channel is closed, cannot enqueue text");
        }
    }
}
