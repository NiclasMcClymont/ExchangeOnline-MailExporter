using Microsoft.Exchange.WebServices.Data;

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



        // EVENTS



        // METHODS

        internal ExchangeOnlineMailExporter(Config config)
        {
            this.config = config;
            logger = new Logger(config);

            // Create WebCredentials object.
            WebCredentials credentials = new WebCredentials(config.address, config.password);

            // create the EmailReciever object.
            switch (config.recieveMode)
            {
                case RecieveMode.Once:
                    emailReciever = new EmailRecieverOnce(credentials);
                    break;
                case RecieveMode.Events:
                    emailReciever = new EmailRecieverEvents();
                    break;
                case RecieveMode.Timer:
                    emailReciever = new EmailRecieverTimer();
                    break;
                default:
                    throw new Exception("RecieveMode not implemented.");
            }

        }


        internal void Start(CancellationTokenSource cts)
        {

        }
    }
}