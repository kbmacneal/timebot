using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using timebot.Classes;
using timebot.Classes.Assets;
using timebot.Classes.Tags;

namespace timebot.Contexts
{
    public class Context : DbContext
    {
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BlameCal> BlameCals { get; set; }
        public DbSet<botcommand> BotCommands { get; set; }
        public DbSet<Command> Command { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Dictionary<string, string> secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(Program.secrets_file));

            optionsBuilder.UseNpgsql(secrets["connection_string"]);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>(entity =>
           {
               entity.HasKey(e => e.ID);
               entity.Property(e => e.ID).HasColumnName("id").ValueGeneratedOnAdd();
               entity.Property(e => e.Name).HasColumnName("Name");
               entity.Property(e => e.HP).HasColumnName("HP");
               entity.Property(e => e.Attack).HasColumnName("Attack");
               entity.Property(e => e.Counterattack).HasColumnName("Counterattack");
               entity.Property(e => e.Description).HasColumnName("Description");
               entity.Property(e => e.Type).HasColumnName("Type");
               entity.Property(e => e.Tier).HasColumnName("Tier");
               entity.Property(e => e.TechLevel).HasColumnName("TechLevel");
               entity.Property(e => e.Cost).HasColumnName("Cost");
               entity.Property(e => e.AssetType).HasColumnName("AssetType");
           });

            modelBuilder.Entity<Tag>(entity =>
           {
               entity.HasKey(e => e.ID);
               entity.Property(e => e.ID).HasColumnName("id").ValueGeneratedOnAdd();
               entity.Property(e => e.Name).HasColumnName("Name");
               entity.Property(e => e.Description).HasColumnName("Description");
           });

            modelBuilder.Entity<BlameCal>(entity =>
           {
               entity.HasKey(e => e.ID);
               entity.Property(e => e.ID).HasColumnName("ID").ValueGeneratedOnAdd();
               entity.Property(e => e.timestamp).HasColumnName("timestamp");
           });

            modelBuilder.Entity<botcommand>(entity =>
           {
               entity.HasKey(e => e.id);
               entity.Property(e => e.id).HasColumnName("id").ValueGeneratedOnAdd();
               entity.Property(e => e.serverid).HasColumnName("serverid");
               entity.Property(e => e.commandname).HasColumnName("commandname");
           });
        }
    }
}