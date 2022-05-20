

namespace MailExporter
{
    class Program
    {
        // The config of the application.
        internal static Config config = new Config();

        // cancelation token
        static CancellationTokenSource cts = new CancellationTokenSource();

        // The ExchangeOnlineMailExporter
        static ExchangeOnlineMailExporter? mailExporter;

        static async Task Main(string[] args)
        {

            // register OnApplicationExit
            AppDomain.CurrentDomain.ProcessExit += OnApplicationExit;
            #region ParameterStuff


            // Configpath for the config file.
            string configPath = "Config.json";

            // check if "--help" is in the arguments.
            if (args.Length > 0 && args[0] == "--help")
            {
                System.Console.WriteLine("MailExporter - A program to export mails from an email account.");
                System.Console.WriteLine("Usage: MailExporter.exe --[Parameter]=[Value]");
                System.Console.WriteLine("Parameters:");
                System.Console.WriteLine("\t--help\t\t\t\tShows this help.");
                System.Console.WriteLine("\t--address=[Email Address]\t\tThe email address the program will connect to.");
                System.Console.WriteLine("\t--password=[Password]\t\tThe password of the email adress.");
                System.Console.WriteLine("\t--recieveMode=[Mode]\t\tThe recieve mode the program will use.");
                System.Console.WriteLine("\t\t\t\t\t0 = recieve mails only once.");
                System.Console.WriteLine("\t\t\t\t\t1 = Use events to get new mails.");
                System.Console.WriteLine("\t\t\t\t\t2 = Use a timer to get new mails. => Not implemented yet.");
                System.Console.WriteLine("\t--loggingMode=[Mode]\t\tThe Logging mode the program will use.");
                System.Console.WriteLine("\t\t\t\t\t0 = No logging.");
                System.Console.WriteLine("\t\t\t\t\t1 = Log to console.");
                System.Console.WriteLine("\t\t\t\t\t2 = Log to file and console.");
                System.Console.WriteLine("\t--archivMode=[Mode]\t\tThe Archiv mode the program will use.");
                System.Console.WriteLine("\t\t\t\t\t0 = Move mails to archive.");
                System.Console.WriteLine("\t\t\t\t\t1 = Delete mails.");
                System.Console.WriteLine("\t\t\t\t\t3 = Do nothing.");
                System.Console.WriteLine("\t--savePath=[Folder Path]\t\tThe path to the folder where the program will save the emails.");
                System.Console.WriteLine("\t\t\t\t\tDefault: Archiv/");
                System.Console.WriteLine("\t--logPath=[Folder Path]\t\tThe path to the folder where the program will save the logs.");
                System.Console.WriteLine("\t\t\t\t\tDefault: Logs/");
                System.Console.WriteLine("\t--mailboxPath=[Path]\t\tThe mailbox folder where the mails will be archived from.");
                System.Console.WriteLine("\t\t\t\t\tDefault: ToArchive/");
                System.Console.WriteLine("\t--archivePath=[Path]\t\tThe mailbox folder where the mails will be archived to.");
                System.Console.WriteLine("\t\t\t\t\tDefault: ToArchive/Done/");
                System.Console.WriteLine("\t--version\t\t\tShows the version of the program.");
                System.Console.WriteLine("\t--save-config\t\t\tSaves the config to a file with the given parameter.");
                Environment.Exit(0);
            }

            // check if "--version" is in the arguments.
            if (args.Length > 0 && args[0] == "--version")
            {
                System.Console.WriteLine("MailExporter - Version 1.0.0");
                Environment.Exit(0);
            }

            // Load config from default path
            if (File.Exists(configPath))
            {
                Config? conf = await Config.Load(configPath, cts.Token);
                // If conf is not null then set config to conf.
                if (conf != null)
                {
                    config = conf;
                }
            }
            // check if "--config=<path>" is in the arguments.
            // check if the config file exists.
            foreach (string arg in args)
            {
                if (arg.StartsWith("--config="))
                {
                    configPath = arg.Substring(9);
                    if (File.Exists(configPath))
                    {
                        System.Console.WriteLine("Using config file: " + configPath);
                        Config? conf = await Config.Load(configPath, cts.Token);
                        // check if the config is valid.
                        if (conf != null)
                        {
                            config = conf;
                        }
                        else
                        {
                            // Throw exception if the config is invalid.
                            throw new Exception("The config is invalid.");
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("The config file does not exist.");
                        System.Console.WriteLine("Using default config.");
                        // if no config file is found, create a new one.
                        config = new Config();
                        await config.Save(configPath, cts.Token);
                        System.Console.WriteLine("Config saved to: " + configPath);
                        
                    }
                }
            }


            // go through the args and set the config values.
            foreach (string arg in args)
            {
                if (arg.StartsWith("--address="))
                {
                    config.address = arg.Substring(10);
                }
                else if (arg.StartsWith("--password="))
                {
                    config.password = arg.Substring(11);
                }
                else if (arg.StartsWith("--recieveMode="))
                {
                    config.recieveMode = (RecieveMode) int.Parse(arg.Substring(15));
                }
                else if (arg.StartsWith("--loggingMode="))
                {
                    config.loggingMode = (LoggingMode) int.Parse(arg.Substring(15));
                }
                else if (arg.StartsWith("--archivMode="))
                {
                    config.archivMode = int.Parse(arg.Substring(14));
                }
                else if (arg.StartsWith("--savePath="))
                {
                    config.savePath = arg.Substring(11);
                }
                else if (arg.StartsWith("--logPath="))
                {
                    config.logPath = arg.Substring(10);
                }
                else if (arg.StartsWith("--mailboxPath="))
                {
                    config.mailboxPath = arg.Substring(14);
                }
                else if (arg.StartsWith("--archivePath="))
                {
                    config.archivePath = arg.Substring(14);
                }
            }

            // Check if the "--save-config" is in the arguments.
            // If so, save the config with the new arguments.
            foreach (string arg in args)
            {
                if (arg == "--save-config")
                {
                    // save the config.
                    await config.Save(configPath, cts.Token);
                    Environment.Exit(0);
                }
            }

            #endregion

            // Create the main ExchangeOnlineMailExporter.
            mailExporter = new ExchangeOnlineMailExporter(config);
            // Start the ExchangeOnlineMailExporter.
            await mailExporter.Start(cts);
        }

        // OnApplicationExit
        private static void OnApplicationExit(object? sender, EventArgs e)
        {
            // Cancel all tasks.
            cts.Cancel(false);

            // unregister OnApplicationExit
            AppDomain.CurrentDomain.ProcessExit -= OnApplicationExit;

            // dispose the cts.
            cts.Dispose();
        }
    }
}