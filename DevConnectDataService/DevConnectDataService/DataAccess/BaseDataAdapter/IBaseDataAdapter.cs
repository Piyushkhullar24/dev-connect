using DevConnectDataService.DataAccess.Documents;
using DevConnectDataService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess.BaseDataAdapter
{
    public interface IBaseDataAdapter<DocumentType, IdType, QueryParametersType>
    where DocumentType : BaseDocument<IdType>
    {
        Task<PaginatedResult<DocumentType>> GetDocumentListAsync(QueryParametersType parameters);
    }
}