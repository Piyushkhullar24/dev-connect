using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DocumentDbRepository.Contracts
{

    /// <summary>
    /// Provides the CRUD operations against documents of type TDoc
    /// </summary>
    /// <typeparam name="TDoc"></typeparam>
    public class DocRepo<TDoc> : IDocRepo<TDoc>, IDisposable
        where TDoc : class
    {
        /// <summary>
        /// 
        /// </summary>
        protected DocCollectionSettings _settings;

        private DocumentClient _docClient = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public DocRepo(DocCollectionSettings settings)
        {

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            //can be null, useful for testing to set it to mock
            //pass one in to share it across repos (recommended)
            _docClient = settings.DocumentClient;

            _settings = settings;
        }

        /// <summary>
        /// Microsoft DocumentClient instance
        /// </summary>
        public DocumentClient DocumentDBClient
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

        //public async Task<string> CreateAsync(TDoc document)
        //{
        //    var docUrl = UriFactory.CreateDocumentCollectionUri(_settings.DatabaseId, _settings.CollectionId);
        //    ResourceResponse<Document> doc = await DocumentDBClient.CreateDocumentAsync(docUrl, document);
        //    return doc.Resource.Id;
        //}

        /// <summary>
        /// Read a doc of type TDoc using its keys.
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="partitionKey"></param>
        /// <param name="throwNotFoundErrors"></param>
        /// <returns></returns>
        public async Task<TDoc> ReadAsync(string documentId, object partitionKey = null, bool throwNotFoundErrors = false)
        {

            try
            {
                RequestOptions options = null;

                var docUrl = UriFactory.CreateDocumentUri(_settings.DatabaseId, _settings.CollectionId, documentId);

                if (partitionKey != null)
                {
                    options = new RequestOptions() { PartitionKey = new PartitionKey(partitionKey) };
                }

                //ResourceResponse<Document> result = null;
                //result = await DocumentDBClient.ReadDocumentAsync(docUrl, options);
                //return JsonConvert.DeserializeObject<TDoc>(result.Resource.ToString());

                return await DocumentDBClient.ReadDocumentAsync<TDoc>(docUrl, options);

            }
            catch (Exception e)
            {
                if (e is DocumentClientException dce)
                {
                    if (dce.StatusCode == HttpStatusCode.NotFound && !throwNotFoundErrors)
                    {
                        return null;
                    }
                }

                //chuck everything else
                throw;
            }


            #region "Hideme"
            //method 2 - apparently this is more expensive
            //var collectionUrl = UriFactory.CreateDocumentCollectionUri(_settings.DatabaseId, _settings.CollectionId);
            //var queryable = DocumentDBClient.CreateDocumentQuery<TDoc>(
            //    collectionUrl, new FeedOptions {MaxItemCount=1})
            //    .Where(x => x.Id == documentId).AsDocumentQuery();
            //var feedResponse = await queryable.ExecuteNextAsync<TDoc>();
            //return feedResponse.FirstOrDefault();
            #endregion

        }

        /// <summary>
        /// Upsert TDoc, returning its Id
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public async Task<string> UpsertAsync(TDoc document)
        {
            var docUrl = UriFactory.CreateDocumentCollectionUri(_settings.DatabaseId, _settings.CollectionId);
            ResourceResponse<Document> doc = await DocumentDBClient.UpsertDocumentAsync(docUrl, document);
            return doc.Resource.Id;
        }

        /// <summary>
        /// Delete document
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string documentId, object partitionKey = null)
        {
            RequestOptions options = null;

            var docUrl = UriFactory.CreateDocumentUri(_settings.DatabaseId, _settings.CollectionId, documentId);

            if (partitionKey != null)
            {
                options = new RequestOptions() { PartitionKey = new PartitionKey(partitionKey) };
            }

            ResourceResponse<Document> doc = await DocumentDBClient.DeleteDocumentAsync(docUrl, options);

            // error not thrown, so must be true, kind of dumb really.. but read some advice about
            // avoiding async Task "void" methods - should always return "something".. had to do with mocking
            // and unit testing.. 
            // should this just return Task?
            return true;
        }

        /// <summary>
        /// Execute SQL against this TDoc's collection, returning list of TDocs
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="enableCrossPartitionQuery">must be true to query across partitions</param>
        /// <returns></returns>
        public async Task<IEnumerable<TDoc>> QueryAsync(string sql, bool enableCrossPartitionQuery = false)
        {
            FeedOptions feedOptions = new FeedOptions();
            if (enableCrossPartitionQuery)
            {
                feedOptions.EnableCrossPartitionQuery = true;
            }

            List<TDoc> results = new List<TDoc>();

            var collectionUrl = UriFactory.CreateDocumentCollectionUri(_settings.DatabaseId, _settings.CollectionId);
            SqlQuerySpec sqlSpec = new SqlQuerySpec(sql);
            var qry = DocumentDBClient.CreateDocumentQuery<TDoc>(collectionUrl, sqlSpec, feedOptions).AsDocumentQuery<TDoc>();
            while (qry.HasMoreResults)
            {
                results.AddRange(await qry.ExecuteNextAsync<TDoc>());
            }

            return results;
        }

        /// <summary>
        /// Execute SQL statement against this TDoc's collection, returning strongly typed list of type TResult. 
        /// Use this method when your results are not of type TDoc but other values like aggregate 
        /// functions like SUM(), COUNT(), etc.
        /// Assumes query is not parameterized.
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
        /// Execute SQL statement against this TDoc's collection, returning strongly typed list of type TResult. 
        /// Use this method when your results are not of type TDoc but other values like aggregate 
        /// functions like SUM(), COUNT(), etc.
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

            var collectionUrl = UriFactory.CreateDocumentCollectionUri(_settings.DatabaseId, _settings.CollectionId);

            var qry = DocumentDBClient.CreateDocumentQuery<TResult>(collectionUrl, sqlSpec, feedOptions).AsDocumentQuery<TResult>();
            while (qry.HasMoreResults)
            {
                var r = await qry.ExecuteNextAsync<TResult>();
                results.AddRange(r);
            }

            return results;
        }

        /// <summary>
        /// Execute Linq query expression against collection of documents of type TDoc, returning list of said type.
        /// </summary>
        /// <param name="predicate">Can be null to retrieve all</param>
        /// <param name="enableCrossPartitionQuery">must be true to query across partitions</param>
        /// <returns></returns>
        public async Task<IEnumerable<TDoc>> QueryAsync(Expression<Func<TDoc, bool>> predicate, bool enableCrossPartitionQuery = false)
        {
            try
            {
                FeedOptions feedOptions = new FeedOptions();
                if (enableCrossPartitionQuery)
                {
                    feedOptions.EnableCrossPartitionQuery = true;
                }

                List<TDoc> results = new List<TDoc>();
                var collectionUrl = UriFactory.CreateDocumentCollectionUri(_settings.DatabaseId, _settings.CollectionId);

                IDocumentQuery<TDoc> qry;

                if (predicate != null)
                {
                    qry = DocumentDBClient.CreateDocumentQuery<TDoc>(collectionUrl, feedOptions).Where(predicate).AsDocumentQuery<TDoc>();
                }
                else
                {
                    qry = DocumentDBClient.CreateDocumentQuery<TDoc>(collectionUrl, feedOptions).AsDocumentQuery<TDoc>();
                }

                while (qry.HasMoreResults)
                {
                    results.AddRange(await qry.ExecuteNextAsync<TDoc>());
                }
                return results;
            }
            catch (Exception ex)
            {
                //to do: error monitoring - NOT THIS
                throw ex;
            }
        }

        /// <summary>
        /// Execute Linq query expression against collection of documents of type TDoc, with an OrderBy or OrderByDesc expression. If not providing
        /// partition key value in predicate, you must set enableCrossPartitionQuery to true.
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="orderBy"></param>
        /// <param name="enableCrossPartitionQuery"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TDoc>> QueryOrderByAsync<R>(Expression<Func<TDoc, bool>> predicate, Expression<Func<TDoc, R>> orderBy, bool enableCrossPartitionQuery = false, bool descending = false)
        {
            try
            {
                FeedOptions feedOptions = new FeedOptions();
                if (enableCrossPartitionQuery)
                {
                    feedOptions.EnableCrossPartitionQuery = true;
                }

                List<TDoc> results = new List<TDoc>();
                var collectionUrl = UriFactory.CreateDocumentCollectionUri(_settings.DatabaseId, _settings.CollectionId);

                IDocumentQuery<TDoc> qry;

                if (predicate != null)
                {
                    if (descending)
                    {
                        qry = DocumentDBClient.CreateDocumentQuery<TDoc>(collectionUrl, feedOptions).Where(predicate).OrderByDescending(orderBy).AsDocumentQuery<TDoc>();
                    }
                    else
                    {
                        qry = DocumentDBClient.CreateDocumentQuery<TDoc>(collectionUrl, feedOptions).Where(predicate).OrderBy(orderBy).AsDocumentQuery<TDoc>();
                    }
                }
                else
                {
                    if (descending)
                    {
                        qry = DocumentDBClient.CreateDocumentQuery<TDoc>(collectionUrl, feedOptions).OrderByDescending(orderBy).AsDocumentQuery<TDoc>();
                    }
                    else
                    {
                        qry = DocumentDBClient.CreateDocumentQuery<TDoc>(collectionUrl, feedOptions).OrderBy(orderBy).AsDocumentQuery<TDoc>();
                    }
                }

                while (qry.HasMoreResults)
                {
                    results.AddRange(await qry.ExecuteNextAsync<TDoc>());
                }
                return results;
            }
            catch (Exception ex)
            {
                //to do: error monitoring - NOT THIS
                throw ex;
            }
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// IDisposable implementation
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DocumentDBClient.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// IDisposable implementation
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

    }

}
