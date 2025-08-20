namespace Common;

public class JwtSettings
{
    public string SecretKey { get; set; } = "your_super_secret_key_32_chars_long";
    public string Issuer { get; set; } = "EcommerceMicroservices";
    public string Audience { get; set; } = "EcommerceUsers";
    public int ExpirationMinutes { get; set; } = 60;
}