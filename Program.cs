namespace GitCommandHelper;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase))
            return SmokeTest.RunAsync().GetAwaiter().GetResult();

        ApplicationConfiguration.Initialize();

        int previewIndex = Array.FindIndex(args, value =>
            string.Equals(value, "--render-preview", StringComparison.OrdinalIgnoreCase));
        if (previewIndex >= 0)
        {
            string outputPath = previewIndex + 1 < args.Length
                ? Path.GetFullPath(args[previewIndex + 1])
                : Path.Combine(Environment.CurrentDirectory, "git-command-helper-preview.png");
            return RenderPreview(outputPath);
        }

        Application.Run(new MainForm());
        return 0;
    }

    private static int RenderPreview(string outputPath)
    {
        try
        {
            string? directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            using var form = new MainForm
            {
                StartPosition = FormStartPosition.Manual,
                Location = new Point(-32000, -32000),
                ShowInTaskbar = false
            };

            CreateHandles(form);
            form.PreparePreview();
            form.PerformLayout();
            using var bitmap = new Bitmap(form.ClientSize.Width, form.ClientSize.Height);
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.ClientSize));
            bitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
            Console.WriteLine(outputPath);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static void CreateHandles(Control control)
    {
        control.CreateControl();
        _ = control.Handle;
        foreach (Control child in control.Controls)
            CreateHandles(child);
    }
}
