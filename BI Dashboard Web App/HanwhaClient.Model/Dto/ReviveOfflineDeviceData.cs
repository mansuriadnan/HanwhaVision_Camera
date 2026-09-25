namespace HanwhaClient.Model.Dto
{
    public class PeopleCountSearchResponse
    {
        public string ResultInterval { get; set; }
        public List<PeopleCountSearchResult> PeopleCountSearchResults { get; set; }
    }

    public class PeopleCountSearchResult
    {
        public string Camera { get; set; }
        public List<LineResult> LineResults { get; set; }
    }

    public class LineResult
    {
        public string Line { get; set; }
        public List<DirectionResult> DirectionResults { get; set; }
    }

    public class DirectionResult
    {
        public string Direction { get; set; }   // In / Out
        public string Result { get; set; }      // Comma-separated hourly values
    }



    public class OfflineDeviceTokenResponse
    {
        public string SearchToken { get; set; }
    }

    public class OfflineDeviceStatusResponse
    {
        public string Status { get; set; }
    }


    public class PeopleCountResponseWise
    {
        public List<CountingRuleWise> CountingRules { get; set; }
        public string ResultInterval { get; set; }
    }

    public class CountingRuleWise
    {
        public int Index { get; set; }
        public List<LineBasedResultWise> LineBasedResults { get; set; }
    }

    public class LineBasedResultWise
    {
        public int Index { get; set; }
        public List<DirectionBasedResultWise> DirectionBasedResult { get; set; }
    }

    public class DirectionBasedResultWise
    {
        public string Direction { get; set; }
        public List<int> Result { get; set; }
    }

    public class VehicleCountResponseSunapi
    {
        public string ResultInterval { get; set; }
        public List<VehicleCountSearchResult> VehicleCountSearchResults { get; set; }
    }

    public class VehicleCountSearchResult
    {
        public string Camera { get; set; }
        public List<LineResultSunapi> LineResults { get; set; }
    }

    public class LineResultSunapi
    {
        public string Line { get; set; }
        public List<DirectionResultSunapi> DirectionResults { get; set; }
    }

    public class DirectionResultSunapi
    {
        public string Direction { get; set; }   // "In" or "Out"
        public string Result { get; set; }
        public string Car { get; set; }
        public string Bus { get; set; }
        public string Truck { get; set; }
        public string Motorcycle { get; set; }
        public string Bicycle { get; set; }
    }




}
