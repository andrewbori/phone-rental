using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhoneRental.Models
{
    public class PhoneRentalContext : DbContext
    {
        public PhoneRentalContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Borrow> Borrows { get; set; }
        public DbSet<PreBorrow> PreBorrows { get; set; }
    }

    [Table("Brand")]
    public class Brand
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Márka")]
        public string Name { get; set; }
    }

    [Table("DeviceType")]
    public class DeviceType
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required( ErrorMessage = "A Márka megadása kötelező!" )]
        [Display(Name = "Márka")]
        public int BrandId { get; set; }
        public virtual Brand Brand { get; set; }

        [Required(ErrorMessage = "A Típus megadása kötelező!")]
        [Display(Name = "Típus")]
        [Remote("IsTypeUnique", "DeviceType", AdditionalFields = "Brand.Name, Id", ErrorMessage = "A megadott típus már létezik!", HttpMethod = "POST")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Az AAIT azonosító mintájának megadása kötelező!")]
        [Display(Name = "AAIT azonosító mintája")]
        [Remote("IsAaitIdPatternUnique", "DeviceType", AdditionalFields="Id", ErrorMessage = "A megadott AAIT azonosító minta már létezik!", HttpMethod = "POST")]
        public string AaitIdPattern { get; set; }

        [Display(Name = "Kép a készülékről")]
        public string ImageUrl { get; set; }

        #region NotMapped

        [NotMapped]
        public bool HasPreBorrowedByUser
        {
            get
            {
                int count;
                using (var db = new PhoneRentalContext())
                {
                    int uid = db.UserProfiles.Single(
                    p => p.UserName == HttpContext.Current.User.Identity.Name).UserId;

                    count = (from preBorrow in db.PreBorrows
                                  where ((preBorrow.UserId == uid) && (preBorrow.DeviceTypeId == Id))
                                  select preBorrow.Id).Count();

                }
                if (count > 0) return true;
                else return false;
            }
        }

        [NotMapped]
        [Display(Name = "Elérhetőség")]
        public string Availability
        {
            get
            {
                int borrowed;
                int preBorrowed;
                string retval;
                DateTime end;
                using (var db = new PhoneRentalContext())
                {
                    int devicecnt = (from device in db.Devices
                                        where (device.DeviceTypeId == Id)
                                        select device.Id).Count();
                    if (devicecnt > 0)
                    {
                     borrowed = (from borrow in db.Borrows
                                    where (borrow.Device.DeviceTypeId == Id) &
                                    (borrow.EndDate == null)
                                    select borrow.DeviceId).Count();
                      preBorrowed = (from preb in db.PreBorrows
                                       where (preb.DeviceTypeId == Id)
                                       select preb.DeviceTypeId).Count();

                        if (devicecnt - (borrowed + preBorrowed) > 0)
                        {
                            retval = "Azonnal elvihető";
                        }
                        else if (preBorrowed < devicecnt)
                        {
                            var x = preBorrowed - (devicecnt - borrowed);
                            end = (from borrow in db.Borrows
                                        where (borrow.Device.DeviceTypeId == Id)
                                        orderby borrow.Deadline
                                        select borrow.Deadline).ToList().ElementAt(x);

                            retval = "Várhatóan: " + end.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            retval = "Jelenleg minden készülékre van előfoglalás";
                        }
                    }
                else
                {
                    retval = "Nincs raktárkészlet";
                }
            }
                return retval;
            }
            set
            {
            }
        }

        #endregion

        public virtual ICollection<Device> Devices { get; set; }
    }

    [Table("Device")]
    public class Device
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "A készülék típusának megadása kötelező!")]
        [Display(Name = "Készülék típusa")]
        [Remote("IsAaitIdNumberUnique", "Device", AdditionalFields = "Id, AaitIdNumber", ErrorMessage = "A megadott AAIT azonosító minta már létezik.", HttpMethod = "POST")]
        public int DeviceTypeId { get; set; }
        public virtual DeviceType DeviceType { get; set; }

        [Required(ErrorMessage = "Az IMEI megadása kötelező!")]
        [Display(Name = "IMEI")]
        [Remote("IsImeiUnique", "Device", AdditionalFields="Id", ErrorMessage = "Az megadott IMEI már létezik.", HttpMethod = "POST")]
        public string Imei { get; set; }

        [Required(ErrorMessage = "Az AAIT azonosító megadása kötelező!")]
        [Display(Name = "AAIT azonosító")]
        [DisplayFormat(DataFormatString = "{0:D3}", ApplyFormatInEditMode = true)]
        [RegularExpression(@"[0-9]*$", ErrorMessage = "Az AAIT azonosítónak egy pozitív egész számnak kell lennie!")]
        [Remote("IsAaitIdNumberUnique", "Device", AdditionalFields="Id, DeviceTypeId",  ErrorMessage = "A megadott AAIT azonosító már létezik.", HttpMethod = "POST")]
        public int AaitIdNumber { get; set; }

        [NotMapped]
        public Borrow Borrow
        {
            get {
                using (var db = new PhoneRentalContext())
                {
                    var borrow = db.Borrows.Where(b => b.DeviceId == Id).Where(b => b.EndDate == null).Include(b => b.User).SingleOrDefault();
                    return borrow;
                }
            }

            set { }
        }
    }

    [Table("Borrow")]
    public class Borrow
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Felhasználó")]
        public int UserId { get; set; }
        public virtual UserProfile User { get; set; }

        [Required]
        [Display(Name = "Készülék")]
        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }
        
        [Required]
        [Display(Name = "Doboz kiadva")]
        public bool IsBoxOut { get; set; }

        [Required]
        [Display(Name = "Töltő kiadva")]
        public bool IsChargerOut { get; set; }

        [Required]
        [Display(Name = "Kölcsönzés kezdete")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Megjegyzés")]
        public String Note { get; set; }

        [Required]
        [Display(Name = "Határidő")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Deadline { get; set; }

        [Display(Name = "Kölcsönzés vége")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public System.Nullable<DateTime> EndDate { get; set; }
    }

    [Table("PreBorrow")]
    public class PreBorrow
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Felhasználó")]
        public int UserId { get; set; }
        public virtual UserProfile User { get; set; }

        [Required]
        [Display(Name = "Készülék típusa")]
        public int DeviceTypeId { get; set; }
        public virtual DeviceType DeviceType { get; set; }

        [Required]
        [Display(Name = "Előfoglalás időpontja")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm}")]
        public DateTime Date { get; set; }
    }


    #region View Models

    public class ChartModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class StatisticsModel
    {
        public IEnumerable<ChartModel> DevicePopularity { get; set; }
        public IEnumerable<ChartModel> UserActivity { get; set; }
        public IEnumerable<ChartModel> UserDelay { get; set; }
    }

    public class BorrowCommon
    {
        [Required(ErrorMessage = "A készülék kiválasztása kötelező!")]
        [RegularExpression(@"[1-9]*$", ErrorMessage = "Készülék kiválasztása kötelező!")]
        public int DeviceId { get; set; }

        [Required(ErrorMessage = "A kiadás időpontjának megadása kötelező!")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "A határidő megadása kötelező!")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime Deadline { get; set; }

        public bool IsChargerOut { get; set; }

        public bool IsBoxOut { get; set; }

        public string Note { get; set; }
    }

    public class BorrowForPreBorrow : BorrowCommon
    {
        [Required(ErrorMessage="Az előfoglalás kiválasztása kötelező!")]
        [RegularExpression(@"[1-9][0-9]*$", ErrorMessage = "Előfoglalás kiválasztása kötelező!")]
        public int PreBorrowId { get; set; }
    }

    public class BorrowForNewUser : BorrowCommon
    {
        [Required(ErrorMessage="A keresztnév megadása kötelező!")]
        public string FirstName { get; set; }

        [Required(ErrorMessage="A vezetéknév megadása kötelező!")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage="Az e-mail cím megadása kötelező!")]
        [RegularExpression(@"([^.@]+)(\.[^.@]+)*@([^.@]+\.)+([^.@]+)", ErrorMessage = "Érvénytelen e-mail cím!")]
        public string UserName { get; set; }
    }

    public class BorrowForExistingUser : BorrowCommon
    {
        [Required(ErrorMessage = "A felhasználó kiválasztása kötelező!")]
        [RegularExpression(@"[1-9][0-9]*$", ErrorMessage = "A felhasználó kiválasztása kötelező!")]
        public int UserId { get; set; }

    }

    public class RoleModel
    {
        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Az e-mail cím megadása kötelező!")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "E-mail cím")]
        [RegularExpression(@"([^.@]+)(\.[^.@]+)*@([^.@]+\.)+([^.@]+)", ErrorMessage = "Érvénytelen e-mail cím!")]
        [System.Web.Mvc.Remote("IsUserNameUnique", "Account", AdditionalFields = "UserId", ErrorMessage = "A megadott e-mail cím már létezik!", HttpMethod = "POST")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Az keresztnév megadása kötelező!")]
        [Display(Name = "Keresztnév")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Az vezetéknév megadása kötelező!")]
        [Display(Name = "Vezetéknév")]
        public string LastName { get; set; }

        [Display(Name = "Szerepkör")]
        public string Role { get; set; }

        [Display(Name = "Adminisztrátori jogosultság")]
        public bool IsAdmin { get; set; }

        public string Avatar { get; set; }

        public RoleModel(UserProfile user)
        {
            this.UserId = user.UserId;
            this.UserName = user.UserName;
            this.LastName = user.LastName;
            this.FirstName = user.FirstName;
        }

        public RoleModel() { }
    }

    public class MyBorrowModel
    {
        public IEnumerable<PreBorrow> PreBorrows { get; set; }
        public IEnumerable<Borrow> Borrows { get; set; }
    }

    public class DeviceSelectModel
    {
        public int Id { get; set; }
        public int DeviceTypeId { get; set; }
        public string Name { get; set; }
    }

    #endregion
}