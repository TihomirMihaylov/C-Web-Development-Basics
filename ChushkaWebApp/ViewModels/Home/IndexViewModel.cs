using System.Collections.Generic;

namespace ChushkaWebApp.ViewModels.Home
{
    public class IndexViewModel
    {
        public ICollection<ProductViewModel> Products { get; set; }
    }
}