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
    public class AdminViewModels
    {
        booktrackingdbEntities db = new booktrackingdbEntities();

        public List<Admin> AdminListing { get; set; }

        public int AdminID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }

        public List<Order> OrderListing { get; set; }
        public List<Order> ApprovedListing { get; set; }
        public List<Order> CheckedOutListing { get; set; }

        


        public List<Student> StudentListing { get; set; }
        public List<Book> BookListing { get; set; }
        public List<Teacher> TeacherListing { get; set; }


    }
}