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
    }

    public class LocalPasswordModel
    {
        [Required(ErrorMessage = "Add meg a jelenlegi jelszót!")]
        [DataType(DataType.Password)]
        [Display(Name = "Jelenlegi jelszó")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Add meg az új jelszót")]
        [StringLength(100, ErrorMessage = "A {0}nak legalább {2} karakter hosszúnak kell lennie.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Új jelszó")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Add meg az új jelszó megerősítését!")]
        [DataType(DataType.Password)]
        [Display(Name = "Új jelszó megerősítése")]
        [Compare("NewPassword", ErrorMessage = "Az új jelszó és a megerősítése nem egyezik meg.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangeUserNameModel
    {
        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Add meg az új e-mail címedet!")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "E-mail cím")]
        [RegularExpression(@"([^.@]+)(\.[^.@]+)*@([^.@]+\.)+([^.@]+)", ErrorMessage = "Érvénytelen e-mail cím!")]
        [System.Web.Mvc.Remote("IsUserNameUnique", "Account", AdditionalFields = "UserId", ErrorMessage = "A megadott e-mail cím már létezik!", HttpMethod = "POST")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Keresztnév")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Vezetéknév")]
        public string LastName { get; set; }
    }

    public class UserProfileModel
    {
        public LocalPasswordModel PasswordModel { get; set; }
        public ChangeUserNameModel UserNameModel { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Add meg az e-mail címedet!")]
        [Display(Name = "E-mail cím")]
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Add meg a jelszavadad!")]
        [DataType(DataType.Password)]
        [Display(Name = "Jelszó")]
        public string Password { get; set; }

        [Display(Name = "Emlékezz rám")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Az e-mail cím megadása kötelező!")]
        [Display(Name = "E-mail cím")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"([^.@]+)(\.[^.@]+)*@([^.@]+\.)+([^.@]+)", ErrorMessage = "Érvénytelen e-mail cím!")]
        [System.Web.Mvc.Remote("IsUserNameUnique", "Account", ErrorMessage = "A megadott e-mail cím már létezik!", HttpMethod = "POST")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "A keresztnév megadása kötelező!")]
        [Display(Name = "Keresztnév")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "A vezetéknév megadása kötelező!")]
        [Display(Name = "Vezetéknév")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "A jelszó megadása kötelező!")]
        [StringLength(100, ErrorMessage = "A {0}nak legalább {2} karakter hosszúnak kell lennie.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Jelszó")]
        public string Password { get; set; }

        [Required(ErrorMessage = "A jelszó megerősítésének megadása kötelező!")]
        [DataType(DataType.Password)]
        [Display(Name = "Jelszó megerősítése")]
        [Compare("Password", ErrorMessage = "Az jelszó és a megerősítése nem egyezik meg.")]
        public string ConfirmPassword { get; set; }
    }
}
