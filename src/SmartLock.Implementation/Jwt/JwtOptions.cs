namespace SmartLock.Implementation.Jwt;

public class JwtOptions
{
    public string Audience { get; set; }

    public string Issuer { get; set; }

    public string SecretKey { get; set; }

    public int ExpireDays { get; set; }

    public bool ValidateAudience { get; set; }

    public bool ValidateIssuer { get; set; }

    public bool ValidateIssuerSigningKey { get; set; }

    public bool ValidateLifetime { get; set; }
}