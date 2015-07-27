using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract]
    [Description("Aktivita skenování")]
    public enum ScanActivity
    {
        /// <summary>
        /// Skenovat
        /// </summary>
        [EnumMember]
        [Description("Skenovat")]
        Scan = 1,

        /// <summary>
        /// Vymazat
        /// </summary>
        [EnumMember]
        [Description("Vymazat")]
        Delete = 2
    }
}
