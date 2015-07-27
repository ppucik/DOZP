using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Text;
using Newtonsoft.Json;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data;
using Comdat.DOZP.Data.Business;

namespace Comdat.DOZP.Services
{
    public class DozpService : IDozpService
    {
        #region Constants
        const string CRYPTO_KEY = "0kcz,ApIv.3*";
        const string OBALKY_KNIH_URL = "www.obalkyknih.cz";
        const string OBALKY_KNIH_IMPORT = OBALKY_KNIH_URL + "/api/import";
        const string OBALKY_KNIH_CACHE = "cache.obalkyknih.cz";
        const string OBALKY_KNIH_CACHE2 = "cache2.obalkyknih.cz";
        const string ERROR_ACCESS_DENIED = "DOZP Service - Přístup byl odepřen.";
        #endregion

        #region Authentication methods

        public string ServiceInfo()
        {
            return "DOZP WCF v1.0 Beta Service Info";
        }

        public User Authenticate()
        {
            try
            {
                if (ServiceSecurityContext.Current.PrimaryIdentity.IsAuthenticated)
                {
                    string userName = ServiceSecurityContext.Current.PrimaryIdentity.Name;
                    return UserComponent.Instance.GetByName(userName);
                }
                else
                {
                    throw new FaultException<DozpServiceFault>(new DozpServiceFault("Přístup zamítnut, uživatele se nepodařilo ověřit."), ERROR_ACCESS_DENIED);
                }
            }
            catch
            {
                throw new FaultException<DozpServiceFault>(new DozpServiceFault("Přístup zamítnut, zadané jméno nebo heslo je nesprávné."), ERROR_ACCESS_DENIED);
                //LogError(DOZP_SERVICE, UserName, Environment.MachineName, ex.Message);
                //return null;
            }
        }

        #region Security methods

        private string GetUserName()
        {
            ServiceSecurityContext ssc = ServiceSecurityContext.Current;

            if (ssc != null && ssc.IsAnonymous == false && ssc.PrimaryIdentity != null)
                return ssc.PrimaryIdentity.Name.ToLower();
            else
                throw new SecurityException(ERROR_ACCESS_DENIED);
        }

        private string GetUserRole()
        {
            IPrincipal principal = Thread.CurrentPrincipal;

            if (principal != null)
            {
                if (principal.IsInRole(RoleConstants.ADMINISTRATOR))
                    return RoleConstants.ADMINISTRATOR;
                else if (principal.IsInRole(RoleConstants.SUPERVISOR))
                    return RoleConstants.SUPERVISOR;
                else if (principal.IsInRole(RoleConstants.CATALOGUER))
                    return RoleConstants.CATALOGUER;
                else if (principal.IsInRole(RoleConstants.USER_OCR))
                    return RoleConstants.USER_OCR;
                else
                    return null;
            }

            throw new SecurityException(ERROR_ACCESS_DENIED);
        }

        private bool Authorize(string role)
        {
            IPrincipal principal = Thread.CurrentPrincipal;

            if ((principal != null) && (principal.IsInRole(role)))
                return true;
            else
                throw new SecurityException(ERROR_ACCESS_DENIED);
        }

        private bool Authorize(string[] roles)
        {
            IPrincipal principal = Thread.CurrentPrincipal;

            if (principal != null)
            {
                foreach (string role in roles)
                {
                    if (principal.IsInRole(role))
                    {
                        return true;
                    }
                }
            }

            throw new SecurityException(ERROR_ACCESS_DENIED);
        }

        private bool Authorize(string[] roles, string userName)
        {
            if (userName == GetUserName())
                return true;
            else
                return Authorize(roles);
        }

        #endregion

        #endregion

        #region Public methods

