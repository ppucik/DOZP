using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Repository
{
    public class EventLogRepository
    {
        public EventLog Select(int eventLogID)
        {
            if (eventLogID == 0) return null;

            EventLog item = null;

            using (var db = new DozpContext())
            {
                item = db.EventLogs
                     .Where(pk => pk.EventLogID == eventLogID)
                     .SingleOrDefault();
            }

            return item;
        }

        public List<EventLog> Select()
        {
            List<EventLog> list = null;

            using (var db = new DozpContext())
            {
                list = db.EventLogs.ToList();
            }

            return list;
        }

        public EventLog Create(EventLog eventLog)
        {
            if (eventLog == null) return null;

            using (var db = new DozpContext())
            {
                db.Entry(eventLog).State = EntityState.Added;
                db.SaveChanges();
            }

            return eventLog;
        }

        public bool Delete(EventLog eventLog)
        {
            if (eventLog == null) return false;

            using (var db = new DozpContext())
            {
                db.Entry(eventLog).State = EntityState.Deleted;
                db.SaveChanges();
            }

            return true;
        }
    }
}
 