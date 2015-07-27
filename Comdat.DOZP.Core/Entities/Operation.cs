using System;
using System.Collections.Generic;

namespace Comdat.DOZP.Core
{
    public partial class Operation
    {
        public int OperationID { get; set; }
        public int ScanFileID { get; set; }
        public string UserName { get; set; }
        public string Computer { get; set; }
        public DateTime Executed { get; set; }
        public string Comment { get; set; }
        public StatusCode Status { get; set; }

        public virtual ScanFile ScanFile { get; set; }
        public virtual User User { get; set; }
    }
}
