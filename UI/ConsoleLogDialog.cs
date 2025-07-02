namespace DOSGameCollection.UI;

public class ConsoleLogDialog : Form
{
    private TextBox logTextBox;

    public ConsoleLogDialog(string initialLogContent)
    {
        InitializeComponent();
        
        if (logTextBox != null)
        {
            logTextBox.Text = initialLogContent;
        }
    }

    private void InitializeComponent()
    {
        Text = "Console Log";
        FormBorderStyle = FormBorderStyle.Sizable;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(600, 400);
        MinimumSize = new Size(400, 300);
        ShowInTaskbar = false;

        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10)
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        logTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9F)
        };

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };

        var okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, Size = new Size(75, 25) };
        okButton.Click += (sender, e) => this.Close();

        var clearButton = new Button { Text = "Clear", Size = new Size(75, 25) };
        clearButton.Click += ClearButton_Click;

        buttonPanel.Controls.Add(okButton);
        buttonPanel.Controls.Add(clearButton);
        mainPanel.Controls.Add(logTextBox, 0, 0);
        mainPanel.Controls.Add(buttonPanel, 0, 1);

        Controls.Add(mainPanel);
        AcceptButton = okButton;
    }

    private void ClearButton_Click(object? sender, EventArgs e)
    {
        AppLogger.ClearLogs();
        logTextBox.Clear();
    }
}
