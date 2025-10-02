namespace FitPass.Domain.Entities;

public class UserPaymentProfile : BaseEntity
{
    public required string ApplicationUserId { get; set; }
    public required string PhoneNumber { get; set; }
    public string? BusinessName { get; set; }
    //CustomerCashBalanceOptions CashBalance
    public string? Description { get; set; }
    public string? IndividualName { get; set; }
    public string? InvoicePrefix { get; set; }
    //CustomerInvoiceSettingsOptions InvoiceSettings
    public string? Plan {  get; set; }
    public List<string>? PreferredLocales { get; set; }
    //CustomerTaxOptions Tax
    public string? TaxExempt { get; set; }
    public string? TestClock { get; set; }
    public bool? Validate { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public Address Address { get; set; } = null!;
}
