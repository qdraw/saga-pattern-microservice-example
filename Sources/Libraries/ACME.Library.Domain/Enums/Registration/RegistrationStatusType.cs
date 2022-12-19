namespace ACME.Library.Domain.Enums.Registration
{
    public enum RegistrationStateType
    {
        None = 0,
        Started = 1,
        AwaitSigningOther = 2,
        Submitted = 3,
        Approved = 4,
        Rejected = 5
    }
}