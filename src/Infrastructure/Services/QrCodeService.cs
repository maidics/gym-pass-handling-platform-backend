using FitPass.Application.Common.Interfaces;
using QRCoder;

namespace FitPass.Infrastructure.Services;

public class QrCodeService : IQrCodeService
{
    public byte[] GenerateQrCode(string textToEncode)
    {
        var qrCodeData = GenerateQrData(textToEncode);

        using var qrCode = new PngByteQRCode(qrCodeData);

        return qrCode.GetGraphic(20);
    }

    private QRCodeData GenerateQrData(string textToEncode)
    {
        using var qrGenerator = new QRCodeGenerator();
        return qrGenerator.CreateQrCode(textToEncode, QRCodeGenerator.ECCLevel.Q);
    }
}