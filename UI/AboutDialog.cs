using System.Drawing;
using System.Windows.Forms;

namespace DOSGameCollection.UI;

public class AboutDialog : Form
{
    public AboutDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "About DOSGameCollection";
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterParent;
        this.ClientSize = new Size(320, 160);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.ShowInTaskbar = false;

        var aboutLabel = new Label
        {
            Text = $"DOSGameCollection\n\nVersion: {BuildInfo.BuildVersion}\n\nAn application to manage and launch your DOS games.",
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };

        var okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Size = new Size(75, 25)
        };
        okButton.Location = new Point((this.ClientSize.Width - okButton.Width) / 2, this.ClientSize.Height - okButton.Height - 12);
        okButton.Click += (sender, e) => this.Close();

        this.Controls.Add(aboutLabel);
        this.Controls.Add(okButton);
        this.AcceptButton = okButton;
    }
}

