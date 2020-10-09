using System;


namespace Task4.Models
{
    public class User
    {
        public User()
        {
            AuthDate = DateTime.Now.ToLongDateString();
            LogDate = AuthDate;
        }
        public int Id { get; set; }
        public bool Check { get; set; }
        public bool IsBlocked { get; set; }
        public string Email { get; set; }
        public string AuthDate { get; set; }
        public string LogDate { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
