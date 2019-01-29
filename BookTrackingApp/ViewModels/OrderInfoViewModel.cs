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
    public class OrderInfoViewModel
    {
        booktrackingdbEntities db = new booktrackingdbEntities();

        public List<Order> OrderListing { get; set; }
        public List<Student> StudentListing { get; set; }
        public List<Teacher> TeacherListing { get; set; }
        public List<Admin> AdminListing { get; set; }
        public List<Book> BookListing { get; set; }

        public int stu_id { get; set; }
        public int adm_id { get; set; }
        public string usernm { get; set; }
        public string pw { get; set; }

        public int OrderID { get; set; }
        public int OStudentID { get; set; }
        public int OTeacherID { get; set; }
        public int OBookID { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public int OClassID { get; set; }
        public decimal Balance { get; set; }
        public DateTime ReturnDate { get; set; }
    }
}