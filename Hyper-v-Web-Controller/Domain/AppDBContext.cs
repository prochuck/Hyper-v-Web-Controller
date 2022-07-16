﻿using Hyper_v_Web_Controller.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyper_v_Web_Controller
{
    public class AppDBContext: DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<VMImage> VMImages { get; set; }
        public DbSet<VM> VMs { get; set; }
       
        
        public AppDBContext(DbContextOptions options):base(options)
        {
           if(Database.EnsureCreated())
            {
                Roles.Add(new Role() { RoleName = "Admin" });
                Roles.Add(new Role() { RoleName = "User" });
                Users.Add(new User() { RoleId=1,Login="admin",PasswordHash="123".GetHashCode().ToString()});
                this.SaveChanges();
            }
        }
    }
}