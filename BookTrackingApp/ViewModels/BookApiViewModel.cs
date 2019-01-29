using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookTrackingApp.ViewModels
{
    public class BookApiViewModel
    {
        public int BookID { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Dept { get; set; }
        public int ClassID { get; set; }
        public int Quantity { get; set; }
        public int ClassName { get; set; }
    }
}