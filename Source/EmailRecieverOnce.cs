using Microsoft.Exchange.WebServices.Data;

namespace MailExporter
{
    internal class EmailRecieverOnce : EmailReciever
    {

        // Constructor.
        internal EmailRecieverOnce(WebCredentials credentials) : base(credentials)
        {
            
        }

        // Constructor.
        internal EmailRecieverOnce(OAuthCredentials credentials) : base(credentials)
        {
            
        }

        internal async override System.Threading.Tasks.Task Start(CancellationTokenSource cts)
        {
            // Create the ExchangeService.
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2016);

            // Set credentials.
            if (webCredentials != null)
            {
                service.Credentials = webCredentials;
            }
            else
            {
                service.Credentials = oAuthCredentials;
            }

            base.SetAutodiscoverUrl(service);

            // Set user
            service.ImpersonatedUserId = new ImpersonatedUserId(ConnectingIdType.SmtpAddress, Program.config.address);

            // Get the folder ID.
            FolderId folderId = await base.GetFolderId(service, Program.config.mailboxPath, cts.Token);
            Folder folder = await Folder.Bind(service, folderId);

            // List of all tasks for calling events
            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
            

            // iterate through all items in the folder and call the new mail recieved event.
            var items = base.GetItems(folder, 10);
            foreach (var item in items)
            {
                // Throw if task was cancelled.
                cts.Token.ThrowIfCancellationRequested();

                // check if item is a Mail
                if (item is EmailMessage)
                {
                    // cast item to Mail
                    EmailMessage mail = (EmailMessage) item;
                    tasks.Add( System.Threading.Tasks.Task.Run( () => {base.OnMailRecieved(new NewMailEventArgs(mail));}));
                }
            }
            // Wait for all tasks to finish.
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
        }
    }
}