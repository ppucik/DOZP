using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract]
    [Description("Typ statistiky")]
    public enum StatisticsType
    {
        /// <summary>
        /// Celkem
        /// </summary>
        [EnumMember]
        [Description("Celkem")]
        Summary = 0,

        /// <summary>
        /// Časové období
        /// </summary>
        [EnumMember]
        [Description("Časové období")]
        TimePeriod = 1,

        /// <summary>
        /// Uživatelé
        /// </summary>
        [EnumMember]
        [Description("Uživatelé")]
        Users = 2,

        /// <summary>
        /// Katalogy
        /// </summary>
        [EnumMember]
        [Description("Katalogy")]
        Catalogues = 3
    }
}
