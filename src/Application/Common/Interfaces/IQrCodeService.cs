namespace FitPass.Application.Common.Interfaces;

public interface IQrCodeService
{
    byte[] GetQrCode(string textToEncode);
}