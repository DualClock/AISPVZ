using QRCoder;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace AISPVZ.Services;

public class QrCodeService
{
    public BitmapImage GenerateQrCodeBitmapImage(string text, int pixelsPerModule = 20)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new BitmapByteQRCode(qrData);
        var bitmapBytes = qrCode.GetGraphic(pixelsPerModule);

        using var ms = new MemoryStream(bitmapBytes);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = ms;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    public byte[] GenerateQrCodeBytes(string text, int pixelsPerModule = 20)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new BitmapByteQRCode(qrData);
        return qrCode.GetGraphic(pixelsPerModule);
    }
}
