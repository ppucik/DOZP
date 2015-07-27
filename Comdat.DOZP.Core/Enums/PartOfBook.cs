using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract]
    [Description("Skenovaná část")]
    public enum PartOfBook : short
    {
        /// <summary>
        /// Obálka
        /// </summary>
        [EnumMember]
        [Description("Obálka")]
        FrontCover = 1,

        /// <summary>
        /// Obsah
        /// </summary>
        [EnumMember]
        [Description("Obsah")]
        TableOfContents = 2
    }
}
