using System.Drawing;
using System.Windows.Forms;

namespace DOSGameCollection.UI;

public class ConsoleLogDialog : Form
{
    public ConsoleLogDialog(string logContents)
    {
        InitializeComponent(logContents);
    }

    private void InitializeComponent(string logContents)
    {
        this.Text = "Console Log";
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.StartPosition = FormStartPosition.CenterParent;
        this.ClientSize = new Size(600, 400);
        this.MinimumSize = new Size(400, 300);
        this.ShowInTaskbar = false;

        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10)
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var logTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9F),
            Text = logContents
        };

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };

        var okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, Size = new Size(75, 25) };
        okButton.Click += (sender, e) => this.Close();

        buttonPanel.Controls.Add(okButton);
        mainPanel.Controls.Add(logTextBox, 0, 0);
        mainPanel.Controls.Add(buttonPanel, 0, 1);

        this.Controls.Add(mainPanel);
        this.AcceptButton = okButton;
    }
}

