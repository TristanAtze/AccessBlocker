using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using QRCoder;


class QRCodeGenerate
{
    public static void GenerateQRCode(string data)
    {
        // Generate the QR code
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        QRCode qrCode = new QRCode(qrCodeData);
        Bitmap qrCodeImage = qrCode.GetGraphic(20);


        // Specify the folder path to save the QR code image (The path of the current directory)
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "QRCODE");

        // Create the folder if it doesn't exist
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        //Delete everything in the Folder
        else
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(folderPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            Directory.Delete(folderPath);
            Directory.CreateDirectory(folderPath);
        }

            // Save the QR code as a PNG image file inside the specified folder
            string fileName = Path.Combine(folderPath, "QRCode.png");
        qrCodeImage.Save(fileName, ImageFormat.Png);
    }

    public static void DisplayQRCodeImage(string imagePath)
    {
        try
        {
            if (System.IO.File.Exists(imagePath))
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = imagePath,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            else
            {
                Console.WriteLine("QR code image not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
