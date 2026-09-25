using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{

    public class InstalledApplicationInfoModel
    {
        public List<InstalledAppInfo> Apps { get; set; }
        public int InstalledApps { get; set; }
    }

    public class InstalledAppInfo
    {
        public string AppID { get; set; }
        public string AppName { get; set; }
        public string Status { get; set; }
        public string InstalledDate { get; set; } // or use DateTime if you plan to parse it
        public string Version { get; set; }
        public bool AutoStart { get; set; }
        public string Priority { get; set; }
        public string ChannelType { get; set; }
        public bool IsDefault { get; set; }
        public List<string> Permission { get; set; }
        public List<int> ActivatedChannel { get; set; }
    }

}
