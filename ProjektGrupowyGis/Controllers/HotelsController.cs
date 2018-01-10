﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjektGrupowyGis.Models;
using ProjektGrupowyGis.DAL;
using System.Net;
using Microsoft.AspNet.Identity;

namespace ProjektGrupowyGis.Controllers
{
    public class HotelsController : Controller
    {
        private HotelsSqlExecutor _hotelsSqlExecutor;
        private AccountSqlExecutor _accountSqlExecutor;

        public HotelsController()
        {
            _hotelsSqlExecutor = new HotelsSqlExecutor();
            _accountSqlExecutor = new AccountSqlExecutor();
        }

        public ActionResult Index()
        {
            List<Hotel> hotels;
            var user = HttpContext.User.Identity;
            var canEdit = false;
            var canRate = false;
            if (user.IsAuthenticated)
            {
                var userID = _accountSqlExecutor.GetUserId(user.Name);
                hotels = _hotelsSqlExecutor.GetHotelsWithUserRate(userID);
                canRate = true;
            }
            else {
                hotels = _hotelsSqlExecutor.GetHotels();
            }
            if (user.Name == "Admin") {
                canEdit = true;
            }
            HotelsModel model = new HotelsModel { Hotels = hotels, CanEdit = canEdit, CanRate = canRate };
            return View(model);
        }

        public ActionResult EmptyHotelsDB()
        {
            int count =_hotelsSqlExecutor.CountHotels();
            var response = new { response = count };
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddHotel(Hotel hotel) {
            _hotelsSqlExecutor.AddHotel(hotel);
            var response = new { response = "ok" };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult RateHotel(RatePost rating) {
            if (User.Identity.IsAuthenticated)
            {
                _hotelsSqlExecutor.RateHotel(rating.userLogin, rating.hotelId, rating.rate);
                var response = new { response =  rating.rate};
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            else return View();
        }

        public ActionResult Edit(int idHotel) {
            if (User.Identity.Name == "Admin")
            {
                Hotel hotel = _hotelsSqlExecutor.FindHotelById(idHotel);
                return View(hotel);
            }
            else return View();
        }

        [HttpPost]
        public ActionResult Edit(Hotel hotel)
        {
            if (User.Identity.Name == "Admin")
            {
                _hotelsSqlExecutor.EditHotel(hotel.Id_Hotel, hotel.Name, hotel.FullAddress, hotel.Webpage, hotel.Phone, hotel.Lat, hotel.Lng);
                return RedirectToAction("Index");
            }
            else return RedirectToAction("Index");
        }
    }
}