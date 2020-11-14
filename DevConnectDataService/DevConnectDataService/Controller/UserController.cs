using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevConnectDataService.BusinessLogic.QueryParamters;
using DevConnectDataService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevConnectDataService.Controller
{
	[Route("api/[controller]")]
	internal class UserController : ControllerBase
	{
		private IUserService _userService;

		public UserController(IUserService userService)
		{
			this._userService = userService;
		}

		[HttpGet]
		public async Task<IActionResult> GetUsers(UserListQueryParameters userListQueryParamter)
		{
			await this._userService.GetUsers(userListQueryParamter);
			return new ObjectResult(null);
		}
	}
}
