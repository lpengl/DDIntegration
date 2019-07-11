using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class H3YunBulkCreateRequest
    {
        public H3YunBulkCreateRequest()
        {
            BizObjectArray = new List<string>();
        }

        public string ActionName { get; set; }
        public string SchemaCode { get; set; }
        public List<string> BizObjectArray { get; set; }
        public bool IsSubmit { get; set; }
    }
}
