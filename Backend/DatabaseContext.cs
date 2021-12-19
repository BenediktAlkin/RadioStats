using Backend.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.IO;

namespace Backend
{
    public class DatabaseContext : DbContext
    {
        private const string DB_FILE = @"RadioStats.sqlite";

        public DatabaseContext(bool dropDb=false) : base(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite($"Data Source={DB_FILE}").Options)
        {
            if (dropDb)
                Database.EnsureDeleted();
            Database.EnsureCreated();

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Song-Artist relation has PrimaryArtist and AllArtist relations
            // so it can't be derived implicitly
            builder.Entity<Song>()
                .HasMany(s => s.Artists)
                .WithMany(a => a.Songs);
            builder.Entity<Song>()
                .HasOne(s => s.PrimaryArtist)
                .WithMany(a => a.PrimaryArtistSongs)
                .HasForeignKey(s => s.PrimaryArtistId);
        }

        public DbSet<Song> Songs { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Artist> Artists { get; set; }

    }
}
