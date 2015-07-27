using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Comdat.DOZP.Services
{
    public class ObalkyKnikReview
    {
        /// <summary>
        /// Datum a čas vytvoření
        /// </summary>
        [DataMember]
        public DateTime created { get; set; }

        /// <summary>
        /// Název knihovny, která komentář vytvořila
        /// </summary>
        [DataMember]
        public string library_name { get; set; }

        /// <summary>
        /// Sigla knihovny, která komentář vytvořila
        /// </summary>
        [DataMember]
        public string sigla { get; set; }

        /// <summary>
        /// Identifikátor záznamu ve zdrojovém informačním systému
        /// </summary>
        [DataMember]
        public int id { get; set; }

        /// <summary>
        /// Číselné hodnocení udělené společně s komentářem
        /// </summary>
        [DataMember]
        public int rating { get; set; }

        /// <summary>
        /// Text komentáře
        /// </summary>
        [DataMember]
        public string html_text { get; set; }
    }
}
