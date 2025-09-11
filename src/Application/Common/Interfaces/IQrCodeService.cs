namespace FitPass.Application.Common.Interfaces;

public interface IQrCodeService
{
    byte[] GenerateQrCode(string textToEncode);
}