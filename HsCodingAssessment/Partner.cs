using System.Collections.Generic;

namespace HsCodingAssessment
{
    public class Partner
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string country { get; set; }
        public List<string> availableDates { get; set; }
    }

}
