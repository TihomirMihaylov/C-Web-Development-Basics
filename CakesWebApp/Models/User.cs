using System;
using System.Collections.Generic;

namespace CakesWebApp.Models
{
    public class User : BaseModel<int>
    {
        //[NotMapped] - няма да влезне в базата. Остава си само като поле тук
        public string Name { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public DateTime DateOfRegistration { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}