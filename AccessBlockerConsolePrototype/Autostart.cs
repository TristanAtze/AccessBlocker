public class Autostart {
    public static void AddToStartup() {
        string startupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "AccessBlockerConsolePrototype.exe");
        if (!File.Exists(startupPath)) {
            File.Create(startupPath).Close();
        }
        File.SetAttributes(startupPath, FileAttributes.Hidden);
    }
    public static void RemoveFromStartup() {
        string startupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "AccessBlockerConsolePrototype.exe");
        if (File.Exists(startupPath)) {
            File.Delete(startupPath);
        }
    }
}