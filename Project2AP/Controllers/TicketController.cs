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
    public class TicketController : Controller
    {
        private MrtContext db = new MrtContext();

        public ActionResult Index(string searchText = "", int page = 1)
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
                var items = db.Purchase.Where(x => (x.Email == email) && x.Status == "Paid").Include(x => x.Payment);
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

                var items = db.Purchase.Where(x => x.Status == "Paid").Include(x => x.Payment).Include(x => x.User);
                var result = items.ToList().Where(x => x.Email.Contains(searchText)).ToPagedList(page, recordsPerPage);

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
        
        public ActionResult Index(string sortOrder, string q, int page = 1, int pageSize = 25)
        {
            ViewBag.searchQuery = String.IsNullOrEmpty(q) ? "" : q;

            page = page > 0 ? page : 1;
            pageSize = pageSize > 0 ? pageSize : 25;

            ViewBag.NameSortParam = sortOrder == "name" ? "name_desc" : "name";
            ViewBag.AddressSortParam = sortOrder == "address" ? "address_desc" : "address";
            ViewBag.DateSortParam = sortOrder == "date" ? "date_desc" : "date";

            ViewBag.CurrentSort = sortOrder;

            DataService service = new DateService(db);
            var query = service.GetAllOrderedListItems(q);

            switch (sortOrder)
            {
                case "name":
                    query = query.OrderBy(x => x.name);
                    break;
                case "address":
                    query = query.OrderBy(x => x.address);
                    break;
                case "address_desc":
                    query = query.OrderByDescending(x => x.address);
                    break;
                case "date":
                    query = query.OrderBy(x => x.date);
                    break;
                case "date_desc":
                    query = query.OrderByDescending(x => x.date);
                    break;
                default:
                    break;
            }
            return View(query.ToPagedList(page, pageSize));
        }

        [HttpGet]
        public ActionResult Purchase()
        {
            if (Session["Roles"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            else if (Session["Roles"].ToString() == "User")
            {
                return View();
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Purchase(Purchase purchase)
        {
            if (ModelState.IsValidField("Origin") && ModelState.IsValidField("Destination") && ModelState.IsValidField("Direction") &&
                ModelState.IsValidField("Category") && ModelState.IsValidField("Quantity"))
            {
                double[,] fares =
                { {0.80, 1.20, 1.80, 2.00, 2.60, 2.70, 3.10, 3.30, 3.20, 3.50, 3.30, 3.40, 3.10, 3.20, 3.30, 3.40, 3.50, 3.60, 3.70, 3.90, 4.00, 4.10, 4.30, 4.50, 4.60, 4.80, 4.80, 5.00, 5.30, 5.40, 5.50},
                {1.20, 0.80, 1.50, 1.80, 2.30, 2.70, 2.80, 3.10, 3.40, 3.30, 3.70, 3.30, 3.70, 3.80, 3.20, 3.30, 3.40, 3.50, 3.60, 3.80, 3.90, 4.00, 4.20, 4.40, 4.50, 4.60, 4.70, 4.90, 5.20, 5.20, 5.40},
                {1.80, 1.50, 0.80, 1.10, 1.80, 2.10, 2.60, 2.60, 3.00, 3.20, 3.30, 3.50, 3.40, 3.50, 3.60, 3.70, 3.20, 3.30, 3.40, 3.50, 3.60, 3.80, 3.90, 4.10, 4.30, 4.40, 4.50, 4.60, 4.90, 5.00, 5.10},
                {2.00, 1.80, 1.10, 0.80, 1.60, 1.90, 2.30, 2.60, 2.80, 3.00, 3.10, 3.30, 3.80, 3.40, 3.40, 3.60, 3.80, 3.20, 3.30, 3.40, 3.50, 3.70, 3.80, 4.00, 4.10, 4.30, 4.40, 4.50, 4.80, 4.90, 5.00},
                {2.60, 2.30, 1.80, 1.60, 0.80, 1.30, 1.80, 2.00, 2.40, 2.80, 3.00, 3.20, 3.30, 3.50, 3.60, 3.20, 3.40, 3.60, 3.70, 3.20, 3.20, 3.40, 3.50, 3.70, 3.90, 4.00, 4.10, 4.30, 4.60, 4.60, 4.80},
                {2.70, 2.70, 2.10, 1.90, 1.30, 0.80, 1.30, 1.70, 2.00, 2.40, 2.70, 2.90, 3.10, 3.30, 3.40, 3.60, 3.80, 3.40, 3.50, 3.70, 3.80, 3.20, 3.40, 3.60, 3.70, 3.90, 4.00, 4.10, 4.40, 4.50, 4.60},
                {3.10, 2.80, 2.60, 2.30, 1.80, 1.30, 0.80, 1.30, 1.70, 2.00, 2.60, 2.80, 3.20, 3.40, 3.10, 3.30, 3.50, 3.70, 3.20, 3.50, 3.60, 3.80, 3.20, 3.40, 3.60, 3.70, 3.80, 3.90, 4.20, 4.30, 4.40},
                {3.30, 3.10, 2.60, 2.60, 2.00, 1.70, 1.30, 0.80, 1.30, 1.70, 2.20, 2.50, 2.90, 3.10, 3.20, 3.40, 3.20, 3.40, 3.60, 3.80, 3.40, 3.60, 3.80, 3.30, 3.40, 3.60, 3.60, 3.80, 4.10, 4.20, 4.30},
                {3.20, 3.40, 3.00, 2.80, 2.40, 2.00, 1.70, 1.30, 0.80, 1.20, 1.80, 2.10, 2.80, 2.80, 2.90, 3.10, 3.40, 3.10, 3.30, 3.60, 3.70, 3.40, 3.60, 3.80, 3.20, 3.40, 3.50, 3.60, 3.90, 4.00, 4.10},
                {3.50, 3.30, 3.20, 3.00, 2.80, 2.40, 2.00, 1.70, 1.20, 0.80, 1.60, 1.80, 2.50, 2.70, 2.60, 2.80, 3.10, 3.30, 3.10, 3.30, 3.50, 3.70, 3.40, 3.60, 3.80, 3.20, 3.30, 3.50, 3.80, 3.90, 4.00},
                {3.30, 3.70, 3.30, 3.10, 3.00, 2.70, 2.60, 2.20, 1.80, 1.60, 0.80, 1.10, 1.80, 2.10, 2.20, 2.50, 2.80, 2.80, 3.00, 3.30, 3.50, 3.30, 3.50, 3.30, 3.50, 3.70, 3.80, 3.20, 3.50, 3.60, 3.70},
                {3.40, 3.30, 3.50, 3.30, 3.20, 2.90, 2.80, 2.50, 2.10, 1.80, 1.10, 0.80, 1.70, 1.90, 2.00, 2.30, 2.60, 2.60, 2.80, 3.10, 3.30, 3.10, 3.40, 3.70, 3.30, 3.50, 3.60, 3.10, 3.40, 3.50, 3.60},
                {3.10, 3.70, 3.40, 3.80, 3.30, 3.10, 3.20, 2.90, 2.80, 2.50, 1.80, 1.70, 0.80, 1.20, 1.30, 1.60, 1.90, 2.10, 2.30, 2.70, 2.70, 3.00, 3.30, 3.20, 3.40, 3.70, 3.20, 3.50, 3.10, 3.20, 3.30},
                {3.20, 3.80, 3.50, 3.40, 3.50, 3.30, 3.40, 3.10, 2.80, 2.70, 2.10, 1.90, 1.20, 0.80, 1.00, 1.30, 1.70, 1.80, 2.10, 2.50, 2.70, 2.80, 3.00, 3.50, 3.30, 3.50, 3.60, 3.30, 3.70, 3.80, 3.20},
                {3.30, 3.20, 3.60, 3.40, 3.60, 3.40, 3.10, 3.20, 2.90, 2.60, 2.20, 2.00, 1.30, 1.00, 0.80, 1.10, 1.50, 1.80, 1.90, 2.30, 2.50, 2.60, 2.90, 3.30, 3.20, 3.40, 3.50, 3.80, 3.60, 3.70, 3.20},
                {3.40, 3.30, 3.70, 3.60, 3.20, 3.60, 3.30, 3.40, 3.10, 2.80, 2.50, 2.30, 1.60, 1.30, 1.10, 0.80, 1.20, 1.50, 1.80, 2.10, 2.30, 2.60, 2.70, 3.10, 3.40, 3.20, 3.30, 3.60, 3.50, 3.60, 3.80},
                {3.50, 3.40, 3.20, 3.80, 3.40, 3.80, 3.50, 3.20, 3.40, 3.10, 2.80, 2.60, 1.90, 1.70, 1.50, 1.20, 0.80, 1.10, 1.40, 1.80, 1.90, 2.30, 2.70, 2.90, 3.10, 3.40, 3.10, 3.40, 3.30, 3.40, 3.60},
                {3.60, 3.50, 3.30, 3.20, 3.60, 3.40, 3.70, 3.40, 3.10, 3.30, 2.80, 2.60, 2.10, 1.80, 1.80, 1.50, 1.10, 0.80, 1.10, 1.50, 1.80, 2.10, 2.40, 2.60, 2.90, 3.20, 3.30, 3.20, 3.70, 3.30, 3.40},
                {3.70, 3.60, 3.40, 3.30, 3.70, 3.50, 3.20, 3.60, 3.30, 3.10, 3.00, 2.80, 2.30, 2.10, 1.90, 1.80, 1.40, 1.10, 0.80, 1.30, 1.50, 1.80, 2.20, 2.70, 2.70, 3.00, 3.20, 3.10, 3.60, 3.70, 3.30},
                {3.90, 3.80, 3.50, 3.40, 3.20, 3.70, 3.50, 3.30, 3.60, 3.30, 3.30, 3.10, 2.70, 2.50, 2.30, 2.10, 1.80, 1.50, 1.30, 0.80, 1.10, 1.50, 1.80, 2.30, 2.60, 2.70, 2.80, 3.20, 3.30, 3.40, 3.60},
                {4.00, 3.90, 3.60, 3.50, 3.20, 3.80, 3.60, 3.40, 3.70, 3.50, 3.50, 3.30, 2.70, 2.70, 2.50, 2.30, 1.90, 1.80, 1.50, 1.10, 0.80, 1.30, 1.70, 2.10, 2.40, 2.80, 2.70, 3.00, 3.10, 3.30, 3.50},
                {4.10, 4.00, 3.80, 3.70, 3.40, 3.20, 3.80, 3.60, 3.40, 3.70, 3.30, 3.10, 3.00, 2.80, 2.60, 2.60, 2.30, 2.10, 1.80, 1.50, 1.30, 0.80, 1.20, 1.80, 2.00, 2.40, 2.60, 2.70, 3.30, 3.40, 3.20},
                {4.30, 4.30, 3.90, 3.80, 3.50, 3.40, 3.20, 3.80, 3.60, 3.40, 3.50, 3.40, 3.30, 3.00, 2.90, 2.70, 2.70, 2.40, 2.20, 1.80, 1.70, 1.20, 0.80, 1.40, 1.80, 2.00, 2.20, 2.60, 3.00, 3.10, 3.40},
                {4.50, 4.40, 4.10, 4.00, 3.70, 3.60, 3.40, 3.30, 3.80, 3.60, 3.30, 3.70, 3.20, 3.50, 3.30, 3.10, 2.90, 2.60, 2.70, 2.30, 2.10, 1.80, 1.40, 0.80, 1.20, 1.60, 1.80, 2.10, 2.60, 2.70, 3.00},
                {4.60, 4.50, 4.30, 4.10, 3.90, 3.70, 3.60, 3.40, 3.20, 3.80, 3.50, 3.30, 3.40, 3.30, 3.20, 3.40, 3.10, 2.90, 2.70, 2.60, 2.40, 2.00, 1.80, 1.20, 0.80, 1.30, 1.50, 1.80, 2.50, 2.70, 2.70},
                {4.80, 4.60, 4.40, 4.30, 4.00, 3.90, 3.70, 3.60, 3.40, 3.20, 3.70, 3.50, 3.70, 3.50, 3.40, 3.20, 3.40, 3.20, 3.00, 2.70, 2.80, 2.40, 2.00, 1.60, 1.30, 0.80, 1.10, 1.50, 2.20, 2.30, 2.70},
                {4.80, 4.70, 4.50, 4.40, 4.10, 4.00, 3.80, 3.60, 3.50, 3.30, 3.80, 3.60, 3.20, 3.60, 3.50, 3.30, 3.10, 3.30, 3.20, 2.80, 2.70, 2.60, 2.20, 1.80, 1.50, 1.10, 0.80, 1.30, 2.00, 2.20, 2.50},
                {5.00, 4.90, 4.60, 4.50, 4.30, 4.10, 3.90, 3.80, 3.60, 3.50, 3.20, 3.10, 3.50, 3.30, 3.80, 3.60, 3.40, 3.20, 3.10, 3.20, 3.00, 2.70, 2.60, 2.10, 1.80, 1.50, 1.30, 0.80, 1.70, 1.80, 2.10},
                {5.30, 5.20, 4.90, 4.80, 4.60, 4.40, 4.20, 4.10, 3.90, 3.80, 3.50, 3.40, 3.10, 3.70, 3.60, 3.50, 3.30, 3.70, 3.60, 3.30, 3.10, 3.30, 3.00, 2.60, 2.50, 2.20, 2.00, 1.70, 0.80, 1.10, 1.40},
                {5.40, 5.20, 5.00, 4.90, 4.60, 4.50, 4.30, 4.20, 4.00, 3.90, 3.60, 3.50, 3.20, 3.80, 3.70, 3.60, 3.40, 3.30, 3.70, 3.40, 3.30, 3.40, 3.10, 2.70, 2.70, 2.30, 2.20, 1.80, 1.10, 0.80, 1.20},
                {5.50, 5.40, 5.10, 5.00, 4.80, 4.60, 4.40, 4.30, 4.10, 4.00, 3.70, 3.60, 3.30, 3.20, 3.20, 3.80, 3.60, 3.40, 3.30, 3.60, 3.50, 3.20, 3.40, 3.00, 2.70, 2.70, 2.50, 2.10, 1.40, 1.20, 0.80}
                };

                IDictionary<int, string> dictOrigin = new Dictionary<int, string>()
                {
                {0, "Sungai Buloh"},
                {1, "Kampung Selamat"},
                {2, "Kwasa Damansara"},
                {3, "Kwasa Sentral" },
                {4, "Kota Damansara" },
                {5, "Surian" },
                {6, "Mutiara Damansara" },
                {7, "Bandar Utama" },
                {8, "Taman Tun Dr Ismail" },
                {9, "Philleo Damansara" },
                {10, "Pusat Bandar Damansara" },
                {11, "Semantan" },
                {12, "Muzium Negara" },
                {13, "Pasar Seni" },
                {14, "Merdeka" },
                {15, "Bukit Bintang"},
                {16, "Tun Razak Exchange" },
                {17, "Cochrane" },
                {18, "Maluri" },
                {19, "Taman Pertama" },
                {20, "Taman Midah" },
                {21, "Taman Mutiara" },
                {22, "Taman Connought" },
                {23, "Taman Suntex" },
                {24, "Sri Raya" },
                {25, "Bandar Tun Hussein Onn" },
                {26, "Batu Sebelas Cheras" },
                {27, "Bukit Dukung" },
                {28, "Sungai Jernih" },
                {29, "Stadium Kajang" },
                {30, "Kajang" }

                };

                IDictionary<int, string> dictDestination = new Dictionary<int, string>()
                {
                {0, "Sungai Buloh"},
                {1, "Kampung Selamat"},
                {2, "Kwasa Damansara"},
                {3, "Kwasa Sentral" },
                {4, "Kota Damansara" },
                {5, "Surian" },
                {6, "Mutiara Damansara" },
                {7, "Bandar Utama" },
                {8, "Taman Tun Dr Ismail" },
                {9, "Philleo Damansara" },
                {10, "Pusat Bandar Damansara" },
                {11, "Semantan" },
                {12, "Muzium Negara" },
                {13, "Pasar Seni" },
                {14, "Merdeka" },
                {15, "Bukit Bintang"},
                {16, "Tun Razak Exchange" },
                {17, "Cochrane" },
                {18, "Maluri" },
                {19, "Taman Pertama" },
                {20, "Taman Midah" },
                {21, "Taman Mutiara" },
                {22, "Taman Connought" },
                {23, "Taman Suntex" },
                {24, "Sri Raya" },
                {25, "Bandar Tun Hussein Onn" },
                {26, "Batu Sebelas Cheras" },
                {27, "Bukit Dukung" },
                {28, "Sungai Jernih" },
                {29, "Stadium Kajang" },
                {30, "Kajang" }

                };

                int OriginIndex = int.Parse(purchase.Origin);
                int DestinationIndex = int.Parse(purchase.Destination);

                string origin = dictOrigin[OriginIndex];
                string destination = dictOrigin[DestinationIndex];

                double ticketPrice = fares[OriginIndex, DestinationIndex];

                string category = purchase.Category;
                string direction = purchase.Direction;

                if (category == "Standard")
                {
                    ticketPrice = ticketPrice * 1;
                }

                else if (category == "Senior Citizen")
                {
                    ticketPrice = ticketPrice * 0.5;
                }

                else if (category == "Student")
                {
                    ticketPrice = ticketPrice * 0.5;
                }

                else if (purchase.Category == "Disabled")
                {
                    ticketPrice = ticketPrice * 0.5;
                }

                else
                {
                    ticketPrice = ticketPrice * 1;
                }

                int quantity = purchase.Quantity;
                double subtotal;

                if (direction == "One-way")
                {
                    subtotal = ticketPrice * quantity * 1;
                }

                else
                {
                    subtotal = ticketPrice * quantity * 2;
                }

                string email = Session["Email"].ToString();

                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MrtContext"].ConnectionString);
                SqlCommand cmd = new SqlCommand("spInsertPurchase", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Origin", origin);
                cmd.Parameters.AddWithValue("@Destination", destination);
                cmd.Parameters.AddWithValue("@Direction", direction);
                cmd.Parameters.AddWithValue("@Category", category);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                cmd.Parameters.AddWithValue("@Status", "Unpaid");

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    return RedirectToAction("Index", "Cart");
                }
                catch
                {
                    ViewBag.Message = "Not success";
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

        public ActionResult Receipt(int? paymentId, int page = 1)
        {
            if (Session["Roles"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            else if (Session["Roles"].ToString() == "User" || Session["Roles"].ToString() == "Admin")
            {
                if (paymentId == null)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.Email = Session["Email"];
                ViewBag.Roles = Session["Roles"].ToString();
                ViewBag.PaymentID = paymentId;
                int recordsPerPage;

                recordsPerPage = 1;
                var items = db.Purchase.Where(x => x.PaymentID == paymentId).Include(x => x.Payment).Include(x => x.User);
                var result = items.ToList().ToPagedList(page, recordsPerPage);

                return View(result);
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult MailReceipt(int? paymentId)
        {
            if (Session["Roles"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            else if (Session["Roles"].ToString() == "User" || Session["Roles"].ToString() == "Admin")
            {
                if (paymentId == null)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.Roles = Session["Roles"].ToString();
                ViewBag.Email = Session["Email"];
                ViewBag.PaymentID = paymentId;

                var items = db.Purchase.Where(x => x.PaymentID == paymentId).Include(x => x.Payment).Include(x => x.User);
                var result = items.ToList();

                return View(result);
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Details(int? purchaseId, int page = 1)
        {
            if (Session["Roles"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            else if (Session["Roles"].ToString() == "User" || Session["Roles"].ToString() == "Admin")
            {
                if (purchaseId == null)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.Roles = Session["Roles"].ToString();
                ViewBag.PurchaseID = purchaseId;
                int recordsPerPage = 1;

                var items = db.Purchase.Where(x => x.PurchaseID == purchaseId).Include(x => x.Payment).Include(x => x.User);
                var result = items.ToList().ToPagedList(page, recordsPerPage);

                return View(result);
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
