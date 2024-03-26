using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serverTestXakaton1
{
    class SecurityRule
    {
        public List<string> VersionOS { get; set; } = new List<string> { "10", "11" };
        public bool FireWall { get; set; } = true;
        public bool activeAntivirus { get; set; } = true;
    }
}
