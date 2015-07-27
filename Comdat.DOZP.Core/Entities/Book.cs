using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Comdat.DOZP.Core
{
    [DataContract(IsReference = true)]
    public partial class Book //: IValidatableObject
    {
        public Book()
        {
            this.ScanFiles = new List<ScanFile>();
        }

        public Book(BookSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            this.BookID = settings.BookID;
            this.CatalogueID = settings.CatalogueID;
            this.SysNo = settings.SysNo;
            this.ISBN = settings.ISBN;
            this.ISSN = settings.ISSN;
            this.NBN = settings.NBN;
            this.OCLC = settings.OCLC;
            this.Author = settings.Author;
            this.Title = settings.Title;
            this.Year = settings.Year;
            this.Volume = settings.Volume;
            this.Barcode = settings.Barcode;
        }

        [DataMember]
        public int BookID { get; set; }

        [DataMember]
        public int CatalogueID { get; set; }

        [DataMember]
        public string SysNo { get; set; }

        [DataMember]
        public short FileIndex { get; set; }

        [DataMember]
        public string ISBN { get; set; }

        [DataMember]
        public string ISSN { get; set; }

        [DataMember]
        public string NBN { get; set; }

        [DataMember]
        public string OCLC { get; set; }

        [DataMember]
        public string Author { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Year { get; set; }

        [DataMember]
        public string Volume { get; set; }

        /// <summary>
        /// Author: Title, Year, Volume, ISBN
        /// </summary>
        public string Publication
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (!String.IsNullOrEmpty(this.Author)) sb.AppendFormat("{0}: ", this.Author);
                if (!String.IsNullOrEmpty(this.Title)) sb.AppendFormat("{0}", this.Title);
                if (!String.IsNullOrEmpty(this.Year)) sb.AppendFormat(", {0}", this.Year);
                if (!String.IsNullOrEmpty(this.ISBN)) sb.AppendFormat(", ISBN {0}", this.ISBN);
                if (!String.IsNullOrEmpty(this.Volume)) sb.AppendFormat(" ({0})", this.Volume);

                return sb.ToString();
            }
        }

        [DataMember]
        public string Barcode { get; set; }

        [DataMember]
        public DateTime Created { get; set; }

        [DataMember]
        public DateTime Modified { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public virtual Catalogue Catalogue { get; set; }

        [DataMember]
        public virtual ICollection<ScanFile> ScanFiles { get; set; }

        public ScanFile FrontCover
        {
            get
            {
                return (ScanFiles != null ? ScanFiles.SingleOrDefault(f => f.PartOfBook == PartOfBook.FrontCover) : null);
            }
        }

        public ScanFile TableOfContents
        {
            get
            {
                return (ScanFiles != null ? ScanFiles.SingleOrDefault(f => f.PartOfBook == PartOfBook.TableOfContents) : null);
            }
        }

        /// <summary>
        /// Textuje existenci skenovaní èásti publikace
        /// </summary>
        /// <param name="partOfBook"></param>
        /// <returns></returns>
        public bool HasPartOfBook(PartOfBook partOfBook)
        {
            return (ScanFiles != null ? ScanFiles.Count(f => f.PartOfBook == partOfBook) > 0 : false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public bool IsExported()
        {
            return ((this.FrontCover != null && this.FrontCover.Status == StatusCode.Exported) &&
                    (this.TableOfContents != null && this.TableOfContents.Status == StatusCode.Exported));
        }
        public bool IsExported(PartOfBook partOfBook)
        {
            switch (partOfBook)
            {
                case PartOfBook.FrontCover:
                    return (this.FrontCover != null && this.FrontCover.Status == StatusCode.Exported);
                case PartOfBook.TableOfContents:
                    return (this.TableOfContents != null && this.TableOfContents.Status == StatusCode.Exported);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partOfBook"></param>
        /// <returns></returns>
        public bool CanImageChanged(PartOfBook partOfBook)
        {
            switch (partOfBook)
            {
                case PartOfBook.FrontCover:
                    return (FrontCover == null ||
                            FrontCover.Status == StatusCode.Scanned ||
                            FrontCover.Status == StatusCode.Discarded ||
                            FrontCover.Status == StatusCode.Complete);

                case PartOfBook.TableOfContents:
                    return (TableOfContents == null ||
                            TableOfContents.Status == StatusCode.Scanned ||
                            TableOfContents.Status == StatusCode.Discarded ||
                            TableOfContents.Status == StatusCode.Complete);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partOfBook"></param>
        public void SetImageChanged(PartOfBook partOfBook)
        {
            switch (partOfBook)
            {
                case PartOfBook.FrontCover:
                    if (this.FrontCover != null)
                        this.FrontCover.ImageChanged = true;
                    break;

                case PartOfBook.TableOfContents:
                    if (this.TableOfContents != null)
                        this.TableOfContents.ImageChanged = true;
                    break;

                default:
                    break;
            }
        }

        public string GetFileName()
        {
            if (!String.IsNullOrEmpty(this.SysNo))
            {
                string bookISBN = String.IsNullOrEmpty(this.ISBN) ? "" : this.ISBN.Replace("-", "").TrimStart("978");

                if (!String.IsNullOrEmpty(bookISBN))
                    return String.Format("{0}_{1}", this.SysNo, bookISBN);
                else
                    return this.SysNo;
            }
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetDirectoryPath()
        {
            if (Catalogue == null) throw new ArgumentNullException("Catalogue");

            return Catalogue.GetDirectoryPath();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public BookSettings GetSettings()
        {
            BookSettings settitngs = new BookSettings();
            settitngs.BookID = this.BookID;
            settitngs.CatalogueID = this.CatalogueID;
            settitngs.SysNo = this.SysNo;
            settitngs.ISBN = this.ISBN;
            settitngs.ISSN = this.ISSN;
            settitngs.NBN = this.NBN;
            settitngs.OCLC = this.OCLC;
            settitngs.Author = this.Author;
            settitngs.Title = this.Title;
            settitngs.Year = this.Year;
            settitngs.Volume = this.Volume;
            settitngs.Barcode = this.Barcode;

            return settitngs;
        }
    }
}
