namespace GitCommandHelper;

internal sealed class MainForm : Form
{
    private static readonly Color BackgroundColor = Color.FromArgb(245, 247, 250);
    private static readonly Color BorderColor = Color.FromArgb(214, 219, 226);
    private static readonly Color PrimaryColor = Color.FromArgb(24, 119, 242);

    private readonly TextBox _directoryTextBox = new();
    private readonly TextBox _localBranchTextBox = new();
    private readonly TextBox _remoteUrlTextBox = new();
    private readonly TextBox _commitMessageTextBox = new();
    private readonly TextBox _remoteBranchTextBox = new();
    private readonly CheckBox _sameBranchCheckBox = new();
    private readonly Label _terminalStatusLabel = new();
    private readonly InteractiveTerminal _terminal = new();
    private readonly List<Control> _commandControls = [];
    private Button _initializeButton = null!;
    private Button _statusButton = null!;
    private Button _remoteButton = null!;
    private Button _addButton = null!;
    private Button _commitButton = null!;
    private Button _pushButton = null!;
    private Button _allButton = null!;
    private Button _browseButton = null!;

    private BashShellSession? _shell;
    private CancellationTokenSource? _commandCancellation;
    private string _activeDirectory = string.Empty;
    private string _shellDisplayDirectory = string.Empty;
    private bool _isBusy;

    public MainForm()
    {
        Text = "Git 命令助手";
        Icon = LoadAppIcon();
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(940, 620);
        Size = new Size(1120, 720);
        BackColor = BackgroundColor;
        Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeLayout();
        ConfigureDefaults();

        Shown += (_, _) => ActivateDirectory(showPrompt: true);
        FormClosed += (_, _) =>
        {
            _commandCancellation?.Cancel();
            _shell?.Dispose();
        };
    }

    private static Icon LoadAppIcon()
    {
        using Stream stream = typeof(MainForm).Assembly.GetManifestResourceStream("GitCommandHelper.AppIcon.ico")
            ?? throw new InvalidOperationException("找不到应用程序图标资源。");
        using var icon = new Icon(stream);
        return (Icon)icon.Clone();
    }

    public void PreparePreview()
    {
        ActivateDirectory(showPrompt: true);
    }

