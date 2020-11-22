using DevConnectDataService.DataAccess.BaseDataAdapter;
using DevConnectDataService.DataAccess.CosmosDBService;
using DevConnectDataService.DataAccess.Documents;
using DevConnectDataService.Model;
using DocumentDbRepository.Contracts;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess
{
	internal abstract class BaseDataAdapter<DocumentType, IdType, QueryParametersType> : IBaseDataAdapter<DocumentType, IdType, QueryParametersType>
	where DocumentType : BaseDocument<IdType>
	where QueryParametersType : BaseQueryParameters
	{
		#region Properties and Fields
		protected DocumentClientHolder _clientHolder;
		protected IDocRepo<DocumentType> _docRepo;
		#endregion

		public BaseDataAdapter(IDocCollection<DocCollectionSettings> collection, DocumentClientHolder clientHolder)
		{
			_clientHolder = clientHolder;
			_docRepo = collection.GetDocumentRepository<DocumentType>();
		}

		public async Task<PaginatedResult<DocumentType>> GetDocumentListAsync(QueryParametersType parameters)
		{
			var query = BuildQuery(parameters, "");

			var queryResults = await query.AsDocumentQuery().ExecuteNextAsync<DocumentType>();

			return new PaginatedResult<DocumentType>(queryResults)
			{
				ContinuationToken = GetContinuationTokenFromFeedResponse(queryResults)
			};
		}

		public IQueryable<DocumentType> BuildQuery(QueryParametersType parameters, string environmentId)
		{
			// Set up Query Options and Query
			var options = new FeedOptions()
			{
				MaxItemCount = parameters.Limit,
				RequestContinuation = parameters.ContinuationToken
			};

			IQueryable<DocumentType> query = _clientHolder.Client.CreateDocumentQuery<DocumentType>(
				_clientHolder.VolunteerCollectionUri, options);

			query = FilterQuery(parameters, query);
			query = SortQuery(parameters, query);
			return query;
		}

		protected static string GetContinuationTokenFromFeedResponse(FeedResponse<DocumentType> feedResponse)
		{
			try
			{
				return feedResponse.ResponseContinuation;
			}
			catch (NullReferenceException)
			{
				return null;
			}
		}

		#region Virtual and Abstract
		public abstract IQueryable<DocumentType> FilterQuery(QueryParametersType parameters, IQueryable<DocumentType> query);

		public virtual IQueryable<DocumentType> SortQuery(QueryParametersType parameters, IQueryable<DocumentType> query)
		{
			return query;
		}
		#endregion
	}
}