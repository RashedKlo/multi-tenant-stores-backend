namespace Infrastructure.Settings;

public class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = default!;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FromEmail { get; set; } = default!;
    public string FromName { get; set; } = default!;
    public bool UseSsl { get; set; } = true;
}
