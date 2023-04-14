#region Using Directives

using System.Data.Common;

#endregion

namespace Chat.HRB.Common.CosmosDB
{
    internal class AzureDocumentDBConnectionString
    {

        #region Constructor

        public AzureDocumentDBConnectionString(string connectionString)
        {
            // Use this generic builder to parse the connection string
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            if (builder.TryGetValue("AccountKey", out object key))
            {
                AuthKey = key.ToString();
            }

            if (builder.TryGetValue("AccountEndpoint", out object uri))
            {
                ServiceEndpoint = new Uri(uri.ToString());
            }
        }

        #endregion

        #region Properties

        public Uri ServiceEndpoint { get; set; }
        public string AuthKey { get; set; }

        #endregion
    }
}
