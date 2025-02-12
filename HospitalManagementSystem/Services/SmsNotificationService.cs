using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.Extensions.Configuration;

namespace HospitalManagementSystem.Services
{
    public class SmsNotificationService
    {
        private readonly string _accountSid;
        private readonly string _authToken;

        public SmsNotificationService(IConfiguration configuration)
        {
            _accountSid = configuration["TwilioSettings:AccountSid"];
            _authToken = configuration["TwilioSettings:AuthToken"];
            TwilioClient.Init(_accountSid, _authToken);
        }

        public async Task SendSmsReminder(string phoneNumber, DateTime appointmentTime)
        {
            try
            {
                await MessageResource.CreateAsync(
                    body: $"Reminder: You have an appointment at {appointmentTime:hh:mm tt} on {appointmentTime:dd MMM yyyy}.",
                    from: new Twilio.Types.PhoneNumber("+18438834437"), // Remplacez par votre numéro Twilio
                    to: new Twilio.Types.PhoneNumber(phoneNumber)
                );
            }
            catch (Exception ex)
            {
                // Log l'erreur ou relancez-la pour que Hangfire puisse la gérer
                throw new Exception("Failed to send SMS", ex);
            }
        }
    }
}