using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using reportCore.Entities;
using System;
using System.Reflection;

namespace reportInfrastructure.Data
{
    public class MultitenantDbContext : DbContext
    {
        public string idTenant;

        public IConfiguration _configuration { get; }

        public MultitenantDbContext(IHttpContextAccessor accessor, IConfiguration configuration)
        {
            if (accessor != null
                && accessor.HttpContext != null
                && accessor.HttpContext.Request != null
                && accessor.HttpContext.Request.Headers != null
                && !string.IsNullOrEmpty(accessor.HttpContext.Request.Headers["x-tenant-id"]))
            {
                idTenant = accessor.HttpContext.Request.Headers["x-tenant-id"].ToString();
            }
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(getConnectionString());
            base.OnConfiguring(optionsBuilder);
        }

        private string getConnectionString()
        {
            var connectionString = _configuration.GetConnectionString("AppTenantCS");

            var server = Environment.GetEnvironmentVariable("SERVER");
            var user = Environment.GetEnvironmentVariable("USER");
            var password = Environment.GetEnvironmentVariable("PASSWORD");

            connectionString = connectionString.Replace(@"{{SERVER}}", server).Replace(@"{{DATABASE}}", idTenant).Replace(@"{{USER}}", user).Replace(@"{{PASSWORD}}", password);

            return connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

    }
}
