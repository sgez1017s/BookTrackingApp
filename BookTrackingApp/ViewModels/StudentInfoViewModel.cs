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
    public class StudentInfoViewModel
    {
        booktrackingdbEntities db = new booktrackingdbEntities();

        public List<Student> StudentListing { get; set; }

        public int StudentID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string ClassOne { get; set; }
        public string ClassTwo { get; set; }
        public string ClassThree { get; set; }

        public List<Order> OrderListing { get; set; }
        public List<Order> CheckedOutListing { get; set; }

        public int OrderID { get; set; }
        public int OStudentID { get; set; }
        public int OTeacherID { get; set; }
        public int OBookID { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public int OClassID { get; set; }
        public decimal Balance { get; set; }

        public List<Book> BookListing { get; set; }
        public List<Teacher> TeacherListing { get; set; }

        public string BookName { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public int ClassName { get; set; }

    }
}