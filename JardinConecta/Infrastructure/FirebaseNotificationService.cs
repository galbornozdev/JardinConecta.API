using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using JardinConecta.Configurations;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace JardinConecta.Services
{
    public class FirebaseNotificationService : INotificationService
    {
        private readonly FirebaseMessaging? _firebaseMessaging;

        public FirebaseNotificationService(IOptions<FirebaseOptions> options)
        {
            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    var firebaseOptions = options.Value;

                    if (!string.IsNullOrEmpty(firebaseOptions.PrivateKey))
                    {
                        var credentialJson = JsonSerializer.Serialize(new
                        {
                            type = firebaseOptions.Type,
                            project_id = firebaseOptions.ProjectId,
                            private_key_id = firebaseOptions.PrivateKeyId,
                            private_key = firebaseOptions.PrivateKey.Replace("\\n", "\n"),
                            client_email = firebaseOptions.ClientEmail,
                            client_id = firebaseOptions.ClientId,
                            auth_uri = firebaseOptions.AuthUri,
                            token_uri = firebaseOptions.TokenUri
                        });

                        var credential = GoogleCredential.FromJson(credentialJson);
                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = credential
                        });
                    }
                    else
                    {
                        // Intentar usar credenciales por defecto (variable GOOGLE_APPLICATION_CREDENTIALS)
                        var credential = GoogleCredential.GetApplicationDefault();
                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = credential
                        });
                    }
                }
                _firebaseMessaging = FirebaseMessaging.DefaultInstance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Firebase initialization error: {ex.Message}");
                _firebaseMessaging = null;
            }
        }

        public async Task<Result> SendPushAsync(string deviceToken, string title, string body)
        {
            if (_firebaseMessaging == null)
            {
                return Result.Failure("Firebase no está configurado");
            }

            try
            {
                var message = new Message
                {
                    Token = deviceToken,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    }
                };

                var response = await _firebaseMessaging.SendAsync(message);
                Console.WriteLine($"Push notification sent: {response}");
                return Result.Success();
            }
            catch (FirebaseMessagingException ex)
            {
                Console.WriteLine($"Firebase Messaging error: {ex.Message}");
                return Result.Failure($"Error enviando notificación: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending push: {ex.Message}");
                return Result.Failure($"Error enviando notificación: {ex.Message}");
            }
        }
    }
}
