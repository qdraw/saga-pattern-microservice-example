using System;
using System.Collections.Generic;
using ACME.Library.Domain.Enums;

namespace ACME.Library.Domain.Core;

public class User
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string FirstName { get; set; }
    public string PrefixLastName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string UserName { get; set; }
    public ICollection<string> Roles { get; set; }
    public ICollection<string> CommunicationPreferences { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string SecundaryPhone { get; set; }
    public string Occupation { get; set; }
    public IEnumerable<Guid> Subscriptions { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public bool IsPhoneNumberConfirmed { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public string Gender { get; set; }
    public LocaleType Locale { get; set; }
    public Guid CorrelationId { get; set; }
    public string State { get; set; }
        
    public string GetFullName()
    {
        {
            return string.IsNullOrEmpty(PrefixLastName) ?
                $"{FirstName} {LastName}" :
                $"{FirstName} {PrefixLastName} {LastName}";
        }
    }
    
}