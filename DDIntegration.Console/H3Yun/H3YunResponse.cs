using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class H3YunResponse
    {
        public int DataType { get; set; }
        public string ErrorMessage { get; set; }
        public bool Logined { get; set; }
        public bool Successful { get; set; }
    }
}
