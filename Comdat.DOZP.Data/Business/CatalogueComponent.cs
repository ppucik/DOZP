using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data.Repository;

namespace Comdat.DOZP.Data.Business
{
    public class CatalogueComponent
    {
        private static readonly CatalogueComponent _instance = new CatalogueComponent();

        public static CatalogueComponent Instance
        {
            get
            {
                return _instance;
            }
        }

        public string GetScanDirPath(Catalogue catalogue)
        {
            if (catalogue == null) throw new ArgumentNullException("catalogue");
            if (String.IsNullOrEmpty(catalogue.DatabaseName)) throw new ArgumentNullException("DatabaseName");

            string path = null;

            try
            {
                path = Path.Combine(App.REPOSITORY_DIR, catalogue.DatabaseName);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format("Chyba při vytváření složky pro katalog: '{0}'", ex.Message));
            }

            return path;
        }

        public string GetFtpDirPath(Catalogue catalogue)
        {
            if (catalogue == null) throw new ArgumentNullException("catalogue");
            if (String.IsNullOrEmpty(catalogue.DatabaseName)) throw new ArgumentNullException("DatabaseName");

            string path = null;

            try
            {
                path = Path.Combine(App.FTP_DIR, catalogue.DatabaseName, DateTime.Now.ToString("yyyyMMdd"));

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format("Chyba při vytváření FTP složky pro katalog: '{0}'", ex.Message));
            }

