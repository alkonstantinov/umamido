﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umamido.Common;

namespace Umamido.Site.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ContactUs()
        {
            return View();
        }

        public ActionResult OrderPayment()
        {
            var model = DL.GetTextByLang("ORDERPAYMENT", Lang);
            return View(model);
        }

        public ActionResult Terms()
        {
            var model = DL.GetTextByLang("TERMS", Lang);
            return View(model);
        }

        public ActionResult Restaurants() {
            var r = DL.GetRestaurantsByLang(Lang);
            return View(r.ToArray());
        }

        [HttpGet]
        public ActionResult GetImage(int imageId)
        {
            ImageFileModel ifm = DL.GetImageFile(imageId);
            string filename = ifm.FileName;
            byte[] filedata = ifm.Content;
            string contentType = MimeMapping.GetMimeMapping(ifm.FileName);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = filename,
                Inline = true,
            };

            Response.AppendHeader("Content-Disposition", cd.ToString());

            return File(filedata, contentType);
        }


        [HttpGet]
        public ActionResult Restaurant(int restaurantId)
        {
            var r = DL.GetRestaurantByLang(Lang, restaurantId);
            return View(r);
        }

        [HttpPost]
        public ActionResult AddMessage(MessageModel model)
        {
            DL.AddMessage(model);
            return View("Contactus");
        }
    }
}