using DevConnectDataService.DataAccess.BaseDataAdapter;
using DevConnectDataService.DataAccess.Documents;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess.CosmosDBService
{
	internal class CosmosDbService<DocumentType, IdType, QueryParametersType> : ICosmosDbService<DocumentType, IdType, QueryParametersType>
where DocumentType : BaseDocument<IdType>
where QueryParametersType : BaseQueryParameters
	{
		private Container _container;

		public CosmosDbService(
			CosmosClient dbClient,
			string databaseName,
			string containerName)
		{
			this._container = dbClient.GetContainer(databaseName, containerName);
		}

		public async Task<IEnumerable<DocumentType>> GetDocumentListAsync(QueryParametersType parameters)
		{
			try
			{
				var iterator = _container.GetItemLinqQueryable<DocumentType>().ToFeedIterator();
				var results = await iterator.ReadNextAsync();

				return results;
			}
			catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return null;
			}
		}
	}
}
