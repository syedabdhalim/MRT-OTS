using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project2AP.Models;
using Project2AP.Process;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using PagedList;

namespace Project2AP.Controllers
{
    public class UserController : Controller
    {
        private MrtContext db = new MrtContext();

        // GET: Users
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValidField("Email") && ModelState.IsValidField("FirstName") && ModelState.IsValidField("LastName") &&
                ModelState.IsValidField("PhoneNumber") && ModelState.IsValidField("ICNumber") && ModelState.IsValidField("Password"))
            {
                string password = user.Password;
                PBKDF2 PwdHash = new PBKDF2(password);
                string passwordhash = PwdHash.HashedPassword;

                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MrtContext"].ConnectionString);
                SqlCommand cmd = new SqlCommand("spInsertUser", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                cmd.Parameters.AddWithValue("@LastName", user.LastName);
                cmd.Parameters.AddWithValue("@ICNumber", user.ICNumber);
                cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                cmd.Parameters.AddWithValue("@Password", passwordhash);
                cmd.Parameters.AddWithValue("@Roles", "User");

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    return RedirectToAction("Login");
                }
                catch
                {
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

        // GET: Users
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User userModel)
        {
            if (ModelState.IsValidField("Email") && ModelState.IsValidField("Password"))
            {
                string sql = "SELECT * FROM Users WHERE Email = @email";
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MrtContext"].ConnectionString);
                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@email", userModel.Email);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                sda.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    Object objpassword = dt.Rows[0]["Password"];
                    Object objroles = dt.Rows[0]["Roles"];
                    Object objfirstname = dt.Rows[0]["FirstName"];
                    Object objlastname = dt.Rows[0]["LastName"];

                    string password = userModel.Password;

                    string storedpasswordhash = objpassword.ToString();
                    PBKDF2 PwdHash = new PBKDF2(password, storedpasswordhash);

                    bool passwordcheck = PwdHash.PasswordCheck;

                    if (passwordcheck == true)
                    {
                        Session["Email"] = userModel.Email;
                        Session["Roles"] = objroles.ToString();
                        Session["Name"] = objfirstname.ToString() + " " + objlastname.ToString();

                        ViewBag.Status = "Successful";
                        ViewBag.Email = Session["Email"];
                        ViewBag.Roles = Session["Roles"];
                        ViewBag.Name = Session["Name"];

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewBag.Status = "Incorrect e-mail or password";
                        return View();
                    }
                }

                else
                {
                    return View();
                }
            }

            else
            {
                return View();
            }

        }
        public ActionResult Index(string searchText = "", int page = 1, string sortOrder = "")
        {
            if (Session["Roles"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            else if (Session["Roles"].ToString() == "User")
            {
                return RedirectToAction("Index", "Home");
            }

            else if (Session["Roles"].ToString() == "Admin")
            {
                ViewBag.SearchText = searchText;
                ViewBag.Roles = "Admin";
                int recordsPerPage = 10;

                var items = db.User.Where(x => x.Roles == "User").Where(x => x.Email.Contains(searchText));
                
                switch (sortOrder)
                {
                    case "E-mail: A to Z":
                        items = db.User.Where(x => x.Roles == "User").Where(x => x.Email.Contains(searchText)).OrderBy(x => x.Email);
                        break;

                    case "E-mail: Z to A":
                        items = db.User.Where(x => x.Roles == "User").Where(x => x.Email.Contains(searchText)).OrderByDescending(x => x.Email); ;
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
        public ActionResult Update(string email)
        {
            if (email == null)
            {
                return RedirectToAction("Index", "Home");
            }

            User user = db.User.Find(email);

            if(user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult Update(User userupdate)
        {
            if (ModelState.IsValidField("Email") && ModelState.IsValidField("FirstName") && ModelState.IsValidField("LastName") &&
                ModelState.IsValidField("PhoneNumber") && ModelState.IsValidField("ICNumber"))
            {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MrtContext"].ConnectionString);
                SqlCommand cmd = new SqlCommand("spUpdateUser", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", userupdate.Email);
                cmd.Parameters.AddWithValue("@FirstName", userupdate.FirstName);
                cmd.Parameters.AddWithValue("@LastName", userupdate.LastName);
                cmd.Parameters.AddWithValue("@ICNumber", userupdate.ICNumber);
                cmd.Parameters.AddWithValue("@PhoneNumber", userupdate.PhoneNumber);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    ViewBag.Message = "Profile successfully updated";

                    return RedirectToAction("Index", "Home");
                }
                catch
                {
                    ViewBag.Message = "Profile not successfully updated";

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
    }
}
