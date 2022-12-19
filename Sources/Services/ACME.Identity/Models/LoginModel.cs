namespace ACME.Identity.Models
{
    public class LoginModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool? RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
        
        public string? redirect_uri { get; set; }

        public string? response_type { get; set; }
        
        public string? state { get; set; }

        public string? scope { get; set; }

        public string? code_challenge { get; set; }

        public string? code_challenge_method { get; set; }

        public string? response_mode { get; set; }

        public string? nonce { get; set; }
    }
}
