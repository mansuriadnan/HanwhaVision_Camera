using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Common
{
    public class IdracApiConstant
    {
        /// <summary>
        /// Get main components of Idrac system 
        /// </summary>
        public const string IdracMainComponent  = "/redfish/v1/Systems/System.Embedded.1";
        public const string IdracAlertEventServiceEnable1 = "/redfish/v1/EventService";
        public const string IdracAlertEventServiceEnable2 = "/redfish/v1/Managers/iDRAC.Embedded.1/Oem/Dell/DellAttributes/iDRAC.Embedded.1";
        public const string IdracAlertEventServiceSubscriptions = "/redfish/v1/EventService/Subscriptions";
        public const string IdracLedStateChange = "/redfish/v1/Chassis/System.Embedded.1";
        public const string IdracPowerAction =  "/redfish/v1/Systems/System.Embedded.1/Actions/ComputerSystem.Reset";
        public const string SystemInformation = "/redfish/v1/Systems/System.Embedded.1";
        public const string ServerOsAttributes = "/redfish/v1/Managers/System.Embedded.1/Attributes?$select=ServerOS.*";
        public const string IPv4Attributes = "/redfish/v1/Managers/iDRAC.Embedded.1/Attributes?$select=IPv4.*";
        public const string NicAttributes = "/redfish/v1/Managers/iDRAC.Embedded.1/Attributes?$select=NIC.*";
        public const string IdracInfoAttributes = "/redfish/v1/Managers/iDRAC.Embedded.1/Attributes?$select=Info.*";
        public const string BiosInformation = "/redfish/v1/Systems/System.Embedded.1/Bios?$select=Attributes/SystemBiosVersion,Attributes/SystemModelName";
        public const string LicenseCollection = "/redfish/v1/LicenseService/Licenses";
        public const string IdracAlertEventServiceReceiver = "/api/IDracManagement/IdracEventAlarmReceiver";

        /// <summary>
        /// Get data of idrac colling fan details 
        /// </summary>
        public const string IdracCoolingFan = "/redfish/v1/Chassis/System.Embedded.1/Thermal";

        /// <summary>
        /// Get data of idrac power system details 
        /// </summary>
        public const string IdracPowerSystem = "/redfish/v1/Chassis/System.Embedded.1/PowerSubsystem";

        /// <summary>
        /// Get data of idrac network adapter 
        /// </summary>
        public const string IdracNetworkAdapter = "/redfish/v1/Chassis/System.Embedded.1/NetworkAdapters";

        /// <summary>
        /// Get data of idrac storage details
        /// </summary>
        public const string IdracStorage = "/redfish/v1/Systems/System.Embedded.1/Storage";

        /// <summary>
        /// Get data of system board CPU usage sensor
        /// </summary>
        public const string IdracSystemBoardCPUUsage = "/redfish/v1/Chassis/System.Embedded.1/Sensors/SystemBoardCPUUsage";

        /// <summary>
        /// Get data of system board MEM usage sensor
        /// </summary>
        public const string IdracSystemBoardMEMUsage = "/redfish/v1/Chassis/System.Embedded.1/Sensors/SystemBoardMEMUsage";

        /// <summary>
        /// 
        /// </summary>
        public const string IdracSystemLogs = "/redfish/v1/Managers/iDRAC.Embedded.1/LogServices/Sel/Entries?$skip=0&$top=100";
    }
}
