using FitPass.Application.Common.Interfaces;
using QRCoder;

namespace FitPass.Infrastructure.QRCode;

public class QrCodeService : IQrCodeService
{
    public byte[] GetQrCode(string gymId)
    {
        var qrCodeData = GetQrData(gymId);

        using var qrCode = new PngByteQRCode(qrCodeData);

        return qrCode.GetGraphic(20);
    }

    private QRCodeData GetQrData(string textToEncode)
    {
        using var qrGenerator = new QRCodeGenerator();
        return qrGenerator.CreateQrCode(textToEncode, QRCodeGenerator.ECCLevel.Q);
    }
}