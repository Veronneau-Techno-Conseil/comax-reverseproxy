
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComaxRpUI.Models.Configurations
{
    /// <summary>
    /// Initial configuration for any extensions added to the default AspNet and openiddict authentication system
    /// </summary>
    public class InitialConfig : IModelConfig
    {
        public void SetupFields(ModelBuilder builder)
        {
            builder.Entity<RpEntry>()
                .HasKey(x => x.Id);

            builder.Entity<RpEntry>().HasIndex(x=>x.Name, "LookupIndex")
                .IsUnique(true)
                .IsDescending(false);

            builder.Entity<RpEntry>().HasIndex(x => x.IngressHost, "UKHost")
                .IsUnique(true)
                .IsDescending(false);

            builder.Entity<RpEntry>()
                .Property(x => x.Id)
                .IsRequired();

            builder.Entity<RpEntry>()
                .Property(x => x.IngressCertSecret)
                .IsRequired();

            builder.Entity<RpEntry>()
                .Property(x => x.ForwardAddress)
                .IsRequired();

            builder.Entity<RpEntry>()
                .Property(x => x.IngressHost)
                .IsRequired();

            builder.Entity<RpEntry>()
                .Property(x => x.Name)
                .IsRequired();

            builder.Entity<RpEntry>()
                .Property(x => x.IngressCertManager)
                .IsRequired(false);

            builder.Entity<RpEntry>()
                .Property(x=>x.CreatedDate)
                .IsRequired();

            builder.Entity<RpEntry>()
                .Property(x => x.ModifiedDate)
                .IsRequired();
        }

        public void SetupRelationships(ModelBuilder builder)
        {
            
        }

        public void SetupTables(ModelBuilder builder)
        {
            builder.Entity<RpEntry>()
                .ToTable("RpEntries");
        }
    }
}
