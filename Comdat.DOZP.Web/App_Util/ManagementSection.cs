using System;
using System.Configuration;

using Comdat.DOZP.Core;

//<configSections>
//    <sectionGroup name="dozpSettings">
//        <section name="management" type="Comdat.TOC.Data.Configuration.ManagementSection"/>
//    </sectionGroup>
//</configSections>
//<dozpSettings>
//    <management>
//        <storagefolders>
//            <add name="warehouse" path="C:\DATA"/>
//            <add name="ftp" path="C:\FTP"/>
//            <add name="www" path="C:\WWW"/>
//        </storagefolders>
//        <frontCovers>
//            <add name="tiff" resolution="100"/>
//            <add name="jpeg" width="0" height="600" quality="Optimal"/>
//            <add name="thumbnail" width="125" height="0" quality="Optimal"/>
//        </frontCovers>
//        <tableOfContents>
//            <add name="tiff" resolution="600"/>
//            <add name="pdf" resolution="300"/>
//        </tableOfContents>
//        <rssFeeds>
//            <add name="tasks" ttl="10" count="10">
//        </rssFeeds>
//        <googleAnalytics trackID="UA-3470901-1"/>
//    </management>
//</dozpSettings>

namespace Comdat.DOZP.Web
{
    public class ManagementSection : ConfigurationSection
    {
        public const string SectionName = "dozpSettings/management";

        [ConfigurationProperty("storagefolders")]
        public StorageFolderElementCollection StorageFolders
        {
            get { return (StorageFolderElementCollection)this["storagefolders"]; }
            set { this["storagefolders"] = value; }
        }

        [ConfigurationProperty("frontCovers")]
        public FileImageElementCollection FrontCovers
        {
            get { return (FileImageElementCollection)this["frontCovers"]; }
            set { this["frontCovers"] = value; }
        }

        [ConfigurationProperty("tableOfContents")]
        public FileImageElementCollection TableOfContents
        {
            get { return (FileImageElementCollection)this["tableOfContents"]; }
            set { this["tableOfContents"] = value; }
        }

        [ConfigurationProperty("rssFeeds")]
        public RssFeedElementCollection RssFeeds
        {
            get { return (RssFeedElementCollection)this["rssFeeds"]; }
            set { this["rssFeeds"] = value; }
        }

        [ConfigurationProperty("googleAnalytics")]
        public GoogleAnalyticsElement GoogleAnalytics
        {
            get { return (GoogleAnalyticsElement)this["googleAnalytics"]; }
            set { this["googleAnalytics"] = value; }
        }
    }

    #region Storage folders

    public class StorageFolderElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = false)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("path", IsRequired = true)]
        public string Path
        {
            get { return (string)this["path"]; }
            set { this["path"] = value; }
        }
    }

    public class StorageFolderElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new StorageFolderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((StorageFolderElement)element).Name;
        }

        public StorageFolderElement this[int index]
        {
            get
            {
                return (StorageFolderElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public StorageFolderElement this[string name]
        {
            get
            {
                return (StorageFolderElement)BaseGet(name);
            }
        }
    }

    #endregion

    #region File images

    public class FileImageElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = false)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("resolution", IsRequired = false, DefaultValue = 300)]
        [IntegerValidator(MinValue = 75, MaxValue = 600, ExcludeRange = false)]
        public int Resolution
        {
            get { return (int)this["resolution"]; }
            set { this["resolution"] = value; }
        }

        [ConfigurationProperty("width", IsRequired = false, DefaultValue = 0)]
        public int Width
        {
            get { return (int)this["width"]; }
            set { this["width"] = value; }
        }

        [ConfigurationProperty("height", IsRequired = false, DefaultValue = 0)]
        public int Height
        {
            get { return (int)this["height"]; }
            set { this["height"] = value; }
        }

        //[ConfigurationProperty("quality", IsRequired = false, DefaultValue = ScanImage.JpegQuality.Original)]
        //public ScanImage.JpegQuality Quality
        //{
        //    get { return (ScanImage.JpegQuality)this["quality"]; }
        //    set { this["quality"] = value; }
        //}
    }

    public class FileImageElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new FileImageElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FileImageElement)element).Name;
        }

        public FileImageElement this[int index]
        {
            get
            {
                return (FileImageElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public FileImageElement this[string name]
        {
            get
            {
                return (FileImageElement)BaseGet(name);
            }
        }
    }

    #endregion

    #region RSS feeds

    public class RssFeedElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = false)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("ttl", IsRequired = false, DefaultValue = 10)]
        [IntegerValidator(MinValue = 1, MaxValue = 60, ExcludeRange = false)]
        public int Ttl
        {
            get { return (int)this["ttl"]; }
            set { this["ttl"] = value; }
        }

        [ConfigurationProperty("count", IsRequired = false, DefaultValue = 10)]
        [IntegerValidator(MinValue = 1, MaxValue = 100, ExcludeRange = false)]
        public int Count
        {
            get { return (int)this["count"]; }
            set { this["count"] = value; }
        }
    }

    public class RssFeedElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new RssFeedElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RssFeedElement)element).Name;
        }

        public RssFeedElement this[int index]
        {
            get
            {
                return (RssFeedElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public RssFeedElement this[string name]
        {
            get
            {
                return (RssFeedElement)BaseGet(name);
            }
        }
    }

    #endregion

    #region Google analytics

    public class GoogleAnalyticsElement : ConfigurationElement
    {
        [ConfigurationProperty("trackID", IsRequired = false, DefaultValue = "")]
        public string TrackID
        {
            get { return (string)this["trackID"]; }
            set { this["trackID"] = value; }
        }
    }

    #endregion
}
