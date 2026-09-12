namespace GymKitten.Infrastructure.Mail;

public sealed class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = "noreply@gymkitten.com";
    public string DisplayName { get; set; } = "GymKitten Store";
    public bool EnableSsl { get; set; } = true;
}
