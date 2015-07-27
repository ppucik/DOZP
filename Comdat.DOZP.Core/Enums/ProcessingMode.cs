using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract]
    [Description("Způsob zpracování")]
    public enum ProcessingMode : short
    {
        /// <summary>
        /// Obálka
        /// </summary>
        [EnumMember]
        [Description("Skenovat")]
        Scan = 0,

        /// <summary>
        /// Obsah
        /// </summary>
        [EnumMember]
        [Description("OCR")]
        OCR = 1
    }
}
