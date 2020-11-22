using DevConnectDataService.BusinessLogic.QueryParamters;
using DevConnectDataService.DataAccess.BaseDataAdapter;
using DevConnectDataService.DataAccess.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess
{
	public interface IUserDataAdapter : IBaseDataAdapter<UserDocument, Guid, UserListQueryParameters>
	{
	}
}
