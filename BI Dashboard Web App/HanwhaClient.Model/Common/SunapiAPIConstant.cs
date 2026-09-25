namespace HanwhaClient.Model.Common
{
    public static class SunapiAPIConstant
    {
        /// <summary>
        /// Get the basic device information
        /// </summary>
        public const string DeviceInfo = "/stw-cgi/system.cgi?msubmenu=deviceinfo&action=view";

        /// <summary>
        /// Check the status of events
        /// </summary>
        public const string EventStatus = "/stw-cgi/eventstatus.cgi?msubmenu=eventstatus&action=check";

        /// <summary>
        /// View the count of people detected
        /// </summary>
        public const string PeopleCountView = "/stw-cgi/eventsources.cgi?msubmenu=peoplecount&action=view";

        /// <summary>
        /// Check the count of people detected
        /// </summary>
        public const string PeopleCountCheck = "/stw-cgi/eventsources.cgi?msubmenu=peoplecount&action=check";

        /// <summary>
        /// Configure object counting for a specific channel
        /// </summary>
        public const string ChannelWiseAPI = "/opensdk/WiseAI/configuration/objectcounting?channel=#channelnumber#";

        /// <summary>
        /// Check the count of vehicles detected, including AI statistics
        /// </summary>
        public const string VehicleCountCheckSunapi = "/stw-cgi/eventsources.cgi?msubmenu=vehiclecount&action=check&ShowAIStats=True";

        /// <summary>
        /// View the count of vehicles detected
        /// </summary>
        public const string VehicleCount = "/stw-cgi/eventsources.cgi?msubmenu=vehiclecount&action=view";

        /// <summary>
        /// View the available event source options
        /// </summary>
        public const string EventSources = "/stw-cgi/eventsources.cgi?msubmenu=sourceoptions&action=view";

        /// <summary>
        /// View running event of camera
        /// </summary>
        public const string EventTrack = "/stw-cgi/eventstatus.cgi?msubmenu=eventstatus&action=monitordiff";

        /// <summary>
        /// Reset all counting of the device
        /// </summary>
        public const string ResetDeviceCount = "/stw-cgi/system.cgi?msubmenu=databasereset&action=control&IncludeDataType=All";

        /// <summary>
        /// Get People Heatmap 
        /// </summary>
        public const string PeopleHeatmap = "/stw-cgi/eventsources.cgi?msubmenu=heatmap&action=check&IncludeDataType=All";

        /// <summary>
        /// Get People Heatmap 
        /// </summary>
        public const string PeopleHeatmapConfiguration = "/stw-cgi/eventsources.cgi?msubmenu=heatmap&action=view";

        /// <summary>
        /// Get image for heatmap 
        /// </summary>
        public const string HeatmapImage = "/stw-cgi/video.cgi?msubmenu=snapshot&action=view";


        /// <summary>
        /// Check the X Series device for SUNAPI or Wise API
        /// </summary>
        public const string SunapiOrWise = "/stw-cgi/opensdk.cgi?msubmenu=apps&action=view";


        //To restore lost data of devices while devices are offline

        /// <summary>
        /// used for generate token by giving lost timing 
        /// </summary>
        public const string PeopleOfflineDeviceGenerateToken = "/stw-cgi/recording.cgi?msubmenu=peoplecountsearch&action=control&FromDate=";

        /// <summary>
        /// used for validate the token  
        /// </summary>
        public const string PeopleOfflineDeviceVerifyToken = "/stw-cgi/recording.cgi?msubmenu=peoplecountsearch&action=view&Type=Status&SearchToken=";

        /// <summary>
        /// used for get data devices   
        /// </summary>
        public const string PeopleOfflineDeviceGetData = "/stw-cgi/recording.cgi?msubmenu=peoplecountsearch&action=view&Type=Results&SearchToken=";


        /// <summary>
        /// used for generate token by giving lost timing 
        /// </summary>
        public const string VehicleOfflineDeviceGenerateToken = "/stw-cgi/recording.cgi?msubmenu=vehiclecountsearch&action=control&FromDate=";

        /// <summary>
        /// used for validate the token  
        /// </summary>
        public const string VehicleOfflineDeviceVerifyToken = "/stw-cgi/recording.cgi?msubmenu=vehiclecountsearch&action=view&Type=Status&SearchToken=";

        /// <summary>
        /// used for get data devices   
        /// </summary>
        public const string VehicleOfflineDeviceGetData = "/stw-cgi/recording.cgi?msubmenu=vehiclecountsearch&action=view&Type=Results&SearchToken=";

        /// <summary>
        /// used for open ANPR gate barrier
        /// </summary>
        public const string OpenAnprGateBarrier  = "/stw-cgi/io.cgi?msubmenu=alarmoutput&action=control&AlarmOutput.1.State=On";

        /// <summary>
        /// used for open ANPR gate barrier
        /// </summary>
        public const string OpenAnprGateBarrierAlways = "/stw-cgi/io.cgi?msubmenu=alarmoutput&action=control&AlarmOutput.1.State=";


    }
}
