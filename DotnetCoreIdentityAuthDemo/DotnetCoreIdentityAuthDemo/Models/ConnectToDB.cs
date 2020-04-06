using DotnetCoreIdentityAuthDemo.Models.ExtendUser;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetCoreIdentityAuthDemo.Models
{
    public class ConnectToDB: IdentityDbContext<CustomUser>
    {
        public ConnectToDB(DbContextOptions<ConnectToDB> options)
        : base(options)
        {
        }

        

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    modelBuilder.Seed();
        //}
    }
}
