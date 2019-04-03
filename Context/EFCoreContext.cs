using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using timebot.Classes.Assets;

namespace timebot.Context
{
    public class SchoolContext : DbContext
    {
        public DbSet<Asset> Assets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        { 
            Dictionary<string, string> secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(Program.secrets_file));
            
            optionsBuilder.UseNpgsql(secrets["connection_string"]);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Asset>(entity =>
            {
                entity.Property(e=>e.Attack).HasColumnName("Attack");

            });
        }
    }
}