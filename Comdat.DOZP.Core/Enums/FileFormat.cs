using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract]
    [Description("Formát souboru")]
    public enum FileFormat
    {
        /// <summary>
        /// Formát pro skenování obsahu
        /// </summary>
        [EnumMember]
        [Description("Tiff")]
        Tif = 0,

        /// <summary>
        /// Formát pro skenování obálek
        /// </summary>
        [EnumMember]
        [Description("Jpeg")]
        Jpg = 1,

        /// <summary>
        /// Formát pro export obsahů
        /// </summary>
        [EnumMember]
        [Description("Pdf")]
        Pdf = 2,

        /// <summary>
        /// Formát pro OCR obsahů
        /// </summary>
        [EnumMember]
        [Description("Txt")]
        Txt = 3,

        /// <summary>
        /// Formát pro založné soubory
        /// </summary>
        [EnumMember]
        [Description("Bak")]
        Bak = 9
    }
}
