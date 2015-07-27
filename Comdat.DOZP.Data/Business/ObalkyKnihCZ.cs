using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data
{
    public class ObalkyKnih
    {
        public const string OBALKY_KNIH_IMPORT = "www.obalkyknih.cz/api/import";

        private string _sigla;
        private string _login;
        private string _password;

        public enum PostDataContentType
        {
            Data = 0,
            File = 1,
            JPG,
            TIFF,
            PDF,
            XML
        }

        public ObalkyKnih(string sigla, string login, string password)
        {
            this.Sigla = sigla;
            this.Login = login;
            this.Password = password;
        }

        public string Sigla
        {
            get { return _sigla; }
            private set { _sigla = value; }
        }

        public string Login
        {
            get { return _login; }
            private set { _login = value; }
        }

        public string Password
        {
            get { return _password; }
            private set { _password = value; }
        }

        public bool Import(ScanFile scanFile)
        {
//#if DEBUG
//            return true;
//#endif
            try
            {
                //Import URL ObalkyKnih.cz
                string importUrl = String.Format("https://{0}", OBALKY_KNIH_IMPORT);
                string custom = String.Format("{0}-{1}", this.Sigla, scanFile.Book.SysNo);

                HttpWebRequest requestToServer = (HttpWebRequest)WebRequest.Create(importUrl);
                requestToServer.ContentType = String.Format("multipart/form-data; boundary={0}", PostData.Boundary);
                requestToServer.Method = WebRequestMethods.Http.Post;
                requestToServer.AllowWriteStreamBuffering = false;
                requestToServer.KeepAlive = false;
                requestToServer.Timeout = 60000;

                // Text parameters
                PostData pData = new PostData();
                pData.AddDataParam("login", this.Login);
                pData.AddDataParam("password", this.Password);
                pData.AddDataParam("isbn", scanFile.Book.ISBN);
                pData.AddDataParam("issn", scanFile.Book.ISSN);
                pData.AddDataParam("oclc", scanFile.Book.OCLC);
                pData.AddDataParam("nbn", (String.IsNullOrEmpty(scanFile.Book.NBN) ? custom : scanFile.Book.NBN));
                pData.AddDataParam("author", scanFile.Book.Author);
                pData.AddDataParam("title", scanFile.Book.Title);
                pData.AddDataParam("year", scanFile.Book.Year);
                pData.AddDataParam("ocr", "no");

                // Meta parameters
                string metaXml = GetMetaXmlData(scanFile.PartOfBook, scanFile.PageCount);
                pData.AddFileParam("meta", "meta.xml", metaXml, PostDataContentType.XML);

                // File data
                //string filepath = scanFile.GetScanFilePath();
                //string extension = Path.GetExtension(filepath);
                switch (scanFile.PartOfBook)
                {
                    case PartOfBook.FrontCover:
                        pData.AddFileParam("cover", "cover.jpg", scanFile.GetScanFilePath(), PostDataContentType.JPG);
                        break;
                    case PartOfBook.TableOfContents:
                        pData.AddFileParam("toc", "toc.tif", scanFile.GetScanFilePath(), PostDataContentType.TIFF);
                        break;
                    default:
                        break;
                }

                // Write the http request body directly to the server
                byte[] buffer = pData.GetPostDataBytes();
                requestToServer.ContentLength = buffer.Length;

                using (Stream s = requestToServer.GetRequestStream())
                {
                    s.Write(buffer, 0, buffer.Length);
                    s.Flush();
                    s.Close();
                }

                // Grab the response from the server
                WebResponse response = requestToServer.GetResponse();
                StreamReader responseReader = new StreamReader(response.GetResponseStream());

                //return Encoding.UTF8.GetString(buffer);
                return responseReader.ReadToEnd().Equals("OK", StringComparison.OrdinalIgnoreCase);
            }
            catch (WebException we)
            {
                string message = String.Empty;

                if (we.Response != null)
                {
                    HttpWebResponse response = (we.Response as HttpWebResponse);

                    //WebException will be thrown when a HTTP OK status is not returned
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            message = "Chyba autorizace: Přihlašovací údaje nejsou správné.";
                            break;
                        case HttpStatusCode.InternalServerError:
                            message = String.Format("Chyba na straně serveru: {0}", response.StatusDescription);
                            break;
                        default:
                            message = String.Format("{0}: {1}", response.StatusCode, response.StatusDescription);
                            break;
                    }
                }
                else
                {
                    message = String.Format("{0}: {1}", we.Status, we.Message);
                }

                throw new PostDataException(String.Format("Odesílání neúspěšné: {0}", message), we);
            }
            catch (Exception ex)
            {
                throw new PostDataException(String.Format("Počas odesílání nastala neznámá výjimka, je možné, že data nebyla odeslána: {0}", ex.Message), ex);
            }
        }

        private string GetMetaXmlData(PartOfBook partOfBook, int pages)
        {
            try
            {
                XElement rootElement = new XElement("meta");
                XDocument xmlDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), rootElement);

                //autorizace uživatele systému ObalkyKnih.cz, možno zaslat jako HTTP-Basic
                rootElement.Add(new XElement("sigla", this.Sigla));
                rootElement.Add(new XElement("user", this.Login));

                //identifikace klientske aplikace
                XElement clientElement = new XElement("client");
                clientElement.Add(new XElement("name", "DOZP"));
                clientElement.Add(new XElement("version", "1.0"));

                //IP adresy
                IPHostEntry host;
                string localIPv4 = "?";
                string localIPv6 = "?";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    switch (ip.AddressFamily)
                    {
                        case AddressFamily.InterNetwork:
                            localIPv4 = ip.ToString();
                            break;
                        case AddressFamily.InterNetworkV6:
                            localIPv6 = ip.ToString();
                            break;
                        default:
                            break;
                    }
                }
                clientElement.Add(new XElement("local-IPv4-address", localIPv4));
                clientElement.Add(new XElement("local-IPv6-address", localIPv6));
                rootElement.Add(clientElement);

                switch (partOfBook)
                {
                    case PartOfBook.FrontCover:
                        XElement coverElement = new XElement("cover");
                        rootElement.Add(coverElement);
                        break;
                    case PartOfBook.TableOfContents:
                        XElement tocElement = new XElement("toc");
                        tocElement.Add(new XElement("pages", pages));
                        rootElement.Add(tocElement);
                        break;
                    default:
                        break;
                }

                return xmlDoc.ToString();
            }
            catch (Exception ex)
            {
                throw new ApplicationException(String.Format("Nastala chyba při tvorbě metasouboru: {0}", ex.Message));
            }
        }

        public class PostData
        {
            public static string Boundary = "----------" + DateTime.Now.Ticks.ToString("x");

            private List<PostDataParam> _params;

            public List<PostDataParam> Params
            {
                get
                {
                    if (_params == null)
                        _params = new List<PostDataParam>();

                    return _params;
                }
            }

            public void AddDataParam(string name, string value)
            {
                if (!String.IsNullOrEmpty(value))
                    this.Params.Add(new PostDataParam(name, value));
                else
                    return;
            }

            public void AddFileParam(string name, string filename, string value, PostDataContentType type)
            {
                if (value != null && value.Length > 0)
                    this.Params.Add(new PostDataParam(name, filename, value, type));
                else
                    return;
            }

            public byte[] GetPostDataBytes()
            {
                byte[] postData = null;

                using (Stream ms = new System.IO.MemoryStream())
                {
                    Encoding encoding = Encoding.UTF8;

                    foreach (PostDataParam p in this.Params)
                    {
                        StringBuilder param = new StringBuilder();
                        param.AppendLine(String.Format("--{0}", Boundary));

                        if (p.Type == PostDataContentType.Data)
                        {
                            // Add header of the parameter.
                            param.AppendLine(String.Format("Content-Disposition: form-data; name=\"{0}\"", p.Name));
                            param.AppendLine();
                            param.AppendLine(p.Value);

                            ms.Write(encoding.GetBytes(param.ToString()), 0, encoding.GetByteCount(param.ToString()));
                        }
                        else
                        {
                            // Add header of the parameter.
                            param.AppendLine(String.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"", p.Name, p.FileName));
                            switch (p.Type)
                            {
                                case PostDataContentType.JPG:
                                    param.AppendLine("Content-Type: image/jpeg");
                                    break;
                                case PostDataContentType.TIFF:
                                    param.AppendLine("Content-Type: image/tiff");
                                    break;
                                case PostDataContentType.PDF:
                                    param.AppendLine("Content-Type: application/pdf");
                                    break;
                                case PostDataContentType.XML:
                                    param.AppendLine("Content-Type: text/xml");
                                    break;
                                default:
                                    param.AppendLine("Content-Type: application/octet-stream");
                                    break;
                            }
                            param.AppendLine();

                            if (p.Type == PostDataContentType.XML)
                            {
                                param.AppendLine(p.Value);
                                ms.Write(encoding.GetBytes(param.ToString()), 0, encoding.GetByteCount(param.ToString()));
                            }
                            else
                            {
                                ms.Write(encoding.GetBytes(param.ToString()), 0, encoding.GetByteCount(param.ToString()));
                                //byte[] buffer = Encoding.UTF8.GetBytes(p.Value);
                                byte[] buffer = File.ReadAllBytes(p.Value);
                                ms.Write(buffer, 0, buffer.Length);
                                ms.Write(encoding.GetBytes(Environment.NewLine), 0, encoding.GetByteCount(Environment.NewLine));
                            }
                        }
                    }

                    // Add the end of the request.
                    StringBuilder footer = new StringBuilder();
                    footer.AppendLine(String.Format("--{0}--", Boundary));
                    ms.Write(encoding.GetBytes(footer.ToString()), 0, encoding.GetByteCount(footer.ToString()));

                    // Dump the Stream into a byte array
                    ms.Position = 0;
                    postData = new byte[ms.Length];
                    ms.Read(postData, 0, postData.Length);
                    ms.Close();
                }

                return postData;
            }
        }

        public class PostDataParam
        {
            public PostDataParam(string name, string value)
            {
                if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

                this.Name = name;
                this.Value = value;
                this.Type = PostDataContentType.Data;
            }

            public PostDataParam(string name, string filename, string value, PostDataContentType type)
            {
                if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
                if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("filename");

                this.Name = name;
                this.FileName = filename;
                this.Value = value;
                this.Type = type;
            }

            public string Name { get; private set; }
            public string FileName { get; set; }
            public string Value { get; set; }
            public PostDataContentType Type { get; private set; }
        }

        public class PostDataException : Exception
        {
            public PostDataException(string reason)
                : base(reason)
            {
            }

            public PostDataException(string reason, Exception cause)
                : base(reason, cause)
            {
            }

            public string GetMessage()
            {
                return base.Message;
            }
        }
    }
}
