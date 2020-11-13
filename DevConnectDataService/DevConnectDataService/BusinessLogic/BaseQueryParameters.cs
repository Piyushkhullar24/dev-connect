using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess
{
    /// <summary>
    /// Base class for query parameters
    /// </summary>
    public class BaseQueryParameters
    {
        /// <summary>
        /// The number of documents to be returned.
        /// </summary>
        /// <value></value>
        [FromQuery(Name = "limit")]
        public int Limit { get; set; } = 100;

        /// <summary>
        /// The continuation token for the Volunteer List call.
        /// </summary>
        [FromQuery(Name = "continuation_token")]
        public string ContinuationToken { get; set; }

    }
}
