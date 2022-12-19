#nullable enable
using ACME.Library.Domain.Enums;
using ACME.Library.Domain.Interfaces.Saga;
using Microsoft.AspNetCore.Identity;

namespace ACME.Identity.Models;

public class ApplicationUser : IdentityUser, ISagaData
{
    public string Code { get; set; }
    public string FirstName { get; set; }
    public string? PrefixLastName { get; set; }
    public string LastName { get; set; }
    public LocaleType Locale { get; set; }
    public string? SecondaryPhoneNumber { get; set; }
    public string? Occupation { get; set; }
    public Guid CorrelationId { get; set; }
    public string State { get; set; }

    public ICollection<CompanyUserRoleEntity>? CompanyRoles { get;set; }
}
