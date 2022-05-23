using MailExporter.Lib;
using Serilog;
namespace MailExporter.UI {

    class Program {

        
        // Main Starting Point of the Program
        static async Task Main(string[] args) {

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Starting MailExporter");

            // Create a login info object
            var loginInfo = new AppLoginInfo(
                "<your-app-id>", 
                "<tenant id>", 
                "<your secret key>");

            // Create a new token
            var token = new Token(loginInfo);

            System.Console.WriteLine("Token: " + token.Authentication.AccessToken);

            await Task.Delay(1000);
        }


    }

}