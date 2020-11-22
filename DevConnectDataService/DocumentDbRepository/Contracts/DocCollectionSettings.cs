using System;
using Microsoft.Azure.Documents.Client;

namespace DocumentDbRepository.Contracts
{
	public class DocCollectionSettings
	{
        /// <summary>
        ///
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string CollectionId { get; set; }

        /// <summary>
        /// path to partition property. null if no partition.
        /// </summary>
        [Obsolete("Use PartitionPath instead")]
        public string PartitonPath
        {
            get => PartitionPath;
            set
            {
                PartitionPath = value;
            }
        }

        /// <summary>
        /// path to partition property. null if no partition.
        /// </summary>
        public string PartitionPath { get; set; }


        /// <summary>
        ///
        /// </summary>
        public string EndpointUri { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DocumentClient DocumentClient { get; set; }

        /// <summary>
        /// If null, will default to 400
        /// </summary>
        public int? InitialOfferThroughput { get; set; }

        /// <summary>
        /// By default, will automatically call CreateIfNotExistAsync for your Database and Collection. THIS CALL IS AWAITED IN A SYNCHRONOUS THREAD AND CAN DEADLOCK. Disable this by setting to false.
        /// </summary>
        public bool InitializeAutomatically { get; set; } = true;
    }
}
