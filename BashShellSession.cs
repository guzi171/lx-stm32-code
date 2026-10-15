using System.Diagnostics;
using System.Text;

namespace GitCommandHelper;

internal sealed class BashShellSession : IDisposable
{
    private readonly SemaphoreSlim _commandLock = new(1, 1);
    private readonly object _stateLock = new();
    private Process? _process;
    private TaskCompletionSource<ShellCommandResult>? _completion;
    private string? _marker;
    private bool _disposed;

    public event Action<string>? OutputReceived;

    public string BashPath { get; }
    public string WorkingDirectory { get; private set; }
    public bool IsRunning => _process is { HasExited: false };

    public BashShellSession(string workingDirectory)
    {
        BashPath = FindBashPath() ?? throw new FileNotFoundException(
            "没有找到 Git Bash。请先安装 Git for Windows。");
        WorkingDirectory = workingDirectory;
    }

    public static string? FindBashPath()
    {
        string[] candidates =
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Git", "bin", "bash.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Git", "bin", "bash.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Git", "bin", "bash.exe")
        ];

        return candidates.FirstOrDefault(File.Exists);
    }

    public void SetWorkingDirectory(string directory)
    {
        if (string.Equals(WorkingDirectory, directory, StringComparison.OrdinalIgnoreCase))
            return;

        StopProcess();
        WorkingDirectory = directory;
    }

    public async Task<ShellCommandResult> ExecuteAsync(
        string command,
        CancellationToken cancellationToken = default,
        bool resetToWorkingDirectory = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _commandLock.WaitAsync(cancellationToken);

        try
        {
            bool startedNow = EnsureStarted();
            if (_process is null || _process.HasExited)
                throw new InvalidOperationException("Git Bash 启动失败。");

            string token = Guid.NewGuid().ToString("N");
            string marker = $"__GCH_DONE_{token}__:";
            var completion = new TaskCompletionSource<ShellCommandResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            lock (_stateLock)
            {
                _marker = marker;
                _completion = completion;
            }

            using CancellationTokenRegistration registration = cancellationToken.Register(() =>
            {
                completion.TrySetCanceled(cancellationToken);
                StopProcess();
            });

            string commandToRun = command;
            if (startedNow || resetToWorkingDirectory)
            {
                string bashDirectory = WorkingDirectory.Replace('\\', '/');
                commandToRun = $"cd -- {BashQuote(bashDirectory)} && {command}";
            }

            await _process.StandardInput.WriteLineAsync(commandToRun);
            await _process.StandardInput.WriteLineAsync(
                $"__gch_code=$?; printf '{marker}%s\\t%s\\n' \"$__gch_code\" \"$PWD\"");
            await _process.StandardInput.FlushAsync();

            return await completion.Task;
        }
        finally
        {
            lock (_stateLock)
            {
                _marker = null;
                _completion = null;
            }
            _commandLock.Release();
        }
    }

    public void CancelCurrentCommand() => StopProcess();

    private bool EnsureStarted()
    {
        if (IsRunning)
            return false;

        if (!Directory.Exists(WorkingDirectory))
            throw new DirectoryNotFoundException($"工程目录不存在：{WorkingDirectory}");

        var startInfo = new ProcessStartInfo
        {
            FileName = BashPath,
            WorkingDirectory = WorkingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(false),
            StandardOutputEncoding = new UTF8Encoding(false),
            StandardErrorEncoding = new UTF8Encoding(false)
        };
        startInfo.ArgumentList.Add("--noprofile");
        startInfo.ArgumentList.Add("--norc");
        startInfo.Environment["TERM"] = "dumb";

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.OutputDataReceived += OnOutputDataReceived;
        process.ErrorDataReceived += OnErrorDataReceived;
        process.Exited += OnProcessExited;

        if (!process.Start())
            throw new InvalidOperationException("Git Bash 启动失败。");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        _process = process;
        return true;
    }

    private static string BashQuote(string value) =>
        "'" + value.Replace("'", "'\\''", StringComparison.Ordinal) + "'";

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is null)
            return;

        string? marker;
        TaskCompletionSource<ShellCommandResult>? completion;
        lock (_stateLock)
        {
            marker = _marker;
            completion = _completion;
        }

        if (marker is not null)
        {
            int markerIndex = e.Data.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex >= 0)
            {
                string precedingText = e.Data[..markerIndex];
                if (precedingText.Length > 0)
                    OutputReceived?.Invoke(precedingText + Environment.NewLine);

                string resultText = e.Data[(markerIndex + marker.Length)..];
                string[] parts = resultText.Split('\t', 2);
                int code = int.TryParse(parts[0], out int parsedCode) ? parsedCode : -1;
                string shellDirectory = parts.Length == 2 ? parts[1] : string.Empty;
                completion?.TrySetResult(new ShellCommandResult(code, shellDirectory));
                return;
            }
        }

        OutputReceived?.Invoke(e.Data + Environment.NewLine);
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is not null)
            OutputReceived?.Invoke(e.Data + Environment.NewLine);
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        TaskCompletionSource<ShellCommandResult>? completion;
        lock (_stateLock)
            completion = _completion;

        completion?.TrySetException(new InvalidOperationException("Git Bash 会话已结束。"));
    }

    private void StopProcess()
    {
        Process? process = _process;
        _process = null;
        if (process is null)
            return;

        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Closing the helper must not be blocked by an already-exited shell.
        }
        finally
        {
            process.Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        StopProcess();
        _commandLock.Dispose();
    }
}

internal readonly record struct ShellCommandResult(int ExitCode, string ShellDirectory);
