#region Using Directives

using Chat.HRB.Common.Interfaces;
using Chat.HRB.Models;

#endregion

namespace Chat.HRB.Interface
{
    public interface IDocumentDbRepositoryFactory
    {
        #region Methods

        /// <summary>
        /// Gets the document db repository for the specified document db type
        /// </summary>
        /// <param name="dbType">Document Db Type</param>
        /// <returns>IDocumentDbRepository</returns>
        IDocumentDbRepository GetDocumentDbRepository(DocumentDbType dbType);

        #endregion
    }
}
