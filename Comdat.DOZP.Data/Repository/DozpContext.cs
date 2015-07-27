using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Text;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data.Mapping;

namespace Comdat.DOZP.Data.Repository
{
    public partial class DozpContext : DbContext
    {
        static DozpContext()
        {
            Database.SetInitializer<DozpContext>(null);
        }

        public DozpContext()
            : base("Name=DozpContext")
        {
            //this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Catalogue> Catalogues { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<ScanFile> ScanFiles { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new BookMap());
            modelBuilder.Configurations.Add(new CatalogueMap());
            modelBuilder.Configurations.Add(new EventLogMap());
            modelBuilder.Configurations.Add(new ScanFileMap());
            modelBuilder.Configurations.Add(new InstitutionMap());
            modelBuilder.Configurations.Add(new OperationMap());
            modelBuilder.Configurations.Add(new RoleMap());
            modelBuilder.Configurations.Add(new UserMap());

            modelBuilder.Entity<User>().Ignore(s => s.RoleName);
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var result in ex.EntityValidationErrors)
                {
                    foreach (var error in result.ValidationErrors)
                    {
                        sb.AppendLine(String.Format("Entity of type '{0}' in state '{1}' has property '{2}' validation error: {3}", 
                            result.Entry.Entity.GetType().Name, 
                            result.Entry.State, 
                            error.PropertyName, error.ErrorMessage));
                    }
                }

                throw new DbEntityValidationException(sb.ToString(), ex);
            }
        }
    }
}
