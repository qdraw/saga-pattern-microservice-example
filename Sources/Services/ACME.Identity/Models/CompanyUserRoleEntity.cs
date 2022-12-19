namespace ACME.Identity.Models
{
    public class CompanyUserRoleEntity
    {
        public string UserId { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyRole { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}