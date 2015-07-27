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
        [Required(AllowEmptyStrings = false, ErrorMessage = "Po�adov�na je SIGLA instituce")]
        //[MaxLength(6, ErrorMessage = "SIGLA instituce m��e b�t max. 6 znak�")]
        [RegularExpression("^[A-Z]{3}[0-9]{3}$")]
        public string Sigla { get; set; }

        [DataMember]
        [Display(Name = "N�zev instituce", ShortName = "N�zev")]
        [DataType(DataType.Text)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Po�adov�n je n�zev instituce")]
        //[MaxLength(250, ErrorMessage = "N�zev instituce m��e b�t max. 250 znak�")]
        public string Name { get; set; }

        [DataMember]
        [Display(Name = "Zkratka instituce", ShortName = "Zkratka")]
        //[MaxLength(20, ErrorMessage = "Zkratka instituce m��e b�t max. 20 znak�")]
        [DataType(DataType.Text)]
        public string Acronym { get; set; }

        [DataMember]
        [Display(Name = "Popis instituce", ShortName = "Popis")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [DataMember]
        [Display(Name = "Adresa instituce", ShortName = "Adresa")]
        [DataType(DataType.Text)]
        //[MaxLength(500, ErrorMessage = "URL adresa domovsk� str�nky m��e b�t max. 500 znak�")]
        public string Address { get; set; }

        [DataMember]
        [Display(Name="Domovsk� str�nka", ShortName="URL")]
        [DataType(DataType.Url, ErrorMessage = "Chybn� URL hodnota")]
        //[MaxLength(100, ErrorMessage = "URL adresa domovsk� str�nky m��e b�t max. 100 znak�")]
        public string Homepage { get; set; }

        [DataMember]
        [Display(Name = "URL adresa ALEPHu", ShortName = "ALEPH")]
        [DataType(DataType.Url, ErrorMessage = "Chybn� URL hodnota")]
        //[MaxLength(100, ErrorMessage = "URL adresa ALEPHu m��e b�t max. 100 znak�")]
        public string AlephUrl { get; set; }

        [DataMember]
        [Display(Name = "Email adresa", ShortName = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage="Chybn� email addresa")]
        //[MaxLength(100, ErrorMessage = "Email adresa m��e b�t max. 100 znak�")]
        public string Email { get; set; }

        [DataMember]
        [Display(Name = "Telefon", ShortName = "Telefon")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Chybn� telefonn� ��slo")]
        //[MaxLength(50, ErrorMessage = "Email adresa m��e b�t max. 50 znak�")]
        public string Telephone { get; set; }

        [Display(Name = "Aktivn�")]
        public bool Enabled { get; set; }

        [DataMember]
        [Display(Name = "Katalogy instituce", ShortName = "Katalogy")]
        public virtual ICollection<Catalogue> Catalogues { get; set; }

        [Display(Name = "U�ivatel� instituce", ShortName = "U�ivatel�")]
        public virtual ICollection<User> Users { get; set; }
    }
}
