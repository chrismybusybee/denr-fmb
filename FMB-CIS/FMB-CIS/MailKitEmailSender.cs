using FMB_CIS;
using FMB_CIS.Data;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Threading.Tasks;

public class MailKitEmailSender : IEmailSender
{

    public MailKitEmailSender(IOptions<MailKitEmailSenderOptions> options, IOptions<SMTPCredentials> credentials, IOptions<SMTPCredentialsSecondary> credentials2)
    {
        this.Options = options.Value;

        _credentials = credentials.Value;
        _credentials2 = credentials2.Value;
    }


    private readonly SMTPCredentials _credentials;
    private readonly SMTPCredentialsSecondary _credentials2;

    public MailKitEmailSenderOptions Options { get; set; }

    public Task SendEmailAsync(string email, string subject, string message)
    {
        return Execute(email, subject, message);
    }

    public Task Execute(string to, string subject, string message)
    {
        //// create message
        //var email = new MimeMessage();
        //email.Sender = MailboxAddress.Parse(Options.Sender_EMail);
        //if (!string.IsNullOrEmpty(Options.Sender_Name))
        //    email.Sender.Name = Options.Sender_Name;
        //email.From.Add(email.Sender);
        //email.To.Add(MailboxAddress.Parse(to));
        //email.Subject = subject;
        //email.Body = new TextPart(TextFormat.Html) { Text = message };

        //// send email
        //using (var smtp = new SmtpClient())
        //{
        //    smtp.Connect(Options.Host_Address, Options.Host_Port, Options.Host_SecureSocketOptions);
        //    smtp.Authenticate(Options.Host_Username, Options.Host_Password);
        //    smtp.Send(email);
        //    smtp.Disconnect(true);
        //}


        try
        {
            // create message
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_credentials.Sender_EMail);
            if (!string.IsNullOrEmpty(_credentials.Sender_Name))
                email.Sender.Name = _credentials.Sender_Name;
            email.From.Add(email.Sender);
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = message };

            // send email
            using (var smtp = new SmtpClient())
            {
                smtp.Connect(_credentials.Host_Address, _credentials.Host_Port, Options.Host_SecureSocketOptions);
                smtp.Authenticate(_credentials.Host_Username, _credentials.Host_Password);
                smtp.Send(email);
                smtp.Disconnect(true);
            }
        }
        catch (Exception ex)
        {
            // Code to handle the exception
            Console.WriteLine($"An error occurred while sending the email 1: {ex.Message}");
            // You can log the exception, show a user-friendly message, or take appropriate actions

            try
            {
                // create message
                var email = new MimeMessage();
                email.Sender = MailboxAddress.Parse(_credentials2.Sender_EMail);
                if (!string.IsNullOrEmpty(_credentials2.Sender_Name))
                    email.Sender.Name = _credentials2.Sender_Name;
                email.From.Add(email.Sender);
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = message };

                // send email
                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(_credentials2.Host_Address, _credentials2.Host_Port, Options.Host_SecureSocketOptions);
                    smtp.Authenticate(_credentials2.Host_Username, _credentials2.Host_Password);
                    smtp.Send(email);
                    smtp.Disconnect(true);
                }
            }
            catch
            {
                Console.WriteLine($"An error occurred while sending the email 2: {ex.Message}");
            }
        }
        //finally
        //{
            
        //}

        return Task.FromResult(true);
    }
}