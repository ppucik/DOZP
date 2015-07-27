using System;
using System.ComponentModel;

namespace Comdat.DOZP.Core
{
    /// <summary>
    /// Image Components Twain pixel types (ImgScan.ICTwainPixelTypes)
    /// </summary>
    [Description("Barený mód")]
    public enum TwainPixelTypes
    {
        [Description("Černobíle")]
        BW = 0,
        [Description("Stupně šedi")]
        GRAY = 1,
        [Description("Barva")]
        RGB = 2 
    }

    /// <summary>
    /// Image Components Twain paper supported sizes (ImgScan.ICTwainSupportedSizes)
    /// </summary>
    [Description("Velikost stránky")]
    public enum TwainPageSizes
    {
        [Description("Výchozí velikost")]
        None = 0,
        /// <summary>
        /// A4 8.27x11.69in (210x297mm)
        /// </summary>
        [Description("A4 formát")]
        A4 = 1,
        /// <summary>
        /// A5 5.83x8.27in (148x210mm)
        /// </summary>
        [Description("A5 formát")]
        A5 = 5
    }

    /// <summary>
    /// Image Components Twain paper orientations (ImgScan.ICTwainOrientations)
    /// </summary>
    [Description("Orientace stránky")]
    public enum TwainOrientations
    {
        [Description("Automaticky")]
        Auto = 4,
        [Description("Na výšku")]
        Portrait = 0,
        [Description("Na šířku")]
        Landscape = 3
    }

    /// <summary>
    /// 
    /// </summary>
    [Description("Orientace stránky")]
    public enum TwainAutoAdjustment
    {
        [Description("Žádné")]
        None = 0,
        [Description("Aplikace")]
        Application = 2,
        [Description("Skener")]
        Scanner = 1
    }
}
