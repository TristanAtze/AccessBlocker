using System.Drawing;
using System.Windows.Forms;

public class LockMouse {
    public static async Task HandleMouse(CancellationToken cancellationToken) {
        Console.WriteLine("Locking mouse");
        while (true) {
            if (cancellationToken.IsCancellationRequested) {
                break;
            }   
            await Task.Delay(50, cancellationToken);
            Cursor.Hide();
            Cursor.Position = new Point(0, 0);
        }
        Cursor.Show();
    }
}