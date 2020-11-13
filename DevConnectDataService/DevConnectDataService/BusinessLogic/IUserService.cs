using DevConnectDataService.BusinessLogic.QueryParamters;
using DevConnectDataService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.Services
{
    internal interface IUserService
    {

        /// <summary>
        /// Gets a paginated list of user records
        /// </summary>
        /// <returns></returns>
        Task GetUsers(UserListQueryParameters userListQueryParamter);

    }
}
