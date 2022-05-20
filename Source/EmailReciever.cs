using Microsoft.Exchange.WebServices.Data;

namespace MailExporter
{
    abstract internal class EmailReciever
    {
        // User credentials.
        internal WebCredentials? webCredentials;
        internal OAuthCredentials? oAuthCredentials;

        // Event that is called when a list of new mails is recieved.
        internal event EventHandler<NewMailEventArgs>? NewMailRecieved;

        // Constructor.
        internal EmailReciever(WebCredentials credentials)
        {
            this.webCredentials = credentials;
        }

        internal EmailReciever(OAuthCredentials credentials)
        {
            this.oAuthCredentials = credentials;
        }


        // Method that starts the recieving of mails.
        internal abstract System.Threading.Tasks.Task Start(CancellationTokenSource cts);


        // Method for setting autodiscover url.
        protected void SetAutodiscoverUrl(ExchangeService service)
        {
            
            service.Url = new Uri("https://outlook.office365.com/ews/Exchange.asmx");
            return;

            // NOT NEEDED FOR OFFICE 365. EWS IS ALWAY THE SAME. (i think)
            // Set autodiscover url.
            // service.AutodiscoverUrl(Program.config.address, (redirectionUrl) =>
            // {
            //     //The default for the validation callback is to reject the URL
            //     bool result = false;

            //     Uri redirectionUri = new Uri(redirectionUrl);
            //     if (redirectionUri.Scheme == "https")
            //     {
            //         result = true;
            //     }
            //     return result;
            // });
        }

        // Method for getting a folder ID.
        protected async Task<FolderId> GetFolderId(ExchangeService service, string folderpath, CancellationToken token)
        {
            // Split folder path into folder names.
            string[] folders = folderpath.Split('/');
            // Starting in the root folder, searching the displayname of the folders in the path.
            FolderId folderId;
            FindFoldersResults? results = await service.FindFolders(
                WellKnownFolderName.MsgFolderRoot,
                new SearchFilter.IsEqualTo(FolderSchema.DisplayName, folders.First()),
                new FolderView(1));
            folderId = results.First().Id;
            // Iterate over all folder but the first one.
            foreach (string folder in folders.SkipLast(1).Skip(1))
            {
                // Throw if task was cancelled.
                token.ThrowIfCancellationRequested();

                // Get the folder.
                results = await service.FindFolders(folderId, new SearchFilter.IsEqualTo(FolderSchema.DisplayName, folder), new FolderView(1));
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
            // Get batchsize amount of items + 1 to get an ancor.
            ItemView view = new ItemView(batchsize + 1, 0, OffsetBasePoint.Beginning);
            // Get only IDs
            view.PropertySet = new PropertySet(BasePropertySet.IdOnly);
            // Set offset to 0.
            view.Offset = 0;
            // Result variable.
            FindItemsResults<Item>? results = null;
            // Create item anchor to check if items where added.
            Item? anchor = null;
            
            do {
                // Get the items.
                results = folder.FindItems(view).Result;
                // Check if the anchor is the first item in the results.
                if ( anchor is not null || anchor != results.First())
                {
                    // if the anchor is not the first item or null, there as items added.
                    // Currently we dont do anything an just skip the item. if the item is not detected, it will be detected in the next batch.
                }
                foreach (Item item in results.Take(batchsize))
                {
                    yield return item;
                }
                view.Offset += batchsize;
                // set anchor to the last item in the batch.
                anchor = results.Last();
            } while (results.MoreAvailable);
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