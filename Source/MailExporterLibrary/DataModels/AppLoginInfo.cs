namespace MailExporter.Lib
{
    public  record AppLoginInfo
    {
        public string appID = "";
        public string tenantID = "";
        public string secretKey = "";

        public AppLoginInfo(string appID, string tenantID, string secretKey)
        {
            this.appID = appID;
            this.tenantID = tenantID;
            this.secretKey = secretKey;
        }
    }
}