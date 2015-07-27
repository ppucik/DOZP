using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Web.Controls
{
    public partial class EnumDropDownList : System.Web.UI.UserControl
    {
        #region Private members
        private string _enumType = String.Empty;
        private bool _emptyVisible = true;
        #endregion

        #region Public properties

        public bool AutoPostback
        {
            get
            {
                return this.DropDownList.AutoPostBack;
            }
            set
            {
                this.DropDownList.AutoPostBack = value;
            }
        }

        public bool HeaderVisible
        {
            get
            {
                return this.FilterHeader.Visible;
            }
            set
            {
                this.FilterHeader.Visible = value;
            }
        }

        public bool EmptyVisible
        {
            get
            {
                return _emptyVisible;
            }
            set
            {
                _emptyVisible = value;
            }
        }

        public string SelectedValue
        {
            get
            {
                return this.DropDownList.SelectedValue;
            }
            set
            {
                this.DropDownList.SelectedValue = value;
            }
        }

        public string EnumType
        {
            get
            {
                return _enumType;
            }
            set
            {
                _enumType = value;
            }
        }

        #endregion

        #region UserControl events

        protected void Page_Init(object sender, EventArgs e)
        {
            Type type = null;
            List<EnumItem> enums = null;
           
            if (EmptyVisible)
            {
                this.DropDownList.Items.Add(new ListItem("(Všechny)", String.Empty));
            }

            switch (EnumType)
            {
                case "PartOfBook":
                    type = typeof(PartOfBook);
                    break;
                case "ProcessingMode":
                    type = typeof(ProcessingMode);
                    break;
                case "StatusCode":
                    type = typeof(StatusCode);
                    break;
                default:
                    break;
            }

            if (type != null)
            {
                this.FilterHeader.InnerText = Enumeration.GetEnumName(type);
                enums = Enumeration.GetList(type);
            }
            else
            {
                this.FilterHeader.InnerText = EnumType;
            }

            if (enums != null)
            {
                foreach (var item in enums)
                {
                    this.DropDownList.Items.Add(new ListItem(item.Description, Convert.ToInt32(item.Value).ToString()));
                }
            }
        }

        #endregion
    }
}