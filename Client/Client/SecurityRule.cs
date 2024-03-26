using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class SecurityRule
    {
        public List<string> VersionOS { get; set; }
        public bool FireWall { get; set; }
        public bool activeAntivirus { get; set; }
    }
}
