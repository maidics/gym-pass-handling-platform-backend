namespace FitPass.Domain.Entities;

public class PaymentProfile
{
    public required string CustomerId { get; set; }
    public required string ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
}