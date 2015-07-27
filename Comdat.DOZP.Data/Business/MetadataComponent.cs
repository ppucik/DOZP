using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

using Comdat.DOZP.Core;
using Comdat.ZRIS.Zoom;
using Comdat.ZRIS.Zoom.Yaz;

namespace Comdat.DOZP.Data.Business
{
    public class MetadataComponent
    {
        const int MAX_RECORORD_COUNT = 100;

        private static readonly MetadataComponent _instance = new MetadataComponent();

        public static MetadataComponent Instance
        {
            get
            {
                return _instance;
            }
        }

        public Report SearchBook(int catalogueID, string sysno, string isbn, string barcode)
        {
            if (catalogueID == 0) throw new ArgumentNullException("catalogueID");
            if (String.IsNullOrEmpty(sysno) && String.IsNullOrEmpty(isbn) && String.IsNullOrEmpty(barcode)) 
                throw new ArgumentNullException("sysno+isbn+barcode");

            Report report = new Report(catalogueID, sysno, isbn, barcode);

            try
            {
                Catalogue catalogue = CatalogueComponent.Instance.GetByID(catalogueID);

                if (catalogue != null)
                {
                    IConnection connection = ConnectionFactory.Create(catalogue.ZServerUrl, catalogue.ZServerPort.Value);
                    connection.DatabaseName = catalogue.DatabaseName;
                    connection.Syntax = RecordSyntax.USMARC;
                    connection.Charset = catalogue.Charset;
                    connection.UserName = catalogue.UserName;
                    connection.Password = catalogue.Password;
                    connection.Timeout = 5;

                    RecordCharset charset = RecordCharset.Default;
                    Enum.TryParse(catalogue.Charset, true, out charset);

                    IPrefixQuery prefixQuery = new PrefixQuery();
                    if (!String.IsNullOrEmpty(sysno))
                    {
                        prefixQuery.Add(UseAttribute.LocalNumber_12, sysno);
                    }
                    if (!String.IsNullOrEmpty(isbn))
                    {
                        prefixQuery.Add(UseAttribute.ISBN_7, isbn);
                    }
                    if (!String.IsNullOrEmpty(barcode))
                    {
                        prefixQuery.Add(UseAttribute.Barcode_1063, barcode);
                    }
                    IResultSet resultSet = connection.Search(prefixQuery);

                    report.Size = (int)resultSet.Size;
                    report.XmlData = resultSet.GetData(RecordFormat.XML, charset, MAX_RECORORD_COUNT);
                    report.Errors = ((report.Size > MAX_RECORORD_COUNT) ? String.Format("Příliš mnoho záznamů ({0}), upřesněte dotaz.", report.Size) : null);
                    report.Success = true;
                }
                else
                {
                    report.Errors = String.Format("Katalog ID={0} neexistuje, nelze vyhledat čárový kód.", catalogueID);
                }
            }
            catch (Bib1Exception be)
            {
                report.Errors = String.Format("{0}: {1}. ", be.DiagnosticCode, be.Message) + (be.InnerException != null ? be.InnerException.Message : "");
            }
            catch (Exception ex)
            {
                report.Errors = ex.Message + (ex.InnerException != null ? String.Format(". {0}", ex.InnerException.Message) : "");
            }

            return report;
        }
    }
}
