using DOSGameCollection.Models;

namespace DOSGameCollection.UI;

public class LoadGameListProgressDialog : Form
{
    private ProgressBar? progressBar;
    private Label? statusLabel;

    public LoadGameListProgressDialog()
    {
        InitializeComponent();
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.ControlBox = false;
        this.Text = "Please Wait...";
        this.ShowInTaskbar = false;
    }

    private void InitializeComponent()
    {
        this.statusLabel = new Label();
        this.progressBar = new ProgressBar();

        // lblMessage setup
        this.statusLabel.AutoSize = true;
        this.statusLabel.Location = new Point(20, 20);
        this.statusLabel.Text = "Loading data...";
        this.statusLabel.Font = new Font(this.statusLabel.Font, FontStyle.Bold);

        // progressBar setup
        this.progressBar.Location = new Point(20, 50);
        this.progressBar.Size = new Size(260, 23);
        this.progressBar.Style = ProgressBarStyle.Marquee;
        this.progressBar.MarqueeAnimationSpeed = 30;

        // Form layout
        this.ClientSize = new Size(300, 100);
        this.Controls.Add(this.statusLabel);
        this.Controls.Add(this.progressBar);
    }

    // Method to handle progress reports from the background task
    // This is the core method the UI thread will call via the IProgress<T> interface
    public void HandleProgressReport(ProgressReport report)
    {
        if (this.InvokeRequired)
        {
            // If called from a non-UI thread, marshal the call to the UI thread
            this.Invoke(new Action<ProgressReport>(HandleProgressReport), report);
        }
        else
        {
            // Update UI elements directly on the UI thread
            if (statusLabel != null && progressBar != null)
            {
                statusLabel.Text = report.Message;

                if (report.TotalSteps > 0 && report.CurrentStep <= report.TotalSteps)
                {
                    if (progressBar.Style != ProgressBarStyle.Blocks)
                    {
                        progressBar.Style = ProgressBarStyle.Blocks;
                        progressBar.MarqueeAnimationSpeed = 0; // Stop marquee
                    }
                    progressBar.Maximum = report.TotalSteps;
                    progressBar.Value = report.CurrentStep;
                }
                else if (progressBar.Style != ProgressBarStyle.Marquee)
                {
                    // If indeterminate or initial state, set to marquee
                    progressBar.Style = ProgressBarStyle.Marquee;
                    progressBar.MarqueeAnimationSpeed = 30;
                }
                
            }
        }
    }
}