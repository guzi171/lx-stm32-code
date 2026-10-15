using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace GitCommandHelper;

internal sealed class RoundedPanel : Panel
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 10;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = Color.FromArgb(225, 230, 237);

    public RoundedPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateRegion();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Width < 2 || Height < 2)
            return;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath path = CreateRoundedPath(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), CornerRadius);
        using var pen = new Pen(BorderColor, 1f);
        e.Graphics.DrawPath(pen, path);
    }

    private void UpdateRegion()
    {
        if (Width <= 0 || Height <= 0)
            return;

        using GraphicsPath path = CreateRoundedPath(new RectangleF(0, 0, Width, Height), CornerRadius);
        Region?.Dispose();
        Region = new Region(path);
    }

    internal static GraphicsPath CreateRoundedPath(RectangleF bounds, float radius)
    {
        float diameter = Math.Min(radius * 2f, Math.Min(bounds.Width, bounds.Height));
        var path = new GraphicsPath();
        if (diameter <= 1f)
        {
            path.AddRectangle(bounds);
            path.CloseFigure();
            return path;
        }

        var arc = new RectangleF(bounds.X, bounds.Y, diameter, diameter);
        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}

internal sealed class InputFrame : Panel
{
    private readonly TextBox _textBox;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 7;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = Color.FromArgb(207, 214, 224);

    public InputFrame(TextBox textBox)
    {
        _textBox = textBox;
        Height = 34;
        MinimumSize = new Size(0, 34);
        Dock = DockStyle.Top;
        Margin = new Padding(0, 0, 0, 7);
        Padding = new Padding(10, 0, 10, 0);
        BackColor = textBox.BackColor;
        DoubleBuffered = true;
        ResizeRedraw = true;
        TabStop = false;

        textBox.BorderStyle = BorderStyle.None;
        textBox.Dock = DockStyle.None;
        textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        textBox.Margin = new Padding(0);
        Controls.Add(textBox);

        textBox.BackColorChanged += (_, _) =>
        {
            BackColor = textBox.BackColor;
            Invalidate();
        };
        textBox.ReadOnlyChanged += (_, _) => Invalidate();
        textBox.GotFocus += (_, _) => Invalidate();
        textBox.LostFocus += (_, _) => Invalidate();
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        LayoutTextBox();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        LayoutTextBox();
        if (Width <= 0 || Height <= 0)
            return;

        using GraphicsPath path = RoundedPanel.CreateRoundedPath(new RectangleF(0, 0, Width, Height), CornerRadius);
        Region?.Dispose();
        Region = new Region(path);
    }

    private void LayoutTextBox()
    {
        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
            return;

        int textHeight = _textBox.PreferredSize.Height;
        int top = Math.Max(0, (ClientSize.Height - textHeight) / 2 + 1);
        int width = Math.Max(0, ClientSize.Width - Padding.Horizontal);
        _textBox.SetBounds(Padding.Left, top, width, textHeight);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Width < 2 || Height < 2)
            return;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        bool showFocus = _textBox.Focused && !_textBox.ReadOnly;
        Color color = showFocus ? Color.FromArgb(84, 139, 245) : BorderColor;
        float width = showFocus ? 1.5f : 1f;
        using GraphicsPath path = RoundedPanel.CreateRoundedPath(new RectangleF(0.75f, 0.75f, Width - 1.5f, Height - 1.5f), CornerRadius);
        using var pen = new Pen(color, width);
        e.Graphics.DrawPath(pen, path);
    }
}

internal sealed class RoundedButton : Button
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 7;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = Color.Transparent;

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        UseVisualStyleBackColor = false;
        ResizeRedraw = true;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (Width <= 0 || Height <= 0)
            return;

        using GraphicsPath path = RoundedPanel.CreateRoundedPath(new RectangleF(0, 0, Width, Height), CornerRadius);
        Region?.Dispose();
        Region = new Region(path);
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        base.OnPaint(pevent);
        if (BorderColor == Color.Transparent || Width < 2 || Height < 2)
            return;

        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath path = RoundedPanel.CreateRoundedPath(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), CornerRadius);
        using var pen = new Pen(BorderColor, 1f);
        pevent.Graphics.DrawPath(pen, path);
    }
}
