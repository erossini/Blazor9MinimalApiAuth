using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MinimalApiAuth.Persistence
{
    /// <summary>
    /// Persistence Service Registration
    /// </summary>
    public static class PersistenceServiceRegistration
    {
        /// <summary>
        /// Default Connection Name
        /// </summary>
        public const string ConnectionName = "DefaultConnection";

        /// <summary>
        /// Add Persistence Service
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="cnnStringName">Name of the CNN string.</param>
        /// <param name="cnnString">The connection string.</param>
        /// <returns></returns>
        public static IServiceCollection AddAppPersistenceServices(this IServiceCollection services,
            IConfiguration configuration,
            string cnnStringName = ConnectionName,
            string cnnString = null)
        {
            if (cnnString == null)
                cnnString = configuration.GetConnectionString(cnnStringName);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(cnnString)
            );

            return services;
        }
    }
}