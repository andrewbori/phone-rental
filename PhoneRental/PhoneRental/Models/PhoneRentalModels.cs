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

                            retval = "Várhatóan: " + end.ToString();
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
        [Display(Name = "Kölcsönzés kezdete")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Határidő")]
        public DateTime Deadline { get; set; }

        [Display(Name = "Kölcsönzés vége")]
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
        public DateTime Date { get; set; }
    }


    #region Statistics View Models

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

    public class RoleModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string Avatar { get; set; }

        public RoleModel(UserProfile user)
        {
            this.UserId = user.UserId;
            this.UserName = user.UserName;
            this.LastName = user.LastName;
            this.FirstName = user.FirstName;
        }
    }

    #endregion
}