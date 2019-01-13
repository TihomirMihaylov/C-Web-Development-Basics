using System;
using System.Collections.Generic;

namespace CakesWebApp.Models
{
    public class Order : BaseModel<int>
    {
        public int UserId { get; set; }
        public virtual User User { get; set; } //virtual за да направи EF още една заявка към базата и да види кой е юзъра

        public DateTime DateOfCreation { get; set; } = DateTime.UtcNow;

        //поле IsDeleted - това се нарича Soft delete. Данните никога не се трият от базата

        public virtual ICollection<OrderProduct> Products { get; set; } = new HashSet<OrderProduct>();
    }
}