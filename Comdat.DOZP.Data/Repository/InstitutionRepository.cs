using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Repository
{
    public class InstitutionRepository
    {
        public List<Institution> Select()
        {
            List<Institution> list = null;

            using (var db = new DozpContext())
            {
                list = db.Institutions.Include(e => e.Catalogues).ToList();
            }

            return list;
        }

        public Institution Select(int institutionID)
        {
            if (institutionID == 0) return null;

            Institution item = null;

            using (var db = new DozpContext())
            {
                item = db.Institutions
                     .Include(e => e.Catalogues)
                     .Where(pk => pk.InstitutionID == institutionID)
                     .SingleOrDefault();
            }

            return item;
        }

        public List<Institution> Select(InstitutionFilter filter)
        {
            if (filter == null) return null;

            List<Institution> list = null;

            using (var db = new DozpContext())
            {
                list = (from i in db.Institutions.Include(e => e.Catalogues)
                        where (0 == filter.InstitutionID || i.InstitutionID == filter.InstitutionID) &&
                              (String.IsNullOrEmpty(filter.UserName) || i.Users.Count(r => r.UserName == filter.UserName) > 0) &&
                              (!filter.Active || i.Enabled == true)
                        select i).ToList();
            }

            return list;
        }

        public Institution Create(Institution institution)
        {
            if (institution == null) return null;

            using (var db = new DozpContext())
            {
                db.Entry(institution).State = EntityState.Added;
                db.SaveChanges();
            }

            return institution;
        }

        public Institution Update(Institution institution)
        {
            if (institution == null) return null;

            //try
            //{
            using (var db = new DozpContext())
            {
                db.Entry(institution).State = EntityState.Modified;
                db.SaveChanges();
            }
            //}
            //catch (DbEntityValidationException ex)
            //{
            //    StringBuilder sb = new StringBuilder();
            //    foreach (var result in ex.EntityValidationErrors)
            //    {
            //        sb.AppendLine(string.Format(ErrorMessage.ENTITY_VALIDATION_ERROR, result.Entry.Entity.GetType().Name, result.Entry.State));
            //        foreach (var error in result.ValidationErrors)
            //        {
            //            sb.AppendLine(String.Format(ErrorMessage.PROPERTY_VALIDATION_ERROR, error.PropertyName, error.ErrorMessage));
            //        }
            //    }
            //    throw new DbEntityValidationException(sb.ToString(), ex);
            //}

            return institution;
        }

        public bool Delete(Institution institution)
        {
            if (institution == null) return false;

            using (var db = new DozpContext())
            {
                db.Entry(institution).State = EntityState.Deleted;
                db.SaveChanges();
            }

            return true;
        }
    }
}
