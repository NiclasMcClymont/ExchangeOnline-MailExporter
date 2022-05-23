using Microsoft.Identity.Client;
using Polly;
using Serilog;
using System.Linq;

namespace MailExporter.Lib
{

    /// <summary>
    /// This class is used to create and store a token. It should create a new token if the current token is expired.
    /// </summary>
    public class Token
    {

        // Timer for token expiration
        Timer timer;

        // The tokenresult from a token request
        private AuthenticationResult authentication;

        public AuthenticationResult Authentication
        {
            get { return authentication; }
        }

        // The LoginInfo the token was created with
        AppLoginInfo appLoginInfo;

        public Token(AppLoginInfo loginInfo)
        {
            timer = new Timer(OnTokenExpired);
            appLoginInfo = loginInfo;
            authentication = CreateToken(loginInfo);

            int expiresIn = (int) (authentication.ExpiresOn.DateTime - DateTime.UtcNow).TotalMilliseconds - 10000;

            timer.Change(expiresIn, Timeout.Infinite);
        }


        // This Method is called when the token expires
        private void OnTokenExpired(object? state)
        {
            Log.Debug("Token expired", this);
            authentication = CreateToken(appLoginInfo);

            // Calculate milliseconds until the token expires
            int expiresIn = (int) (authentication.ExpiresOn.DateTime - DateTime.UtcNow).TotalMilliseconds - 10000;
            timer.Change(expiresIn, Timeout.Infinite);
        }

        // Get the access token. Retry if something goes wrong.
        private static AuthenticationResult CreateToken(AppLoginInfo appLoginInfo)
        {
            Log.Debug("Creating token", appLoginInfo.tenantID, appLoginInfo.appID);
            var scopes = new string[] { "https://outlook.office.com/.default" };

            var cca = ConfidentialClientApplicationBuilder
            .Create(appLoginInfo.appID)
            .WithTenantId(appLoginInfo.tenantID)
            .WithClientSecret(appLoginInfo.secretKey)
            .Build();
            
            AuthenticationResult? result = null;

            var capture = Policy.Handle<AggregateException>()
            .WaitAndRetry(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1),
            } ,(e, i) => {
                Log.Error("Token Acquisation failed with Exception {0}. Retrying in {1}", e.Message, i);
            })
            .ExecuteAndCapture( () => {
                result = cca.AcquireTokenForClient(scopes).ExecuteAsync().Result;
            });

            if (capture.Outcome == OutcomeType.Failure)
            {
                Log.Fatal("Token Acquisation failed");
                throw capture.FinalException;
            }
            return result!;
        }
        


    }
}