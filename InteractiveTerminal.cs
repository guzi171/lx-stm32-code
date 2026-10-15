using System.ComponentModel;
using System.Text.RegularExpressions;

namespace GitCommandHelper;

internal sealed partial class InteractiveTerminal : RichTextBox
{
    private readonly List<string> _history = [];
    private int _historyIndex;
    private int _inputStart;
    private bool _acceptingInput;

    public event Func<string, Task>? CommandSubmitted;
    public event Action? CancelRequested;
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<string>? WorkingDirectoryProvider { get; set; }

    public InteractiveTerminal()
    {
        BackColor = Color.FromArgb(5, 5, 5);
        ForeColor = Color.Gainsboro;
        BorderStyle = BorderStyle.None;
        Font = new Font("Consolas", 11.5f, FontStyle.Regular, GraphicsUnit.Point);
        DetectUrls = false;
        WordWrap = false;
        HideSelection = false;
        AcceptsTab = true;
        ShortcutsEnabled = true;
        KeyDown += OnTerminalKeyDown;
        MouseDown += (_, _) => BeginInvoke(KeepCaretInInputArea);
    }

    public void ResetForDirectory(string directory)
    {
        Clear();
        AppendPrompt(directory);
    }

    public void AppendOutput(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendOutput(text));
            return;
        }

        string clean = AnsiRegex().Replace(text, string.Empty).Replace("\r\r\n", "\r\n");
        SelectionStart = TextLength;
        SelectionLength = 0;
        SelectionColor = Color.Gainsboro;
        AppendText(clean);
        SelectionColor = ForeColor;
        ScrollToCaret();
    }

    public void ShowCommand(string command)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => ShowCommand(command));
            return;
        }

        SelectionStart = TextLength;
        SelectionLength = 0;
        SelectionColor = Color.WhiteSmoke;
        AppendText(command + Environment.NewLine);
        SelectionColor = ForeColor;
        _acceptingInput = false;
    }

    public void AppendPrompt(string directory)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendPrompt(directory));
            return;
        }

        if (TextLength > 0 && !Text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            AppendText(Environment.NewLine);

        string user = Environment.UserName;
        string machine = Environment.MachineName.ToLowerInvariant();
        string displayPath = ToBashDisplayPath(directory);

        SelectionStart = TextLength;
        SelectionColor = Color.LimeGreen;
        AppendText($"{user}@{machine} ");
        SelectionColor = Color.Magenta;
        AppendText("MINGW64 ");
        SelectionColor = Color.Gold;
        AppendText(displayPath + Environment.NewLine);
        SelectionColor = Color.WhiteSmoke;
        AppendText("$ ");
        SelectionColor = ForeColor;

        _inputStart = TextLength;
        _acceptingInput = true;
        ReadOnly = false;
        SelectionStart = TextLength;
        ScrollToCaret();
        Focus();
    }

    public void SetBusy(bool busy)
    {
        _acceptingInput = !busy;
        ReadOnly = busy;
        if (!busy)
        {
            SelectionStart = TextLength;
            Focus();
        }
    }

    public void ClearAndPrompt(string directory)
    {
        Clear();
        AppendPrompt(directory);
    }

    private async void OnTerminalKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.C)
        {
            if (!_acceptingInput)
            {
                e.SuppressKeyPress = true;
                CancelRequested?.Invoke();
            }
            return;
        }

        if (!_acceptingInput)
        {
            e.SuppressKeyPress = true;
            return;
        }

        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            string command = Text[_inputStart..].TrimEnd('\r', '\n');
            AppendText(Environment.NewLine);
            _acceptingInput = false;
            ReadOnly = true;

            if (!string.IsNullOrWhiteSpace(command))
            {
                _history.Add(command);
                _historyIndex = _history.Count;
                if (CommandSubmitted is not null)
                    await CommandSubmitted.Invoke(command);
            }
            else
            {
                AppendPrompt(WorkingDirectoryProvider?.Invoke() ?? Environment.CurrentDirectory);
            }
            return;
        }

        if (e.KeyCode == Keys.Back && SelectionStart <= _inputStart && SelectionLength == 0)
        {
            e.SuppressKeyPress = true;
            return;
        }

        if (e.KeyCode == Keys.Home)
        {
            e.SuppressKeyPress = true;
            SelectionStart = _inputStart;
            return;
        }

        if (e.KeyCode is Keys.Up or Keys.Down)
        {
            e.SuppressKeyPress = true;
            NavigateHistory(e.KeyCode == Keys.Up ? -1 : 1);
            return;
        }

        if (SelectionStart < _inputStart)
            SelectionStart = TextLength;
    }

    private void NavigateHistory(int delta)
    {
        if (_history.Count == 0)
            return;

        _historyIndex = Math.Clamp(_historyIndex + delta, 0, _history.Count);
        string value = _historyIndex == _history.Count ? string.Empty : _history[_historyIndex];
        Select(_inputStart, TextLength - _inputStart);
        SelectedText = value;
        SelectionStart = TextLength;
    }

    private void KeepCaretInInputArea()
    {
        if (_acceptingInput && SelectionStart < _inputStart && SelectionLength == 0)
            SelectionStart = TextLength;
    }

    private static string ToBashDisplayPath(string directory)
    {
        if (directory.StartsWith("/", StringComparison.Ordinal))
            return directory;

        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (directory.StartsWith(home, StringComparison.OrdinalIgnoreCase))
        {
            string relative = directory[home.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return relative.Length == 0 ? "~" : "~/" + relative.Replace('\\', '/');
        }

        if (directory.Length >= 3 && directory[1] == ':')
            return "/" + char.ToLowerInvariant(directory[0]) + directory[2..].Replace('\\', '/');

        return directory.Replace('\\', '/');
    }

    [GeneratedRegex("\\x1B(?:[@-Z\\\\-_]|\\[[0-?]*[ -/]*[@-~])")]
    private static partial Regex AnsiRegex();
}
