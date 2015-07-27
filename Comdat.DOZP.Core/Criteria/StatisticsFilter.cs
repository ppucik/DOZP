using System;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    public class StatisticsFilter
    {
        public StatisticsFilter(StatisticsType c)
        {
            this.StatisticsType = StatisticsType;
        }

        [DataMember]
        public StatisticsType StatisticsType { get; set; }

        [DataMember]
        public int CatalogueID { get; set; }

        [DataMember]
        public PartOfBook? PartOfBook { get; set; }

        [DataMember]
        public bool? UseOCR { get; set; }

        [DataMember]
        public int? Year { get; set; }

        [DataMember]
        public int? Month { get; set; }

        [DataMember]
        public int? Day { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public StatusCode? Status { get; set; }
    }
}
