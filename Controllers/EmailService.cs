using System.Net.Mail;
using System.Net;

public static class EmailService
{
    public static async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpHost = "smtp.gmail.com";
        var smtpPort = 465;
        var smtpUser = "notifications@gravitasgames.com";
        var smtpPass = "Version101";
        var enableSsl = true;

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = enableSsl
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpUser),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }
}
