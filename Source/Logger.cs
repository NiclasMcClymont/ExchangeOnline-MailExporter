namespace MailExporter
{
    class Logger
    {
        Config config;

        // construktor
        internal Logger(Config conf)
        {
            config = conf;
        }

        // log a message
        internal void Log(string message)
        {
            // if no logging is enabled or config is null, return.
            if (config.loggingMode == LoggingMode.None || config == null)
                return;

            // log to console.
            System.Console.WriteLine(message);

            // if logging to file is enabled, log to file.
            if (config.loggingMode == LoggingMode.File)
            {
                // TODO: implement logging to file.
            }    
        }
    }



    internal enum LoggingMode
    {
        None = 0,
        Console = 1,
        File = 2
    }
}