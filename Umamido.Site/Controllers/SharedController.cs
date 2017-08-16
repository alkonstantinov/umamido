﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Umamido.Site.Controllers
{
    public class SharedController : BaseController
    {
        // GET: Shared
        public ActionResult SetLanguage(string language)
        {
            Lang =  language;
            return Redirect(Request.UrlReferrer.AbsoluteUri);

        }
    }
}