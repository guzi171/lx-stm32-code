using System.Text;

namespace GitCommandHelper;

internal static class SmokeTest
{
    public static async Task<int> RunAsync()
    {
        string testRoot = Path.Combine(Path.GetTempPath(), "GitCommandHelperSmoke-" + Guid.NewGuid().ToString("N"));
        string workDirectory = Path.Combine(testRoot, "work");
        string remoteDirectory = Path.Combine(testRoot, "remote.git");
        Directory.CreateDirectory(workDirectory);

        var output = new StringBuilder();
        try
        {
            using var shell = new BashShellSession(workDirectory);
            shell.OutputReceived += text => output.Append(text);

            await MustSucceed(shell, "printf 'shell-ok\\n'");
            Require(output.ToString().Contains("shell-ok", StringComparison.Ordinal), "没有收到 Shell 输出");

            ShellCommandResult cdResult = await MustSucceed(shell, "mkdir sub && cd sub");
            Require(cdResult.ShellDirectory.EndsWith("/sub", StringComparison.Ordinal), "Shell 当前目录没有更新");
            ShellCommandResult resetResult = await MustSucceed(shell, "pwd", resetToWorkingDirectory: true);
            Require(resetResult.ShellDirectory.EndsWith("/work", StringComparison.Ordinal),
                "按钮命令没有强制返回所选工程目录");

            await MustSucceed(shell, "git init -b smoke-local");
            await MustSucceed(shell, "git config user.name 'Git Command Helper Test'");
            await MustSucceed(shell, "git config user.email 'git-command-helper@example.invalid'");

            File.WriteAllText(Path.Combine(workDirectory, "smoke.txt"), "smoke test", new UTF8Encoding(false));
            await MustSucceed(shell, "git add -A");
            await MustSucceed(shell, "git commit -m 'smoke commit'");

            await MustSucceed(shell, $"git init --bare {Quote(remoteDirectory)}");
            await MustSucceed(shell, $"git remote add origin {Quote(remoteDirectory)}");
            await MustSucceed(shell, "git push -u origin smoke-local:smoke-remote");
            await MustSucceed(shell, $"git --git-dir={Quote(remoteDirectory)} show-ref --verify refs/heads/smoke-remote");
            await MustSucceed(shell, "git status --porcelain");

            Console.WriteLine("SMOKE TEST PASSED");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("SMOKE TEST FAILED");
            Console.Error.WriteLine(ex);
            Console.Error.WriteLine(output.ToString());
            return 1;
        }
        finally
        {
            string tempRoot = Path.GetFullPath(Path.GetTempPath());
            string resolvedTestRoot = Path.GetFullPath(testRoot);
            if (resolvedTestRoot.StartsWith(tempRoot, StringComparison.OrdinalIgnoreCase) &&
                Path.GetFileName(resolvedTestRoot).StartsWith("GitCommandHelperSmoke-", StringComparison.Ordinal))
            {
                try { Directory.Delete(resolvedTestRoot, recursive: true); }
                catch { }
            }
        }
    }

    private static async Task<ShellCommandResult> MustSucceed(
        BashShellSession shell,
        string command,
        bool resetToWorkingDirectory = false)
    {
        ShellCommandResult result = await shell.ExecuteAsync(
            command,
            resetToWorkingDirectory: resetToWorkingDirectory);
        Require(result.ExitCode == 0, $"命令失败（{result.ExitCode}）：{command}");
        return result;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static string Quote(string value) => "'" + value.Replace("'", "'\\''", StringComparison.Ordinal) + "'";
}
