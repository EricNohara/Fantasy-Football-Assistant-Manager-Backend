namespace FFOracle.Services;

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var smtpSettings = _config.GetSection("Smtp");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            smtpSettings["SenderName"],
            smtpSettings["SenderEmail"]
        ));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var body = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        message.Body = body.ToMessageBody();

        using var client = new SmtpClient();

        var useStartTls = bool.Parse(smtpSettings["UseStartTls"]);
        var useSsl = bool.Parse(smtpSettings["UseSSL"]);

        // connect
        await client.ConnectAsync(
            smtpSettings["Host"],
            int.Parse(smtpSettings["Port"]),
            useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls
        );

        // authenticate
        await client.AuthenticateAsync(
            smtpSettings["Username"],
            smtpSettings["Password"]
        );

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}

