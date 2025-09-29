namespace FitPass.Domain.Enums;

public enum PassType
{
    SingleUse, //Ticket type, not tied to expiration date 
    MultiUse, //Limited uses, not tied to expiration date
    Unlimited //Unlimited use pass until it expires
}
