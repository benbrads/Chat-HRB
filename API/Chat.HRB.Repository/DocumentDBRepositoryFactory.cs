#region Using Directives

using Chat.HRB.Common.CosmosDB;
using Chat.HRB.Common.Interfaces;
using Chat.HRB.Interface;
using Chat.HRB.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

#endregion

namespace Chat.HRB.Repository
{
    public class DocumentDbRepositoryFactory : IDocumentDbRepositoryFactory
    {
        #region [ Private Constants ]

        private const string CHAT_HISTORY = "ChatHistoryDb";
        private const string PROMPT_DATA = "PromptDataDb";

        #endregion

        #region [ Fields ]

        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<DocumentDbType, IDocumentDbRepository> _repositories;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of DocumentDbRepositoryFactory with the specified IConfiguration
        /// </summary>
        /// <param name="configuration">IConfiguration</param>
        public DocumentDbRepositoryFactory(IConfiguration configuration)
        {
            _configuration = configuration;
            _repositories = new ConcurrentDictionary<DocumentDbType, IDocumentDbRepository>();
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Returns a IDocumentDbRepository for the specified DbType
        /// </summary>
        /// <param name="dbType">DocumentDbType</param>
        /// <returns>IDocumentDbRepository Object</returns>
        public IDocumentDbRepository GetDocumentDbRepository(DocumentDbType dbType) => _repositories.GetOrAdd(dbType, AddDocumentDbRepository);

        #endregion

        #region [ Private Methods ]

        /// <summary>
        /// Returns an AzureDocumentDbRepository object for the specified dbType
        /// </summary>
        /// <param name="dbType">Document Db Type</param>
        /// <returns>AzureDocumentDbRepository object</returns>
        private IDocumentDbRepository AddDocumentDbRepository(DocumentDbType dbType) => dbType switch
        {
            DocumentDbType.ChatHistory => new AzureDocumentDbRepository(_configuration.GetSection(CHAT_HISTORY)),
            DocumentDbType.Prompt => new AzureDocumentDbRepository(_configuration.GetSection(PROMPT_DATA)),

            _ => throw new ArgumentException($"Document Type {dbType} is not valid!")
        };

        #endregion
    }
}
