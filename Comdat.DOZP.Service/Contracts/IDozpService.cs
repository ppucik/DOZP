using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Services
{
    [ServiceContract(Name = "DozpService", Namespace = App.NAMESPACE, SessionMode = SessionMode.Allowed)]
    public interface IDozpService
    {
        /// <summary>
        /// Informace o WCF službě
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        String ServiceInfo();

        /// <summary>
        /// Ověření uživatele
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        User Authenticate();

        /// <summary>
        /// Načte instituci přihlášeného uživatele
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        Institution GetInstitution();

        /// <summary>
        /// Vyhledá publikaci podle čárového kódu
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        Report SearchBook(SearchRequest request);

        /// <summary>
        /// Vyhledá publikaci na serveru ObalkyKnih.cz
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        ObalkyKnihResponse SearchObalkyKnihCZ(ObalkyKnihRequest request);

        /// <summary>
        /// Načte informace o záznamu knihy.
        /// </summary>
        /// <param name="bookID">Indentifikátor knihy</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        Book GetBook(int bookID);

        /// <summary>
        /// Načte seznam knih podle filtru.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        List<Book> GetBooks(BookFilter filter);

        /// <summary>
        /// Vytvoří nový záznam publikace.
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        Book SaveBook(Book book);

        /// <summary>
        /// Vymaže záznam publikace.
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        bool DeleteBook(int bookID);

        /// <summary>
        /// Načte seznam souborů podle filtru.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        List<ScanFile> GetScanFiles(ScanFileFilter filter);

        /// <summary>
        /// Vrátí naskenovanou obálku nebo obsah na lokální počítač.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        ScanImageResponse GetScanImage(ScanFileRequest request);

        /// <summary>
        /// Načte první nezpracovaný obsah pro OCR zpracování.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        ScanFile GetContentsToOCR();

        /// <summary>
        /// Uloží naskenovanou obálku nebo obsah na server.
        /// </summary>
        /// <param name="request"></param>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        ScanFileResponse SaveScanImage(ScanImageRequest request);

        /// <summary>
        /// Stáhne naskenovaný obsah na lokální počítač.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        ScanImageResponse CheckOutContents(ScanFileRequest request);

        /// <summary>
        /// Uloží zpracovaný text obsahu a PDF soubor na server.
        /// </summary>
        /// <param name="request"></param>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        OcrFileResponse CheckInContents(OcrFileRequest request);

        /// <summary>
        /// Vyřadí záznam obsahu ze zpracování.
        /// </summary>
        /// <param name="request"></param>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        ScanFileResponse DiscardContents(ScanFileRequest request);

        /// <summary>
        /// Zruší OCR zpracování obsahu.
        /// </summary>
        /// <param name="request"></param>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        ScanFileResponse CancelOcrContents(ScanFileRequest request);

        /// <summary>
        /// Vrátí zpět předcházející operaraci.
        /// </summary>
        /// <param name="request"></param>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        ScanFileResponse UndoContents(ScanFileRequest request);

        /// <summary>
        /// Vymaže záznam souboru.
        /// </summary>
        /// <param name="request"></param>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        ScanFileResponse DeleteScanFile(ScanFileRequest request);

        /// <summary>
        /// Zapíše chybu do logu na serveri.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="machineName"></param>
        /// <param name="message"></param>
        [OperationContract]
        [FaultContract(typeof(DozpServiceFault))]
        bool LogError(string application, string machineName, string message);
    }
}
