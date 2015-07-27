using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;

using Comdat.DOZP.Core;
using Comdat.DOZP.Process.DozpService;

namespace Comdat.DOZP.Process
{
    public class DozpController
    {
        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Institution GetInstitution()
        {
            return AuthController.GetProxy().Execute(client => client.GetInstitution());
        }

        /// <summary>
        /// Zapíše chybu do logu na server.
        /// </summary>
        /// <param name="message">Chybová zpráva zobrazená uživateli</param>
        /// <param name="exception"></param>
        public static bool LogError(string message, Exception exception = null)
        {
            string appname = Assembly.GetExecutingAssembly().GetName().Name;
            string computer = Environment.MachineName;
            if (exception != null) message += String.Format("\nError: {0}", exception.Message);

            return AuthController.GetProxy().Execute(client => client.LogError(appname, computer, message));
        }

        #endregion

        #region Scan methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookID"></param>
        /// <returns></returns>
        public static Book GetBook(int bookID)
        {
            return AuthController.GetProxy().Execute(client => client.GetBook(bookID));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sysno"></param>
        /// <returns></returns>
        public static List<Book> GetBooks(int catalogueID, string sysno, string isbn = null)
        {
            BookFilter filter = new BookFilter();
            filter.CatalogueID = catalogueID;
            filter.SysNo = sysno;
            filter.ISBN = isbn;

            return AuthController.GetProxy().Execute(client => client.GetBooks(filter));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scanFileID"></param>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static bool GetScanImage(int scanFileID, string fullName)
        {
            if (scanFileID == 0) throw new ArgumentException("Nebyl zadán identifikátor souboru", "scanFileID");
            if (String.IsNullOrEmpty(fullName)) throw new ArgumentException("Nebyla zadána cesta k uložení souboru", "fullName");
            if (System.IO.File.Exists(fullName)) System.IO.File.Delete(fullName);

            bool result = false;

            ScanFileRequest request = new ScanFileRequest();
            request.ScanFileID = scanFileID;
            request.Computer = Environment.MachineName;
            ScanImageResponse response = AuthController.GetProxy().Execute(client => client.GetScanImage(request));

            if (response != null)
            {
                result = ImageFunctions.WriteFile(fullName, response.Image, true) > 0;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public static Book SaveBook(Book book)
        {
            return AuthController.GetProxy().Execute(client => client.SaveBook(book));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public static bool DeleteBook(int bookID)
        {
            return AuthController.GetProxy().Execute(client => client.DeleteBook(bookID));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="catalogueID"></param>
        /// <param name="barcode"></param>
        /// <returns></returns>
        public static Report SearchBook(int catalogueID, string sysno = null, string isbn = null, string barcode = null)
        {
            SearchRequest request = new SearchRequest();
            request.CatalogueID = catalogueID;
            request.SysNo = sysno;
            request.ISBN = isbn;
            request.Barcode = barcode;

            return AuthController.GetProxy().Execute(client => client.SearchBook(request));
        }

        /// <summary>
        /// Načte vyřazené obsahy katalogu pri OCR zpracování.
        /// </summary>
        /// <param name="catalogueID"></param>
        /// <returns></returns>
        public static List<ScanFile> GetDiscardContents(int catalogueID, string userName)
        {
            ScanFileFilter filter = new ScanFileFilter();
            filter.CatalogueID = catalogueID;
            filter.UserName = userName;
            filter.Status = StatusCode.Discarded;
            List<ScanFile> files = AuthController.GetProxy().Execute(client => client.GetScanFiles(filter));

            return (files != null ? files.OrderBy(f => f.Modified).ToList() : null);
        }

        /// <summary>
        /// Uloží data obálky nebo obsahu na server.
        /// </summary>
        public static bool CancelOcrContents(int scanFileID, string comment = null)
        {
            if (scanFileID == 0) throw new ArgumentException("Nebyl zadán identifikátor obsahu", "scanFileID");

            ScanFileRequest request = new ScanFileRequest();
            request.ScanFileID = scanFileID;
            request.Computer = Environment.MachineName;
            request.Comment = comment;
            ScanFileResponse response = AuthController.GetProxy().Execute(client => client.CancelOcrContents(request));

            return response.Result;
        }

        /// <summary>
        /// Uloží naskenovanou obálku nebo obsah na server.
        /// </summary>
        public static bool InsertScanImage(int bookID, PartOfBook partOfBook, bool useOCR, string fullName, string comment = null, bool obalkyKnihCZ = false)
        {
            if (bookID == 0) throw new ArgumentException("Nebyl zadán identifikátor knihy", "bookID");
            if (String.IsNullOrEmpty(fullName)) throw new ArgumentNullException("Nebyla zadána cesta k souboru", "fullName");

            ScanImageRequest request = new ScanImageRequest();
            request.BookID = bookID;
            request.PartOfBook = partOfBook;
            request.UseOCR = useOCR;
            request.Computer = Environment.MachineName;
            request.Image = ImageFunctions.ReadFile(fullName);
            request.Comment = comment;
            request.ObalkyKnihCZ = obalkyKnihCZ;
            ScanFileResponse response = AuthController.GetProxy().Execute(client => client.SaveScanImage(request));

            return response.Result;
        }

        /// <summary>
        /// Uloží aktualizovanou obálku nebo obsah na server.
        /// </summary>
        /// <param name="scanFile"></param>
        /// <param name="fullName"></param>
        /// <param name="obalkyKnihCZ"></param>
        /// <returns></returns>
        public static bool UpdateScanImage(ScanFile scanFile, string fullName, bool obalkyKnihCZ = false)
        {
            if (scanFile == null) throw new ArgumentNullException("scanFile");
            if (String.IsNullOrEmpty(fullName)) throw new ArgumentNullException("Nebyla zadána cesta k souboru", "fullName");

            ScanImageRequest request = new ScanImageRequest();
            request.ScanFileID = scanFile.ScanFileID;
            request.BookID = scanFile.BookID;
            request.PartOfBook = scanFile.PartOfBook;
            request.UseOCR = scanFile.UseOCR;
            request.Computer = Environment.MachineName;
            request.Image = ImageFunctions.ReadFile(fullName);
            request.Comment = scanFile.Comment;
            request.ObalkyKnihCZ = obalkyKnihCZ;
            ScanFileResponse response = AuthController.GetProxy().Execute(client => client.SaveScanImage(request));

            return response.Result;
        }


        /// <summary>
        /// Vymaže záznam obálky nebo obsahu.
        /// </summary>
        /// <param name="scanFileID">Identifikátor obsahu</param>
        /// <param name="comment">Komentář</param>  
        /// <returns></returns>
        public static bool DeleteScanFile(int scanFileID, string comment = null)
        {
            if (scanFileID == 0) throw new ArgumentException("Nebyl zadán identifikátor souboru", "scanFileID");

            ScanFileRequest request = new ScanFileRequest();
            request.ScanFileID = scanFileID;
            request.Computer = Environment.MachineName;
            request.Comment = comment;
            ScanFileResponse response = AuthController.GetProxy().Execute(client => client.DeleteScanFile(request));

            return response.Result;
        }

        #endregion

        #region OCR methods

        /// <summary>
        /// Načte první nezpracovaný obsah pro OCR zpracování.
        /// </summary>
        /// <returns>Nezpracovaný obsah</returns>
        public static ScanFile GetContentsToOCR()
        {
            return AuthController.GetProxy().Execute(client => client.GetContentsToOCR());
        }

        /// <summary>
        /// Načte nezpracované obsahy katalogu pro OCR zpracování.
        /// </summary>
        /// <param name="catalogueID"></param>
        /// <returns>Nezpracované obsahy</returns>
        public static List<ScanFile> GetUnprocessedContents(int catalogueID)
        {
            ScanFileFilter filter = new ScanFileFilter();
            filter.CatalogueID = catalogueID;
            filter.PartOfBook = PartOfBook.TableOfContents;
            filter.UseOCR = true;
            filter.Status = StatusCode.Scanned;
            List<ScanFile> files = AuthController.GetProxy().Execute(client => client.GetScanFiles(filter));

            return (files != null ? files.OrderBy(f => f.Created).ToList() : null);
        }

        /// <summary>
        /// Načte aktuálně zpracovávaný OCR obsah.
        /// </summary>
        /// <returns>Záznam obsahu</returns>
        public static ScanFile GetCheckOutContents()
        {
            ScanFileFilter filter = new ScanFileFilter();
            filter.UserName = AuthController.UserIdentity.LoginUser.UserName;
            filter.PartOfBook = PartOfBook.TableOfContents;
            filter.UseOCR = true;
            filter.Status = StatusCode.InProgress;
            List<ScanFile> files = AuthController.GetProxy().Execute(client => client.GetScanFiles(filter));

            return (files != null ? files.FirstOrDefault() : null);
        }

        /// <summary>
        /// Stáhne naskenovaný obsah na lokální počítač.
        /// </summary>
        /// <param name="scanFileID">Identifikátor obsahu</param>
        /// <param name="tifPath">Cesta souboru pro uložení naskenovaného TIF souboru</param>
        /// <param name="comment">Komentář</param>
        /// <returns></returns>
        public static bool CheckOutContents(int scanFileID, string tifPath, string comment = null)
        {
            if (scanFileID == 0) throw new ArgumentException("Nebyl zadán identifikátor obsahu", "scanFileID");
            if (String.IsNullOrEmpty(tifPath)) throw new ArgumentException("Nebyla zadána cesta k uložení souboru", "tifPath");
            if (System.IO.File.Exists(tifPath)) System.IO.File.Delete(tifPath);

            bool result = false;

            ScanFileRequest request = new ScanFileRequest();
            request.ScanFileID = scanFileID;
            request.Computer = Environment.MachineName;
            request.Comment = comment;
            ScanImageResponse response = AuthController.GetProxy().Execute(client => client.CheckOutContents(request));

            if (response != null)
            {
                result = ImageFunctions.WriteFile(tifPath, response.Image, true) > 0;
            }

            return result;
        }

        /// <summary>
        /// Uloží zpracovaný text obsahu a PDF soubor na server.
        /// </summary>
        /// <param name="scanFileID">Identifikátor obsahu</param>
        /// <param name="txtPath">Zpracovaný OCR text</param>
        /// <param name="pdfPath">Cesta k PDF souboru</param>
        /// <param name="comment">Komentář</param>
        /// <returns></returns>
        public static bool CheckInContents(int scanFileID, string ocrText, string pdfPath, string comment = null)
        {
            if (scanFileID == 0) throw new ArgumentException("Nebyl zadán identifikátor obsahu", "scanFileID");
            if (String.IsNullOrEmpty(ocrText)) throw new ArgumentException("Nebyl zadán zpracovaný OCR text", "ocrText");
            if (String.IsNullOrEmpty(pdfPath)) throw new ArgumentException("Nebyla zadána cesta k PDF souboru", "pdfPath");
            if (!System.IO.File.Exists(pdfPath)) throw new ArgumentException("Zadaná cesta k PDF souboru neexistuje", "pdfPath");

            OcrFileRequest request = new OcrFileRequest();
            request.ScanFileID = scanFileID;
            request.Computer = Environment.MachineName;
            request.OcrText = ocrText;
            request.PdfFile = ImageFunctions.ReadFile(pdfPath);
            request.Comment = comment;
            OcrFileResponse response = AuthController.GetProxy().Execute(client => client.CheckInContents(request));

            return response.Result;
        }

        /// <summary>
        /// Vyřadí záznam obsahu ze zpracování.
        /// </summary>
        /// <param name="scanFileID">Identifikátor obsahu</param>
        /// <param name="comment">Komentář</param>
        /// <returns></returns>
        public static bool DiscardContents(int scanFileID, string comment = null)
        {
            if (scanFileID == 0) throw new ArgumentException("Nebyl zadán identifikátor obsahu", "scanFileID");

            ScanFileRequest request = new ScanFileRequest();
            request.ScanFileID = scanFileID;
            request.Computer = Environment.MachineName;
            request.Comment = comment;
            ScanFileResponse response = AuthController.GetProxy().Execute(client => client.DiscardContents(request));

            return response.Result;
        }

        /// <summary>
        /// Vrátí zpět předcházející operaraci zpracování.  
        /// </summary>
        /// <param name="scanFileID">Identifikátor obsahu</param>
        /// <param name="comment">Komentář</param>                                                                                                                                                                                  
        /// <returns></returns>
        public static bool UndoContents(int scanFileID, string comment = null)
        {
            if (scanFileID == 0) throw new ArgumentException("Nebyl zadán identifikátor obsahu", "scanFileID");

            ScanFileRequest request = new ScanFileRequest();
            request.ScanFileID = scanFileID;
            request.Computer = Environment.MachineName;
            request.Comment = comment;
            ScanFileResponse response = AuthController.GetProxy().Execute(client => client.UndoContents(request));

            return response.Result;
        }

        #endregion

        #region ObalkyKnih.CZ

        /// <summary>
        /// 
        /// </summary>
        /// <param name="catalogueID"></param>
        /// <param name="sysno"></param>
        /// <param name="isbn"></param>
        /// <returns></returns>
        public static string SearchCoverUrl(string zserverUrl, Book book)
        {
            if (book == null) throw new ArgumentNullException("book");

            string frontCoverUrl = null;

            ObalkyKnihRequest request = new ObalkyKnihRequest();
            request.zserverUrl = zserverUrl;

            ObalkyKnihBibInfo bibinfo = new ObalkyKnihBibInfo();
            bibinfo.sysno = book.SysNo;
            bibinfo.authors = new List<string>() { book.Author };
            bibinfo.title = book.Title;
            bibinfo.year = book.Year;
            bibinfo.isbn = book.ISBN;
            bibinfo.nbn = book.NBN;
            bibinfo.oclc = book.OCLC;
            request.bibinfo = bibinfo;

            ObalkyKnihResponse response = AuthController.GetProxy().Execute(client => client.SearchObalkyKnihCZ(request));           
            
            if (response != null)
            {
                frontCoverUrl = response.cover_medium_url;
            }

            return frontCoverUrl;
        }

        public static bool SearchCoverOK(string zserverUrl, Book book, string fullName)
        {
            if (book == null) throw new ArgumentNullException("book");
            if (String.IsNullOrEmpty(fullName)) throw new ArgumentNullException("Nebyla zadána cesta k souboru", "fullName");

            bool result = false;

            ObalkyKnihRequest request = new ObalkyKnihRequest();
            request.zserverUrl = zserverUrl;

            ObalkyKnihBibInfo bibinfo = new ObalkyKnihBibInfo();
            bibinfo.sysno = book.SysNo;
            bibinfo.authors = new List<string>() { book.Author };
            bibinfo.title = book.Title;
            bibinfo.year = book.Year;
            bibinfo.isbn = book.ISBN;
            bibinfo.nbn = book.NBN;
            bibinfo.oclc = book.OCLC;
            request.bibinfo = bibinfo;

            ObalkyKnihResponse response = AuthController.GetProxy().Execute(client => client.SearchObalkyKnihCZ(request));

            if (response != null && response.cover_image != null && response.cover_image.Length > 0)
            {
                result = (ImageFunctions.WriteFile(fullName, response.cover_image, true) > 0);
            }

            return result;
        }

        #endregion
    }
}
