namespace Infrastructure.Settings;

public class GoogleAuthSettings
{
    public const string SectionName = "GoogleAuth";

    /// <summary>
    /// OAuth client id used as the expected audience when validating Google ID tokens.
    /// </summary>
    public string ClientId { get; set; } = default!;
}
