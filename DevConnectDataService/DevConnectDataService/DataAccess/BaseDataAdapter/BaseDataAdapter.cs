using DevConnectDataService.DataAccess.BaseDataAdapter;
using DevConnectDataService.DataAccess.CosmosDBService;
using DevConnectDataService.DataAccess.Documents;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess
{
	internal abstract class BaseDataAdapter<DocumentType, IdType, QueryParametersType> : IBaseDataAdapter<DocumentType, IdType, QueryParametersType>
	where DocumentType : BaseDocument<IdType>
	where QueryParametersType : BaseQueryParameters
	{
		private ICosmosDbService<DocumentType, IdType, QueryParametersType> _cosmosDbService;

		public BaseDataAdapter(ICosmosDbService<DocumentType,IdType, QueryParametersType> cosmosDbService)
		{
			this._cosmosDbService = cosmosDbService;
		}

		public async Task<IEnumerable<DocumentType>> GetDocumentListAsync(QueryParametersType parameters)
		{
			return await _cosmosDbService.GetDocumentListAsync(parameters);
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
