using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;

namespace HospitalManagementSystem.Services
{
    public class EmailNotificationService
    {
        private readonly string _email;
        private readonly string _password;

        public EmailNotificationService(IConfiguration configuration)
        {
            _email = configuration["EmailSettings:Email"];
            _password = configuration["EmailSettings:Password"];
        }

        public async Task SendAppointmentReminder(string email, DateTime appointmentTime)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Hospital Management System", _email));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = "Appointment Reminder";
                message.Body = new TextPart("plain")
                {
                    Text = $"You have an appointment scheduled for {appointmentTime:dd MMM yyyy hh:mm tt}."
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, false);
                    await client.AuthenticateAsync(_email, _password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // Log l'erreur ou relancez-la pour que Hangfire puisse la gérer
                throw new Exception("Failed to send email", ex);
            }
        }
    }
}