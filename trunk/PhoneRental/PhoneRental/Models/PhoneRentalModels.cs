using System;
using System.Collections.Generic;
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
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Borrow> Borrows { get; set; }
        public DbSet<PreBorrow> PreBorrows { get; set; }
    }

    public class DeviceType
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }
        public string AaitIdPattern { get; set; }
        public string ImageUrl { get; set; }
        public virtual ICollection<Device> Devices { get; set; }
    }

    public class Device
    {
        public int Id { get; set; }
        public int DeviceTypeId { get; set; }
        public virtual DeviceType DeviceType { get; set; }
        public string Imei { get; set; }
        public int AaitIdNumber { get; set; }
    }

    public class Borrow
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserProfile User { get; set; }
        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime Deadline { get; set; }
    }

    public class PreBorrow
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserProfile User { get; set; }
        public int DeviceTypeId { get; set; }
        public virtual DeviceType DeviceType { get; set; }
        public DateTime Date { get; set; }
    }
}