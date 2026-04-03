using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Channels;
using NAudio.Wave;

namespace KeyCass.Modules.Speaker;

public static class Speaker
{
    private static Channel<string>? _channel;
    private static Task? _processingTask;
    private static CancellationTokenSource? _cancellationTokenSource;

    // Piper voice model (pt_BR-faber-medium for Brazilian Portuguese)
    private static string PiperModel = "pt_BR-faber-medium";

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

    // Para a thread de processamento
    public static async Task Stop()
    {
        if (_channel is null)
        {
            Console.WriteLine("Speaker is not running");
            return;
        }

        _channel.Writer.Complete();

        if (_processingTask is not null)
        {
            try
            {
                await _processingTask;
                Console.WriteLine("Speaker stopped gracefully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping speaker: {ex.Message}");
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
                await SayAsync(text, cancellationToken);
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

    private static async Task SayAsync(string text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Console.WriteLine("Text is empty, nothing to speak");
            return;
        }

        try
        {
            if (OperatingSystem.IsWindows())
            {
                await SayAsyncWindows(Normalizer.Normalize(text), cancellationToken);
            }
            else
            {
                await SayAsyncLinux(text, cancellationToken);
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

    private static async Task SayAsyncWindows(string normalizedText, CancellationToken cancellationToken)
    {
        var piperDir = Environment.GetEnvironmentVariable("KEYCASS_PIPER_DIR")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeyCass", "piper");
        var piperExe = Path.Combine(piperDir, "piper.exe");
        var modelPath = Path.Combine(piperDir, $"{PiperModel}.onnx");
        var configPath = Path.Combine(piperDir, $"{PiperModel}.onnx.json");

        if (!File.Exists(piperExe))
        {
            Console.WriteLine($"Piper não encontrado: {piperExe}");
            Console.WriteLine("Defina KEYCASS_PIPER_DIR ou coloque piper.exe na pasta padrão (veja README - Windows).");
            return;
        }

        if (!File.Exists(modelPath) || !File.Exists(configPath))
        {
            Console.WriteLine($"Modelo de voz não encontrado em: {piperDir}");
            Console.WriteLine("Baixe pt_BR-faber-medium.onnx e .onnx.json (veja README - Windows).");
            return;
        }


        var tempWav = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.wav");

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = piperExe,
                Arguments = $"-m \"{modelPath}\" -c \"{configPath}\" -f \"{tempWav}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true
                // Não redirecionar stderr para evitar deadlock: o Piper escreve e termina
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                Console.WriteLine("Falha ao iniciar piper.");
                return;
            }

            // Envia uma linha e fecha stdin = EOF; o Piper processa e grava o WAV
            process.StandardInput.WriteLine(normalizedText);
            process.StandardInput.Flush();
            process.StandardInput.Close();

            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode != 0)
            {
                Console.WriteLine($"Piper saiu com código: {process.ExitCode}");
                return;
            }

            if (!File.Exists(tempWav) || new FileInfo(tempWav).Length == 0)
            {
                Console.WriteLine("Piper não gerou áudio.");
                return;
            }

            await PlayWavAsync(tempWav, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saying: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempWav))
                File.Delete(tempWav);
        }
    }

    private static Task PlayWavAsync(string wavPath, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

        using var reader = new AudioFileReader(wavPath);
        using var waveOut = new WaveOutEvent();
        waveOut.PlaybackStopped += (_, e) =>
        {
            if (e.Exception != null)
                tcs.TrySetException(e.Exception);
            else
                tcs.TrySetResult();
        };
        waveOut.Init(reader);
        waveOut.Play();

        return tcs.Task;
    }

    private static async Task SayAsyncLinux(string text, CancellationToken cancellationToken)
    {
        var command = $"echo 'ok, {Normalizer.Normalize(text).Replace("'", "\\'")}' | piper -m {PiperModel} --output-raw | sox -r 22050 -b 16 -e signed -c 1 -t raw - -t raw - trim 0.4 | aplay -r 22050 -f S16_LE -c 1";

        var processInfo = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"",
            UseShellExecute = false,
            CreateNoWindow = false
        };

        var process = Process.Start(processInfo);
        if (process != null)
        {
            await process.WaitForExitAsync(cancellationToken);
            Console.WriteLine($"piper finished with exit code: {process.ExitCode}");
        }
        else
        {
            Console.WriteLine("Failed to start piper process");
        }
    }

    public static async Task EnqueueText(string text)
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
