namespace GameSirSimple;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        if(args.Contains("--render-preview")) { VisualCheck.Run(args); return; }
        if(args.Contains("--diagnostic")) { Diagnostics.Run(); return; }
        DownloadMark.Clear(Environment.ProcessPath);
        I18n.Language=Settings.Load().Language;
        // Created before the mutex so a second launch always finds it, even while the first one is starting.
        using var showRequests = new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\GameSirSimple.X5.Show");
        using var single = new Mutex(true,"Local\\GameSirSimple.X5",out bool first);
        if(!first) {
            // Already running (maybe hidden in the tray): let it take the foreground and show its window.
            AllowSetForegroundWindow(-1);
            showRequests.Set();
            return;
        }
        try { Application.Run(new MainWindow(showRequests:showRequests)); }
        catch(Exception ex) { MessageBox.Show(I18n.T("No se pudo iniciar: {0}",ex.Message),"GameSir Simple"); }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern bool AllowSetForegroundWindow(int processId);
}
