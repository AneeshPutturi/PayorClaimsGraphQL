namespace PayorClaims.Api.Options;

public class AuthOptions
{
    public const string SectionName = "Auth";
    public string Issuer { get; set; } = "payorclaims";
    public string Audience { get; set; } = "payorclaims";
    public string SigningKey { get; set; } = "";
    public bool RequireHttps { get; set; } = false;
}
