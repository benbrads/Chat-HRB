#region Using Directives

using Microsoft.Extensions.Configuration;

#endregion

namespace Chat.HRB.Common.Model
{
    public class DocumentDbSettings
    {

        #region Constructor

        public DocumentDbSettings()
        {

        }

        public DocumentDbSettings(IConfiguration configuration)
        {
            try
            {
                DatabaseName = configuration["DatabaseName"];
                CollectionName = configuration["CollectionName"];
                ConnectionString = configuration["ConnectionString"];
                PartitionKey = configuration["PartitionKey"];
                int maxConnectionLimit = 0;
                int.TryParse(configuration["MaxConnectionLimit"], out maxConnectionLimit);
                this.MaxConnectionLimit = maxConnectionLimit;
            }
            catch (Exception ex)
            {
                throw new MissingFieldException("IConfiguration missing a valid Azure DocumentDB fields on DocumentDB > [DatabaseName,CollectionName,EndpointUri,Key]", ex);
            }
        }

        #endregion

        #region Properties

        public string DatabaseName { get; private set; }
        public string CollectionName { get; private set; }
        public string ConnectionString { get; private set; }
        public string PartitionKey { get; private set; }
        public int MaxConnectionLimit { get; private set; }

        #endregion
    }
}
