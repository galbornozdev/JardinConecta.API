using JardinConecta.Configurations;
using JardinConecta.Services;
using JardinConecta.Services.Infrastructure;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace JardinConecta.Infrastructure
{
    public class TwilioSmsService : ISmsService
    {
        private TwilioOptions _twilioOptions;

        public TwilioSmsService(IOptions<TwilioOptions> twilioOptions)
        {
            _twilioOptions = twilioOptions.Value;
            TwilioClient.Init(_twilioOptions.AccountSid, _twilioOptions.AuthToken);
        }
        public async Task<Result> SendAsync(string to, string message)
        {
            try
            {
                var result = await MessageResource.CreateAsync(
                    body: message,
                    from: new PhoneNumber(_twilioOptions.FromPhoneNumber),
                    to: new PhoneNumber(to)
                );

                if (result.ErrorCode != null)
                {
                    return Result.Failure($"Twilio error {result.ErrorCode}: {result.ErrorMessage}");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unexpected SMS error: {ex.Message}");
            }
            
        }
    }
}
