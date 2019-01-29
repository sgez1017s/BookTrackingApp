using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using BookTrackingApp;
using BookTrackingApp.Models;
using BookTrackingApp.ViewModels;
using System.Data.Entity.Validation;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Net.Mail;

namespace BookTrackingApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new StudentInfoViewModel();

            model.StudentListing = db.Students.Where(x => x.FirstName == "Pete").ToList();

            return View(model);
        }

        public ActionResult StudentLogin(string status)
        {
            ViewBag.Message = "Student Login.";

            if (status == "login failed")
            {
                TempData["error"] = "Login failed! Please try again.";
            }

            return View();
        }

        public ActionResult StudentHomePage(string username, string password, string stuid, string requestAdded, string pendingCancelled)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            if (requestAdded == "yes")
            {
                TempData["confirm"] = "Book request added.";
            }
            if (pendingCancelled == "yes")
            {
                TempData["confirm"] = "Book request cancelled.";
            }

            if (stuid != null)
            {
                int id = Convert.ToInt32(stuid);
                username = db.Students.Where(x => x.StudentID == id).FirstOrDefault().email;
                password = db.Students.Where(x => x.StudentID == id).FirstOrDefault().password;
            }

            var model = new StudentInfoViewModel();

            model.StudentListing = db.Students.Where(x => x.email == username && x.password == password).ToList();

            if (model.StudentListing.Count() == 0)
            {
                return RedirectToAction("StudentLogin", new { status = "login failed" });
            }
            else
            {
                ViewBag.firstname = model.StudentListing.FirstOrDefault().FirstName;
                ViewBag.lastname = model.StudentListing.FirstOrDefault().LastName;
                ViewBag.email = model.StudentListing.FirstOrDefault().email;
                ViewBag.studentid = model.StudentListing.FirstOrDefault().StudentID;
                ViewBag.classone = model.StudentListing.FirstOrDefault().ClassOne;
                ViewBag.classtwo = model.StudentListing.FirstOrDefault().ClassTwo;
                ViewBag.classthree = model.StudentListing.FirstOrDefault().ClassThree;

                model.StudentID = model.StudentListing.FirstOrDefault().StudentID;
                int id = ViewBag.studentid;

                model.OrderListing = db.Orders.Where(x => x.OStudentID == id).ToList();
                model.OrderListing = model.OrderListing.Where(x => x.Status == "Pending" || x.Status == "Approved" || x.Status == "Declined").ToList();
                model.CheckedOutListing = db.Orders.Where(x => x.OStudentID == id && x.Status == "Checked Out").ToList();
                model.BookListing = db.Books.ToList();

                return View(model);
            }
        }

        public ActionResult StudentSearchBooks(int id, string searchString)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new BookInfoViewModel();

            model.stu_id = id;

            if (!String.IsNullOrEmpty(searchString))
            {
                model.BookListing = db.Books.Where(x => x.BookName.Contains(searchString)).ToList();
                if (model.BookListing.Count == 0)
                {
                    model.BookListing = db.Books.Where(x => x.Author.Contains(searchString)).ToList();
                }
                if (model.BookListing.Count == 0)
                {
                    model.BookListing = db.Books.Where(x => x.ISBN.Contains(searchString)).ToList();
                }
            }
            else
            {
                model.BookListing = db.Books.ToList();
            }

            model.thisStuOrderListing = db.Orders.Where(x => x.OStudentID == id).ToList();
            model.thisStuOrderListing = model.thisStuOrderListing.Where(x => x.Status == "Pending" || x.Status == "Approved" || x.Status == "Declined" || x.Status == "Checked Out").ToList();

            model.display = true;

            return View(model);
        }

        //request
        public ActionResult RequestBook(int stuid, int bookid, int classid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new OrderInfoViewModel();

            model.stu_id = stuid;

            int orderid;
            if (db.Orders.ToList().Count == 0)
            {
                orderid = 1;
            }
            else
            {
                orderid = db.Orders.ToList().Last().OrderID + 1;
            }

            db.Orders.Add(new Order
            {
                OrderID = orderid,
                OStudentID = stuid,
                OBookID = bookid,
                OClassID = classid,
                Status = "Pending"
            });
            db.SaveChanges();

            return RedirectToAction("sendRequestEmail", new { stuid = model.stu_id, requestAdded = "yes", bookid = bookid, classid = classid });
        }

        //email
        public ActionResult sendRequestEmail(int stuid, string requestAdded, int bookid, int classid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new StudentInfoViewModel();

            var theTeather = db.Teachers.Where(x => x.classIDOne == classid || x.classIDTwo == classid || x.classIDThree == classid).FirstOrDefault();
            var theStu = db.Students.Where(x => x.StudentID == stuid).FirstOrDefault();

            string teaName = "Dear " + theTeather.FirstName + " " + theTeather.LastName + ",";
            string teaEmail = theTeather.email;

            string stuName = theStu.FirstName + " " + theStu.LastName;


            string bookName = db.Books.Where(x => x.BookID == bookid).Select(x => x.BookName).FirstOrDefault();
            string isbn = db.Books.Where(x => x.BookID == bookid).Select(x => x.ISBN).FirstOrDefault();

            string subject = "Book Request Received - High School Textbook Services";

            string body = teaName + "\r\n\r\nYou received a book request, please login to your account to view more information."
                + "\r\n\r\nStudent Name: " + stuName + "\r\nBook Requested: " + bookName + "\r\nISBN: " + isbn;

            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("sgez1017s@gmail.com", "sdfg4ESZxll");

            MailMessage mm = new MailMessage("donotreply@domain.com", teaEmail, subject, body);
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);

            return RedirectToAction("StudentHomePage", new { stuid = stuid, requestAdded = requestAdded });
        }

        //cancel
        public ActionResult CancelPending(int stuid, int bookid, int classid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new OrderInfoViewModel();

            model.stu_id = stuid;
            model.OrderID = db.Orders.Where(x => x.OStudentID == stuid && x.OBookID == bookid).Select(x => x.OrderID).FirstOrDefault();

            db.Orders.Remove(db.Orders.Find(model.OrderID));
            db.SaveChanges();

            return RedirectToAction("sendCancelEmail", new { stuid = model.stu_id, pendingCancelled = "yes", bookid = bookid, classid = classid });
        }

        //email
        public ActionResult sendCancelEmail(int stuid, string pendingCancelled, int bookid, int classid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new StudentInfoViewModel();

            var theTeather = db.Teachers.Where(x => x.classIDOne == classid || x.classIDTwo == classid || x.classIDThree == classid).FirstOrDefault();
            var theStu = db.Students.Where(x => x.StudentID == stuid).FirstOrDefault();

            string teaName = "Dear " + theTeather.FirstName + " " + theTeather.LastName + ",";
            string teaEmail = theTeather.email;

            string stuName = theStu.FirstName + " " + theStu.LastName;


            string bookName = db.Books.Where(x => x.BookID == bookid).Select(x => x.BookName).FirstOrDefault();
            string isbn = db.Books.Where(x => x.BookID == bookid).Select(x => x.ISBN).FirstOrDefault();

            string subject = "Book Request Cancelled - High School Textbook Services";

            string body = teaName + "\r\n\r\nThe following book request has been cancelled, please login to your account to view more information."
                + "\r\n\r\nStudent Name: " + stuName + "\r\nBook Requested: " + bookName + "\r\nISBN: " + isbn;

            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("sgez1017s@gmail.com", "sdfg4ESZxll");

            MailMessage mm = new MailMessage("donotreply@domain.com", teaEmail, subject, body);
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);

            return RedirectToAction("StudentHomePage", new { stuid = stuid, pendingCancelled = pendingCancelled });
        }

        public ActionResult TeacherLogin(string status)
        {
            ViewBag.Message = "Teacher Login";

            if (status == "login failed")
            {
                TempData["error"] = "Login failed! Please try again.";
            }

            return View();
        }

        public ActionResult TeacherHomePage(string username, string password, string teaid, string requestApproved, string requestDeclined, string declinedReleased)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            if (requestApproved == "yes")
            {
                TempData["confirm"] = "Request has been approved.";
            }
            if (requestDeclined == "yes")
            {
                TempData["confirm"] = "Request has been declined.";
            }
            if (declinedReleased == "yes")
            {
                TempData["confirm"] = "Declined request has been released, now you can approve the request.";
            }

            if (teaid != null)
            {
                int id = Convert.ToInt32(teaid);
                username = db.Teachers.Where(x => x.TeacherID == id).FirstOrDefault().email;
                password = db.Teachers.Where(x => x.TeacherID == id).FirstOrDefault().password;
            }

            var model = new TeacherInfoViewModel();

            model.TeacherListing = db.Teachers.Where(x => x.email == username && x.password == password).ToList();

            if (model.TeacherListing.Count() == 0)
            {
                return RedirectToAction("TeacherLogin", new { status = "login failed" });
            }
            else
            {
                ViewBag.firstname = model.TeacherListing.FirstOrDefault().FirstName;
                ViewBag.lastname = model.TeacherListing.FirstOrDefault().LastName;
                ViewBag.email = model.TeacherListing.FirstOrDefault().email;
                ViewBag.teacherid = model.TeacherListing.FirstOrDefault().TeacherID;

                model.TeacherID = model.TeacherListing.FirstOrDefault().TeacherID;
                int id = ViewBag.teacherid;

                ViewBag.classIDOne = model.TeacherListing.FirstOrDefault().classIDOne;
                ViewBag.classIDTwo = model.TeacherListing.FirstOrDefault().classIDTwo;
                ViewBag.classIDThree = model.TeacherListing.FirstOrDefault().classIDThree;

                model.OrderListing = db.Orders.Where(x => x.Status == "Pending" || x.Status == "Approved").ToList();
                model.OrderListing = model.OrderListing.Where(x => x.OClassID == ViewBag.classIDOne || x.OClassID == ViewBag.classIDTwo || x.OClassID == ViewBag.classIDThree).ToList();

                model.DeclinedListing = db.Orders.Where(x => x.Status == "Declined").ToList();
                model.DeclinedListing = model.DeclinedListing.Where(x => x.OClassID == ViewBag.classIDOne || x.OClassID == ViewBag.classIDTwo || x.OClassID == ViewBag.classIDThree).ToList();

                model.StudentListing = db.Students.ToList();
                model.BookListing = db.Books.ToList();

                return View(model);
            }
        }

        //approve
        public ActionResult ApprovePending(int teaid, int orderid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new OrderInfoViewModel();
            var selectedOrder = db.Orders.Where(x => x.OrderID == orderid).FirstOrDefault();
            selectedOrder.Status = "Approved";
            selectedOrder.OTeacherID = teaid;
            selectedOrder.DueDate = DateTime.Now.AddDays(7); ;
            var selectedBook = db.Books.Where(x => x.BookID == selectedOrder.OBookID).FirstOrDefault();
            selectedBook.Quantity--;

            db.SaveChanges();

            return RedirectToAction("sendApproveEmail", new { teaid = teaid, requestApproved = "yes", orderid = orderid });
        }

        //email
        public ActionResult sendApproveEmail(int teaid, string requestApproved, int orderid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new StudentInfoViewModel();

            var theTeather = db.Teachers.Where(x => x.TeacherID == teaid).FirstOrDefault();
            var theOrder = db.Orders.Where(x => x.OrderID == orderid).FirstOrDefault();
            var theStu = db.Students.Where(x => x.StudentID == theOrder.OStudentID).FirstOrDefault();

            int bookid = theOrder.OBookID;

            string teaName = theTeather.FirstName + " " + theTeather.LastName;
            string stuEmail = theStu.email;

            string stuName = "Dear " + theStu.FirstName + " " + theStu.LastName + ",";


            string bookName = db.Books.Where(x => x.BookID == bookid).Select(x => x.BookName).FirstOrDefault();
            string isbn = db.Books.Where(x => x.BookID == bookid).Select(x => x.ISBN).FirstOrDefault();

            string subject = "Book Request Approved - High School Textbook Services";

            string body = stuName + "\r\n\r\nThe following book request has been approved, you can check out your book now."
                + "\r\n\r\nApproved By: " + teaName + "\r\nBook Approved: " + bookName + "\r\nISBN: " + isbn;

            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("sgez1017s@gmail.com", "sdfg4ESZxll");

            MailMessage mm = new MailMessage("donotreply@domain.com", stuEmail, subject, body);
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);

            return RedirectToAction("TeacherHomePage", new { teaid = teaid, requestApproved = requestApproved });
        }

        //decline
        public ActionResult DeclinePending(int teaid, int orderid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new OrderInfoViewModel();
            var selectedOrder = db.Orders.Where(x => x.OrderID == orderid).FirstOrDefault();
            selectedOrder.Status = "Declined";
            selectedOrder.OTeacherID = teaid;
            db.SaveChanges();

            return RedirectToAction("sendDeclineEmail", new { teaid = teaid, requestDeclined = "yes", orderid = orderid });
        }

        //email
        public ActionResult sendDeclineEmail(int teaid, string requestDeclined, int orderid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new StudentInfoViewModel();

            var theTeather = db.Teachers.Where(x => x.TeacherID == teaid).FirstOrDefault();
            var theOrder = db.Orders.Where(x => x.OrderID == orderid).FirstOrDefault();
            var theStu = db.Students.Where(x => x.StudentID == theOrder.OStudentID).FirstOrDefault();

            int bookid = theOrder.OBookID;

            string teaName = theTeather.FirstName + " " + theTeather.LastName;
            string stuEmail = theStu.email;
            string teaEmail = theTeather.email;

            string stuName = "Dear " + theStu.FirstName + " " + theStu.LastName + ",";


            string bookName = db.Books.Where(x => x.BookID == bookid).Select(x => x.BookName).FirstOrDefault();
            string isbn = db.Books.Where(x => x.BookID == bookid).Select(x => x.ISBN).FirstOrDefault();

            string subject = "Book Request Declined - High School Textbook Services";

            string body = stuName + "\r\n\r\nThe following book request has been declined, contact the teacher for more infomation."
                + "\r\n\r\nDeclined By: " + teaName + "\r\nTeacher Email: " + teaEmail + "\r\nBook Declined: " + bookName + "\r\nISBN: " + isbn;

            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("sgez1017s@gmail.com", "sdfg4ESZxll");

            MailMessage mm = new MailMessage("donotreply@domain.com", stuEmail, subject, body);
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);

            return RedirectToAction("TeacherHomePage", new { teaid = teaid, requestDeclined = requestDeclined });
        }

        //release
        public ActionResult ReleaseDeclinedOrder(int teaid, int orderid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new OrderInfoViewModel();
            var selectedOrder = db.Orders.Where(x => x.OrderID == orderid).FirstOrDefault();
            selectedOrder.Status = "Pending";
            selectedOrder.OTeacherID = null;
            db.SaveChanges();

            return RedirectToAction("sendReleaseEmail", new { teaid = teaid, declinedReleased = "yes", orderid = orderid });
        }

        //email
        public ActionResult sendReleaseEmail(int teaid, string declinedReleased, int orderid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new StudentInfoViewModel();

            var theTeather = db.Teachers.Where(x => x.TeacherID == teaid).FirstOrDefault();
            var theOrder = db.Orders.Where(x => x.OrderID == orderid).FirstOrDefault();
            var theStu = db.Students.Where(x => x.StudentID == theOrder.OStudentID).FirstOrDefault();

            int bookid = theOrder.OBookID;

            string teaName = theTeather.FirstName + " " + theTeather.LastName;
            string stuEmail = theStu.email;
            string teaEmail = theTeather.email;

            string stuName = "Dear " + theStu.FirstName + " " + theStu.LastName + ",";


            string bookName = db.Books.Where(x => x.BookID == bookid).Select(x => x.BookName).FirstOrDefault();
            string isbn = db.Books.Where(x => x.BookID == bookid).Select(x => x.ISBN).FirstOrDefault();

            string subject = "Book Declined Request Released - High School Textbook Services";

            string body = stuName + "\r\n\r\nThe following declined book request has been released, now the request can be approved."
                + "\r\n\r\nReleased By: " + teaName + "\r\nTeacher Email: " + teaEmail + "\r\nBook Name: " + bookName + "\r\nISBN: " + isbn;

            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("sgez1017s@gmail.com", "sdfg4ESZxll");

            MailMessage mm = new MailMessage("donotreply@domain.com", stuEmail, subject, body);
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);

            return RedirectToAction("TeacherHomePage", new { teaid = teaid, declinedReleased = declinedReleased });
        }

        public ActionResult AdminLogin(string status)
        {
            ViewBag.Message = "Admin Login";

            if (status == "login failed")
            {
                TempData["error"] = "Login failed! Please try again.";
            }

            return View();
        }

        public ActionResult AdminHomePage(string username, string password, string admid, string checkedOut, string returned, string emailSent, string balanceUpdated)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            if (checkedOut == "yes")
            {
                TempData["confirm"] = "Book has been checked out successfully.";
            }
            if (returned == "yes")
            {
                TempData["confirm"] = "Book has been returned successfully.";
            }
            if (emailSent == "yes")
            {
                TempData["confirm"] = "Emails have been sent to students with late book orders.";
            }
            if (balanceUpdated == "yes")
            {
                TempData["confirm"] = "Balance has been updated on student account with late orders.";
            }

            if (admid != null)
            {
                int id = Convert.ToInt32(admid);
                username = db.Admins.Where(x => x.AdminID == id).FirstOrDefault().email;
                password = db.Admins.Where(x => x.AdminID == id).FirstOrDefault().password;
            }

            var model = new AdminViewModels();

            model.AdminListing = db.Admins.Where(x => x.email == username && x.password == password).ToList();

            if (model.AdminListing.Count() == 0)
            {
                return RedirectToAction("AdminLogin", new { status = "login failed" });
            }
            else
            {
                ViewBag.firstname = model.AdminListing.FirstOrDefault().FirstName;
                ViewBag.lastname = model.AdminListing.FirstOrDefault().LastName;
                ViewBag.email = model.AdminListing.FirstOrDefault().email;
                ViewBag.adminid = model.AdminListing.FirstOrDefault().AdminID;

                model.AdminID = model.AdminListing.FirstOrDefault().AdminID;
                int id = ViewBag.adminid;

                model.ApprovedListing = db.Orders.Where(x => x.Status == "Approved").ToList();
                model.CheckedOutListing = db.Orders.Where(x => x.Status == "Checked Out").ToList();

                model.StudentListing = db.Students.ToList();
                model.BookListing = db.Books.ToList();

                return View(model);
            }
        }

        public ActionResult checkoutBook(int admid, int orderid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new OrderInfoViewModel();
            var selectedOrder = db.Orders.Where(x => x.OrderID == orderid).FirstOrDefault();
            selectedOrder.Status = "Checked Out";
            selectedOrder.OAdminID = admid;
            db.SaveChanges();

            return RedirectToAction("AdminHomePage", new { admid = admid, checkedOut = "yes" });
        }

        public ActionResult returnBook(int admid, int orderid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new OrderInfoViewModel();
            var selectedOrder = db.Orders.Where(x => x.OrderID == orderid).FirstOrDefault();
            selectedOrder.Status = "Returned";
            selectedOrder.OAdminID = admid;
            selectedOrder.Balance = null;
            selectedOrder.ReturnDate = DateTime.Now;
            var selectedBook = db.Books.Where(x => x.BookID == selectedOrder.OBookID).FirstOrDefault();
            selectedBook.Quantity++;

            db.SaveChanges();

            return RedirectToAction("AdminHomePage", new { admid = admid, returned = "yes" });
        }

        public ActionResult viewHistory(int admid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new OrderInfoViewModel();

            model.adm_id = admid;
            model.OrderListing = db.Orders.Where(x => x.Status == "Returned").ToList();
            model.StudentListing = db.Students.ToList();
            model.TeacherListing = db.Teachers.ToList();
            model.AdminListing = db.Admins.ToList();
            model.BookListing = db.Books.ToList();

            return View(model);
        }

        public ActionResult viewStudent(int admid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new StudentInfoViewModel();

            model.StudentListing = db.Students.ToList();

            return View(model);
        }

        public ActionResult viewTeacher(int admid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new TeacherInfoViewModel();

            model.TeacherListing = db.Teachers.ToList();

            return View(model);
        }

        public ActionResult bookInventory(int admid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new BookInfoViewModel();

            model.BookListing = db.Books.ToList();

            return View(model);
        }

        

        public ActionResult addBooks(int id)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new BookInfoViewModel();

            model.adm_id = id;

            return View(model);
        }


        public ActionResult sendEmail(int admid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new StudentInfoViewModel();

            var openOrder = db.Orders.Where(x => x.Status == "Approved" || x.Status == "Checked Out").ToList();
            var lateOrder = openOrder.Where(x => x.DueDate.Value < DateTime.Now).ToList();

            int lateCount = lateOrder.Count();

            for (int i = 0; i < lateCount; i++)
            {
                int stuID = lateOrder[i].OStudentID;
                var lateStu = db.Students.Where(x => x.StudentID == stuID).FirstOrDefault();
                string lateStuEmail = lateStu.email;
                int bookID = lateOrder[i].OBookID;
                string bookName = db.Books.Where(x => x.BookID == bookID).Select(x => x.BookName).FirstOrDefault();
                string dueDate = lateOrder[i].DueDate.ToString();
                dueDate = dueDate.Substring(0, dueDate.Length - 12);
                string stuName = "Dear " + lateStu.FirstName + " " + lateStu.LastName + ",";

                string subject = "Book Due - High School Textbook Services";

                string body = stuName + "\r\n\r\nYour textbook is already due, please return the following book to Admin in library."
                    + "\r\n\r\nBook Due: " + bookName + "\r\nDue Date: " + dueDate;

                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.Host = "smtp.gmail.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential("sgez1017s@gmail.com", "sdfg4ESZxll");

                MailMessage mm = new MailMessage("donotreply@domain.com", lateStuEmail, subject, body);
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                client.Send(mm);
            }

            return RedirectToAction("AdminHomePage", new { admid = admid, emailSent = "yes" });
        }

        public ActionResult updateBalance(int admid)
        {
            booktrackingdbEntities db = new booktrackingdbEntities();

            var model = new StudentInfoViewModel();

            var ordersToUpdate = db.Orders.Where(x => x.DueDate.Value < DateTime.Now).ToList();

            for (int i = 0; i < ordersToUpdate.Count; i++)
            {
                ordersToUpdate[i].Balance = DateTime.Now.Subtract(ordersToUpdate[i].DueDate.Value).Days;
                db.SaveChanges();
            }

            return RedirectToAction("AdminHomePage", new { admid = admid, balanceUpdated = "yes" });
        }

    }
}