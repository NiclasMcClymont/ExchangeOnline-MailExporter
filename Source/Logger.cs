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

            // get the current formated date with milliseconds.
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // log to console.
            System.Console.WriteLine(date + ": " + message);

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