            return path;
        }

        public List<Catalogue> GetAll()
        {
            List<Catalogue> result = default(List<Catalogue>);

            try
            {
                CatalogueRepository repository = new CatalogueRepository();
                result = repository.Select();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }

            return result;
        }

        public Catalogue GetByID(int catalogueID)
        {
            if (catalogueID == 0) throw new ArgumentNullException("catalogueID");

            CatalogueRepository repository = new CatalogueRepository();
            return repository.Select(catalogueID);
        }

        public List<Catalogue> GetByInstitutionID(int institutionID, string sortExpression)
        {
            CatalogueFilter filter = new CatalogueFilter();
            filter.InstitutionID = institutionID;

            CatalogueRepository repository = new CatalogueRepository();
            return GetList(filter).OrderBy(sortExpression).ToList();
        }

        public List<Catalogue> GetList(CatalogueFilter filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");

            CatalogueRepository repository = new CatalogueRepository();

            if (filter.UserRole == RoleConstants.ADMINISTRATOR)
                return repository.Select();
            else
                return repository.Select(filter);
        }

        public Catalogue Save(Catalogue catalogue)
        {
            if (catalogue == null)
                throw new ArgumentNullException("catalogue");

            if (String.IsNullOrEmpty(catalogue.Name))
                throw new ArgumentNullException("Neplatný parametr Name");

            CatalogueRepository repository = new CatalogueRepository();

            if (catalogue.CatalogueID == 0)
            {
                catalogue.Created = DateTime.Now;
                catalogue.Modified = catalogue.Created;
                catalogue = repository.Create(catalogue);
            }
            else
            {
                catalogue.Modified = DateTime.Now;
                catalogue = repository.Update(catalogue);
            }

            return catalogue;
        }

        public int Export(int catalogueID, string userName, string computer)
        {
            //kontrola vstupnich parametru
            if (catalogueID == 0)
                throw new ArgumentNullException("Neplatný parametr identifikátor katalogu.");

            int result = 0;
            CatalogueRepository repository = new CatalogueRepository();

            //kontrola existence katalogu
            Catalogue catalogue = repository.Select(catalogueID);
            if (catalogue == null)
                throw new ApplicationException(String.Format("Katalog (ID={0}) neexistuje.", catalogueID));

            StreamWriter sw = null;

            try
            {
                BookFilter filter = new BookFilter();
                filter.CatalogueID = catalogue.CatalogueID;
                filter.Status = StatusCode.Complete;
                List<Book> books = BookComponent.Instance.GetList(filter);

                filter.UseOCR = false;
                filter.Status = StatusCode.Scanned;
                books.AddRange(BookComponent.Instance.GetList(filter));

                if (books != null && books.Count > 0)
                {
                    string logFilePath = Path.Combine(catalogue.GetDirectoryFTP(), "Export.log");

                    if (File.Exists(logFilePath))
                        throw new ApplicationException(String.Format("Soubor s exportom '{0}' již existuje.", logFilePath));

                    foreach (var book in books)
                    {
                        try
                        {
                            if (BookComponent.Instance.Export(book.BookID, userName, computer))
                            {
                                string publication = book.Title;
                                if (!String.IsNullOrEmpty(book.Author))
                                    publication = String.Format("{0}: {1}", book.Author, publication);
                                if (!String.IsNullOrEmpty(book.Year))
                                    publication = String.Format("{0}, {1}", publication, book.Year);

                                string volume = null;
                                string jpgFileName = null;
                                string pdfFileName = null;
                                string txtFileName = null;

                                if (book.FrontCover != null)
                                {
                                    jpgFileName = book.FrontCover.FileName;
                                }

                                if (book.TableOfContents != null)
                                {
                                    volume = String.Format("Obsah {0}", book.Volume).Trim();
                                    pdfFileName = book.TableOfContents.OcrFileName;

                                    if (book.TableOfContents.UseOCR)
                                    {
                                        txtFileName = book.TableOfContents.TxtFileName;
                                    }
                                }

                                string exportBook = String.Join(" | ", new string[] { book.SysNo, publication, volume, jpgFileName, pdfFileName, txtFileName });

                                if (sw == null)
                                    sw = new StreamWriter(logFilePath, false);
                                sw.WriteLine(exportBook);
                                sw.Flush();

                                result++;
                            }
                        }
                        catch
                        {
                            //zapis chyby do textoveho suboru Errors.log
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(String.Format("Nepodařilo se exportovat katalog (ID={0}) na FTP: {1}", catalogueID, ex.Message));
            }
            finally
            {
                if (sw != null)
                {
                    sw.Flush();
                    sw.Close();
                }
            }

            return result;
        }

        //[Obsolete]
        //public int Export2(int catalogueID, DateTime expoted)
        //{
        //    //kontrola vstupnich parametru
        //    if (catalogueID == 0)
        //        throw new ArgumentNullException("Neplatný parametr identifikátor katalogu.");

        //    int result = 0;
        //    CatalogueRepository repository = new CatalogueRepository();

        //    //kontrola existence katalogu
        //    Catalogue catalogue = repository.Select(catalogueID);
        //    if (catalogue == null)
        //        throw new ApplicationException(String.Format("Katalog (ID={0}) neexistuje.", catalogueID));

        //    try
        //    {
        //        BookFilter filter = new BookFilter();
        //        filter.CatalogueID = catalogue.CatalogueID;
        //        filter.Modified.From = expoted;
        //        filter.Modified.To = expoted;
        //        filter.Status = StatusCode.Exported;
        //        List<Book> books = BookComponent.Instance.GetList(filter);

        //        if (books != null && books.Count > 0)
        //        {
        //            foreach (var book in books)
        //            {
        //                try
        //                {
        //                    if (book.BookID != 1)
        //                    {
        //                        //string ftpPath = Path.Combine(App.FTP_DIR, catalogue.DatabaseName, book.Modified.ToString("yyyyMMdd"));
        //                        string ftpPath = Path.Combine(App.FTP_DIR, catalogue.DatabaseName, DateTime.Now.ToString("yyyyMMdd"));
        //                        if (!Directory.Exists(ftpPath)) Directory.CreateDirectory(ftpPath);
        //                        string logFilePath = Path.Combine(ftpPath, "Export.log");
        //                        StreamWriter sw = new StreamWriter(logFilePath, true);

        //                        if (BookComponent.Instance.Export2(book.BookID, ftpPath))
        //                        {
        //                            string publication = book.Title;
        //                            if (!String.IsNullOrEmpty(book.Author))
        //                                publication = String.Format("{0}: {1}", book.Author, publication);
        //                            if (!String.IsNullOrEmpty(book.Year))
        //                                publication = String.Format("{0}, {1}", publication, book.Year);

        //                            string volume = null;
        //                            string jpgFileName = null;
        //                            string pdfFileName = null;
        //                            string txtFileName = null;

        //                            if (book.FrontCover != null)
        //                            {
        //                                jpgFileName = book.FrontCover.FileName;
        //                            }

        //                            if (book.TableOfContents != null)
        //                            {
        //                                volume = String.Format("Obsah {0}", book.Volume).Trim();
        //                                pdfFileName = book.TableOfContents.OcrFileName;

        //                                if (book.TableOfContents.UseOCR)
        //                                {
        //                                    txtFileName = book.TableOfContents.TxtFileName;
        //                                }
        //                            }

        //                            string exportBook = String.Join(" | ", new string[] { book.SysNo, publication, volume, jpgFileName, pdfFileName, txtFileName });

        //                            sw.WriteLine(exportBook);
        //                            sw.Flush();
        //                            sw.Close();
        //                            sw = null;

        //                            result++;
        //                        }
        //                    }

        //                }
        //                catch
        //                {
        //                    //zapis chyby do textoveho suboru Errors.log
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApplicationException(String.Format("Nepodařilo se exportovat katalog (ID={0}) na FTP: {1}", catalogueID, ex.Message));
        //    }

        //    return result;
        //}

        public bool Delete(int catalogueID)
        {
            CatalogueRepository repository = new CatalogueRepository();
            Catalogue catalogue = repository.Select(catalogueID);

            return repository.Delete(catalogue);
        }
    }
}
