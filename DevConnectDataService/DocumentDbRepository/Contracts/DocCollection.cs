using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DocumentDbRepository.Contracts
{
    /// <summary>
    /// Provides the functions you can perform against a specific collection, identified by TSettings
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    [Serializable]
    public class DocCollection<TSettings> : IDocCollection<TSettings> where TSettings : DocCollectionSettings
    {
        private DocCollectionSettings _settings;
        private DocumentClient _docClient = null;

        /// <summary>
        /// Name of the stored prcocedure for performing batch updates
        /// </summary>
        public const string TRANSACTION_BATCH_PROCID = "TransactionBatch";
        private const int DefaultInitialOfferThroughput = 400;

        /// <summary>
        /// Constructor. Provide a type derived from DocCollectionSettings to identify the collection.
        /// </summary>
        /// <param name="settings"></param>
        public DocCollection(TSettings settings)
        {

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            if (settings.AccessKey == null)
            {
                throw new ArgumentNullException(nameof(settings.AccessKey));
            }
            if (settings.DatabaseId == null)
            {
                throw new ArgumentNullException(nameof(settings.DatabaseId));
            }
            if (settings.EndpointUri == null)
            {
                throw new ArgumentNullException(nameof(settings.EndpointUri));
            }
            if (settings.CollectionId == null)
            {
                throw new ArgumentNullException(nameof(settings.CollectionId));
            }

            //can be null, useful for testing to set it to mock
            //pass one in to share it across repos (recommended)
            _docClient = settings.DocumentClient;

            _settings = settings;

            if (settings.InitializeAutomatically)
            {
                // make sure db and collection exist...
                InitDocumentDBAsync().GetAwaiter().GetResult();
            }
        }

        ConcurrentDictionary<string, object> _repos = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Execute stored procedure in this collection
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="procId"></param>
        /// <param name="partitionKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<TResult> ExecuteStoredProcedureAsync<TResult>(string procId, object partitionKey, object args)
        {
            var url = UriFactory.CreateStoredProcedureUri(_settings.DatabaseId, _settings.CollectionId, procId);

            RequestOptions options = new RequestOptions() { PartitionKey = new PartitionKey(partitionKey) };

            return await DocumentDbClient.ExecuteStoredProcedureAsync<TResult>(url, options, args);
        }

        /// <summary>
        /// Execute stored procedure in this collection
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="procId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<TResult> ExecuteStoredProcedureAsync<TResult>(string procId, object args)
        {
            var url = UriFactory.CreateStoredProcedureUri(_settings.DatabaseId, _settings.CollectionId, procId);

            return await DocumentDbClient.ExecuteStoredProcedureAsync<TResult>(url, args);
        }

        /// <summary>
        /// Get a singleton repository for executing CRUD against a document of type TDoc, in this collection.
        /// </summary>
        /// <typeparam name="TDoc"></typeparam>
        /// <returns></returns>
        public IDocRepo<TDoc> GetDocumentRepository<TDoc>() where TDoc : class
        {
            // ** 11-21-17 moved this out to constructor, changed this method back to sync
            // cant find a better place to put this, since this class is instatiated by DI
            // and constructor cant be async...?
            // so unfortunately all calls to get a Repo must alspo
            // await InitDocumentDB();
            // **

            string typeName = typeof(TDoc).FullName;
            DocRepo<TDoc> repo = new DocRepo<TDoc>(this._settings);

            return (IDocRepo<TDoc>)_repos.GetOrAdd(typeName, repo);

            //_repos.AddOrUpdate(
            //    typeName,
            //    repo,
            //    (key, existingValue) => {
            //        return existingValue;
            //    });

            //if (!_repos.ContainsKey(typeName))
            //{
            //    repo = new DocRepo<TDoc>(this._settings);
            //    _repos.Add(typeName, repo);
            //}

            //return (IDocRepo<TDoc>)_repos[typeName];

        }
        /// <summary>
        /// Execute SQL statement against this collection, returning strongly typed list of type TResult.
        /// Use this method when your results are not neccessarily a specific type of document but other values like aggregate
        /// functions like SUM(), COUNT(), etc. Searched documents must be in the same partition as PartitionKey.
        /// Assumes the query is not parameterized.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> QueryCustomResultAsync<TResult>(string sql, object partitionKey = null)
        {
            SqlQuerySpec sqlSpec = new SqlQuerySpec(sql);

            return await QueryCustomResultAsync<TResult>(sqlSpec, partitionKey);
        }

        /// <summary>
        /// Execute SQL query spec against this collection, returning strongly typed list of type TResult.
        /// Use this method when your results are not neccessarily a specific type of document but other values like aggregate
        /// functions like SUM(), COUNT(), etc. Searched documents must be in the same partition as PartitionKey.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sqlSpec"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> QueryCustomResultAsync<TResult>(SqlQuerySpec sqlSpec, object partitionKey = null)
        {
            List<TResult> results = new List<TResult>();

            FeedOptions feedOptions = null;

            if (partitionKey != null)
            {
                feedOptions = new FeedOptions
                {
                    PartitionKey = new PartitionKey(partitionKey)
                };
            }

            var qry = DocumentDbClient.CreateDocumentQuery<TResult>(this.CollectionUri, sqlSpec, feedOptions).AsDocumentQuery<TResult>();
            while (qry.HasMoreResults)
            {
                var r = await qry.ExecuteNextAsync<TResult>();
                results.AddRange(r);
            }

            return results;
        }

        /// <summary>
        /// Documentdb client
        /// </summary>
        public DocumentClient DocumentDbClient
        {
            get
            {
                if (_docClient == null)
                {
                    _docClient = new DocumentClient(new Uri(_settings.EndpointUri), _settings.AccessKey);
                }
                return _docClient;
            }
        }

        /// <summary>
        /// Returns the collection's uri (selflink)
        /// </summary>
        public Uri CollectionUri
        {
            get
            {
                return UriFactory.CreateDocumentCollectionUri(_settings.DatabaseId, _settings.CollectionId);
            }
        }

        private async Task InitDocumentDBAsync()
        {
            await EnsureDatabaseExistsAsync();
            await EnsureCollectionExistsAsync();
            // await MyRunMigrations();
        }

        private async Task EnsureDatabaseExistsAsync()
        {
            try
            {
                await DocumentDbClient.CreateDatabaseIfNotExistsAsync(new Database { Id = _settings.DatabaseId });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task EnsureCollectionExistsAsync()
        {
            DocumentCollection myCollection = new DocumentCollection
            {
                Id = _settings.CollectionId
            };

            if (!string.IsNullOrWhiteSpace(_settings.PartitionPath))
            {
                myCollection.PartitionKey.Paths.Add(_settings.PartitionPath);
            }
            RequestOptions options = new RequestOptions()
            {
                OfferThroughput = _settings.InitialOfferThroughput ?? DefaultInitialOfferThroughput
            };

            await DocumentDbClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(_settings.DatabaseId), myCollection);
        }

        /// <summary>
        /// If stored proc throws a serialized custom object in its error message, use this to deserialize it.
        /// See TransactionBatch.js.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public TResult DeserializeStoredProcErrorObject<TResult>(string message)
        {
            string messagePrefix = "Exception = Error:";
            var posKeyText = message.IndexOf(messagePrefix);

            TResult errorObject = default(TResult);

            if (posKeyText > -1)
            {
                var posStart = posKeyText + messagePrefix.Length;
                var posEnd = message.IndexOf("\\r\\n", posStart);
                string errText = message.Substring(posStart, (posEnd - posStart));
                errText = errText.Trim().Replace(@"\", @"");

                errorObject = JsonConvert.DeserializeObject<TResult>(errText);
            }
            return errorObject;
        }
    }

}
