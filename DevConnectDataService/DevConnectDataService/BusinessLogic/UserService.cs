using DevConnectDataService.BusinessLogic.QueryParamters;
using DevConnectDataService.DataAccess;
using DevConnectDataService.DataAccess.CosmosDBService;
using DevConnectDataService.DataAccess.Documents;
using DevConnectDataService.Model;
using DevConnectDataService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.BusinessLogic
{
	internal class UserService : IUserService
	{
		IUserDataAdapter _userDataAdapter;

		public UserService(IUserDataAdapter userDataAdapter)
		{
			this._userDataAdapter = userDataAdapter;
		}

		public async Task<PaginatedResult<UserDocument>> GetUsers(UserListQueryParameters userListQueryParamter)
		{
			return  await this._userDataAdapter.GetDocumentListAsync(userListQueryParamter);
		}
	}
}