    private void InitializeLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(14, 12, 14, 14),
            BackColor = BackgroundColor
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        root.Controls.Add(BuildDirectoryBar(), 0, 0);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel2,
            SplitterWidth = 7,
            BackColor = BorderColor
        };
        split.Panel1.BackColor = BackgroundColor;
        split.Panel2.BackColor = BackgroundColor;
        split.SizeChanged += (_, _) =>
        {
            if (split.Width < 500)
                return;

            int desired = Math.Clamp(split.Width - 355, 300, split.Width - 100);
            if (desired > 0 && desired < split.Width)
                split.SplitterDistance = desired;
        };
        root.Controls.Add(split, 0, 1);

        split.Panel1.Controls.Add(BuildTerminalArea());
        split.Panel2.Controls.Add(BuildShortcutPanel());
    }

    private Control BuildDirectoryBar()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 3,
            Margin = new Padding(0, 0, 0, 10),
            BackColor = Color.White,
            Padding = new Padding(12, 7, 12, 7)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));

        var label = CreateLabel("工程目录");
        label.Font = new Font(Font, FontStyle.Bold);
        panel.Controls.Add(label, 0, 0);

        var directoryInput = CreateInput(_directoryTextBox);
        directoryInput.Dock = DockStyle.Fill;
        directoryInput.Margin = new Padding(0);
        _directoryTextBox.Leave += (_, _) => ActivateDirectory(showPrompt: true);
        _directoryTextBox.TextChanged += (_, _) => UpdateStatus();
        _directoryTextBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            ActivateDirectory(showPrompt: true);
        };
        panel.Controls.Add(directoryInput, 1, 0);

        _browseButton = CreateButton("浏览");
        _browseButton.Margin = new Padding(8, 0, 0, 0);
        _browseButton.Click += (_, _) => BrowseDirectory();
        panel.Controls.Add(_browseButton, 2, 0);
        return panel;
    }

    private Control BuildTerminalArea()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.White,
            Margin = new Padding(0)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            MinimumSize = new Size(0, 46),
            ColumnCount = 4,
            BackColor = Color.White,
            Padding = new Padding(12, 5, 6, 5)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 98));

        var title = CreateLabel("交互终端");
        title.Font = new Font(Font, FontStyle.Bold);
        header.Controls.Add(title, 0, 0);

        _terminalStatusLabel.Text = "Git 检查中";
        _terminalStatusLabel.ForeColor = Color.DimGray;
        _terminalStatusLabel.Dock = DockStyle.Fill;
        _terminalStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _terminalStatusLabel.Margin = new Padding(14, 0, 0, 0);
        header.Controls.Add(_terminalStatusLabel, 1, 0);

        var clearButton = CreateButton("清空");
        clearButton.Margin = new Padding(4, 0, 4, 0);
        clearButton.Click += (_, _) => _terminal.ClearAndPrompt(CurrentDirectory);
        header.Controls.Add(clearButton, 2, 0);

        var copyButton = CreateButton("复制输出");
        copyButton.Margin = new Padding(4, 0, 0, 0);
        copyButton.Click += (_, _) =>
        {
            if (!string.IsNullOrEmpty(_terminal.Text))
                Clipboard.SetText(_terminal.Text);
        };
        header.Controls.Add(copyButton, 3, 0);

        panel.Controls.Add(header, 0, 0);
        _terminal.Dock = DockStyle.Fill;
        _terminal.Margin = new Padding(0);
        _terminal.WorkingDirectoryProvider = () =>
            string.IsNullOrWhiteSpace(_shellDisplayDirectory) ? CurrentDirectory : _shellDisplayDirectory;
        _terminal.CommandSubmitted += command => ExecuteFromTerminalAsync(command);
        _terminal.CancelRequested += () =>
        {
            _commandCancellation?.Cancel();
            _shell?.CancelCurrentCommand();
        };
        panel.Controls.Add(_terminal, 0, 1);
        return panel;
    }

    private Control BuildShortcutPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12, 10, 8, 12),
            BackColor = BackgroundColor
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        panel.Controls.Add(CreateSectionTitle("快捷操作"));

        var localContent = CreateCard(out var localCard);
        localContent.Controls.Add(CreateCardTitle("仓库设置"));
        localContent.Controls.Add(CreateLabel("本地分支"));
        localContent.Controls.Add(CreateInput(_localBranchTextBox));
        _localBranchTextBox.TextChanged += (_, _) =>
        {
            if (_sameBranchCheckBox.Checked)
                _remoteBranchTextBox.Text = _localBranchTextBox.Text;
        };

        var initRow = CreateButtonRow();
        _initializeButton = CreateButton("初始化仓库", primary: true);
        _statusButton = CreateButton("查看状态");
        _initializeButton.Margin = new Padding(0, 0, 4, 6);
        _statusButton.Margin = new Padding(4, 0, 0, 6);
        _initializeButton.Click += async (_, _) => await InitializeRepositoryAsync();
        _statusButton.Click += async (_, _) => await ShowRepositoryStatusAsync();
        initRow.Controls.Add(_initializeButton, 0, 0);
        initRow.Controls.Add(_statusButton, 1, 0);
        localContent.Controls.Add(initRow);

        localContent.Controls.Add(CreateLabel("远程仓库地址"));
        localContent.Controls.Add(CreateInput(_remoteUrlTextBox));
        _remoteButton = CreateButton("关联远程");
        _remoteButton.Click += async (_, _) => await AssociateRemoteAsync();
        _remoteButton.Margin = new Padding(0);
        localContent.Controls.Add(_remoteButton);
        panel.Controls.Add(localCard);

        var publishContent = CreateCard(out var publishCard);
        publishContent.Controls.Add(CreateCardTitle("提交与推送"));
        publishContent.Controls.Add(CreateLabel("提交说明"));
        publishContent.Controls.Add(CreateInput(_commitMessageTextBox));

        var commitRow = CreateButtonRow();
        _addButton = CreateButton("添加全部文件");
        _commitButton = CreateButton("创建提交");
        _addButton.Margin = new Padding(0, 0, 4, 6);
        _commitButton.Margin = new Padding(4, 0, 0, 6);
        _addButton.Click += async (_, _) => await AddAllAsync();
        _commitButton.Click += async (_, _) => await CommitAsync();
        commitRow.Controls.Add(_addButton, 0, 0);
        commitRow.Controls.Add(_commitButton, 1, 0);
        publishContent.Controls.Add(commitRow);

        publishContent.Controls.Add(CreateLabel("远程分支"));
        publishContent.Controls.Add(CreateInput(_remoteBranchTextBox));

        _sameBranchCheckBox.Text = "与本地分支同名";
        _sameBranchCheckBox.AutoSize = true;
        _sameBranchCheckBox.Checked = true;
        _sameBranchCheckBox.Margin = new Padding(2, 2, 0, 6);
        _sameBranchCheckBox.CheckedChanged += (_, _) =>
        {
            _remoteBranchTextBox.ReadOnly = _sameBranchCheckBox.Checked;
            _remoteBranchTextBox.BackColor = _sameBranchCheckBox.Checked ? Color.FromArgb(245, 246, 248) : Color.White;
            if (_sameBranchCheckBox.Checked)
                _remoteBranchTextBox.Text = _localBranchTextBox.Text;
        };
        publishContent.Controls.Add(_sameBranchCheckBox);

        _pushButton = CreateButton("仅推送");
        _pushButton.Click += async (_, _) => await PushAsync();

        _allButton = CreateButton("提交并推送", primary: true);
        _allButton.Click += async (_, _) => await CommitAndPushAsync();
        var pushRow = CreateButtonRow();
        _pushButton.Margin = new Padding(0, 0, 4, 0);
        _allButton.Margin = new Padding(4, 0, 0, 0);
        pushRow.Controls.Add(_pushButton, 0, 0);
        pushRow.Controls.Add(_allButton, 1, 0);
        publishContent.Controls.Add(pushRow);
        panel.Controls.Add(publishCard);

        panel.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = BackgroundColor });

        _commandControls.AddRange(
        [
            _initializeButton, _statusButton, _remoteButton, _addButton, _commitButton,
            _pushButton, _allButton, _localBranchTextBox, _remoteUrlTextBox,
            _commitMessageTextBox, _remoteBranchTextBox, _sameBranchCheckBox
        ]);

        return panel;
    }

    private void ConfigureDefaults()
    {
        string workspace = @"C:\Users\57233\Desktop\lx-stm32-code\github-code\t05-02-easylog_rtt";
        _directoryTextBox.Text = Directory.Exists(workspace) ? workspace : Environment.CurrentDirectory;
        _localBranchTextBox.Text = "t05-02-easylog_rtt";
        _remoteUrlTextBox.Text = "https://github.com/guzi171/lx-stm32-code.git";
        _commitMessageTextBox.Text = "初次提交";
        _remoteBranchTextBox.Text = _localBranchTextBox.Text;
        _remoteBranchTextBox.ReadOnly = true;
        _remoteBranchTextBox.BackColor = Color.FromArgb(245, 246, 248);
    }

    private async Task ExecuteFromTerminalAsync(string command)
    {
        await ExecuteCommandCoreAsync(command, showCommand: false);
    }

    private async Task<int> ExecuteButtonCommandAsync(string command)
    {
        return await ExecuteCommandCoreAsync(command, showCommand: true);
    }

    private async Task<int> ExecuteCommandCoreAsync(string command, bool showCommand)
    {
        if (!TryPrepareDirectory())
        {
            _terminal.AppendPrompt(CurrentDirectory);
            return -1;
        }

        SetBusy(true);
        if (showCommand)
            _terminal.ShowCommand(command);

        _commandCancellation = new CancellationTokenSource();
        try
        {
            ShellCommandResult result = await _shell!.ExecuteAsync(
                command,
                _commandCancellation.Token,
                resetToWorkingDirectory: showCommand);
            _shellDisplayDirectory = result.ShellDirectory;
            if (result.ExitCode != 0)
                _terminal.AppendOutput($"命令执行失败，退出码：{result.ExitCode}{Environment.NewLine}");
            return result.ExitCode;
        }
        catch (OperationCanceledException)
        {
            _terminal.AppendOutput("^C" + Environment.NewLine);
            return 130;
        }
        catch (Exception ex)
        {
            _terminal.AppendOutput("错误：" + ex.Message + Environment.NewLine);
            return -1;
        }
        finally
        {
            _commandCancellation.Dispose();
            _commandCancellation = null;
            SetBusy(false);
            UpdateStatus();
            _terminal.AppendPrompt(string.IsNullOrWhiteSpace(_shellDisplayDirectory) ? CurrentDirectory : _shellDisplayDirectory);
        }
    }

    private async Task InitializeRepositoryAsync()
    {
        await InitializeRepositoryCoreAsync(askForConfirmation: false);
    }

    private async Task<bool> InitializeRepositoryCoreAsync(bool askForConfirmation)
    {
        string branch = _localBranchTextBox.Text.Trim();
        if (!ValidateBranch(branch, "本地分支"))
            return false;

        if (!TryPrepareDirectory())
            return false;

        if (Directory.Exists(Path.Combine(CurrentDirectory, ".git")))
        {
            return true;
        }

        if (askForConfirmation &&
            MessageBox.Show(this,
                $"当前工程还没有初始化 Git。{Environment.NewLine}{Environment.NewLine}是否现在创建本地分支 {branch}？",
                "需要先初始化",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1) != DialogResult.Yes)
        {
            return false;
        }

        return await ExecuteButtonCommandAsync($"git init -b {BashQuote(branch)}") == 0;
    }

    private async Task ShowRepositoryStatusAsync()
    {
        if (!TryPrepareDirectory())
            return;

        if (!Directory.Exists(Path.Combine(CurrentDirectory, ".git")))
        {
            _terminal.ShowCommand("git status -sb");
            _terminal.AppendOutput("当前工程尚未初始化 Git。可以点击“初始化仓库”，也可以直接点击“提交并推送”。" + Environment.NewLine);
            _terminal.AppendPrompt(CurrentDirectory);
            return;
        }

        await ExecuteButtonCommandAsync("git status -sb");
    }

    private async Task AssociateRemoteAsync()
    {
        string url = _remoteUrlTextBox.Text.Trim();
        if (url.Length == 0)
        {
            ShowValidation("请输入远程仓库地址。", _remoteUrlTextBox);
            return;
        }

        if (!await InitializeRepositoryCoreAsync(askForConfirmation: true))
            return;

        string quoted = BashQuote(url);
        string command =
            $"if git remote get-url origin >/dev/null 2>&1; then git remote set-url origin {quoted}; else git remote add origin {quoted}; fi && git remote -v";
        await ExecuteButtonCommandAsync(command);
    }

    private async Task CommitAsync()
    {
        string message = _commitMessageTextBox.Text.Trim();
        if (message.Length == 0)
        {
            ShowValidation("请输入提交说明。", _commitMessageTextBox);
            return;
        }

        if (!await InitializeRepositoryCoreAsync(askForConfirmation: true))
            return;

        await ExecuteButtonCommandAsync($"git commit -m {BashQuote(message)}");
    }

    private async Task AddAllAsync()
    {
        if (!await InitializeRepositoryCoreAsync(askForConfirmation: true))
            return;

        await ExecuteButtonCommandAsync("git add -A");
    }

    private async Task PushAsync()
    {
        if (!TryGetPushValues(out string local, out string remote))
            return;

        if (!await InitializeRepositoryCoreAsync(askForConfirmation: true))
            return;

        if (!ConfirmPush(local, remote))
            return;

        await ExecuteButtonCommandAsync($"git push -u origin {BashQuote(local + ":" + remote)}");
    }

    private async Task CommitAndPushAsync()
    {
        if (!TryGetPushValues(out string local, out string remote))
            return;

        string message = _commitMessageTextBox.Text.Trim();
        if (message.Length == 0)
        {
            ShowValidation("请输入提交说明。", _commitMessageTextBox);
            return;
        }

        string url = _remoteUrlTextBox.Text.Trim();
        if (url.Length == 0)
        {
            ShowValidation("请输入远程仓库地址。", _remoteUrlTextBox);
            return;
        }

        if (!ConfirmPush(local, remote))
            return;

        int code;
        if (!Directory.Exists(Path.Combine(CurrentDirectory, ".git")))
        {
            code = await ExecuteButtonCommandAsync($"git init -b {BashQuote(local)}");
            if (code != 0) return;
        }
        else
        {
            code = await ExecuteButtonCommandAsync(
                $"if git show-ref --verify --quiet refs/heads/{BashQuote(local)}; then git switch {BashQuote(local)}; else git switch -c {BashQuote(local)}; fi");
            if (code != 0) return;
        }

        string quotedUrl = BashQuote(url);
        code = await ExecuteButtonCommandAsync(
            $"if git remote get-url origin >/dev/null 2>&1; then git remote set-url origin {quotedUrl}; else git remote add origin {quotedUrl}; fi");
        if (code != 0) return;

        code = await ExecuteButtonCommandAsync("git add -A");
        if (code != 0) return;

        code = await ExecuteButtonCommandAsync(
            $"if git diff --cached --quiet; then printf '没有需要提交的更改\\n'; else git commit -m {BashQuote(message)}; fi");
        if (code != 0) return;

        await ExecuteButtonCommandAsync($"git push -u origin {BashQuote(local + ":" + remote)}");
    }

    private bool TryGetPushValues(out string local, out string remote)
    {
        local = _localBranchTextBox.Text.Trim();
        remote = _remoteBranchTextBox.Text.Trim();
        return ValidateBranch(local, "本地分支") && ValidateBranch(remote, "远程分支");
    }

    private bool ValidateBranch(string branch, string fieldName)
    {
        if (!IsValidBranchName(branch))
        {
            ShowValidation($"{fieldName}名称不合法。", fieldName == "本地分支" ? _localBranchTextBox : _remoteBranchTextBox);
            return false;
        }
        return true;
    }

    private void SyncBranchForDirectory(string directory)
    {
        string suggested = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        string headPath = Path.Combine(directory, ".git", "HEAD");
        if (File.Exists(headPath))
        {
            try
            {
                const string prefix = "ref: refs/heads/";
                string head = File.ReadAllText(headPath).Trim();
                if (head.StartsWith(prefix, StringComparison.Ordinal))
                    suggested = head[prefix.Length..];
            }
            catch
            {
                // Keep the folder name if HEAD cannot be read.
            }
        }

        if (IsValidBranchName(suggested))
            _localBranchTextBox.Text = suggested;
    }

    private static bool IsValidBranchName(string branch) =>
        branch.Length > 0 &&
        !branch.Any(char.IsWhiteSpace) &&
        !branch.Contains("..", StringComparison.Ordinal) &&
        !branch.StartsWith('.') &&
        !branch.EndsWith('.') &&
        !branch.EndsWith('/') &&
        !branch.Contains('~') &&
        !branch.Contains('^') &&
        !branch.Contains(':') &&
        !branch.Contains('?') &&
        !branch.Contains('*') &&
        !branch.Contains('[') &&
        !branch.Contains('\\');

    private bool ConfirmPush(string local, string remote)
    {
        string message = $"即将推送到远程仓库：{Environment.NewLine}{_remoteUrlTextBox.Text.Trim()}{Environment.NewLine}{Environment.NewLine}" +
                         $"{local}  →  origin/{remote}{Environment.NewLine}{Environment.NewLine}是否继续？";
        return MessageBox.Show(this, message, "确认推送", MessageBoxButtons.YesNo,
            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
    }

    private bool TryPrepareDirectory()
    {
        string directory;
        try
        {
            directory = Path.GetFullPath(CurrentDirectory);
        }
        catch
        {
            MessageBox.Show(this, "工程目录格式不正确。", "目录错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _directoryTextBox.Focus();
            return false;
        }

        if (!Directory.Exists(directory))
        {
            MessageBox.Show(this, "工程目录不存在。", "目录错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _directoryTextBox.Focus();
            return false;
        }

        if (!string.Equals(_activeDirectory, directory, StringComparison.OrdinalIgnoreCase))
            ActivateDirectory(showPrompt: false);

        return _shell is not null &&
               string.Equals(_activeDirectory, directory, StringComparison.OrdinalIgnoreCase);
    }

    private void ActivateDirectory(bool showPrompt)
    {
        string directory;
        try
        {
            directory = Path.GetFullPath(CurrentDirectory);
        }
        catch
        {
            UpdateStatus();
            return;
        }

        if (!Directory.Exists(directory))
        {
            UpdateStatus();
            return;
        }

        if (!string.Equals(_activeDirectory, directory, StringComparison.OrdinalIgnoreCase))
        {
            _shell?.Dispose();
            try
            {
                _shell = new BashShellSession(directory);
                _shell.OutputReceived += text => _terminal.AppendOutput(text);
                _activeDirectory = directory;
                _shellDisplayDirectory = directory;
                SyncBranchForDirectory(directory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Git Bash 不可用", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _shell = null;
            }
        }

        if (!string.Equals(_directoryTextBox.Text, directory, StringComparison.Ordinal))
            _directoryTextBox.Text = directory;

        UpdateStatus();
        if (showPrompt && _shell is not null)
            _terminal.ResetForDirectory(directory);
    }

    private void BrowseDirectory()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择要执行 Git 操作的本地工程目录",
            InitialDirectory = Directory.Exists(CurrentDirectory) ? CurrentDirectory : Environment.CurrentDirectory,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _directoryTextBox.Text = dialog.SelectedPath;
            ActivateDirectory(showPrompt: true);
        }
    }

    private void UpdateStatus()
    {
        bool directoryExists = false;
        string directory = CurrentDirectory;
        try
        {
            directoryExists = Directory.Exists(Path.GetFullPath(directory));
        }
        catch
        {
            // Invalid input is reflected in the status text below.
        }

        bool gitFound = BashShellSession.FindBashPath() is not null;
        bool initialized = directoryExists && Directory.Exists(Path.Combine(Path.GetFullPath(directory), ".git"));
        string directoryState = !directoryExists
            ? "目录不存在"
            : initialized ? "已初始化" : "尚未初始化，请先点击“初始化仓库”";
        _terminalStatusLabel.Text = $"Git {(gitFound ? "已找到" : "未找到")}  ·  {directoryState}";
        _terminalStatusLabel.ForeColor = !directoryExists
            ? Color.FromArgb(190, 80, 55)
            : initialized ? Color.SeaGreen : Color.DimGray;

        ApplyControlState(directoryExists, initialized);
    }

    private void SetBusy(bool busy)
    {
        _isBusy = busy;
        foreach (Control control in _commandControls)
            control.Enabled = !busy;
        _directoryTextBox.Enabled = !busy;
        _browseButton.Enabled = !busy;
        _terminal.SetBusy(busy);
        UseWaitCursor = busy;
        UpdateStatus();
    }

    private void ApplyControlState(bool directoryExists, bool initialized)
    {
        if (_initializeButton is null)
            return;

        bool ready = !_isBusy && directoryExists;
        _initializeButton.Text = initialized ? "已初始化" : "初始化仓库";
        _initializeButton.Enabled = ready && !initialized;
        _statusButton.Enabled = ready;
        _remoteButton.Enabled = ready;
        _addButton.Enabled = ready;
        _commitButton.Enabled = ready;
        _pushButton.Enabled = ready;
        _allButton.Enabled = ready;
    }

    private string CurrentDirectory => _directoryTextBox.Text.Trim();

    private static string BashQuote(string value) => "'" + value.Replace("'", "'\\''", StringComparison.Ordinal) + "'";

    private void ShowValidation(string message, Control target)
    {
        MessageBox.Show(this, message, "输入有误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        target.Focus();
    }

    private static Label CreateLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Margin = new Padding(0, 4, 0, 3),
        TextAlign = ContentAlignment.MiddleLeft
    };

    private static Label CreateSectionTitle(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font("Microsoft YaHei UI", 11f, FontStyle.Bold),
        ForeColor = Color.FromArgb(26, 55, 95),
        Margin = new Padding(2, 2, 0, 10)
    };

    private static Label CreateCardTitle(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font("Microsoft YaHei UI", 9.5f, FontStyle.Bold),
        ForeColor = Color.FromArgb(34, 49, 72),
        Margin = new Padding(0, 0, 0, 5)
    };

    private static InputFrame CreateInput(TextBox textBox)
    {
        textBox.Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Regular);
        textBox.BackColor = Color.White;
        return new InputFrame(textBox);
    }

    private static Button CreateButton(string text, bool primary = false)
    {
        Color normalBackColor = primary ? PrimaryColor : Color.FromArgb(241, 244, 248);
        Color normalForeColor = primary ? Color.White : Color.FromArgb(45, 58, 76);
        var button = new RoundedButton
        {
            Text = text,
            Dock = DockStyle.Fill,
            Height = 32,
            Margin = new Padding(0, 0, 0, 6),
            Cursor = Cursors.Hand,
            BackColor = normalBackColor,
            ForeColor = normalForeColor,
            BorderColor = primary ? PrimaryColor : Color.FromArgb(220, 226, 234)
        };
        button.FlatAppearance.MouseOverBackColor = primary
            ? Color.FromArgb(14, 102, 220)
            : Color.FromArgb(232, 237, 244);
        button.FlatAppearance.MouseDownBackColor = primary
            ? Color.FromArgb(10, 88, 194)
            : Color.FromArgb(222, 229, 238);
        button.EnabledChanged += (_, _) =>
        {
            button.BackColor = button.Enabled ? normalBackColor : Color.FromArgb(242, 244, 247);
            button.ForeColor = button.Enabled ? normalForeColor : Color.FromArgb(155, 163, 174);
            button.BorderColor = button.Enabled
                ? primary ? PrimaryColor : Color.FromArgb(220, 226, 234)
                : Color.FromArgb(229, 233, 238);
        };
        return button;
    }

    private static TableLayoutPanel CreateButtonRow()
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Margin = new Padding(0)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        return row;
    }

    private static TableLayoutPanel CreateCard(out RoundedPanel card)
    {
        card = new RoundedPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(225, 230, 237),
            Padding = new Padding(11),
            Margin = new Padding(0, 0, 4, 10)
        };

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            BackColor = Color.White,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        card.Controls.Add(content);
        return content;
    }
}
