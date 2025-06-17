namespace DOSGameCollection;

static class Program {
    [STAThread]
    static void Main() {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        ApplicationConfiguration.Initialize();
        Application.Run(new TopForm());
    }
}
