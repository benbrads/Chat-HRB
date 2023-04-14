#region User Directives 

using Chat.HRB.Common.Interfaces;
using Chat.HRB.Common.Model;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

#endregion

namespace Chat.HRB.Common.CosmosDB
{
    public class AzureDocumentDbRepository : IDocumentDbRepository
    {

        #region Fields

        private readonly DocumentDbSettings _documentDbSettings;
        private readonly AzureDocumentDBConnectionString _connectionString;
        private static DocumentClient _documentClient;

        #endregion

        #region Constructor

        public AzureDocumentDbRepository(IConfiguration configuration)
        {
            _documentDbSettings = new DocumentDbSettings(configuration);
            _connectionString = new AzureDocumentDBConnectionString(_documentDbSettings.ConnectionString);
            CreateDocumentClient();
            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }

        #endregion

        #region Properties

        public DocumentDbSettings DocumentDbSettings
        {
            get
            {
                return _documentDbSettings ?? new DocumentDbSettings();
            }
        }

        internal AzureDocumentDBConnectionString ConnectionString
        {
            get
            {
                return _connectionString ?? new AzureDocumentDBConnectionString(DocumentDbSettings.ConnectionString);
            }
        }

        internal string DatabaseName
        {
            get
            {
                return _documentDbSettings.DatabaseName;
            }
        }

        internal string CollectionName
        {
            get
            {
                return _documentDbSettings.CollectionName;
            }
        }


        #endregion

        #region Public Methods

        public async Task<T> GetItemAsync<T>(string id) where T : class
        {
            return await GetItemAsync<T>(id, String.Empty);
        }

        public async Task<T> GetItemAsync<T>(string id, string partitionKey) where T : class
        {
            T result = default(T);
            try
            {
                var requestOptions = this.CreateRequestOptions(partitionKey);
                var client = this.CreateDocumentClient();
                var documentUri = UriFactory.CreateDocumentUri(this.DatabaseName, this.CollectionName, id);
                return await client.ReadDocumentAsync<T>(documentUri, requestOptions);

            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return result;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>() where T : class
        {
            var client = this.CreateDocumentClient();
            var query = client.CreateDocumentQuery<T>(
                                                        UriFactory.CreateDocumentCollectionUri(this.DatabaseName, this.CollectionName),
                                                        new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true }
                                                      ).AsDocumentQuery();

            var results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }
            return results;
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var client = this.CreateDocumentClient();
            var query = client.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(this.DatabaseName, this.CollectionName),
                                                      new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true }
                                                      ).Where(predicate)
                                                      .AsDocumentQuery();
            var results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public async Task<T> CreateItemAsync<T>(T item)
        {
            var client = this.CreateDocumentClient();
            var documentUri = UriFactory.CreateDocumentCollectionUri(this.DatabaseName, this.CollectionName);
            var document = await client.CreateDocumentAsync(documentUri, item);
            return JsonConvert.DeserializeObject<T>(document.Resource.ToString());
        }

        public async Task<T> UpdateItemAsync<T>(string id, T item)
        {
            var client = this.CreateDocumentClient();
            var documentUri = UriFactory.CreateDocumentUri(this.DatabaseName, this.CollectionName, id);
            var document = await client.ReplaceDocumentAsync(documentUri, item);
            return JsonConvert.DeserializeObject<T>(document.Resource.ToString());
        }

        public async Task DeleteItemAsync(string id)
        {
            await DeleteItemAsync(id, String.Empty);
        }

        public async Task DeleteItemAsync(string id, string partitionKey)
        {
            var client = this.CreateDocumentClient();
            try
            {
                var requestOptions = this.CreateRequestOptions(partitionKey);
                var documentUri = UriFactory.CreateDocumentUri(this.DatabaseName, this.CollectionName, id);
                var document = await client.ReadDocumentAsync(documentUri, requestOptions);
                if (document?.Resource?.Id == id)
                {
                    await client.DeleteDocumentAsync(documentUri, requestOptions);
                }
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    throw;
                }
            }

        }

        #endregion

        #region Private Methods

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            var client = this.CreateDocumentClient();
            await client.CreateDatabaseIfNotExistsAsync(new Database() { Id = DocumentDbSettings.DatabaseName });
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            var client = this.CreateDocumentClient();
            var partitionKey = _documentDbSettings.PartitionKey;
            if (!String.IsNullOrEmpty(partitionKey))
            {

                var partitionKeyCollection = new Collection<string>();
                partitionKeyCollection.Add(partitionKey);
                var pkDefn = new PartitionKeyDefinition() { Paths = partitionKeyCollection };
                await client.CreateDocumentCollectionIfNotExistsAsync(
                                                                        UriFactory.CreateDatabaseUri(this.DatabaseName),
                                                                        new DocumentCollection { Id = this.CollectionName, PartitionKey = pkDefn },
                                                                        new RequestOptions { PartitionKey = new PartitionKey(partitionKey) }

                                                                     );
            }
            else
            {

                await client.CreateDocumentCollectionIfNotExistsAsync(
                                                                        UriFactory.CreateDatabaseUri(this.DatabaseName),
                                                                        new DocumentCollection { Id = this.CollectionName }
                                                                     );
            }
        }

        private DocumentClient CreateDocumentClient()
        {
            if (_documentClient == null)
            {
                if (_documentDbSettings.MaxConnectionLimit > 0)
                {
                    _documentClient = new DocumentClient(ConnectionString.ServiceEndpoint, ConnectionString.AuthKey, new ConnectionPolicy()
                    {
                        MaxConnectionLimit = _documentDbSettings.MaxConnectionLimit
                    });
                }
                else
                {
                    _documentClient = new DocumentClient(ConnectionString.ServiceEndpoint, ConnectionString.AuthKey);
                }

                _documentClient.OpenAsync().Wait();
            }

            return _documentClient;
        }

        private RequestOptions CreateRequestOptions(string partitionKey)
        {
            var requestOptions = new RequestOptions()
            {
                PartitionKey = new PartitionKey(Undefined.Value)
            };
            if (!String.IsNullOrEmpty(partitionKey))
            {
                requestOptions = new RequestOptions()
                {
                    PartitionKey = new PartitionKey(partitionKey),

                };
            }
            return requestOptions;
        }


        #endregion
    }
}
