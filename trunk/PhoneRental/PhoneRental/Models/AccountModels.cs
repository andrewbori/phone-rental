using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace PhoneRental.Models
{
    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "E-mail cím")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Keresztnév")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Vezetéknév")]
        public string LastName { get; set; }

        [NotMapped]
        public string FullUserName
        {
            get
            {
                return FirstName + " " + LastName + " (" + UserName + ")";
            }
        }
    }

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "E-mail cím")]
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Jelenlegi jelszó")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0}nak legalább {2} karakter hosszúnak kell lennie.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Új jelszó")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Új jelszó megerősítése")]
        [Compare("NewPassword", ErrorMessage = "Az új jelszó és a megerősítése nem egyezik meg.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "E-mail cím")]
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Jelszó")]
        public string Password { get; set; }

        [Display(Name = "Emlékezz rám")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "E-mail cím")]
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Keresztnév")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Vezetéknév")]
        public string LastName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0}nak legalább {2} karakter hosszúnak kell lennie.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Jelszó")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Jelszó megerősítése")]
        [Compare("Password", ErrorMessage = "Az jelszó és a megerősítése nem egyezik meg.")]
        public string ConfirmPassword { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}
