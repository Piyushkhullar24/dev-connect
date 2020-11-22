using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DocumentDbRepository.Contracts
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TDocCollectionSettings"></typeparam>
    public interface IDocCollection<TDocCollectionSettings>
    {
        /// <summary>
        /// 
        /// </summary>
        DocumentClient DocumentDbClient { get; }

        /// <summary>
        /// returns the collection selflink, for building your own queries and other tasks
        /// </summary>
        System.Uri CollectionUri { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="procId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<TResult> ExecuteStoredProcedureAsync<TResult>(string procId, object args);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="procId"></param>
        /// <param name="partitionKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<TResult> ExecuteStoredProcedureAsync<TResult>(string procId, object partitionKey, object args);

        /// <summary>
        /// If stored proc throws a serialized custom object in its error message, use this to deserialize it.
        /// See TransactionBatch.js
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        TResult DeserializeStoredProcErrorObject<TResult>(string message);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TDoc"></typeparam>
        /// <returns></returns>
        IDocRepo<TDoc> GetDocumentRepository<TDoc>() where TDoc : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> QueryCustomResultAsync<TResult>(string sql, object partitionKey = null);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sqlSpec"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> QueryCustomResultAsync<TResult>(SqlQuerySpec sqlSpec, object partitionKey = null);

    }
}
