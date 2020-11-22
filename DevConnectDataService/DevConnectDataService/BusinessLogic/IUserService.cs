using DevConnectDataService.BusinessLogic.QueryParamters;
using DevConnectDataService.DataAccess.Documents;
using DevConnectDataService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.Services
{
    public interface IUserService
    {

        /// <summary>
        /// Gets a paginated list of user records
        /// </summary>
        /// <returns></returns>
        Task<PaginatedResult<UserDocument>> GetUsers(UserListQueryParameters userListQueryParamter);

    }
}
