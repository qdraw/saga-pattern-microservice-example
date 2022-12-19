namespace ACME.Identity.Extentions
{
    public class AuthorizationConfiguration
    {
        public string Authority { get; set; }
        public bool RequireMfa { get; set; }
    }
    
    public static class ConfigurationExtension
    {
        public static HashSet<Uri> GatewayClientPostLogoutRedirectUris(this IConfiguration configuration)
        {
            return configuration.GetSection("GatewayClientPostLogoutRedirectUris").Get<string[]>().Select(url => new Uri(url)).ToHashSet();
        }
        
        public static HashSet<Uri> GatewayClientPostLoginRedirectUris(this IConfiguration configuration)
        {
            return configuration.GetSection("GatewayClientPostLoginRedirectUris").Get<string[]>().Select(url => new Uri(url)).ToHashSet();
        }
        
        public static AuthorizationConfiguration GetAuthorizationConfiguration(this IConfiguration configuration)
        {
            return configuration.GetSection("Authorization").Get<AuthorizationConfiguration>();
        }
    }
}