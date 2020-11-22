using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentDbRepository.Contracts
{
    /// <summary>
    /// Arguments to pass to the TransactionBatch stored procedure
    /// </summary>
    public class TransactionBatchArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TransactionBatchArgs()
        {
            operations = new List<TransactionDocument>();
        }

        /// <summary>
        /// Collection of document operations to perform atomically
        /// </summary>
        [JsonProperty(PropertyName = "operations")]
        public List<TransactionDocument> operations;
    }

    /// <summary>
    /// A document operation to perform as part of a batch
    /// </summary>
    public class TransactionDocument
    {
        /// <summary>
        /// Operation code, can be "u" for update, "r" for replace, or "d" for delete
        /// </summary>
        [JsonProperty(PropertyName = "op")]
        public string op;


        /// <summary>
        /// Document to operate on
        /// </summary>
        [JsonProperty(PropertyName = "document")]
        public object document;
    }
}
