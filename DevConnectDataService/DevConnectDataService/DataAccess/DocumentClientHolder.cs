using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess
{
    internal class DocumentClientHolder
    {
        public IDocumentClient Client { get; set; }

        public Uri VolunteerCollectionUri { get; set; }
    }
}
