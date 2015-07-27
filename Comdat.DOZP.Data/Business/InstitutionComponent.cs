using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data.Repository;

namespace Comdat.DOZP.Data.Business
{
    public class InstitutionComponent
    {
        private static readonly InstitutionComponent _instance = new InstitutionComponent();

        public static InstitutionComponent Instance
        {
            get
            {
                return _instance;
            }
        }

        public List<Institution> GetAll()
        {
            InstitutionRepository repository = new InstitutionRepository();
            return repository.Select();
        }

        public Institution GetByID(int institutionID)
        {
            if (institutionID == 0) throw new ArgumentNullException("institutionID");

            InstitutionRepository repository = new InstitutionRepository();
            return repository.Select(institutionID);
        }

        public Institution GetByUserName(string userName)
        {
            if (String.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException("userName");

            InstitutionFilter filter = new InstitutionFilter();
            filter.UserName = userName;

            InstitutionRepository repository = new InstitutionRepository();
            return repository.Select(filter).SingleOrDefault();
        }

        public Institution Save(Institution institution)
        {
            if (institution == null) throw new ArgumentNullException("institution");

            if (String.IsNullOrEmpty(institution.Sigla))
                throw new ArgumentNullException("Neplatný parametr Sigla");

            if (String.IsNullOrEmpty(institution.Name))
                throw new ArgumentNullException("Neplatný parametr Name");

            InstitutionRepository repository = new InstitutionRepository();

            if (institution.InstitutionID == 0)
            {
                institution = repository.Create(institution);
            }
            else
            {
                institution = repository.Update(institution);
            }

            //return repository.Select(institution.institutionID);
            return institution;
        }

        public bool Delete(int institutionID)
        {
            InstitutionRepository repository = new InstitutionRepository();
            Institution institution = repository.Select(institutionID);

            return repository.Delete(institution);
        }
    }
}
