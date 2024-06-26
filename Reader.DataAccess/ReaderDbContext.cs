﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reader.Domain;

namespace Reader.DataAccess
{
    public class ReaderDbContext : IdentityDbContext<AppUser>
    {
        public ReaderDbContext(DbContextOptions<ReaderDbContext> options) : base(options)
        { }


        public DbSet<ReadBook> ReadBooks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ReadBook>(e =>
            {
                e.HasKey(k => k.Id);
                e.Property(p => p.Name).IsRequired(true);
                e.Property(p => p.GoogleId).IsRequired(true);
                e.Property(p => p.StartDate).IsRequired(false);
                e.Property(p => p.EndDate).IsRequired(false);
            });
        }
    }
}
