using System.Net;
using System.Net.Mail;

namespace ImmoUpdateCheck
{
    internal static class Mailer
    {
        public static async Task SendMailAsync(string fromAddress, IEnumerable<string> toAddress, string smtpServer, string smtpPassword, int smtpPort, string subject, string body,ILogger logger, CancellationToken ct)
        {
            try
            {
                var from = new MailAddress(fromAddress);

                using var client = new SmtpClient()
                {
                    Host = smtpServer,
                    Port = smtpPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    TargetName = "STARTTLS/" + smtpServer,
                    Credentials = new NetworkCredential(from.Address, smtpPassword)
                };

                using var message = new MailMessage()
                {
                    From = from,
                    Subject = subject,
                    Body = body
                };

                foreach (var to in toAddress)
                {
                    message.To.Add(to);
                }

                await client.SendMailAsync(message, ct);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error sending mail");
            }
        }
    }
}
