﻿using Google.Maps;
using Google.Maps.Geocoding;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umamido.Common;
using Umamido.Common.Tools;

namespace Umamido.Site.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home
        public ActionResult Index()
        {
            IndexPageModel model = new IndexPageModel();
            model.Sliders = DL.GetSlidersByLang(Lang);
            model.AboutUsMainHeading = DL.GetTextByLang("ABOUTUSMAINHEADING", Lang).Text;
            model.AboutUsMainText = DL.GetTextByLang("ABOUTUSMAINTEXT", Lang).Text;
            model.Restaurants = DL.GetRestaurantsByLang(Lang);
            model.Blogs = DL.GetBlogsByLang(Lang);
            return View(model);
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

        public ActionResult Restaurants()
        {
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

        public ActionResult GetGoods(int restaurantId)
        {
            var result = DL.GetGoodsByLang(restaurantId, Lang);
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Good(int goodId)
        {
            var model = DL.GetGoodByLang(goodId, Lang);
            model.ClientId = this.ClientData.UserId;
            model.CanOrder = this.ClientData.UserId.HasValue || (ClientData.GoodAddress1 != null && ClientData.GoodAddress2 != null);
            return View(model);
        }

        public ActionResult Order()
        {
            var model = DL.GetRestaurantsByLang(Lang);
            return View(model);
        }

        public ActionResult AboutUs()
        {
            var model = new AboutUsModel()
            {
                Restaurants = DL.GetRestaurantsByLang(Lang),
                Text = DL.GetTextByLang("ABOUTUSTEXT", Lang).Text,
                Title = DL.GetTextByLang("ABOUTUSHEADING", Lang).Text
            };
            return View(model);
        }
        public ActionResult Blog(int blogId)
        {
            var model = DL.GetBlogByLang(blogId, Lang);
            return View(model);
        }

        public ActionResult Blogs()
        {
            var model = DL.GetBlogsByLang(Lang);
            return View(model);
        }


        public ActionResult GoodsPartial(GoodsDisplayInfoModel model)
        {
            model.Restaurants = DL.GetRestaurantsByLang(Lang);
            model.IsLogged = this.ClientData.UserId.HasValue || (ClientData.GoodAddress1 != null && ClientData.GoodAddress2 != null);
            ModelState.Clear();
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult ClientLogin(LoginModel model)
        {
            var result = DL.ClientLogin(model);
            if (result.HasValue)
                this.ClientData.UserId = result;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CheckAddress(string address)
        {
            GoogleSigned.AssignAllServices(new GoogleSigned(ConfigurationManager.AppSettings["GoogleMapsHash"]));

            var request = new GeocodingRequest { Address = "София," + address, Sensor = false };
            var response = new GeocodingService().GetResponse(request);
            List<GeoAddress> result = new List<GeoAddress>();
            foreach (var item in response.Results)
            {
                result.Add(
                    new GeoAddress()
                    {
                        FormatedAddress = item.FormattedAddress,
                        IsOk = Tools.getDistanceFromLatLonInKm(double.Parse(ConfigurationManager.AppSettings["BaseLat"]), double.Parse(ConfigurationManager.AppSettings["BaseLong"]), response.Results[0].Geometry.Location.Latitude, response.Results[0].Geometry.Location.Longitude) < int.Parse(ConfigurationManager.AppSettings["DistanceOK"])
                    }
                    );

            }


            return Json(result.ToArray(), JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult SaveDistantAddress(DistantAddressModel model)
        {
            DL.SaveDistantAddress(model);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GoodAddress(string address1, string address2)
        {
            this.ClientData.GoodAddress1 = address1;
            this.ClientData.GoodAddress2 = address2;

            return Json("OK", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult AddToCart(int goodId, int? quantity)
        {
            foreach (var item in this.ClientData.Goods)
                if (item.GoodId == goodId)
                {
                    item.Quantity += quantity.HasValue ? quantity.Value : 1;
                    return Json("OK", JsonRequestBehavior.AllowGet);
                }

            ClientData.Goods.Add(new CartSessionModel() { GoodId = goodId, Quantity = quantity.HasValue ? quantity.Value : 1 });
            return Json("OK", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult ChangeCartQuantity(int goodId, int quantity)
        {
            foreach (var item in this.ClientData.Goods)
                if (item.GoodId == goodId)
                {
                    item.Quantity = quantity;
                    return Json("OK", JsonRequestBehavior.AllowGet);
                }

            return Json("OK", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult DeleteFromCart(int goodId)
        {
            int i = 0;
            while (this.ClientData.Goods[i].GoodId != goodId && i < this.ClientData.Goods.Count)
                i++;
            if (i < this.ClientData.Goods.Count)
                this.ClientData.Goods.RemoveAt(i);

            return Json("OK", JsonRequestBehavior.AllowGet);

        }

        public ActionResult Cart()
        {
            List<GoodPresentationModel> model = new List<GoodPresentationModel>();
            foreach (var item in this.ClientData.Goods)
            {
                var good = DL.GetGoodByLang(item.GoodId, Lang);
                good.Count = item.Quantity;
                model.Add(good);
            }
            return View(model.ToArray());
        }

        public ActionResult Login()
        {
            if (this.ClientData.UserId.HasValue)
                return RedirectToAction("MyProfile");
            return View(new LoginModel());
        }


        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            var result = DL.ClientLogin(model);
            if (result.HasValue)
            {
                this.ClientData.UserId = result;
                return RedirectToAction("MyProfile");
            }
            model.LoginFailure = true;
            ModelState.Clear();
            return View(model);
        }



        public ActionResult MyProfile()
        {
            if (this.ClientData.UserId == null)
                return RedirectToAction("Login");

            return View(DL.GetMyProfile(this.ClientData.UserId.Value, this.Lang));
        }

        public ActionResult OrderAgain(int reqId)
        {
            this.ClientData.Goods = DL.GetForOrderAgain(reqId);
            return RedirectToAction("MyProfile");
        }

        public ActionResult MyAccountProfile()
        {
            if (this.ClientData.UserId == null)
                return RedirectToAction("Login");

            return View(DL.GetAccountProfile(this.ClientData.UserId.Value));
        }

        [HttpPost]
        public ActionResult MyAccountProfile(MyAccountProfileModel model)
        {
            model.ClientId = this.ClientData.UserId.Value;
            DL.UpdateMyAccountProfile(model);

            return View(model);
        }


        public ActionResult MyProfileAddress()
        {
            if (this.ClientData.UserId == null)
                return RedirectToAction("Login");
            return View(DL.GetInvoiceAddressModel(this.ClientData.UserId.Value));
        }

        [HttpPost]
        public ActionResult SaveInvoiceAddress(InvoiceAddressModel model)
        {
            model.ClientId = this.ClientData.UserId.Value;

            if (
                string.IsNullOrEmpty(model.CompanyAddress) ||
                string.IsNullOrEmpty(model.CompanyName) ||
                string.IsNullOrEmpty(model.Country) ||
                string.IsNullOrEmpty(model.EIK) ||
                string.IsNullOrEmpty(model.PersonName) ||
                string.IsNullOrEmpty(model.PK) ||
                string.IsNullOrEmpty(model.VAT)
                )
            {
                model.ErrorMessage = Resources.Resources.PleaseEnterValue;
            }
            else
                DL.SetInvoiceAddressModel(model);



            return View("MyProfileAddress", model);
        }

        public ActionResult GetProfileAddress(int addressNum)
        {
            AddressModel model = new AddressModel();
            model.AddressNum = addressNum;
            model.ClientId = this.ClientData.UserId.Value;
            DL.GetProfileAddress(model);
            return PartialView("AddressPartial", model);
        }

        public ActionResult SetProfileAddress(AddressModel model)
        {
            if (
                string.IsNullOrEmpty(model.Address) ||
                string.IsNullOrEmpty(model.Family) ||
                string.IsNullOrEmpty(model.Name) ||
                string.IsNullOrEmpty(model.Phone)
                )
                model.ErrorMessage = Resources.Resources.PleaseEnterValue;
            else
            {
                GoogleSigned.AssignAllServices(new GoogleSigned(ConfigurationManager.AppSettings["GoogleMapsHash"]));

                var request = new GeocodingRequest { Address = "София," + model.Address, Sensor = false };
                var response = new GeocodingService().GetResponse(request);
                if (response.Results.Count() == 0)
                    model.ErrorMessage = Resources.Resources.AddressNotFound;
                else
                if (response.Results.Count() == 1)
                {
                    if (Tools.getDistanceFromLatLonInKm(double.Parse(ConfigurationManager.AppSettings["BaseLat"]), double.Parse(ConfigurationManager.AppSettings["BaseLong"]), response.Results[0].Geometry.Location.Latitude, response.Results[0].Geometry.Location.Longitude) < int.Parse(ConfigurationManager.AppSettings["DistanceOK"]))
                    {
                        model.ClientId = this.ClientData.UserId.Value;
                        model.Address = response.Results[0].FormattedAddress;
                        DL.StoreProfileAddress(model);
                    }
                    else
                        model.ErrorMessage = Resources.Resources.AddressOutOfRange;
                }
                else
                {
                    List<string> result = new List<string>();
                    foreach (var item in response.Results)
                    {
                        result.Add(item.FormattedAddress);
                    }
                    model.AddressVariants = result.ToArray();
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult QuitProfile()
        {
            this.ClientData.UserId = null;
            return RedirectToAction("Order");
        }

        public ActionResult SmallGoodPartial(int goodId)
        {
            return PartialView(DL.GetGoodByLang(goodId, Lang));
        }

        public ActionResult Checkout()
        {
            CheckOutModel model = new CheckOutModel();
            model.ClientId = this.ClientData.UserId;
            if (model.ClientId.HasValue)
            {
                model.Addresses = DL.GetAddresses(model.ClientId.Value);
                var inv = DL.GetInvoiceAddressModel(model.ClientId.Value);
                model.CompanyName = inv.CompanyName;
                model.CompanyAddress = inv.CompanyAddress;
                model.Country = inv.Country;
                model.PK = inv.PK;
                model.PersonName = inv.PersonName;
                model.EIK = inv.EIK;
                model.VAT = inv.VAT;
                


            }
            else
            {
                model.Address = this.ClientData.GoodAddress2;
            }

            decimal total = 0;
            foreach (var item in this.ClientData.Goods.ToArray())
            {
                var good = DL.GetGoodByLang(item.GoodId, Lang);
                good.Count = item.Quantity;
                model.CartItems.Add(good);
                total += good.Count * good.Price;
            }

            model.ProductsTotal = total;
            model.DeliveryTotal = decimal.Parse(System.Configuration.ConfigurationManager.AppSettings["DeliveryPrice"]);
            model.TotalTotal = total + decimal.Parse(System.Configuration.ConfigurationManager.AppSettings["DeliveryPrice"]);

            return View(model);
        }

        [HttpPost]
        public ActionResult Checkout(CheckOutModel model)
        {
            if (!this.ClientData.UserId.HasValue)
            {
                if (DL.ClientNameExists(model.UserName))
                {
                    model.ErrorMessage = Resources.Resources.UsernameExists;
                    model.ClientId = this.ClientData.UserId;
                    return View(model);
                }

                if (
                string.IsNullOrEmpty(model.Address) ||
                string.IsNullOrEmpty(model.Family) ||
                string.IsNullOrEmpty(model.FirstName) ||
                string.IsNullOrEmpty(model.Phone) ||
                (
                    model.Invoice && (
                        string.IsNullOrEmpty(model.CompanyAddress) ||
                        string.IsNullOrEmpty(model.CompanyName) ||
                        string.IsNullOrEmpty(model.Country) ||
                        string.IsNullOrEmpty(model.EIK) ||
                        string.IsNullOrEmpty(model.PersonName) ||
                        string.IsNullOrEmpty(model.PK) ||
                        string.IsNullOrEmpty(model.VAT)
                )
                )
                )
                {
                    model.ErrorMessage = Resources.Resources.PleaseEnterValue;
                    model.ClientId = this.ClientData.UserId;
                    return View(model);
                }

                this.ClientData.UserId = DL.CreateClient(model);
                model.AddressNum = 1;



            }
            else
            {

                if (
                    model.Invoice && (
                        string.IsNullOrEmpty(model.CompanyAddress) ||
                        string.IsNullOrEmpty(model.CompanyName) ||
                        string.IsNullOrEmpty(model.Country) ||
                        string.IsNullOrEmpty(model.EIK) ||
                        string.IsNullOrEmpty(model.PersonName) ||
                        string.IsNullOrEmpty(model.PK) ||
                        string.IsNullOrEmpty(model.VAT)
                        )
                        )
                {
                    model.ErrorMessage = Resources.Resources.PleaseEnterValue;
                    model.ClientId = this.ClientData.UserId;
                    return View(model);
                }

                InvoiceAddressModel iam = new InvoiceAddressModel()
                {
                    CompanyName = model.CompanyName,
                    CompanyAddress = model.CompanyAddress,
                    ClientId = this.ClientData.UserId.Value,
                    Country = model.Country,
                    EIK = model.EIK,
                    Email = model.EMail,
                    PersonName = model.PersonName,
                    Phone = model.Phone,
                    PK = model.PK,
                    VAT = model.VAT
                };
                DL.SetInvoiceAddressModel(iam);
            }

            model.ClientId = this.ClientData.UserId;
            decimal total = 0;
            foreach (var item in this.ClientData.Goods.ToArray())
            {
                var good = DL.GetGoodByLang(item.GoodId, Lang);
                good.Count = item.Quantity;
                model.CartItems.Add(good);
                total += good.Count * good.Price;
            }

            model.ProductsTotal = total;
            model.DeliveryTotal = decimal.Parse(System.Configuration.ConfigurationManager.AppSettings["DeliveryPrice"]);
            model.TotalTotal = total + decimal.Parse(System.Configuration.ConfigurationManager.AppSettings["DeliveryPrice"]);
            DL.CreateReq(model);
            this.ClientData.Goods.Clear();
            return RedirectToAction("CheckoutComplete");
        }

        public ActionResult CheckoutComplete()
        {
            
            return View();
        }

    }
}