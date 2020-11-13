using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess.Documents
{
    internal class BaseDocument<T>
    {
        /// <summary>
        ///
        /// </summary>
        /// <value></value>
        [JsonProperty(PropertyName = "id")]
        public T Id { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <value></value>
        public DocumentType DocType { get; protected set; }


        public DateTimeOffset DateAdded { get; set; }

        public DateTimeOffset DateModified { get; set; }

        public BaseDocument() { }

        /// <summary>
        ///
        /// </summary>
        public BaseDocument(string environmentId)
        {
            this.DateAdded = DateTimeOffset.Now;
            this.DateModified = DateTimeOffset.Now;
        }

    }

}
