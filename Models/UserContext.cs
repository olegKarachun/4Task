﻿using Microsoft.EntityFrameworkCore;
using Task4.Models;


namespace Task4.Models
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public UserContext(DbContextOptions<UserContext> options):base(options)
        {
            Database.EnsureCreated();
        }
    }
}
