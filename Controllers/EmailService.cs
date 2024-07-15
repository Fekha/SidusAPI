using System.Net.Mail;
using System.Net;

public static class EmailService
{
    public static async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpHost = "smtp.gmail.com";
        var smtpPort = 587;
        var smtpUser = "sydus.notifications@gmail.com";
        var smtpPass = "fxqxkdlbxxvunkim";

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
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
