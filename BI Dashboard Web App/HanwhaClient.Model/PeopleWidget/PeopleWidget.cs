namespace HanwhaClient.Model.PeopleWidget
{
    public class GenderWisePeopleCounting
    {
        public string Gender { get; set; }
        public int Count { get; set; } = 0;
        public int MinCount { get; set; } = int.MaxValue;
        public DateTime? MinDate { get; set; }
        public int MaxCount { get; set; } = 0;
        public DateTime? MaxDate { get; set; }
    }

    public class GenderWisePeopleAnalysisCount
    {
        public DateTime? DateTime { get; set; }
        public string DeviceId { get; set; }
        public double MaleCount { get; set; } = 0;
        public double FemaleCount { get; set; } = 0;
        public double UndefinedCount { get; set; } = 0;

    }

    public class AgeWisePeopleAnalysisCount
    {
        public DateTime? DateTime { get; set; }
        public string DeviceId { get; set; }
        public int YoungCount { get; set; } = 0;
        public int AdultCount { get; set; } = 0;
        public int SeniorCount { get; set; } = 0;
        public int UnknownCount { get; set; } = 0;

    }

    public class PeopleCountAggregatedDto
    {
        public DateTime Interval { get; set; }
        public int LineIndex { get; set; }
        public double AvgInCount { get; set; }
        public double AvgOutCount { get; set; }
    }
}