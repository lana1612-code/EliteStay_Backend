using System.Net.Mail;
using System.Net;

namespace Hotel_Backend_API.Services
{
    public class SendEmailService
    {
        /*
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var FromEmail = "lana.hasan540@gmail.com";
            var pw = "12029408lanasamer";

            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(FromEmail, pw);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(FromEmail),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = false 
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }

        }
 */


        private readonly IConfiguration configuration;

        public SendEmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task SendEmail(string receptor, string subject, string body)
        {
            var email = configuration.GetValue<string>("EMAIL_CONFIGURATION:EMAIL");
            var password = configuration.GetValue<string>("EMAIL_CONFIGURATION:PASSWORD");
            var host = configuration.GetValue<string>("EMAIL_CONFIGURATION:HOST");
            var port = configuration.GetValue<int>("EMAIL_CONFIGURATION:PORT");

            var smtpClient = new SmtpClient(host, port);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;

            smtpClient.Credentials = new NetworkCredential(email, password);

            var message = new MailMessage(email!, receptor, subject, body);
            await smtpClient.SendMailAsync(message);
        }
    }

}
