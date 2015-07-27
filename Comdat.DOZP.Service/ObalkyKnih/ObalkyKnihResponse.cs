using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using Newtonsoft.Json;

namespace Comdat.DOZP.Services
{
    /// <summary>
    /// Represent json object of response for original toc and cover, used for Serialization
    /// All attributes have same name as in query in JavaScript API of ObalkyKnih.cz
    /// </summary>
    [DataContract]
    public class ObalkyKnihResponse
    {
        /// <summary>
        /// Počet hodnocení díla čtenáři
        /// </summary>
        [DataMember]
        public int rating_count { get; set; }

        /// <summary>
        /// Součet bodů hodnocení díla udělených čtenáři
        /// </summary>
        [DataMember]
        public int rating_sum { get; set; }

        /// <summary>
        /// Pole s komentáři
        /// </summary>
        [DataMember]
        public List<object> reviews { get; set; }

        /// <summary>
        /// EAN, ISBN, ISSN díla, pokud jej kolekce obsahuje
        /// </summary>
        [DataMember]
        public string ean { get; set; }

        /// <summary>
        /// NBN díla, pokud jej kolekce obsahuje
        /// </summary>
        [DataMember]
        public string nbn { get; set; }

        /// <summary>
        /// OCLC díla, pokud jej kolekce obsahuje
        /// </summary>
        [DataMember]
        public string oclc { get; set; }

        /// <summary>
        /// URL adresa malého náhledu obálky 27x36px
        /// </summary>
        [DataMember]
        public string cover_thumbnail_url { get; set; }

        /// <summary>
        /// URL adresa většího náhledu obálky 54x68px
        /// </summary>
        [DataMember]
        public string cover_icon_url { get; set; }

        /// <summary>
        /// URL adresa plného náhledu obálky 170x240px
        /// </summary>
        [DataMember]
        public string cover_medium_url { get; set; }

        /// <summary>
        /// URL adresa plného náhledu obálky 170x240px z cache serveru
        /// </summary>
        [JsonIgnore]
        [DataMember]
        public byte[] cover_image { get; set; }

        /// <summary>
        /// Přepis naskenovaného díla do textu tzv. OCR
        /// </summary>
        [DataMember]
        public string toc_text_url { get; set; }

        /// <summary>
        /// URL adresa plného náhledu obsahu 170x240px
        /// </summary>
        [DataMember]
        public string toc_thumbnail_url { get; set; }

        /// <summary>
        /// URL adresa s naskenovaným obsahem díla
        /// </summary>
        [DataMember]
        public string toc_pdf_url { get; set; }

        /// <summary>
        /// URL adresa zpětného odkazu
        /// </summary>
        [DataMember]
        public string backlink_url { get; set; }

        /// <summary>
        /// Parametry tak, jak byly zaslány v dotazu
        /// </summary>
        [DataMember]
        public ObalkyKnihBibInfo bibinfo { get; set; }

        /// <summary>
        /// ID metadatového záznamu v systému Obálky knih.cz
        /// </summary>
        [DataMember]
        public string bookid { get; set; }

        /// <summary>
        /// Vlastní identifikátor kolekce na daném frontendu
        /// </summary>
        [DataMember]
        public string _id { get; set; }
    }
}
