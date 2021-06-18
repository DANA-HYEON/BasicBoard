using BasicBoard.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;

namespace BasicBoard.Data
{
    public class BasicboardDbContext : DbContext
    {
        public BasicboardDbContext()
        {
        }

        public BasicboardDbContext(DbContextOptions<BasicboardDbContext> options)
        : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

            optionsBuilder.UseMySql("Server=localhost;Port=3306;Database=BasicBoard;Uid=root;Pwd=1234;", serverVersion);
        }

        public DbSet<User> User { get; set; }
        public DbSet<Board> Board { get; set; }
        public DbSet<Reply> Reply { get; set; }
    }
}
