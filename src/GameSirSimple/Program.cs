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
        using var single = new Mutex(true,"Local\\GameSirSimple.X5",out bool first);
        if(!first) { MessageBox.Show(I18n.T("GameSir Simple ya está abierto.")); return; }
        try { Application.Run(new MainWindow()); }
        catch(Exception ex) { MessageBox.Show(I18n.T("No se pudo iniciar: {0}",ex.Message),"GameSir Simple"); }
    }    
}
