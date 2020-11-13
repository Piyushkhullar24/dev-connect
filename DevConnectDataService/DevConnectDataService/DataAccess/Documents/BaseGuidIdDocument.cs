using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess.Documents
{
    internal class BaseGuidIdDocument : BaseDocument<Guid>
    {
        public BaseGuidIdDocument() { }
        public BaseGuidIdDocument(string environmentId) : base(environmentId)
        {
            this.Id = Guid.NewGuid();
        }
    }
}
