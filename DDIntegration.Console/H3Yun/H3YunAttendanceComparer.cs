using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class H3YunAttendanceComparer : IEqualityComparer<H3YunAttendance>
    {
        public bool Equals(H3YunAttendance x, H3YunAttendance y)
        {
            return x.F0000001 == y.F0000001;
        }

        public int GetHashCode(H3YunAttendance obj)
        {
            if (obj == null)
                return 0;

            return obj.F0000001.GetHashCode();
        }
    }
}
