using Microsoft.Exchange.WebServices.Data;
using Microsoft.Identity.Client;
using System;
namespace MailExporter
{
    class ExchangeOnlineMailExporter
    {
        // Config of the application.
        Config config;

        // The EmailReciever object.
        EmailReciever emailReciever;

        // The Logger
        Logger logger;

        // The MailArchiver object.
        MailArchiver? mailArchiver;

        // The email to pdf converter object.
        EmailToPdfConverter? emailToPdfConverter;



        // EVENTS



        // METHODS

        internal ExchangeOnlineMailExporter(Config config)
        {
            this.config = config;
            logger = new Logger(config);

            logger.Log("Starting initialization.");

            var cca = ConfidentialClientApplicationBuilder
                    .Create("APP_ID") // Must be replaced with your app ID.
                    .WithClientSecret(config.password)
                    .WithTenantId("TENANT_ID") // Must be replaced with your tenant ID.
                    .Build();

            var Scopes = new string[] { "https://outlook.office365.com/.default" };
            var authResult = cca.AcquireTokenForClient(Scopes).ExecuteAsync().Result;

            // create the EmailReciever object.
            switch (config.recieveMode)
            {
                case RecieveMode.Once:
                    // log recieve mode
                    logger.Log("Recieve mode: Once");
                    emailReciever = new EmailRecieverOnce(new OAuthCredentials(authResult.AccessToken));
                    break;
                case RecieveMode.Events:
                    // log recieve mode
                    logger.Log("Recieve mode: Events");
                    throw new NotImplementedException();
                //emailReciever = new EmailRecieverEvents();
                //break;
                case RecieveMode.Timer:
                    // log recieve mode
                    logger.Log("Recieve mode: Timer");
                    throw new NotImplementedException();
                //emailReciever = new EmailRecieverTimer();
                //break;
                default:
                    throw new Exception("RecieveMode not implemented.");
            }

            emailToPdfConverter = new EmailToPdfConverter(config, logger);

            // Add the new mail recieved event.
            emailReciever.NewMailRecieved += OnMailRecieved;

            // log initialization done
            logger.Log("Initialization done.");
        }



        // Lock Object for the email converter.
        object emailConverterLock = new object();

        // Method for starting the recieving of mails.
        private void OnMailRecieved(object? sender, NewMailEventArgs e)
        {
            // log new mail recieved
            logger.Log("Mail " + e.NewMail.Id + " recieved.");

            // Send email to be converted to pdf.
            if (emailToPdfConverter is not null)
            {
                // lock the email converter
                lock (emailConverterLock)
                {
                    // convert the email to pdf.
                    emailToPdfConverter.ConvertEmailToPdf(sender, e).Wait();
                }
            }
        }


        // Method for starting the recieving of mails.
        internal async System.Threading.Tasks.Task Start(CancellationTokenSource cts)
        {
            // Log start.
            logger.Log("Start recieving mails.");
            // Start the EmailReciever.
            var recieverTask = emailReciever.Start(cts);

            // Wait for the reciever to finish. If the reciever runs once, it will end after all mails are processed.
            await recieverTask;
        }






    }
}