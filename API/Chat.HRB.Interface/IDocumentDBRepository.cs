#region Using Directives

using System.Linq.Expressions;
using Chat.HRB.Common.Model;

#endregion
namespace Chat.HRB.Common.Interfaces
{
    public interface IDocumentDbRepository
    {
        #region Methods

        Task<T> GetItemAsync<T>(string id) where T : class;
        Task<T> GetItemAsync<T>(string id, string partitionKey) where T : class;
        Task<IEnumerable<T>> GetItemsAsync<T>() where T : class;
        Task<IEnumerable<T>> GetItemsAsync<T>(Expression<Func<T, bool>> predicate) where T : class;
        Task<T> CreateItemAsync<T>(T item);
        Task<T> UpdateItemAsync<T>(string id, T item);
        Task DeleteItemAsync(string id);
        Task DeleteItemAsync(string id, string partitionKey);

        #endregion

        #region Properties

        public DocumentDbSettings DocumentDbSettings { get; }

        #endregion
    }
}
