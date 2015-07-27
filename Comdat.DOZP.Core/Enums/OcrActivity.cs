using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract]
    [Description("OCR aktivita")]
    public enum OcrActivity
    {
        /// <summary>
        /// Vyjmout na zpracování
        /// </summary>
        [EnumMember]
        [Description("Vyjmout na zpracování")]
        CheckOut = 1,

        /// <summary>
        /// Uložit
        /// </summary>
        [EnumMember]
        [Description("Odeslat na server")]
        CheckIn = 2,

        /// <summary>
        /// Uložit
        /// </summary>
        [EnumMember]
        [Description("Vyřadit ze zpracování")]
        Discard = 3,

        /// <summary>
        /// Uložit
        /// </summary>
        [EnumMember]
        [Description("Zrušit zpracování")]
        Undo = 4
    }
}
