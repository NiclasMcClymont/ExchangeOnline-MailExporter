using Microsoft.Exchange.WebServices.Data;

namespace MailExporter
{
    abstract internal class EmailReciever
    {
        // User credentials.
        internal WebCredentials credentials;

        // Event that is called when a list of new mails is recieved.
        internal event EventHandler<NewMailEventArgs>? NewMailRecieved;

        // Constructor.
        internal EmailReciever(WebCredentials credentials)
        {
            this.credentials = credentials;
        }


        // Method that starts the recieving of mails.
        internal abstract System.Threading.Tasks.Task Start(CancellationTokenSource cts);


        // Method for setting autodiscover url.
        protected void SetAutodiscoverUrl(ExchangeService service)
        {
            // Set autodiscover url.
            service.AutodiscoverUrl(Program.config.address, (redirectionUrl) =>
            {
                //The default for the validation callback is to reject the URL
                bool result = false;

                Uri redirectionUri = new Uri(redirectionUrl);
                if (redirectionUri.Scheme == "https")
                {
                    result = true;
                }
                return result;
            });
        }

        // Method for getting a folder ID.
        protected async Task<FolderId> GetFolderId(ExchangeService service, string folderpath, CancellationToken token)
        {
            // Split folder path into folder names.
            string[] folders = folderpath.Split('/');
            // Starting in the root folder, searching the displayname of the folders in the path.
            FolderId folderId = new FolderId(WellKnownFolderName.MsgFolderRoot);
            foreach (string folder in folders)
            {
                // Throw if task was cancelled.
                token.ThrowIfCancellationRequested();

                // Get the folder.
                FindFoldersResults? results = await service.FindFolders(folderId, new SearchFilter.IsEqualTo(FolderSchema.DisplayName, folder), new FolderView(1));
                if (results.Count() == 0 || results == null)
                {
                    throw new Exception("Folder not found.");
                }
                // Set the folderId to the folder.
                folderId = results.First().Id;
            }
            return folderId;
        }

        // Get all mails in a folder.
        protected IEnumerable<Item> GetItems(Folder folder, int batchsize)
        {
            // Get 10 items at a time.
            ItemView view = new ItemView(batchsize);
            // Get only IDs
            view.PropertySet = new PropertySet(BasePropertySet.IdOnly);
            // offset is used to get the next batch of items.
            int offset = 0;
            
            // Do while loop to get all items.
            do
            {
                // Get the items. This operartion can take a while.
                FindItemsResults<Item> results = folder.FindItems(view).Result;
                // Add the items to the list.
                foreach (Item item in results)
                {
                    yield return item;
                }
                // Set the offset.
                offset += batchsize;
                // Set the view.
                view.Offset = offset;
            } while (view.Offset < folder.TotalCount);
            yield break;
        }

        // From https://docs.microsoft.com/de-de/dotnet/csharp/programming-guide/events/how-to-raise-base-class-events-in-derived-classes
        //The event-invoking method that derived classes can override.
        protected virtual void OnMailRecieved(NewMailEventArgs e)
        {
            // Safely raise the event for all subscribers
            NewMailRecieved?.Invoke(this, e);
        }

    }



    // EventArgs for the NewMailRecieved event.
    internal class NewMailEventArgs : EventArgs
    {
        // The list of new mails.
        internal EmailMessage NewMail { get; }

        // initializes the NewMailEventArgs.
        internal NewMailEventArgs(EmailMessage newMail)
        {
            NewMail = newMail;
        }
    }



    internal enum RecieveMode
    {
        Once = 0,
        Events = 1,
        Timer = 2
    }
}