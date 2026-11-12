using System.Runtime.InteropServices;
namespace GameSirSimple;

// Windows marks files downloaded from the internet with a Zone.Identifier stream, and the
// unsigned EXE then shows an "unknown publisher" prompt on every launch. Once the user has
// chosen to run the app, remove that mark from its own EXE so the prompt appears only once.
static class DownloadMark
{
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
    static extern bool DeleteFileW(string path);

    public static bool Has(string file)=>File.Exists(file+":Zone.Identifier");

    public static void Clear(string? file)
    {
        try { if(file!=null && Has(file)) DeleteFileW(file+":Zone.Identifier"); }
        catch { } // Read-only folders or unsupported file systems: keep the default Windows behavior.
    }
}
