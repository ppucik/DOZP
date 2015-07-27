using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using ChinhDo.Transactions;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data.Repository;

namespace Comdat.DOZP.Data.Business
{
    public class ScanFileComponent
    {
        private static readonly ScanFileComponent _instance = new ScanFileComponent();

        public static ScanFileComponent Instance
        {
            get
            {
                return _instance;
            }
        }

        public ScanFile GetByID(int scanFileID)
        {
            ScanFileRepository repository = new ScanFileRepository();
            return repository.Select(scanFileID);
        }

        public List<ScanFile> GetList(ScanFileFilter filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");

            ScanFileRepository repository = new ScanFileRepository();
            return repository.Select(filter);
        }

        public List<ScanFile> FullTextSearch(int institutionID, string text)
        {
            if (String.IsNullOrEmpty(text)) return null;

            ScanFileRepository repository = new ScanFileRepository();
            return repository.Select(institutionID, text);
        }

        public List<ScanFile> GetBySysNo(string sysno)
        {
            ScanFileFilter filter = new ScanFileFilter();
            filter.SysNo = sysno;

            ScanFileRepository repository = new ScanFileRepository();
            return repository.Select(filter);
        }

        public List<ScanFile> GetByFilter(int catalogueID, DateTime modifiedFrom, DateTime modifiedTo, string userName, short? partOfBook, short? processingMode, int? status, string sortExpression)
        {
            ScanFileFilter filter = new ScanFileFilter();
            filter.CatalogueID = catalogueID;
            filter.Modified = new DateRange(modifiedFrom, modifiedTo);
            filter.UserName = userName;
            filter.PartOfBook = (PartOfBook?)partOfBook;
            filter.UseOCR = (processingMode.HasValue ? (bool?)(processingMode.Value == (short)ProcessingMode.OCR) : null);
            filter.Status = (StatusCode?)status;

            if (sortExpression.Contains("SysNo"))
                return GetList(filter).OrderBy(o => o.Book.SysNo).ToList();
            else
                return GetList(filter).OrderBy(sortExpression).ToList();
        }

