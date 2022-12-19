using System.ComponentModel.DataAnnotations;
using ACME.Library.Domain.Enums;

namespace ACME.Identity.Models
{
    public class RegisterUserModel
    {
        [Key]
        public long Id { get; set; }

        public Guid? IdentityId { get; set; }
        public string? Code { get; set; }
        public string FirstName { get; set; }
        public string? PrefixLastName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public LocaleType Locale { get; set; }
        public string? SecondaryPhoneNumber { get; set; }
        public string? Occupation { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string? MobilePhoneNumber { get; set; }
        public string? CompanyCode { get; set; }
        public string? CompanyRole { get; set; }
        public Guid CorrelationId { get; set; }
        public string State { get; set; }
    }
}