using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace Project2AP.Models
{
    public class User
    {
        [Key]
        [Required(ErrorMessage ="Please enter e-mail address")]
        [EmailAddress(ErrorMessage = "Please enter valid e-mail address format")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your first name")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter your last name")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please enter your IC number")]
        [Display(Name = "IC Number")]
        public string ICNumber { get; set; }

        [Required(ErrorMessage = "Please enter your phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please select a role")]
        [Display(Name = "Roles")]
        public string Roles { get; set; }
        
        [Display(Name = "Registration Date Time")]
        public DateTime RegistrationDateTime { get; set; }

        public virtual ICollection<Purchase> Purchases { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }

    public class Purchase
    {
        [Key]
        [Display(Name = "e-Ticket")]
        public int PurchaseID { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Payment ID")]
        public int? PaymentID { get; set; }

        [Required(ErrorMessage = "Please select origin station")]
        [Display(Name = "Origin")]
        public string Origin { get; set; }

        [Required(ErrorMessage = "Please select destination station")]
        [Display(Name = "Destination")]
        public string Destination { get; set; }

        [Required(ErrorMessage = "Please select direction")]
        [Display(Name = "Direction")]
        public string Direction { get; set; }

        [Required(ErrorMessage = "Please select category")]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Please enter quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid quantity")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Subtotal")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public double Subtotal { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }
        
        public virtual User User { get; set; }
        
        public virtual Payment Payment { get; set; }
    }

    public class Payment
    {
        [Key]
        [Display(Name = "Payment ID")]
        public int PaymentID { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please select payment method")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Date & Time")]
        public DateTime PaymentDateTime { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public double Total { get; set; }

        public virtual ICollection<Purchase> Purchases { get; set; }
        
        public virtual User User { get; set; }
    }

    public class MrtContext : DbContext
    {
        public MrtContext() : base("name=MrtContext")
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<Purchase> Purchase { get; set; }
        public DbSet<Payment> Payment { get; set; }
    }
}