        public ScanFile GetScanImage(int scanFileID, ref byte[] image)
        {
            //kontrola vstupnich parametru
            if (scanFileID == 0)
                throw new ArgumentNullException("Neplatný parametr identifikátor souboru.");

            ScanFile result = null;

            try
            {
                ScanFileRepository repository = new ScanFileRepository();
                result = repository.Select(scanFileID);

                if (result != null)
                {
                    string filePath = result.GetScanFilePath();
                    image = ImageFunctions.ReadFile(filePath);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(String.Format("Nepodařilo se načíst naskenovaný soubor (ID={0}) z disku: {1}.", scanFileID, ex.Message));
            }

            return result;
        }

        public ScanFile GetContentsToOCR(string userName)
        {
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Neplatný parametr jméno uživatele.");

            //kontrola, jestli jiz uzivatel neco nezpracovava
            ScanFileFilter filter = new ScanFileFilter();
            filter.PartOfBook = PartOfBook.TableOfContents;
            filter.UseOCR = true;
            filter.UserName = userName;
            filter.Status = StatusCode.InProgress;

            //nacteni posledniho nezpracovaneho obsahu
            ScanFileRepository repository = new ScanFileRepository();
            ScanFile result = repository.Select(filter).FirstOrDefault();

            if (result == null)
            {
                filter.UserName = null;
                filter.Status = StatusCode.Scanned;
                result = repository.Select(filter).OrderBy(f => f.ScanFileID).FirstOrDefault();
            }

            return result;
        }

        public ScanFile InsertScanImage(int bookID, PartOfBook partOfBook, bool useOCR, string userName, string computer, string comment, byte[] image, bool obalkyKnihCZ)
        {
            //kontrola vstupnich parametru
            if (bookID == 0)
                throw new ArgumentNullException("Neplatný parametr identifikátor publikace.");

            string extension = null;

            switch (partOfBook)
            {
                case PartOfBook.FrontCover:
                    extension = FileFormat.Jpg.ToString();
                    break;
                case PartOfBook.TableOfContents:
                    extension = FileFormat.Tif.ToString();
                    break;
                default:
                    throw new ArgumentException(String.Format("Neplatný parametr '{0}' skenovaná část publikace.", partOfBook));
            }

            if (String.IsNullOrEmpty(userName))
                throw new ArgumentNullException("Neplatný parametr jméno uživatele.");

            if (String.IsNullOrEmpty(computer))
                throw new ArgumentNullException("Neplatný parametr název počítače.");

            if ((image == null) || (image.Length == 0))
                throw new ArgumentNullException("Naskenovaný obrázek je prázdný.");

            //kontrola existence publikace
            Book book = BookComponent.Instance.GetByID(bookID);
            if (book == null)
                throw new ApplicationException(String.Format("Záznam publikace (ID={0}) neexistuje.", bookID));

            if (book.HasPartOfBook(partOfBook))
                throw new ApplicationException(String.Format("Záznam publikace (ID={0}) již obsahuje část '{1}'.", bookID, partOfBook.ToDisplay()));

            //vytvoreni nazvu souboru
            ScanFile result = new ScanFile();
            result.FileName = String.Format("{0}.{1}", book.GetFileName(), extension.ToLower());

            //ulozenie souboru naskenovaneho obrazku      
            string filePath = null;

            try
            {
                filePath = Path.Combine(book.GetDirectoryPath(), result.FileName);
                result.PageCount = ImageFunctions.WriteFile(filePath, image);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(String.Format("Nepodařilo se uložit naskenovaný soubor '{0}' na disk: {1}.", filePath, ex.Message));
            }

            //ulozenie zaznamu do databaze
            ScanFileRepository repository = new ScanFileRepository();

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    result.BookID = book.BookID;
                    result.PartOfBook = partOfBook;
                    result.UseOCR = (partOfBook == PartOfBook.FrontCover ? false : useOCR);
                    result.Comment = comment.Left(1000);
                    result.Created = DateTime.Now;
                    result.Modified = result.Created;
                    result.Status = StatusCode.Scanned;
                    repository.Create(result);

                    LogOperation(result.ScanFileID, userName, computer, result.Modified, result.Comment, result.Status);

                    ts.Complete();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(String.Format("Nepodařilo se uložit data souboru publikace (ID={0}) do databáze.", bookID), ex);
                }
            }

            //operace dokonceni pro obalky a obsahy bez OCR
            switch (result.PartOfBook)
            {
                case PartOfBook.FrontCover:
                    if (obalkyKnihCZ)
                    {
                        result = ImportObalkyKnih(result.ScanFileID, userName, computer);
                    }
                    break;

                case PartOfBook.TableOfContents:
                    if (!result.UseOCR)
                    {
                        result = CompleteContents(result.ScanFileID, userName, computer);
                    }
                    break;

                default:
                    break;
            }

            return result;
        }

