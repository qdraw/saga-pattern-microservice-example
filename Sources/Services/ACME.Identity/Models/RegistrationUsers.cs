using ACME.Library.Domain.Interfaces.Saga;

namespace ACME.Identity.Models
{
    public class RegistrationUsers : RegisterUserModel, ISagaData 
    {
        public string State { get; set; }
    }
}
