using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Web.Catalogues
{
    public partial class FileStream : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                string path = Convert.ToString(Request.QueryString["path"]);
                int width = Convert.ToInt32(Request.QueryString["width"]);
                int height = Convert.ToInt32(Request.QueryString["height"]);
                int page = Convert.ToInt32(Request.QueryString["page"]);

                if (page == 0) page = 1;

                try
                {
                    if (File.Exists(path))
                    {
                        System.Drawing.Image img = ImageFunctions.LoadThumbnail(path, width, height);
                        Response.Clear();
                        Response.ContentType = "image/jpeg";
                        img.Save(Response.OutputStream, ImageFormat.Jpeg);
                        img.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}