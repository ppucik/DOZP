using System;
using System.Collections.Generic;

namespace Comdat.DOZP.Core
{
    public partial class EventLog
    {
        public int EventLogID { get; set; }
        public int Level { get; set; }
        public string UserName { get; set; }
        public string Computer { get; set; }
        public DateTime Logged { get; set; }
        public string Message { get; set; }
    }
}
