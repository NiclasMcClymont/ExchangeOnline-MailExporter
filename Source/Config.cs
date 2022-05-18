using System.IO;
using Newtonsoft.Json;

namespace MailExporter
{
    // Contains the config of the application.
    class Config 
    {
        // The version of the config format.
        internal const int Version = 1;

        // The email address the program will connect to.
        internal string address = string.Empty;

        // The password of the email adress.
        // TODO: Encrypt the password.
        internal string password = string.Empty;

        // What recieve mode the program will use.
        // 0 = recieve mails only once.
        // 1 = Use events to get new mails.
        // 2 = Use a timer to get new mails. => Not implemented yet.
        internal RecieveMode recieveMode = 0;

        // The Logging mode the program will use.
        // 0 = No logging.
        // 1 = Log to console.
        // 2 = Log to file.
        internal LoggingMode loggingMode = 0;

        // Archiv mode the program will use.
        // 0 = Move mails to archive.
        // 1 = Delete mails.
        internal int archivMode = 0;

        // The path to the folder where the program will save the emails.
        internal string savePath = "Archiv/";

        // Logging folder path.
        internal string logPath = "Logs/";

        // The mailbox folder where the mails will be archived from.
        internal string mailboxPath = "ToArchive/";

        // The mailbox folder where the mails will be archived to.
        internal string archivePath = "ToArchive/Done/";




        // Loads the config from a file.
        internal static  async Task<Config?> Load (string path, CancellationToken token) {
            // Read the config file.
            string json = await File.ReadAllTextAsync(path, cancellationToken: token);

            // Deserialize the config.
            return JsonConvert.DeserializeObject<Config>(json);
        }

        // Save the config to a file.
        internal async Task Save (string path, CancellationToken token) {
            // Serialize the config.
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            // Write the config file.
            await File.WriteAllTextAsync(path, json, cancellationToken: token);
        }
    }
}