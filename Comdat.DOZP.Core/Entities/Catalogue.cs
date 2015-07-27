using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Comdat.DOZP.Core
{
    [DataContract(IsReference = true)]
    public partial class Catalogue //: IValidatableObject
    {
        public Catalogue()
        {
            this.Books = new List<Book>();
            this.Institutions = new List<Institution>();
        }

        [DataMember]
        public int CatalogueID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        //[Display(Name = "URL adresa Z39.50 Serveru", ShortName = "URL")]
        //[DataType(DataType.Url, ErrorMessage = "Chybná URL hodnota")]
        //[MaxLength(100, ErrorMessage = "URL adresa Z39.50 Serveru mùže být max. 100 znakù")]
        [DataMember]
        public string ZServerUrl { get; set; }

        [DataMember]
        public Nullable<int> ZServerPort { get; set; }

        [DataMember]
        public string DatabaseName { get; set; }
        
        public string Charset { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Configuration { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public bool Enabled { get; set; }

        public virtual ICollection<Book> Books { get; set; }
        public virtual ICollection<Institution> Institutions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetDirectoryPath()
        {
            if (String.IsNullOrEmpty(DatabaseName)) throw new ArgumentNullException("DatabaseName");

            string path = null;

            try
            {
                path = Path.Combine(App.REPOSITORY_DIR, DatabaseName);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format("Chyba pøi vytváøení složky pro katalog: '{0}'", ex.Message));
            }

            return path;
        }

        public string GetDirectoryFTP(bool create = false)
        {
            if (String.IsNullOrEmpty(DatabaseName)) throw new ArgumentNullException("DatabaseName");

            string path = null;

            try
            {
                path = Path.Combine(App.FTP_DIR, DatabaseName, DateTime.Now.ToString("yyyyMMdd"));

                if (create && !Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format("Chyba pøi vytváøení FTP složky pro katalog: '{0}'", ex.Message));
            }

            return path;
        }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    var results = new List<ValidationResult>();

        //    //if (OwnerID == 0)
        //    //{
        //    //    results.Add(new ValidationResult("OwnerID = 0", new[] { "OwnerID" }));
        //    //}

        //    return results;

        //    //if (InstitutionID == 0)
        //    //{
        //    //    yield return new ValidationResult("OwnerID = 0", new[] { "OwnerID" });
        //    //}
        //}
    }
}
