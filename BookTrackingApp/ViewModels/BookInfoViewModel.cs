using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BookTrackingApp.Models;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookTrackingApp.ViewModels
{
    public class BookInfoViewModel
    {
        booktrackingdbEntities db = new booktrackingdbEntities();

        public List<Book> BookListing { get; set; }

        public int stu_id { get; set; }
        public int adm_id { get; set; }
        public string usernm { get; set; }
        public string pw { get; set; }

        public Boolean display { get; set; }

        public int BookID { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Dept { get; set; }
        public int ClassID { get; set; }
        public int Quantity { get; set; }
        public int ClassName { get; set; }

        public List<Order> thisStuOrderListing { get; set; }
    }
}