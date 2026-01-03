using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace hrms.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var senderEmail = emailSettings["SenderEmail"];
                var senderName = emailSettings["SenderName"];
                var username = emailSettings["Username"];
                var password = emailSettings["Password"];

                // Validate settings
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || 
                    string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger.LogError("Email settings are not configured properly");
                    throw new InvalidOperationException("Email settings are not configured properly");
                }

                _logger.LogInformation($"Attempting to send email to {toEmail} via {smtpServer}:{smtpPort}");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                // Enable detailed logging for troubleshooting
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                
                _logger.LogInformation($"Connecting to SMTP server: {smtpServer}:{smtpPort}");
                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                
                _logger.LogInformation($"Authenticating with username: {username}");
                await client.AuthenticateAsync(username, password);
                
                _logger.LogInformation($"Sending email to: {toEmail}");
                await client.SendAsync(message);
                
                _logger.LogInformation($"Disconnecting from SMTP server");
                await client.DisconnectAsync(true);

                _logger.LogInformation($"? Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error sending email to {toEmail}: {ex.Message}");
                _logger.LogError($"Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                }
                
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }
    }
}
