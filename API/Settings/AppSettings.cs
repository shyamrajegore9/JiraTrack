namespace JiraTrack.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "JiraTrack";
    public string Audience { get; set; } = "JiraTrackClient";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

public class AppSettings
{
    public const string SectionName = "AppSettings";
    public string FrontendUrl { get; set; } = "http://localhost:4200";
    public bool ExposeResetTokenInDev { get; set; } = true;
    public bool EnableOpenApi { get; set; }
}
