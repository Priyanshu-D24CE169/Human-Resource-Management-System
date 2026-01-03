using System.Net;
using System.Net.Mail;

namespace Human_Resource_Management_System.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmployeeWelcomeEmailAsync(string employeeEmail, string employeeName, string employeeCode, string registrationLink, string temporaryPassword);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetLink);
        Task<bool> SendEmployeeRegistrationReminderAsync(string employeeEmail, string employeeName, string registrationLink);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmployeeWelcomeEmailAsync(string employeeEmail, string employeeName, string employeeCode, string registrationLink, string temporaryPassword)
        {
            try
            {
                _logger.LogInformation($"?? Preparing welcome email for {employeeName} ({employeeCode}) at {employeeEmail}");
                
                var subject = $"Welcome to {_configuration["ApplicationSettings:CompanyName"] ?? "Odoo India"} HRMS - Complete Your Registration";
                var body = GenerateWelcomeEmailBody(employeeName, employeeCode, registrationLink, temporaryPassword);

                var result = await SendEmailAsync(employeeEmail, subject, body, isHtml: true);
                
                if (result)
                {
                    _logger.LogInformation($"? Welcome email sent successfully to {employeeName} ({employeeCode})");
                }
                else
                {
                    _logger.LogWarning($"? Failed to send welcome email to {employeeName} ({employeeCode})");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"?? Exception while sending welcome email to {employeeEmail}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetLink)
        {
            try
            {
                var subject = "Password Reset Request - Odoo India HRMS";
                var body = GeneratePasswordResetEmailBody(resetLink);

                return await SendEmailAsync(email, subject, body, isHtml: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset email to {email}");
                return false;
            }
        }

        public async Task<bool> SendEmployeeRegistrationReminderAsync(string employeeEmail, string employeeName, string registrationLink)
        {
            try
            {
                var subject = "Reminder: Complete Your Registration - Odoo India HRMS";
                var body = GenerateReminderEmailBody(employeeName, registrationLink);

                return await SendEmailAsync(employeeEmail, subject, body, isHtml: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send reminder email to {employeeEmail}");
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            try
            {
                _logger.LogInformation($"?? Starting email send process to: {toEmail}");
                
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings["SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");
                var senderEmail = emailSettings["SenderEmail"] ?? "noreply@odooindiahrms.com";
                var senderPassword = emailSettings["SenderPassword"] ?? "";
                var senderName = emailSettings["SenderName"] ?? "Odoo India HRMS";

                _logger.LogInformation($"?? SMTP Configuration:");
                _logger.LogInformation($"   Host: {smtpHost}");
                _logger.LogInformation($"   Port: {smtpPort}");
                _logger.LogInformation($"   SSL: {enableSsl}");
                _logger.LogInformation($"   Sender: {senderEmail}");
                _logger.LogInformation($"   Password Length: {senderPassword.Length} characters");

                if (string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogError("? SMTP password is empty or null!");
                    return false;
                }

                _logger.LogInformation($"?? Creating SMTP client...");
                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = enableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                // Set timeout
                client.Timeout = 30000; // 30 seconds

                _logger.LogInformation($"?? Creating email message...");
                using var message = new MailMessage();
                message.From = new MailAddress(senderEmail, senderName);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                _logger.LogInformation($"?? Sending email via SMTP...");
                await client.SendMailAsync(message);
                
                _logger.LogInformation($"? Email sent successfully to {toEmail}");
                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"?? SMTP Error sending email to {toEmail}: {smtpEx.Message}");
                _logger.LogError($"   SMTP Status Code: {smtpEx.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"?? General error sending email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        private string GenerateWelcomeEmailBody(string employeeName, string employeeCode, string registrationLink, string temporaryPassword)
        {
            var companyName = _configuration["ApplicationSettings:CompanyName"] ?? "Odoo India";
            
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <title>Welcome to {companyName} HRMS</title>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f7fa; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.1); overflow: hidden; }}
                        .header {{ background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%); color: white; padding: 40px 30px; text-align: center; }}
                        .header h1 {{ font-size: 2.2rem; margin-bottom: 10px; font-weight: 700; }}
                        .header p {{ opacity: 0.9; margin-bottom: 0; font-size: 1.1rem; }}
                        .body-content {{ padding: 40px 30px; }}
                        .welcome-text {{ font-size: 1.1rem; color: #374151; margin-bottom: 25px; line-height: 1.6; }}
                        .info-box {{ background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%); padding: 25px; border-radius: 10px; margin: 25px 0; border-left: 4px solid #6366f1; }}
                        .info-box h3 {{ color: #1f2937; margin-top: 0; margin-bottom: 20px; font-size: 1.2rem; }}
                        .credential-grid {{ display: grid; grid-template-columns: 1fr 2fr; gap: 15px; }}
                        .credential-label {{ font-weight: 600; color: #6b7280; }}
                        .credential-value {{ color: #1f2937; font-weight: 600; font-family: 'Courier New', monospace; background: #e5e7eb; padding: 8px 12px; border-radius: 6px; }}
                        .button {{ display: inline-block; background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%); color: white; padding: 16px 32px; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 1.1rem; margin: 25px 0; text-align: center; }}
                        .button:hover {{ background: linear-gradient(135deg, #5856eb 0%, #7c3aed 100%); }}
                        .steps-container {{ background: #fef9e7; border: 2px solid #f59e0b; border-radius: 10px; padding: 25px; margin: 25px 0; }}
                        .steps-container h4 {{ color: #92400e; margin-top: 0; }}
                        .step-list {{ color: #374151; }}
                        .step-list li {{ margin-bottom: 8px; }}
                        .warning-box {{ background: #fef3c7; border: 2px solid #f59e0b; color: #92400e; padding: 20px; border-radius: 8px; margin: 25px 0; }}
                        .footer {{ margin-top: 40px; padding-top: 25px; border-top: 2px solid #e5e7eb; font-size: 0.9rem; color: #6b7280; }}
                        .footer strong {{ color: #374151; }}
                        .company-logo {{ font-size: 2.5rem; margin-bottom: 10px; }}
                        @media (max-width: 600px) {{
                            .container {{ margin: 10px; border-radius: 8px; }}
                            .header {{ padding: 30px 20px; }}
                            .body-content {{ padding: 30px 20px; }}
                            .credential-grid {{ grid-template-columns: 1fr; }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='company-logo'>??</div>
                            <h1>Welcome to {companyName}!</h1>
                            <p>Your HRMS account has been created</p>
                        </div>
                        
                        <div class='body-content'>
                            <div class='welcome-text'>
                                Dear <strong>{employeeName}</strong>,
                                <br><br>
                                We are thrilled to welcome you to the {companyName} team! Your employee account has been successfully created in our Human Resource Management System (HRMS).
                            </div>
                            
                            <div class='info-box'>
                                <h3>?? Your Employee Information</h3>
                                <div class='credential-grid'>
                                    <div class='credential-label'>Employee Code:</div>
                                    <div class='credential-value'>{employeeCode}</div>
                                    <div class='credential-label'>Login Email:</div>
                                    <div class='credential-value'>{employeeName.Split(' ')[0].ToLower()}@company.com</div>
                                </div>
                            </div>
                            
                            <div class='warning-box'>
                                <strong>?? Important:</strong> You must complete your registration within <strong>7 days</strong> to activate your account and access the HRMS system.
                            </div>
                            
                            <div style='text-align: center;'>
                                <a href='{registrationLink}' class='button'>
                                    ?? Complete Your Registration
                                </a>
                            </div>
                            
                            <div class='steps-container'>
                                <h4>?? Registration Process</h4>
                                <ol class='step-list'>
                                    <li>Click the 'Complete Your Registration' button above</li>
                                    <li>Fill in your personal details and contact information</li>
                                    <li>Add emergency contact details for safety</li>
                                    <li>Create a secure password for your account</li>
                                    <li>Upload your profile picture (optional)</li>
                                    <li>Submit the form to activate your account</li>
                                </ol>
                                <p><strong>Once completed, you'll be able to:</strong></p>
                                <ul class='step-list'>
                                    <li>?? View your dashboard and profile</li>
                                    <li>? Track your attendance and working hours</li>
                                    <li>?? Access payroll and salary information</li>
                                    <li>?? Apply for leaves and view leave balance</li>
                                    <li>?? Participate in company activities and training</li>
                                </ul>
                            </div>
                            
                            <div class='footer'>
                                <p><strong>Need Help?</strong></p>
                                <p>If you encounter any issues during registration or have questions about the HRMS system, please don't hesitate to contact our HR team.</p>
                                <br>
                                <p><strong>{companyName} HR Team</strong><br>
                                ?? Email: hr@{companyName.ToLower().Replace(" ", "")}.com<br>
                                ?? Phone: +91-1234567890<br>
                                ?? Website: www.{companyName.ToLower().Replace(" ", "")}.com</p>
                                
                                <p style='margin-top: 25px; font-size: 0.8rem; color: #9ca3af;'>
                                    This is an automated message from {companyName} HRMS. Please do not reply to this email.
                                    <br>© {DateTime.Now.Year} {companyName}. All rights reserved.
                                </p>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GeneratePasswordResetEmailBody(string resetLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <title>Password Reset - Odoo India HRMS</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ color: #6366f1; font-size: 28px; font-weight: bold; margin-bottom: 10px; }}
                        .button {{ display: inline-block; background-color: #dc2626; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; margin: 20px 0; }}
                        .button:hover {{ background-color: #b91c1c; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb; font-size: 14px; color: #6b7280; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>?? Odoo India HRMS</div>
                            <h2 style='color: #374151;'>Password Reset Request</h2>
                        </div>
                        
                        <p>You have requested to reset your password. Click the button below to reset your password:</p>
                        
                        <div style='text-align: center;'>
                            <a href='{resetLink}' class='button'>Reset Password</a>
                        </div>
                        
                        <p>This link will expire in 24 hours for security reasons.</p>
                        <p>If you did not request this password reset, please ignore this email.</p>
                        
                        <div class='footer'>
                            <p><strong>Odoo India HRMS Team</strong></p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateReminderEmailBody(string employeeName, string registrationLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <title>Registration Reminder - Odoo India HRMS</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ color: #6366f1; font-size: 28px; font-weight: bold; margin-bottom: 10px; }}
                        .button {{ display: inline-block; background-color: #f59e0b; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; margin: 20px 0; }}
                        .button:hover {{ background-color: #d97706; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb; font-size: 14px; color: #6b7280; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>? Odoo India HRMS</div>
                            <h2 style='color: #374151;'>Registration Reminder</h2>
                        </div>
                        
                        <p>Dear <strong>{employeeName}</strong>,</p>
                        <p>This is a friendly reminder that you haven't completed your employee registration yet.</p>
                        <p>Please complete your registration as soon as possible to access your HRMS account.</p>
                        
                        <div style='text-align: center;'>
                            <a href='{registrationLink}' class='button'>Complete Registration Now</a>
                        </div>
                        
                        <div class='footer'>
                            <p><strong>Odoo India HRMS Team</strong></p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}