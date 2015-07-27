using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comdat.DOZP.Process
{
    public sealed class ExceptionMessage
    {
        public const string ACCESS_DENIED = "Přístup zamítnut, zadané jméno nebo heslo je nesprávné.";
        public const string AUTHENTICATION = "Authentication error: ";
        public const string GENERAL = "General error: ";
        public const string SERVICE = "Service error: ";
        public const string COMMUNICATION = "Chyba při komunikaci se serverem: ";
        public const string TIMEOUT = "Byl překročen nastavený časový limit pro zpracování.";
    }
}
