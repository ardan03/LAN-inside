using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientWPF 
{
    public class Configuration
    {
        public string ipAdress { get; set; }
        public string PcName { get; set; }
        public string osVersion { get; set; }
        public string osBuild { get; set; }
        public bool FireWallActive { get; set; }
        public bool AntivirusActive { get; set; }
    }
}
