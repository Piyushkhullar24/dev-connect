using DevConnectDataService.DataAccess.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess.BaseDataAdapter
{
    internal interface IBaseDataAdapter<DocumentType, IdType, QueryParametersType>
    where DocumentType : BaseDocument<IdType>
    {
        Task<IEnumerable<DocumentType>> GetDocumentListAsync(QueryParametersType parameters);
    }
}