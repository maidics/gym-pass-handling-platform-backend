using FitPass.Domain.ValueObjects;

/*
namespace FitPass.Domain.Entities;

public class PaymentResult : BaseAuditableEntity
{
    public required bool Success { get; init; }
    public required string CustomerId { get; init; }
    public required Money Amount { get; init; }
    //public required string? PaymentMethodId { get; set; }
    public required string? TransactionId { get; init; } //Stripe charge Id
    public PaymentStatus Status { get; init; } = PaymentStatus.Pending;
    public DateTimeOffset? ProcessedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public string? DeclineCode { get; init; }

    public static PaymentResult Successful(string customerId, Money amount, string transactionId)
    {
        return new PaymentResult
        {
            Success = true,
            CustomerId = customerId,
            TransactionId = transactionId,
            Amount = amount,
            Status = PaymentStatus.Succeeded,
            ProcessedAt = DateTimeOffset.UtcNow
        };
    }

    public static PaymentResult Failure(string customerId, Money amount, string transactionId, string errorMessage, string? declineCode = null)
    {
        return new PaymentResult
        {
            Success = false,
            CustomerId = customerId,
            Amount = amount,
            TransactionId = transactionId,
            Status = PaymentStatus.Failed,
            ProcessedAt = DateTimeOffset.UtcNow,
            ErrorMessage = errorMessage,
            DeclineCode = declineCode
        };
    }
}
*/