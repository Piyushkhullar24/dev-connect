using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.Model
{
    /// <summary>
    /// A generic wrapper for a collection
    /// </summary>
    public class PaginatedResult<T>
    {
        /// <summary>
        /// Constructs the PaginatedResult, with the collection of items
        /// </summary>
        public PaginatedResult(IEnumerable<T> value = null)
        {
            if (value == null) { value = new List<T>(); }
            this.Value = value;
        }

        internal PaginatedResult(params T[] value)
        {
            this.Value = value;
        }

        /// <summary>
        /// The collection of items
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public IEnumerable<T> Value { get; set; }


        /// <summary>
        /// Continuation token for pagination of collection
        /// </summary>
        public string ContinuationToken { get; set; }
    }
}
