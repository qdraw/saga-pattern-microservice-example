namespace ACME.Identity.Models;

public sealed class AccountResultModel
{
    public AccountResultModel()
    {
    }
    
    public AccountResultModel(int statusCode, string message, Guid? userId = null)
    {
        StatusCode = statusCode;
        Message = message;
        UserId = userId;
    }

    public Guid? UserId { get; set; }

    public string? Message { get; set; }

    public int StatusCode { get; set; }

    public bool RequiresTwoFactor { get; set; }

    public string? LocalRedirectUrl { get; set; }
}