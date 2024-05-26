using Microsoft.EntityFrameworkCore;
using MTUModelContainer.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTUTxService
{
    internal class ApplicationContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                Program.serviceConfig.ConnectionString,
                ServerVersion.AutoDetect(Program.serviceConfig.ConnectionString)
            );
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
    }
}