        [PrincipalPermission(SecurityAction.Demand, Authenticated = true)]
        public Institution GetInstitution()
        {
            try
            {
                string userName = GetUserName();
                return InstitutionComponent.Instance.GetByUserName(userName);
            }
            catch (Exception ex)
            {
                string message = "Chyba při načtení instituce uživatele";
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Authenticated = true)]
        public Book GetBook(int bookID)
        {
            try
            {
                return BookComponent.Instance.GetByID(bookID);
            }
            catch (Exception ex)
            {
                string message = String.Format("Chyba při načtení informací o záznamu knihy (ID={0})", bookID);
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Authenticated = true)]
        public List<Book> GetBooks(BookFilter filter)
        {
            try
            {
                return BookComponent.Instance.GetList(filter);
            }
            catch (Exception ex)
            {
                string message = "Chyba při načtení záznamů knih podle filtru";
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.ADMINISTRATOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.SUPERVISOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.CATALOGUER)]
        public Book SaveBook(Book book)
        {
            if (book == null) throw new ArgumentException("Request parameter Book is null.");

            try
            {
                return BookComponent.Instance.Save(book);
            }
            catch (Exception ex)
            {
                string message = String.Format("Chyba při vytváření nového záznamu knihy v katalogu (ID={0})", book.CatalogueID);
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.ADMINISTRATOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.SUPERVISOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.CATALOGUER)]
        public bool DeleteBook(int bookID)
        {
            try
            {
                string userName = GetUserName();
                return BookComponent.Instance.Delete(bookID, userName);
            }
            catch (Exception ex)
            {
                string message = String.Format("Chyba při vymazání záznamu knihy z katalogu (ID={0})", bookID);
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Authenticated = true)]
        public Report SearchBook(SearchRequest request)
        {
            if (request == null) throw new ArgumentException("Request parameter is null.");

            try
            {
                Report result = MetadataComponent.Instance.SearchBook(request.CatalogueID, request.SysNo, request.ISBN, request.Barcode);

                if (result != null && result.Size == 0 && !String.IsNullOrEmpty(request.ISBN))
                {
                    string isbn = request.ISBN.Replace("-", "");

                    if (isbn.StartsWith("978"))
                        isbn = isbn.TrimStart("978");
                    else
                        isbn = String.Format("978{0}", isbn);

                    result = MetadataComponent.Instance.SearchBook(request.CatalogueID, request.SysNo, isbn, request.Barcode);
                }

                return result;
            }
            catch (Exception ex)
            {
                string message = String.Format("Chyba při vyhledaní čárového kódu '{0}' v katalogu (ID={1}, SysNo={2})", request.Barcode, request.CatalogueID, request.SysNo);
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        #endregion

        #region ObalkyKnihCZ

        [PrincipalPermission(SecurityAction.Demand, Authenticated = true)]
        public ObalkyKnihResponse SearchObalkyKnihCZ(ObalkyKnihRequest request)
        {
            if (request == null) throw new ArgumentException("Request parameter is null.");
            if (request.bibinfo == null) throw new ArgumentException("Request parameter 'bibinfo' is null.");

            ObalkyKnihResponse response = null;

            try
            {
                if (String.IsNullOrEmpty(request.permalink) && !String.IsNullOrEmpty(request.zserverUrl))
                {
                    request.permalink = String.Format(@"http://{0}/F?func=find-c&ccl_term=sys={1}", request.zserverUrl, request.bibinfo.sysno);
                }

                JsonSerializerSettings jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                string jsonData = JsonConvert.SerializeObject(request, Formatting.None, jsonSettings);
                string jsonUrl = String.Format(@"https://{0}/api/book?book={1}", OBALKY_KNIH_URL, Uri.EscapeDataString(jsonData));

                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add(HttpRequestHeader.Referer, request.zserverUrl);
                    Stream stream = webClient.OpenRead(jsonUrl);
                    StreamReader reader = new StreamReader(stream);
                    string responseJson = reader.ReadToEnd();
                    char[] endTrimChars = { ')', ']', ';', '\n' };
                    responseJson = responseJson.Replace("obalky.callback([", "").TrimEnd(endTrimChars);
                    response = JsonConvert.DeserializeObject<ObalkyKnihResponse>(responseJson);
                    
                    if (!String.IsNullOrEmpty(response.cover_medium_url))
                    {
                        //response.cover_cache_url = response.cover_medium_url.Replace("www", "cache");
                        response.cover_image = webClient.DownloadData(response.cover_medium_url);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("Chyba při vyhledaní publikace (SysNo={0}) na serveru ObalkyKnih.cz", request.bibinfo.sysno);
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }

            return response;
        }

        #endregion

        #region File workflow

        [PrincipalPermission(SecurityAction.Demand, Authenticated = true)]
        public List<ScanFile> GetScanFiles(ScanFileFilter filter)
        {
            try
            {
                return ScanFileComponent.Instance.GetList(filter);
            }
            catch (Exception ex)
            {
                string message = "Chyba při načtení záznamů souborů podle filtru";
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Authenticated = true)]
        public ScanImageResponse GetScanImage(ScanFileRequest request)
        {
            try
            {
                ScanImageResponse response = null;

                byte[] image = null;
                ScanFile scanFile = ScanFileComponent.Instance.GetScanImage(request.ScanFileID, ref image);

                if (scanFile != null)
                {
                    response = new ScanImageResponse();
                    response.ScanFileID = scanFile.ScanFileID;
                    response.Image = image;
                }

                return response;
            }
            catch (Exception ex)
            {
                string message = String.Format("Nepodařilo se stáhnout naskenovaný soubor (ID={0}) na lokální počítač.", request.ScanFileID);
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.ADMINISTRATOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.SUPERVISOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.USER_OCR)]
        public ScanFile GetContentsToOCR()
        {
            try
            {
                return ScanFileComponent.Instance.GetContentsToOCR(GetUserName());
            }
            catch (Exception ex)
            {
                string message = "Chyba při načtení záznamů souborů podle filtru";
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.ADMINISTRATOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.SUPERVISOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.CATALOGUER)]
        public ScanFileResponse SaveScanImage(ScanImageRequest request)
        {
            if (request == null) throw new ArgumentException("Request parameter is null.");

            try
            {
                ScanFile scanFile = null;
                string userName = GetUserName();

                if (request.ScanFileID == 0)
                    scanFile = ScanFileComponent.Instance.InsertScanImage(request.BookID, request.PartOfBook, request.UseOCR, userName, request.Computer, request.Comment, request.Image, request.ObalkyKnihCZ);
                else
                    scanFile = ScanFileComponent.Instance.UpdateScanImage(request.ScanFileID, request.UseOCR, userName, request.Computer, request.Comment, request.Image, request.ObalkyKnihCZ);

                if (scanFile != null)
                    return new ScanFileResponse(scanFile.ScanFileID, true);
                else
                    return null;
            }
            catch (Exception ex)
            {
                string message = String.Format("Chyba při ukládání naskenovaného souboru pro záznam knihy (ID={0})", request.BookID);
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.ADMINISTRATOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.SUPERVISOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.USER_OCR)]
        public ScanImageResponse CheckOutContents(ScanFileRequest request)
        {
            if (request == null) throw new ArgumentException("Request parameter is null.");

            try
            {
                string userName = GetUserName();
                byte[] image = ScanFileComponent.Instance.CheckOut(request.ScanFileID, userName, request.Computer, request.Comment);

                return new ScanImageResponse(request.ScanFileID, image);
            }
            catch (Exception ex)
            {
                string message = String.Format("Nepodařilo se stáhnout naskenovaný obsah (ID={0}) na lokální počítač.", request.ScanFileID);
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.ADMINISTRATOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.SUPERVISOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.USER_OCR)]
        public OcrFileResponse CheckInContents(OcrFileRequest request)
        {
            if (request == null) throw new ArgumentException("Request parameter is null.");

            try
            {
                string userName = GetUserName();
                ScanFileComponent.Instance.CheckIn(request.ScanFileID, userName, request.Computer, request.Comment, request.OcrText, request.PdfFile);

                return new OcrFileResponse(request.ScanFileID, true);
            }
            catch (Exception ex)
            {
                string message = String.Format("Nepodařilo se uložit zpracovaný text obsahu a PDF soubor (ID={0}) na server.", request.ScanFileID);
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.ADMINISTRATOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.SUPERVISOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.CATALOGUER)]
        public ScanFileResponse CancelOcrContents(ScanFileRequest request)
        {
            try
            {
                string userName = GetUserName();
                ScanFileComponent.Instance.CancelOcrContents(request.ScanFileID, userName, request.Computer, request.Comment);

                return new ScanFileResponse(request.ScanFileID, true);
            }
            catch (Exception ex)
            {
                string message = String.Format("Chyba při ukládání dát záznamu (ID={0})", request.ScanFileID);
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.ADMINISTRATOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.SUPERVISOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.USER_OCR)]
        public ScanFileResponse DiscardContents(ScanFileRequest request)
        {
            if (request == null) throw new ArgumentException("Request parameter is null.");

            try
            {
                string userName = GetUserName();
                ScanFileComponent.Instance.Discard(request.ScanFileID, userName, request.Computer, request.Comment);

                return new ScanFileResponse(request.ScanFileID, true);
            }
            catch (Exception ex)
            {
                string message = String.Format("Nepodařilo vyřadit záznam (ID={0}) ze zpracování.", request.ScanFileID);
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.ADMINISTRATOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.SUPERVISOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.USER_OCR)]
        public ScanFileResponse UndoContents(ScanFileRequest request)
        {
            if (request == null) throw new ArgumentException("Request parameter is null.");

            try
            {
                string userName = GetUserName();
                ScanFileComponent.Instance.UndoCheckOut(request.ScanFileID, userName, request.Computer, request.Comment);

                return new ScanFileResponse(request.ScanFileID, true);
            }
            catch (Exception ex)
            {
                string message = String.Format("Nepodařilo se vrátit zpět předcházející operaraci záznamu (ID={0}).", request.ScanFileID);
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.ADMINISTRATOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.SUPERVISOR)]
        [PrincipalPermission(SecurityAction.Demand, Role = RoleConstants.CATALOGUER)]
        public ScanFileResponse DeleteScanFile(ScanFileRequest request)
        {
            if (request == null) throw new ArgumentException("Request parameter is null.");

            try
            {
                string userName = GetUserName();
                ScanFileComponent.Instance.Delete(request.ScanFileID, userName);

                return new ScanFileResponse(request.ScanFileID, true);
            }
            catch (Exception ex)
            {
                string message = String.Format("Nepodařilo se vymazat záznam (ID={0}).", request.ScanFileID);
                throw new FaultException<DozpServiceFault>(new DozpServiceFault(message), ex.Message);
            }
        }

        #endregion

        #region Utils

        public bool LogError(string application, string machineName, string message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
