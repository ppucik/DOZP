using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Comdat.DOZP.Core
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(Catalogue))]
    [KnownType(typeof(User))]
    public partial class Institution
    {
        public Institution()
        {
            this.Catalogues = new List<Catalogue>();
            this.Users = new List<User>();
        }

        [DataMember]
        [Required]
        [Display(Name = "ID instituce", ShortName = "ID")]
        [DataType("Hidden")]
        public int InstitutionID { get; set; }

        [DataMember]
        [Display(Name = "SIGLA instituce", ShortName = "SIGLA")]
        [DataType(DataType.Text)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Požadována je SIGLA instituce")]
        //[MaxLength(6, ErrorMessage = "SIGLA instituce mùže být max. 6 znakù")]
        [RegularExpression("^[A-Z]{3}[0-9]{3}$")]
        public string Sigla { get; set; }

        [DataMember]
        [Display(Name = "Název instituce", ShortName = "Název")]
        [DataType(DataType.Text)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Požadován je název instituce")]
        //[MaxLength(250, ErrorMessage = "Název instituce mùže být max. 250 znakù")]
        public string Name { get; set; }

        [DataMember]
        [Display(Name = "Zkratka instituce", ShortName = "Zkratka")]
        //[MaxLength(20, ErrorMessage = "Zkratka instituce mùže být max. 20 znakù")]
        [DataType(DataType.Text)]
        public string Acronym { get; set; }

        [DataMember]
        [Display(Name = "Popis instituce", ShortName = "Popis")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [DataMember]
        [Display(Name = "Adresa instituce", ShortName = "Adresa")]
        [DataType(DataType.Text)]
        //[MaxLength(500, ErrorMessage = "URL adresa domovské stránky mùže být max. 500 znakù")]
        public string Address { get; set; }

        [DataMember]
        [Display(Name="Domovská stránka", ShortName="URL")]
        [DataType(DataType.Url, ErrorMessage = "Chybná URL hodnota")]
        //[MaxLength(100, ErrorMessage = "URL adresa domovské stránky mùže být max. 100 znakù")]
        public string Homepage { get; set; }

        [DataMember]
        [Display(Name = "URL adresa ALEPHu", ShortName = "ALEPH")]
        [DataType(DataType.Url, ErrorMessage = "Chybná URL hodnota")]
        //[MaxLength(100, ErrorMessage = "URL adresa ALEPHu mùže být max. 100 znakù")]
        public string AlephUrl { get; set; }

        [DataMember]
        [Display(Name = "Email adresa", ShortName = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage="Chybná email addresa")]
        //[MaxLength(100, ErrorMessage = "Email adresa mùže být max. 100 znakù")]
        public string Email { get; set; }

        [DataMember]
        [Display(Name = "Telefon", ShortName = "Telefon")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Chybné telefonní èíslo")]
        //[MaxLength(50, ErrorMessage = "Email adresa mùže být max. 50 znakù")]
        public string Telephone { get; set; }

        [Display(Name = "Aktivní")]
        public bool Enabled { get; set; }

        [DataMember]
        [Display(Name = "Katalogy instituce", ShortName = "Katalogy")]
        public virtual ICollection<Catalogue> Catalogues { get; set; }

        [Display(Name = "Uživatelé instituce", ShortName = "Uživatelé")]
        public virtual ICollection<User> Users { get; set; }
    }
}
