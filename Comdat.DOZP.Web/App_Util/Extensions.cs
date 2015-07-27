using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Comdat.DOZP.Web
{
    public static class Extensions
    {
        public static void AddValidationSummary(this Page page, string errorMessage)
        {
            var validator = new CustomValidator();
            validator.IsValid = false;
            validator.ErrorMessage = errorMessage;
            page.Validators.Add(validator);
        }
    }
}