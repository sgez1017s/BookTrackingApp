//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BookTrackingApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Teacher
    {
        public int TeacherID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public Nullable<int> classIDOne { get; set; }
        public Nullable<int> classIDTwo { get; set; }
        public Nullable<int> classIDThree { get; set; }
        public string Dept { get; set; }
    }
}
