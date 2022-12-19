namespace ACME.API.Registration.Validators;

public class ValidatorConstants
{
    public const string RegistrationExists = "ACME.REGISTRATION.VALIDATION.001";
    public const string RegistrationCompanyNameEmpty = "ACME.REGISTRATION.VALIDATION.002";
    public const string RegistrationStateIncorrect = "ACME.REGISTRATION.VALIDATION.003";
    public const string RegistrationActivitiesEmpty = "ACME.REGISTRATION.VALIDATION.004";
    public const string RegistrationContactsEmpty = "ACME.REGISTRATION.VALIDATION.005";
    public const string RegistrationEmailForActionInvalid = "ACME.REGISTRATION.VALIDATION.006";
    public const string EmailForValidationInvalid = "ACME.REGISTRATION.VALIDATION.007";
    public const string EmailForValidationRequired = "ACME.REGISTRATION.VALIDATION.008";
    public const string CorrelationIdRequired = "ACME.REGISTRATION.VALIDATION.009";
}