using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Project2AP.Models;

namespace Project2AP.Controllers
{
    public class CartController : Controller
    {
        private MrtContext db = new MrtContext();

        // GET: Cart
        public ActionResult Index()
        {
            if (Session["Roles"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            else if (Session["Roles"].ToString() == "User")
            {
                string email = Session["Email"].ToString();

                var purchase = db.Purchase.Where(x => x.Email == email && x.Status == "Unpaid");

                if (!purchase.Any())
                {
                    ViewBag.Item = 0;
                }

                else
                {
                    ViewBag.Item = 1;

                    double total = db.Purchase.Where(x => x.Email == email && x.Status == "Unpaid").Sum(x => x.Subtotal);
                    ViewBag.Total = total;
                }

                return View(purchase.ToList());
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Delete(int purchaseId)
        {
            Purchase purchase = db.Purchase.Find(purchaseId);
            db.Purchase.Remove(purchase);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
