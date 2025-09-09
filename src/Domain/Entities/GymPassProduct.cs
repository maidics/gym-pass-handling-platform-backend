namespace FitPass.Domain.Entities;

public class GymPassProduct : BaseEntity
{
    public required string GymId { get; set; }
    public required PassType PassType { get; set; }
    public required decimal EurPrice { get; set; }
}