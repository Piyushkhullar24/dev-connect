using DevConnectDataService.DataAccess.Documents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess.CosmosDBService
{

	public interface ICosmosDbService<DocumentType, IdType, QueryParametersType>
	where DocumentType : BaseDocument<IdType>
	{
		Task<IEnumerable<DocumentType>> GetDocumentListAsync(QueryParametersType parameters);
	}
}
