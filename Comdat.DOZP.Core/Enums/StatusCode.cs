using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract]
    [Description("Stav zpracování")]
    public enum StatusCode
    {
        /// <summary>
        /// Nezahájeno
        /// </summary>
        [EnumMember]
        [Description("Nezahájeno")]
        NotStarted = 0,

        /// <summary>
        /// Naskenováno
        /// </summary>
        [EnumMember]
        [Description("Naskenováno")]
        Scanned = 1,

        /// <summary>
        /// Ve zpracování
        /// </summary>
        [EnumMember]
        [Description("Ve zpracování")]
        InProgress = 2,

        /// <summary>
        /// Vyřazeno
        /// </summary>
        [EnumMember]
        [Description("Vyřazeno")]
        Discarded = 3,

        /// <summary>
        /// Dokončeno
        /// </summary>
        [EnumMember]
        [Description("Dokončeno")]
        Complete = 4,

        /// <summary>
        /// Exportováno
        /// </summary>
        [EnumMember]
        [Description("Exportováno")]
        Exported = 5
    }
}
