using DevConnectDataService.BusinessLogic.QueryParamters;
using DevConnectDataService.DataAccess.BaseDataAdapter;
using DevConnectDataService.DataAccess.CosmosDBService;
using DevConnectDataService.DataAccess.Documents;
using DocumentDbRepository.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess
{
	internal class UserDataAdapter : BaseDataAdapter<UserDocument, Guid, UserListQueryParameters>, IUserDataAdapter
	{
		public UserDataAdapter(
		   IDocCollection<DocCollectionSettings> collection,
		   DocumentClientHolder clientHolder) : base(collection, clientHolder) 
		{
		}

		public override IQueryable<UserDocument> FilterQuery(UserListQueryParameters parameters, IQueryable<UserDocument> query)
		{
			return query;
		}
	}
}
