using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Repository
{
    public class CatalogueRepository
    {
        public List<Catalogue> Select()
        {
            List<Catalogue> list = null;

            using (var db = new DozpContext())
            {
                list = db.Catalogues.ToList();
            }

            return list;
        }

        public Catalogue Select(int catalogueID)
        {
            if (catalogueID == 0) return null;

            Catalogue item = null;

            using (var db = new DozpContext())
            {
                item = db.Catalogues
                     .Where(pk => pk.CatalogueID == catalogueID)
                     .SingleOrDefault();
            }

            return item;
        }

        public List<Catalogue> Select(CatalogueFilter filter)
        {
            if (filter == null) return null;

            List<Catalogue> list = null;

            using (var db = new DozpContext())
            {
                list = (from c in db.Catalogues
                        where (0 == filter.CatalogueID || c.CatalogueID == filter.CatalogueID) &&
                              (0 == filter.InstitutionID || c.Institutions.Count(i => i.InstitutionID == filter.InstitutionID) > 0) &&
                              (!filter.Active || c.Enabled == true)
                        select c).ToList();
            }

            return list;
        }

        public Catalogue Create(Catalogue catalogue)
        {
            if (catalogue == null) return null;

            using (var db = new DozpContext())
            {
                db.Entry(catalogue).State = EntityState.Added;
                db.SaveChanges();
            }

            return catalogue;
        }

        public Catalogue Update(Catalogue catalogue)
        {
            if (catalogue == null) return null;

                using (var db = new DozpContext())
                {
                    db.Entry(catalogue).State = EntityState.Modified;
                    db.SaveChanges();
                }

            return catalogue;
        }

        public bool Delete(Catalogue catalogue)
        {
            if (catalogue == null) return false;

                using (var db = new DozpContext())
                {
                    db.Entry(catalogue).State = EntityState.Deleted;
                    db.SaveChanges();
                }

            return true;
        }
    }
}
