using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

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

        [Required]
        [Display(Name = "Márka")]
        public int BrandId { get; set; }
        public virtual Brand Brand { get; set; }

        [Required]
        [Display(Name = "Típus")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "AAIT azonosító mintája")]
        public string AaitIdPattern { get; set; }

        [Display(Name = "Kép a készülékről")]
        public string ImageUrl { get; set; }

        #region NotMapped
        [NotMapped]
        public string DeviceBrandAndType
        {
            get
            {
                return Brand.Name + " " + Type;
            }
        }

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
        [Display(Name = "Elérhetősége")]
        public string Availability
        {
            get
            {
                int borrowed;
                int preBorrowed;
                string retval;
                DateTime end;
                if (Devices.Count > 0)
                {
                    using (var db = new PhoneRentalContext())
                    {
                        borrowed = (from borrow in db.Borrows
                                    where (borrow.Device.DeviceTypeId == Id) &
                                    (borrow.EndDate == null)
                                    select borrow.DeviceId).Count();
                        preBorrowed = (from preb in db.PreBorrows
                                       where (preb.DeviceTypeId == Id)
                                       select preb.DeviceTypeId).Count();

                        if (Devices.Count - (borrowed + preBorrowed) > 0)
                        {
                            retval = "Azonnal elvihető";
                        }
                        else if (preBorrowed < Devices.Count)
                        {
                            var x = preBorrowed - (Devices.Count - borrowed);
                            end = (from borrow in db.Borrows
                                        where (borrow.Device.DeviceTypeId == Id)
                                        orderby borrow.Deadline
                                        select borrow.Deadline).ToList().ElementAt(x);

                            retval = end.ToString();
                        }
                        else
                        {
                            retval = "Jelenleg minden készülékre van előfoglalás";
                        }
                    }
                }
                else
                {
                    retval = "Ebből a készülékből nincs raktárkészlet";
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

        [Required]
        [Display(Name = "Készülék típusa")]
        public int DeviceTypeId { get; set; }
        public virtual DeviceType DeviceType { get; set; }

        [Required]
        [Display(Name = "IMEI")]
        public string Imei { get; set; }

        [Required]
        [Display(Name = "AAIT azonosító")]
        [DisplayFormat(DataFormatString = "{0:D3}", ApplyFormatInEditMode = true)]
        public int AaitIdNumber { get; set; }
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

        [NotMapped]
        public string UserAndDeviceName
        {
            get
            {
                string s;
                s = User.LastName;
                s += " ";
                s += User.FirstName;
                s += " (";
                s += User.UserName;
                s += ") - ";
                s += DeviceType.Brand.Name;
                s += " ";
                s += DeviceType.Type;
                return s;
            }
        }
    }
}