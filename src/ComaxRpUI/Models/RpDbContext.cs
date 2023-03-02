using ComaxRpUI.Models.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Reflection.Emit;

namespace ComaxRpUI.Models
{
    public class RpDbContext : DbContext
    {
        internal readonly DbConf configs;

        public RpDbContext(IOptionsMonitor<DbConf> options) : base()
        {
            configs = options.CurrentValue;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            DbConfiguration.Setup(builder);
        }

        //the below line was added because accessing the dbsets from controllers where not working
        //when added, everything worked well.. to be validated
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (configs.MemoryDb)
            {
                optionsBuilder.UseInMemoryDatabase("AccountsDb");
            }
            else
            {
                var cs = $"server={configs.Server};port={configs.Port};user={configs.Username};password={configs.Password};database={configs.Database}";
                var version = ServerVersion.AutoDetect(cs);
                optionsBuilder.UseMySql(cs, version, mysqloptions =>
                        mysqloptions.UseNewtonsoftJson(MySqlCommonJsonChangeTrackingOptions.RootPropertyOnly)
                    )
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableDetailedErrors();
            }
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

        }
    }
}
