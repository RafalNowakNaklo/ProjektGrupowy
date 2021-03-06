﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjektGrupowyGis.Models;
using ProjektGrupowyGis.DAL;
using System.Net;
using Microsoft.AspNet.Identity;
using PagedList;

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

        public ActionResult Index(HotelsSearch search, string name, string address, string userRateFrom, string userRateTo, 
            string yourRateFrom, string yourRateTo, string distance, string lat, string lng, int? page)
        {
            ViewBag.CurrentSearch = new HotelsSearch();
            ViewBag.Empty = null;
            if (search != null)
            {
                page = 1;
                ViewBag.CurrentSearch = search;
                search.lat = lat;
                search.lng = lng;
            }
            else {
                ViewBag.CurrentSearch.nameSearch = name;
                ViewBag.CurrentSearch.addressSearch = address;
                ViewBag.CurrentSearch.usersRateFrom = userRateFrom;
                ViewBag.CurrentSearch.usersRateTo = userRateTo;
                ViewBag.CurrentSearch.yourRateFrom = yourRateFrom;
                ViewBag.CurrentSearch.yourRateTo = yourRateTo;
                ViewBag.CurrentSearch.distance = distance;
                ViewBag.CurrentSearch.lat = lat;
                ViewBag.CurrentSearch.lng = lng;
                search = ViewBag.CurrentSearch;
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            bool canRate = User.Identity.IsAuthenticated;
            var user = HttpContext.User.Identity;
            var idUser = _accountSqlExecutor.GetUserId(user.Name);
            var canEdit = false;
            if (user.Name == "Admin")
            {
                canEdit = true;
            }
            var hotels = _hotelsSqlExecutor.FilterHotels(search, canRate, idUser);
            HotelsModel model = new HotelsModel { Hotels = hotels.ToPagedList<Hotel>(pageNumber, pageSize), CanEdit = canEdit, CanRate = canRate, Search = search };
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

        public ActionResult Create()
        {
            if (User.Identity.Name == "Admin")
            {
                Hotel hotel = new Hotel();
                return View(hotel);
            }
            else return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Create(Hotel hotel)
        {
            if (User.Identity.Name == "Admin")
            {
                _hotelsSqlExecutor.AddHotelApp(hotel);
                return RedirectToAction("Index");
            }
            else return RedirectToAction("Index");
        }

        public ActionResult Edit(int idHotel) {
            if (User.Identity.Name == "Admin")
            {
                Hotel hotel = _hotelsSqlExecutor.FindHotelById(idHotel);
                return View(hotel);
            }
            else return RedirectToAction("Index");
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