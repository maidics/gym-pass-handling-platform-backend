namespace FitPass.Domain.Enums;

public enum RequestStatus
{
    Submitted,
    Completed,
    Cancelled,
    Rejected,
    PayloadFailedToSerialize,
    CreatorNotFound,
    RelatedRoleHandlingFailed,
    PayloadWasNull
}