        public ScanFile UpdateScanImage(int scanFileID, bool useOCR, string userName, string computer, string comment, byte[] image, bool obalkyKnihCZ)
        {
            //kontrola vstupnich parametru
            if (scanFileID == 0)
                throw new ArgumentNullException("Neplatný parametr identifikátor souboru.");

            if (String.IsNullOrEmpty(userName))
                throw new ArgumentNullException("Neplatný parametr jméno uživatele.");

            if (String.IsNullOrEmpty(computer))
                throw new ArgumentNullException("Neplatný parametr název počítače.");

            if ((image == null) || (image.Length == 0))
                throw new ArgumentNullException("Naskenovaný obrázek je prázdný.");

            ScanFileRepository repository = new ScanFileRepository();

            //kontrola existence naskenovaneho souboru
            ScanFile result = repository.Select(scanFileID);
            if (result == null)
                throw new ApplicationException(String.Format("Soubor (ID={0}) neexistuje.", scanFileID));

            if (result.Status == StatusCode.Exported)
                throw new ApplicationException(String.Format("Soubor (ID={0}) má status exportován.", result.ScanFileID));

            //ulozenie souboru naskenovaneho obrazku      
            string filePath = result.GetScanFilePath();

            try
            {
                ImageFunctions.DeleteFile(filePath);
                result.PageCount = ImageFunctions.WriteFile(filePath, image);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(String.Format("Nepodařilo se uložit naskenovaný soubor '{0}' na disk: {1}.", filePath, ex.Message));
            }

            //ulozenie zaznamu do databaze
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    result.UseOCR = (result.PartOfBook == PartOfBook.FrontCover ? false : useOCR);
                    result.OcrText = null;
                    result.OcrTime = null;
                    result.Comment = comment.Left(1000);
                    result.Modified = DateTime.Now;
                    result.Status = StatusCode.Scanned;
                    repository.Update(result);

                    LogOperation(result.ScanFileID, userName, computer, result.Modified, result.Comment, result.Status);

                    ts.Complete();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(String.Format("Nepodařilo se uložit data souboru (ID={0}) do databáze.", scanFileID), ex);
                }
            }

            //operace dokonceni pro obalky a obsahy bez OCR
            switch (result.PartOfBook)
            {
                case PartOfBook.FrontCover:
                    if (obalkyKnihCZ)
                    {
                        result = ImportObalkyKnih(result.ScanFileID, userName, computer);
                    }
                    break;

                case PartOfBook.TableOfContents:
                    if (!result.UseOCR)
                    {
                        result = CompleteContents(result.ScanFileID, userName, computer);
                    }
                    break;

                default:
                    break;
            }

            return result;
        }

        public ScanFile ImportObalkyKnih(int scanFileID, string userName, string computer)
        {
            //kontrola vstupnich parametru
            if (scanFileID == 0)
                throw new ArgumentNullException("Neplatný parametr identifikátor souboru.");

            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Neplatný parametr jméno uživatele.");

            if (String.IsNullOrEmpty(computer))
                throw new ArgumentException("Neplatný parametr název počítače.");

            ScanFileRepository repository = new ScanFileRepository();

            //kontrola existence naskenovaneho souboru
            ScanFile result = repository.Select(scanFileID);
            if (result == null)
                throw new ApplicationException(String.Format("Soubor (ID={0}) neexistuje.", scanFileID));

            //kontrola ulozenych parametrov
            if (result.PartOfBook != PartOfBook.FrontCover)
                throw new ApplicationException(String.Format("Soubor (ID={0}) pro import není obálka.", result.ScanFileID));

            if (result.Status != StatusCode.Scanned)
                throw new ApplicationException(String.Format("Soubor (ID={0}) nemá status naskenován.", result.ScanFileID));

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    ObalkyKnih obalkyKnih = new ObalkyKnih("ABD001", "knihovna@ff.cuni.cz", "skenovaniobsahu1425");

                    if (obalkyKnih.Import(result))
                    {
                        result.Comment = "Importováno na ObalkyKnih.cz";
                        result.Modified = DateTime.Now;
                        result.Status = StatusCode.Complete;
                        repository.Update(result);

                        LogOperation(result.ScanFileID, userName, Environment.MachineName, result.Modified, result.Comment, result.Status);

                        ts.Complete();
                    }
                }
                catch
                {
                }
            }

            return result;
        }

        public ScanFile CompleteContents(int scanFileID, string userName, string computer)
        {
            //kontrola vstupnich parametru
            if (scanFileID == 0)
                throw new ArgumentNullException("Neplatný parametr identifikátor souboru.");

            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Neplatný parametr jméno uživatele.");

            if (String.IsNullOrEmpty(computer))
                throw new ArgumentException("Neplatný parametr název počítače.");

            ScanFileRepository repository = new ScanFileRepository();

            //kontrola existence naskenovaneho souboru
            ScanFile result = repository.Select(scanFileID);
            if (result == null)
                throw new ApplicationException(String.Format("Soubor (ID={0}) neexistuje.", scanFileID));

            //kontrola ulozenych parametrov
            if (result.PartOfBook != PartOfBook.TableOfContents)
                throw new ApplicationException(String.Format("Soubor (ID={0}) pro import není obálka.", result.ScanFileID));

            if (result.UseOCR == true)
                throw new ApplicationException(String.Format("Soubor (ID={0}) není obsah bez OCR zpracování.", result.ScanFileID));

            if (result.Status != StatusCode.Scanned)
                throw new ApplicationException(String.Format("Soubor (ID={0}) nemá status naskenován.", result.ScanFileID));

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    string filePath = result.GetScanFilePath();
                    string pdfFilePath = result.GetOcrFilePath();

                    ImageFunctions.DeleteFile(pdfFilePath);

                    if (ImageFunctions.WriteToPdf(filePath, pdfFilePath, result.Book.Author, result.Book.Title, result.Book.ISBN))
                    {
                        result.Comment = "Automaticky převedeno do PDF formátu";
                        result.Modified = DateTime.Now;
                        result.Status = StatusCode.Complete;
                        repository.Update(result);

                        LogOperation(result.ScanFileID, userName, Environment.MachineName, result.Modified, result.Comment, result.Status);

                        ts.Complete();
                    }
                }
                catch
                {
                }
            }

            return result;
        }

        public byte[] CheckOut(int scanFileID, string userName, string computer, string comment)
        {
            //kontrola vstupnich parametru
            if (scanFileID == 0)
                throw new ArgumentException("Neplatný parametr identifikátor souboru.");

            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Neplatný parametr jméno uživatele.");

            if (String.IsNullOrEmpty(computer))
                throw new ArgumentException("Neplatný parametr název počítače.");

            ScanFileRepository repository = new ScanFileRepository();

            //kontrola, jestli uzivatel jiz neco nezpracovava
            ScanFileFilter filter = new ScanFileFilter() { UserName = userName, Status = StatusCode.InProgress };
            if (repository.Select(filter).Count > 0)
                throw new ApplicationException(String.Format("Nelze stáhnout naskenovaný obsah, uživatel '{0}' již zpracovává obsah.", userName));

            //kontrola existence naskenovaneho souboru
            ScanFile result = repository.Select(scanFileID);
            if (result == null)
                throw new ApplicationException(String.Format("Soubor (ID={0}) neexistuje.", scanFileID));

            //kontrola ulozenych parametrov
            if (result.PartOfBook != PartOfBook.TableOfContents)
                throw new ApplicationException(String.Format("Soubor (ID={0}) není typ pro OCR zpracování.", result.ScanFileID));

            if (result.UseOCR == false)
                throw new ApplicationException(String.Format("Soubor (ID={0}) není určen pro OCR zpracování.", result.ScanFileID));

            if (result.Status != StatusCode.Scanned)
                throw new ApplicationException(String.Format("Soubor (ID={0}) nemá status naskenován.", result.ScanFileID));

            //nacitanie souboru naskenovaneho obsahu
            byte[] image = null;
            string filePath = null;

            try
            {
                filePath = result.GetScanFilePath();
                image = ImageFunctions.ReadFile(filePath);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(String.Format("Nepodařilo se načíst naskenovaný soubor '{0}' z disku: {1}.", filePath, ex.Message));
            }

            //ulozenie operace do databazy
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    result.Modified = DateTime.Now;
                    result.Comment = comment.Left(1000);
                    result.Status = StatusCode.InProgress;
                    repository.Update(result);

                    LogOperation(result.ScanFileID, userName, computer, result.Modified, result.Comment, result.Status);

                    ts.Complete();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(String.Format("Nepodařilo se uložit data souboru (ID={0}) do databáze.", scanFileID), ex);
                }
            }

            return image;
        }

        public void CheckIn(int scanFileID, string userName, string computer, string comment, string ocrText, byte[] pdfFile)
        {
            //kontrola vstupnich parametru
            if (scanFileID == 0)
                throw new ArgumentException("Neplatný parametr identifikátor souboru.");

            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Neplatný parametr jméno uživatele.");

            if (String.IsNullOrEmpty(computer))
                throw new ArgumentException("Neplatný parametr název počítače.");

            if (String.IsNullOrEmpty(ocrText))
                throw new ArgumentNullException("Zpracovaný OCR text je prázdný.");

            if ((pdfFile == null) || (pdfFile.Length == 0))
                throw new ArgumentNullException("Zpracovaný PDF soubor je prázdný.");

            ScanFileRepository repository = new ScanFileRepository();

            //kontrola existence naskenovaneho souboru
            ScanFile result = repository.Select(scanFileID);
            if (result == null)
                throw new ApplicationException(String.Format("Soubor (ID={0}) neexistuje.", scanFileID));

            //kontrola ulozenych parametrov + CheckOutBook->UserName ???
            if (result.PartOfBook != PartOfBook.TableOfContents)
                throw new ApplicationException(String.Format("Soubor (ID={0}) není typ pro OCR zpracování.", result.ScanFileID));

            if (result.UseOCR == false)
                throw new ApplicationException(String.Format("Soubor (ID={0}) není určen pro OCR zpracování.", result.ScanFileID));

            if (result.Status != StatusCode.InProgress)
                throw new ApplicationException(String.Format("Soubor (ID={0}) nemá status ve zpracování.", result.ScanFileID));

            //ulozenie PDF suboru vytvoreneho klientom
            string filePath = null;

            try
            {
                filePath = result.GetOcrFilePath();
                ImageFunctions.WriteFile(filePath, pdfFile);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(String.Format("Nepodařilo se uložit zpracovaný OCR soubor '{0}' na disk: {1}.", filePath, ex.Message));
            }

            //ulozenie operace do databazy
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    TimeSpan tsOcrTime = DateTime.Now.Subtract(result.Modified);
                    result.OcrTime = (short)tsOcrTime.TotalSeconds;
                    result.OcrText = ocrText;
                    result.Modified = DateTime.Now;
                    result.Comment = comment.Left(1000);
                    result.Status = StatusCode.Complete;
                    repository.Update(result);

                    LogOperation(result.ScanFileID, userName, computer, result.Modified, result.Comment, result.Status);

                    ts.Complete();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(String.Format("Nepodařilo se uložit data souboru (ID={0}) do databáze.", scanFileID), ex);
                }
            }
        }

        public void Discard(int scanFileID, string userName, string computer, string comment)
        {
            //kontrola vstupnich parametru
            if (scanFileID == 0)
                throw new ArgumentException("Neplatný parametr identifikátor souboru.");

            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Neplatný parametr jméno uživatele.");

            if (String.IsNullOrEmpty(computer))
                throw new ArgumentException("Neplatný parametr název počítače.");

            ScanFileRepository repository = new ScanFileRepository();

            //kontrola existence naskenovaneho souboru
            ScanFile result = repository.Select(scanFileID);
            if (result == null)
                throw new ApplicationException(String.Format("Soubor (ID={0}) neexistuje.", scanFileID));

            //kontrola ulozenych parametrov + kontrola CheckOutBook UserName
            if (result.PartOfBook != PartOfBook.TableOfContents)
                throw new ApplicationException(String.Format("Soubor (ID={0}) není typ pro OCR zpracování.", result.ScanFileID));

            if (result.UseOCR == false)
                throw new ApplicationException(String.Format("Soubor (ID={0}) není určen pro OCR zpracování.", result.ScanFileID));

            if (result.Status != StatusCode.InProgress)
                throw new ApplicationException(String.Format("Soubor (ID={0}) nemá status ve zpracování.", result.ScanFileID));

            //ulozenie operace do databazy
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    result.Modified = DateTime.Now;
                    result.Comment = comment.Left(1000);
                    result.Status = StatusCode.Discarded;
                    repository.Update(result);

                    LogOperation(result.ScanFileID, userName, computer, result.Modified, result.Comment, result.Status);

                    ts.Complete();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(String.Format("Nepodařilo se uložit data souboru (ID={0}) do databáze.", scanFileID), ex);
                }
            }
        }

        public ScanFile CancelOcrContents(int scanFileID, string userName, string computer, string comment)
        {
            //kontrola vstupnich parametru
            if (scanFileID == 0)
                throw new ArgumentNullException("Neplatný parametr identifikátor souboru.");

            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Neplatný parametr jméno uživatele.");

            if (String.IsNullOrEmpty(computer))
                throw new ArgumentException("Neplatný parametr název počítače.");

            ScanFileRepository repository = new ScanFileRepository();

            //kontrola existence naskenovaneho souboru
            ScanFile result = repository.Select(scanFileID);
            if (result == null)
                throw new ApplicationException(String.Format("Soubor (ID={0}) neexistuje.", scanFileID));

            if (result.PartOfBook != PartOfBook.TableOfContents)
                throw new ApplicationException(String.Format("Soubor (ID={0}) není typ pro OCR zpracování.", result.ScanFileID));

            if (result.UseOCR == false)
                throw new ApplicationException(String.Format("Soubor (ID={0}) není určen pro OCR zpracování.", result.ScanFileID));

            if (result.Status != StatusCode.Discarded)
                throw new ApplicationException(String.Format("Soubor (ID={0}) nemá status vyřazen.", result.ScanFileID));

            //ulozenie zaznamu do databaze
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    result.UseOCR = false;
                    result.OcrText = null;
                    result.OcrTime = null;
                    result.Comment = comment.Left(1000);
                    result.Modified = DateTime.Now;
                    result.Status = StatusCode.Scanned;
                    repository.Update(result);

                    LogOperation(result.ScanFileID, userName, computer, result.Modified, result.Comment, result.Status);

                    result = CompleteContents(result.ScanFileID, userName, computer);

                    ts.Complete();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(String.Format("Nepodařilo se uložit data souboru (ID={0}) do databáze.", scanFileID), ex);
                }
            }

            return result;
        }

        public void UndoCheckOut(int scanFileID, string userName, string computer, string comment)
        {
            //kontrola vstupnich parametru
            if (scanFileID == 0)
                throw new ArgumentException("Neplatný parametr identifikátor souboru.");

            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Neplatný parametr jméno uživatele.");

            if (String.IsNullOrEmpty(computer))
                throw new ArgumentException("Neplatný parametr název počítače.");

            ScanFileRepository repository = new ScanFileRepository();

            //kontrola existence naskenovaneho souboru
            ScanFile result = repository.Select(scanFileID);
            if (result == null)
                throw new ApplicationException(String.Format("Soubor (ID={0}) neexistuje.", scanFileID));

            //kontrola ulozenych parametrov
            if (result.PartOfBook != PartOfBook.TableOfContents)
                throw new ApplicationException(String.Format("Soubor (ID={0}) není typ pro OCR zpracování.", result.ScanFileID));

            if (result.UseOCR == false)
                throw new ApplicationException(String.Format("Soubor (ID={0}) není určen pro OCR zpracování.", result.ScanFileID));

            if (result.Status != StatusCode.InProgress)
                throw new ApplicationException(String.Format("Soubor (ID={0}) nemá status ve zpracování.", result.ScanFileID));

            Operation lastOperation = result.Operations.LastOrDefault();

            if (result.Status != lastOperation.Status)
                throw new ApplicationException(String.Format("Soubor (ID={0}) nemá poslední operaci ve zpracování.", result.ScanFileID));

            //ulozenie operace do databazy
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    OperationRepository operations = new OperationRepository();
                    operations.Delete(lastOperation);

                    Operation scanOperation = result.Operations.SingleOrDefault(o => o.Status == StatusCode.Scanned);
                    result.Modified = scanOperation.Executed;
                    result.Comment = comment;
                    result.Status = StatusCode.Scanned;
                    repository.Update(result);

                    ts.Complete();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(String.Format("Nepodařilo se uložit data souboru (ID={0}) do databáze.", scanFileID), ex);
                }
            }
        }

        public void Export(int scanFileID, string userName, string computer, string comment = null)
        {
            //kontrola vstupnich parametru
            if (scanFileID == 0)
                throw new ArgumentException("Neplatný parametr identifikátor souboru.");

            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Neplatný parametr jméno uživatele.");

            if (String.IsNullOrEmpty(computer))
                throw new ArgumentException("Neplatný parametr název počítače.");

            ScanFileRepository repository = new ScanFileRepository();

            //kontrola existence naskenovaneho souboru
            ScanFile result = repository.Select(scanFileID);
            if (result == null)
                throw new ApplicationException(String.Format("Soubor (ID={0}) neexistuje.", scanFileID));

            //kontrola ulozenych parametrov
            if (result.Status != StatusCode.Complete)
                throw new ApplicationException(String.Format("Soubor (ID={0}) nemá status dokončeno.", result.ScanFileID));

            //export ALEPH
            TxFileManager fileMgr = new TxFileManager();
            string filePath = null;
            string ftpPath = null;

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    filePath = result.GetScanFilePath();

                    switch (result.PartOfBook)
                    {
                        case PartOfBook.FrontCover:
                            ftpPath = Path.Combine(result.Book.Catalogue.GetDirectoryFTP(true), result.FileName);
                            fileMgr.Copy(filePath, ftpPath, true);
                            break;

                        case PartOfBook.TableOfContents:
                            if (result.UseOCR)
                            {
                                string txtFilePath = Path.Combine(result.Book.Catalogue.GetDirectoryFTP(true), result.TxtFileName);
                                fileMgr.WriteAllText(txtFilePath, result.OcrText);
                            }

                            string pdfFilePath = result.GetOcrFilePath();
                            ftpPath = Path.Combine(result.Book.Catalogue.GetDirectoryFTP(true), result.OcrFileName);
                            fileMgr.Copy(pdfFilePath, ftpPath, true);
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(String.Format("Nepodařilo se exportovat soubor '{0}' na FTP: {1}.", filePath, ex.Message));
                }

                //ulozenie operace do databazy
                try
                {
                    result.Modified = DateTime.Now;
                    result.Comment = (comment != null ? comment.Left(1000) : null);
                    result.Status = StatusCode.Exported;
                    repository.Update(result);

                    LogOperation(result.ScanFileID, userName, computer, result.Modified, result.Comment, result.Status);

                    ts.Complete();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(String.Format("Nepodařilo se uložit data souboru (ID={0}) do databáze.", scanFileID), ex);
                }
            }
        }

        //[Obsolete]
        //public void Export2(int scanFileID, string ftpPath)
        //{
        //    //kontrola vstupnich parametru
        //    if (scanFileID == 0)
        //        throw new ArgumentException("Neplatný parametr identifikátor souboru.");

        //    ScanFileRepository repository = new ScanFileRepository();

        //    //kontrola existence naskenovaneho souboru
        //    ScanFile result = repository.Select(scanFileID);
        //    if (result == null)
        //        throw new ApplicationException(String.Format("Soubor (ID={0}) neexistuje.", scanFileID));

        //    //kontrola ulozenych parametrov
        //    if (result.Status != StatusCode.Exported)
        //        throw new ApplicationException(String.Format("Soubor (ID={0}) nemá status exportováno.", result.ScanFileID));

        //    //export ALEPH
        //    TxFileManager fileMgr = new TxFileManager();
        //    string filePath = null;

        //    try
        //    {
        //        filePath = result.GetScanFilePath();
        //        string ftpFilePath = null;

        //        switch (result.PartOfBook)
        //        {
        //            case PartOfBook.FrontCover:
        //                ftpFilePath = Path.Combine(ftpPath, result.FileName);
        //                fileMgr.Copy(filePath, ftpFilePath, true);
        //                break;

        //            case PartOfBook.TableOfContents:
        //                if (result.UseOCR)
        //                {
        //                    string txtFilePath = Path.Combine(ftpPath, result.TxtFileName);
        //                    fileMgr.WriteAllText(txtFilePath, result.OcrText);
        //                }

        //                string pdfFilePath = result.GetOcrFilePath();
        //                ftpFilePath = Path.Combine(ftpPath, result.OcrFileName);
        //                fileMgr.Copy(pdfFilePath, ftpFilePath, true);
        //                break;

        //            default:
        //                break;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApplicationException(String.Format("Nepodařilo se exportovat soubor '{0}' na FTP: {1}.", filePath, ex.Message));
        //    }
        //}

        public void Delete(int scanFileID, string userName)
        {
            //kontrola vstupnich parametru
            if (scanFileID == 0)
                throw new ArgumentException("Neplatný parametr identifikátor souboru.");

            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Neplatný parametr jméno uživatele.");

            ScanFileRepository repository = new ScanFileRepository();

            //kontrola existence naskenovaneho souboru
            ScanFile result = repository.Select(scanFileID);
            if (result == null)
                throw new ApplicationException(String.Format("Soubor (ID={0}) neexistuje.", scanFileID));

            //kontrola ulozenych parametrov
            if (result.Status == StatusCode.Exported)
                throw new ApplicationException(String.Format("Soubor (ID={0}) má status exportován.", result.ScanFileID));

            //vymazanie suboru naskenovaneho obrazku
            TxFileManager fileMgr = new TxFileManager();
            string filePath = null;

            //ulozenie operace do databazy
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    filePath = result.GetScanFilePath();
                    if (fileMgr.FileExists(filePath)) fileMgr.Delete(filePath);

                    filePath = result.GetOcrFilePath();
                    if (fileMgr.FileExists(filePath)) fileMgr.Delete(filePath);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(String.Format("Nepodařilo se vymazat soubor '{0}' z disku: {1}.", filePath, ex.Message));
                }

                try
                {
                    repository.Delete(result);

                    Book book = BookComponent.Instance.GetByID(result.BookID);
                    if (book != null && (book.ScanFiles == null || book.ScanFiles.Count == 0))
                    {
                        BookComponent.Instance.Delete(result.BookID, userName);
                    }

                    ts.Complete();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(String.Format("Nepodařilo se vymazat data souboru (ID={0}) z databáze.", scanFileID), ex);
                }
            }
        }

        #region Statistics

        public FileSumList GetStatistics(StatisticsType type,
            int catalogueID,
            short? partOfBook,
            short? processingMode,
            int? modifiedYear,
            int? modifiedMonth,
            int? modifiedDay,
            string userName,
            int? status)
        {
            OperationRepository repository = new OperationRepository();

            StatisticsFilter filter = new StatisticsFilter(StatisticsType.TimePeriod);
            filter.CatalogueID = catalogueID;
            filter.PartOfBook = (PartOfBook?)partOfBook;
            filter.UseOCR = (processingMode.HasValue ? (bool?)(processingMode.Value == (short)ProcessingMode.OCR) : null);
            filter.Year = modifiedYear;
            filter.Month = modifiedMonth;
            filter.Day = modifiedDay;
            filter.UserName = userName;
            filter.Status = (StatusCode?)status;

            switch (type)
            {
                case StatisticsType.TimePeriod:
                    return repository.GetTimeStatistics(filter);
                case StatisticsType.Users:
                    return repository.GetUserStatistics(filter);
                case StatisticsType.Catalogues:
                    return null;
                default:
                    return null;
            }
        }

        #endregion

        #region Private methods

        //private byte[] GetScanImage(int scanFileID)
        //{
        //    byte[] image = null;

        //    try
        //    {
        //        ScanFileRepository repository = new ScanFileRepository();
        //        ScanFile scanFile = repository.Select(scanFileID);

        //        if (scanFile != null)
        //        {
        //            string filePath = scanFile.GetScanFilePath();
        //            image = ImageFunctions.ReadFile(filePath);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApplicationException(String.Format("Nepodařilo se načíst naskenovaný soubor (ID={0}) z disku: {1}.", scanFileID, ex.Message));
        //    }

        //    return image;
        //}

        private Operation LogOperation(int scanFileID, string userName, string computer, DateTime executed, string comment, StatusCode status)
        {
            Operation operation = new Operation();
            operation.ScanFileID = scanFileID;
            operation.UserName = userName;  //prihlaseny uzivatel
            operation.Computer = computer;  //pocitac uzivatele
            operation.Executed = executed;  //datum zmeny
            operation.Comment = comment;    //popis zpracovani
            operation.Status = status;      //stav zpracovani

            OperationRepository repository = new OperationRepository();
            return repository.Create(operation);
        }

        #endregion
    }
}
