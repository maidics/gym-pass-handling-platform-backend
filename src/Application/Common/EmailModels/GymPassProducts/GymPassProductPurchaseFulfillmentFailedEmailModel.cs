namespace FitPass.Application.Common.EmailModels.GymPassProducts;

public class GymPassProductPurchaseFulfillmentFailedEmailModel : IEmailModel
{
    public required string? Language { get; set; }
    public required string UserFirstName { get; set; }
    public required string ReceiptId { get; set; }
    public required string GymName { get; set; }
}
