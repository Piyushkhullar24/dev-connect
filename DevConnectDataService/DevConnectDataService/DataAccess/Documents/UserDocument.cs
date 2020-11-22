using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevConnectDataService.DataAccess.Documents
{
	public class UserDocument : BaseGuidIdDocument
	{
		public UserDocument()
		{
			this.DocType = DocumentType.User;
		}

		public UserDocument(string environmentId) : base(environmentId)
		{
			this.DocType = DocumentType.User;
		}

		public int techStackId { get; set; }
	}
}
