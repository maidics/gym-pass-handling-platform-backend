namespace FitPass.Domain.Enums;

public enum RequestStatus
{
    Submitted,
    Approved,
    Cancelled,
    Rejected,
    Error //when request should be completed but fails to be completed
}
