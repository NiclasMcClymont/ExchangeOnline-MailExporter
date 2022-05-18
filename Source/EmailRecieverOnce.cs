using Microsoft.Exchange.WebServices.Data;

namespace MailExporter
{
    internal class EmailRecieverOnce : EmailReciever
    {

        // Constructor.
        internal EmailRecieverOnce(WebCredentials credentials) : base(credentials)
        {
            
        }

        internal async override System.Threading.Tasks.Task Start(CancellationTokenSource cts)
        {
            // Create the ExchangeService.
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2016);

            // Set credentials.
            service.Credentials = credentials;

            base.SetAutodiscoverUrl(service);

            // Get the folder ID.
            FolderId folderId = await base.GetFolderId(service, Program.config.savePath, cts.Token);
            Folder folder = await Folder.Bind(service, folderId);


        

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
                    base.OnMailRecieved(new NewMailEventArgs(mail));
                }
            }
            
        }
    }
}