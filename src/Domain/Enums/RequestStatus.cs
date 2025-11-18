namespace FitPass.Domain.Enums;

public enum RequestStatus
{
    Submitted,
    Completed,
    Cancelled,
    Rejected,
    Error //when request should be completed but fails to be completed
}
