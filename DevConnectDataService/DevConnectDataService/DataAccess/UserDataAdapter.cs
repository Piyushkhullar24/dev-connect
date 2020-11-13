using DevConnectDataService.BusinessLogic.QueryParamters;
using DevConnectDataService.DataAccess.BaseDataAdapter;
using DevConnectDataService.DataAccess.CosmosDBService;
using DevConnectDataService.DataAccess.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess
{
	internal class UserDataAdapter : BaseDataAdapter<UserDocument, Guid, UserListQueryParameters>, IUserDataAdapter
	{
		public UserDataAdapter(ICosmosDbService<UserDocument, Guid, UserListQueryParameters> cosmosDbService) : base(cosmosDbService)
		{
		}

		public override IQueryable<UserDocument> FilterQuery(UserListQueryParameters parameters, IQueryable<UserDocument> query)
		{
			throw new NotImplementedException();
		}
	}
}
