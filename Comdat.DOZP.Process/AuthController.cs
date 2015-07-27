using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;

using Comdat.DOZP.Core;
using Comdat.DOZP.Process.DozpService;

namespace Comdat.DOZP.Process
{
    public class AuthController
    {
        #region Private members
        private static DozpIdentity _userIdentity = new DozpIdentity();
        #endregion

        #region Initialize WCF service

        /// <summary>
        /// Přihlášení uživatele.
        /// </summary>
        public class DozpIdentity
        {
            private User _loginUser = null;
            private string _username = null;
            private string _password = null;

            public User LoginUser
            {
                get { return _loginUser; }
            }

            internal string Password
            {
                get { return _password; }
            }

            public bool IsAuthenticated
            {
                get { return (_loginUser != null); }
            }

            public bool IsAdministrator
            {
                get
                {
                    return (IsAuthenticated && LoginUser.RoleName.Equals(RoleConstants.ADMINISTRATOR));
                }
            }

            public bool IsSupervisor
            {
                get
                {
                    return (IsAuthenticated && LoginUser.RoleName.Equals(RoleConstants.SUPERVISOR));
                }
            }

            public bool IsCataloguer
            {
                get
                {
                    return (IsAuthenticated && LoginUser.RoleName.Equals(RoleConstants.CATALOGUER));
                }
            }

            public bool IsUserOCR
            {
                get
                {
                    return (IsAuthenticated && LoginUser.RoleName.Equals(RoleConstants.USER_OCR));
                }
            }

            public bool Authenticate(string userName, string password)
            {
                if (String.IsNullOrEmpty(userName)) throw new ArgumentNullException("userName");
                if (String.IsNullOrEmpty(password)) throw new ArgumentNullException("password");

                DozpServiceClient proxy = new DozpServiceClient();
                bool success = false;

                _username = userName;

                try
                {
                    proxy.ClientCredentials.UserName.UserName = userName;
                    proxy.ClientCredentials.UserName.Password = password;
                    _loginUser = proxy.Authenticate();
                    _password = password;
                    proxy.Close();
                    success = true;
                }
                catch (FaultException<DozpServiceFault> ex)
                {
                    throw new ApplicationException(ex.Message + ex.Detail.Message);
                }
                catch (FaultException ex)
                {
                    throw new ApplicationException(ExceptionMessage.SERVICE + ex.Message);
                }
                catch (MessageSecurityException ex)
                {
                    throw new ApplicationException(ExceptionMessage.ACCESS_DENIED, ex);
                }
                catch (CommunicationException ex)
                {
                    throw new ApplicationException(ExceptionMessage.COMMUNICATION, ex);
                }
                catch (TimeoutException ex)
                {
                    throw new ApplicationException(ExceptionMessage.TIMEOUT, ex);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(ExceptionMessage.AUTHENTICATION + ex.Message);
                }
                finally
                {
                    if (!success) proxy.Abort();
                }

                return true;
            }

            public override string ToString()
            {
                if (this.LoginUser != null)
                    return this.LoginUser.FullName;
                else
                    return _username;
            }
        }

        /// <summary>
        /// Přihlašovací údaje uživatele
        /// </summary>
        public static DozpIdentity UserIdentity
        {
            get { return _userIdentity; }
        }

        /// <summary>
        /// Inicializace WCF služby.
        /// </summary>
        /// <returns>Proxy objekt</returns>
        public static DozpServiceClient GetProxy()
        {
            DozpServiceClient proxy = null;

            if ((UserIdentity != null) && UserIdentity.IsAuthenticated)
            {
                proxy = new DozpServiceClient();
                proxy.ClientCredentials.UserName.UserName = UserIdentity.LoginUser.UserName;
                proxy.ClientCredentials.UserName.Password = UserIdentity.Password;
            }
            else
            {
                if (proxy != null) proxy.Abort();
                throw new ApplicationException(ExceptionMessage.AUTHENTICATION + "User is not authenticated.");
            }

            return proxy;
        }

        #endregion
    }
}
