using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Project2AP.Models;

namespace Project2AP.Controllers
{
    public class PaymentController : Controller
    {
        private MrtContext db = new MrtContext();



        public ActionResult Index(string searchText = "", int page = 1, string sortOrder = "")
        {
            ViewBag.SearchText = searchText;
            int recordsPerPage = 10;

            if (Session["Roles"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            else if (Session["Roles"].ToString() == "User")
            {
                ViewBag.Roles = "User";

                string email = Session["Email"].ToString();
                var items = db.Payment.Where(x => x.Email == email);
                
                switch (sortOrder)
                {
                    case "Latest First":
                        items = db.Payment.Where(x => x.Email == email).OrderByDescending(x => x.PaymentID);
                        break;

                    case "Oldest First":
                        items = db.Payment.Where(x => x.Email == email).OrderBy(x => x.PaymentID);
                        break;
                }

                var result = items.ToList().ToPagedList(page, recordsPerPage);

                if (!result.Any())
                {
                    ViewBag.Item = 0;
                }
                else
                {
                    ViewBag.Item = 1;
                }                

                return View(result);
            }

            else if (Session["Roles"].ToString() == "Admin")
            {
                ViewBag.Roles = "Admin";

                var items = db.Payment.Include(x => x.User).Where(x => x.Email.Contains(searchText));

                switch (sortOrder)
                {
                    case "Latest First":
                        items = db.Payment.Include(x => x.User).Where(x => x.Email.Contains(searchText)).OrderByDescending(x => x.PaymentID);
                        break;

                    case "Oldest First":
                        items = db.Payment.Include(x => x.User).Where(x => x.Email.Contains(searchText)).OrderBy(x => x.PaymentID);
                        break;
                }

                var result = items.ToList().ToPagedList(page, recordsPerPage);

                if (!result.Any())
                {
                    ViewBag.Item = 0;
                }
                else
                {
                    ViewBag.Item = 1;
                }

                return View(result);
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public ActionResult Proceed()
        {
            if (Session["Roles"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            else if (Session["Roles"].ToString() == "User")
            {
                string email = Session["Email"].ToString();

                string firstName = db.User.Where(x => x.Email == email).Take(1).Select(x => x.FirstName).ToList().FirstOrDefault();
                string lastName = db.User.Where(x => x.Email == email).Take(1).Select(x => x.LastName).ToList().FirstOrDefault();
                string icNumber = db.User.Where(x => x.Email == email).Take(1).Select(x => x.ICNumber).ToList().FirstOrDefault();
                string phoneNumber = db.User.Where(x => x.Email == email).Take(1).Select(x => x.PhoneNumber).ToList().FirstOrDefault();

                double total = db.Purchase.Where(x => x.Email == email && x.Status == "Unpaid").Sum(x => x.Subtotal);

                ViewBag.Name = firstName + " " + lastName;
                ViewBag.IcNumber = icNumber;
                ViewBag.PhoneNumber = phoneNumber;
                ViewBag.Email = email;

                ViewBag.Total = total;

                return View();
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult Proceed(Payment payment)
        {
            string email = Session["Email"].ToString();

            string firstName = db.User.Where(x => x.Email == email).Take(1).Select(x => x.FirstName).ToList().FirstOrDefault();
            string lastName = db.User.Where(x => x.Email == email).Take(1).Select(x => x.LastName).ToList().FirstOrDefault();
            string icNumber = db.User.Where(x => x.Email == email).Take(1).Select(x => x.ICNumber).ToList().FirstOrDefault();
            string phoneNumber = db.User.Where(x => x.Email == email).Take(1).Select(x => x.PhoneNumber).ToList().FirstOrDefault();

            double total = db.Purchase.Where(x => x.Email == email && x.Status == "Unpaid").Sum(x => x.Subtotal);

            ViewBag.Name = firstName + " " + lastName;
            ViewBag.IcNumber = icNumber;
            ViewBag.PhoneNumber = phoneNumber;
            ViewBag.Email = email;

            ViewBag.Total = total;

            if (ModelState.IsValidField("PaymentMethod"))
            {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MrtContext"].ConnectionString);
                SqlCommand cmd = new SqlCommand("spInsertPayment", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
                cmd.Parameters.AddWithValue("@PaymentDateTime", DateTime.Now.ToString());
                cmd.Parameters.AddWithValue("@Total", total);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    ViewBag.Message = "Successful";

                    SqlCommand cmd2 = new SqlCommand("spUpdatePurchase", conn);
                    cmd2.CommandType = CommandType.StoredProcedure;

                    int lastRecord2 = db.Payment.Where(x => x.Email == email).OrderByDescending(x => x.PaymentID).Take(1).Select(x => x.PaymentID).ToList().FirstOrDefault();

                    ViewBag.Message = lastRecord2;

                    cmd2.Parameters.AddWithValue("@Email", email);
                    cmd2.Parameters.AddWithValue("@PaymentID", lastRecord2);

                    try
                    {
                        cmd2.ExecuteNonQuery();

                        return RedirectToAction("MailReceipt", "Ticket", new { paymentId = lastRecord2 });
                    }

                    catch
                    {
                        ViewBag.Message = "Purchase update not successful";
                        return View();
                    }
                }
                catch
                {
                    ViewBag.Message = "Payment not successful";
                    return View();
                }
                finally
                {
                    conn.Close();
                }
            }

            else
            {
                return View();
            }
        }

        public ActionResult Details(int? paymentId)
        {
            if (Session["Roles"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            else if (Session["Roles"].ToString() == "User" || Session["Roles"].ToString() == "Admin")
            {
                ViewBag.Roles = Session["Roles"].ToString();
                string email = Session["Email"].ToString();

                string firstName = db.User.Where(x => x.Email == email).Take(1).Select(x => x.FirstName).ToList().FirstOrDefault();
                string lastName = db.User.Where(x => x.Email == email).Take(1).Select(x => x.LastName).ToList().FirstOrDefault();
                string icNumber = db.User.Where(x => x.Email == email).Take(1).Select(x => x.ICNumber).ToList().FirstOrDefault();
                string phoneNumber = db.User.Where(x => x.Email == email).Take(1).Select(x => x.PhoneNumber).ToList().FirstOrDefault();
                string paymentMethod = db.Payment.Where(x => x.PaymentID == paymentId).Take(1).Select(x => x.PaymentMethod).ToList().FirstOrDefault();
                string paymentDateTime = db.Payment.Where(x => x.PaymentID == paymentId).Take(1).Select(x => x.PaymentDateTime).ToList().FirstOrDefault().ToString();
                double total = db.Payment.Where(x => x.PaymentID == paymentId).Take(1).Select(x => x.Total).ToList().FirstOrDefault();

                ViewBag.Email = email;
                ViewBag.FirstName = firstName;
                ViewBag.LastName = lastName;
                ViewBag.IcNumber = icNumber;
                ViewBag.PhoneNumber = phoneNumber;
                ViewBag.PaymentID = paymentId;
                ViewBag.PaymentMethod = paymentMethod;
                ViewBag.PaymentDateTime = paymentDateTime;
                ViewBag.Total = total;

                if (paymentId == null)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.PaymentID = paymentId;

                var items = db.Payment.Where(x => x.PaymentID == paymentId).Include(x => x.User).Include(x => x.Purchases);
                var result = items.ToList();

                return View(result);
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
