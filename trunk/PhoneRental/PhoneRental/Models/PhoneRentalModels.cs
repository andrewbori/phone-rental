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
        public DateTime EndDate { get; set; }
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
}