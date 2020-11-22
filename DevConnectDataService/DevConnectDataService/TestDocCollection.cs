using DocumentDbRepository.Contracts;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService
{
    public class TestDocCollection<TSettings> : IDocCollection<TSettings> where TSettings : DocCollectionSettings
    {
        /// <summary>
        /// </summary>
        public DocumentClient DocumentDbClient
        {
            get
            {
                throw new NotSupportedException("TestDocCollection does not support DocumentDbClient");
            }
        }

        /// <summary>
        /// </summary>
        public Uri CollectionUri
        {
            get
            {
                throw new NotSupportedException("TestDocCollection does not support CollectionUri");
            }
        }

        /// <summary>
        /// </summary>
        public TResult DeserializeStoredProcErrorObject<TResult>(string message)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// </summary>
        public Task<TResult> ExecuteStoredProcedureAsync<TResult>(string procId, object args)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// </summary>
        public Task<TResult> ExecuteStoredProcedureAsync<TResult>(string procId, string partitionKey, object args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        public Task<TResult> ExecuteStoredProcedureAsync<TResult>(string procId, object partitionKey, object args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        public IDocRepo<TDoc> GetDocumentRepository<TDoc>() where TDoc : class
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// </summary>
        public Task<IEnumerable<TResult>> QueryCustomResultAsync<TResult>(string sql, string partitionKey = null)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// </summary>
        public Task<IEnumerable<TResult>> QueryCustomResultAsync<TResult>(Microsoft.Azure.Documents.SqlQuerySpec sqlSpec, string partitionKey = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        public Task<IEnumerable<TResult>> QueryCustomResultAsync<TResult>(string sql, object partitionKey = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        public Task<IEnumerable<TResult>> QueryCustomResultAsync<TResult>(SqlQuerySpec sqlSpec, object partitionKey = null)
        {
            throw new NotImplementedException();
        }
    }
}
