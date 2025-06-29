using System.Reflection;

namespace DOSGameCollection.UI;

public class AboutDialog : Form
{
    public AboutDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "About DOSGameCollection";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(320, 280);
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var logoPictureBox = new PictureBox
        {
            Anchor = AnchorStyles.None,
            Margin = new Padding(10)
        };

        Assembly assembly = Assembly.GetExecutingAssembly();
        using (Stream? imageStream = assembly.GetManifestResourceStream("DOSGameCollection.Resources.DGC-logo-200.png"))
        {
            if (imageStream != null)
            {
                logoPictureBox.Image = Image.FromStream(imageStream);
                logoPictureBox.Size = logoPictureBox.Image.Size;
            }
        }

        string version = GetAppVersion();

        var aboutLabel = new Label
        {
            Text = $"DOSGameCollection\n\nVersion: {version}\n\nAn application to manage and launch your DOS games. By Homero Trevino",
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };

        var okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Size = new Size(75, 25),
            Anchor = AnchorStyles.None,
            Margin = new Padding(10)
        };
        okButton.Click += (sender, e) => this.Close();

        mainPanel.Controls.Add(logoPictureBox, 0, 0);
        mainPanel.Controls.Add(aboutLabel, 0, 1);
        mainPanel.Controls.Add(okButton, 0, 2);

        Controls.Add(mainPanel);
    }

    private string GetAppVersion()
    {
        // Get the informational version from the assembly. This is set by the build process via a command-line property.
        var version = Assembly.GetExecutingAssembly()
                              .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                              .InformationalVersion;

        // Provide a fallback if the version isn't set (e.g., during local development where the property isn't passed).
        return string.IsNullOrEmpty(version) ? "dev" : version;
    }
}
