using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Calabonga.UnitOfWork.MongoDb
{
    /// <summary>
    /// Extension for <see cref="IServiceCollection"/>
    /// </summary>
    public static class UnitOfWorkServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the unit of work given context as a service in the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        /// <param name="services"></param>
        /// <param name="implementationFactory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddUnitOfWork(this IServiceCollection services, Func<IServiceProvider, IDatabaseSettings> implementationFactory)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (implementationFactory == null)
            {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            services.TryAddScoped<IUnitOfWork, UnitOfWork>();
            services.TryAddScoped<IDatabaseBuilder, DatabaseBuilder>();
            services.TryAddScoped<ICollectionNameSelector, DefaultCollectionNameSelector>();
            services.TryAddScoped(typeof(IDatabaseSettings), implementationFactory);
            return services;
        }

        ///// <summary>
        ///// Registers the unit of work given context as a service in the <see cref="IServiceCollection"/>.
        ///// </summary>
        ///// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        ///// <returns>The same service collection so that multiple calls can be chained.</returns>
        ///// <remarks>
        ///// This method only support one db context, if been called more than once, will throw exception.
        ///// </remarks>
        public static IServiceCollection AddUnitOfWork(this IServiceCollection services, Action<DatabaseSettings> applyConfiguration)
        {
            services.TryAddScoped<IUnitOfWork, UnitOfWork>();
            services.TryAddScoped<IDatabaseBuilder, DatabaseBuilder>();
            services.TryAddScoped<ICollectionNameSelector, DefaultCollectionNameSelector>();

            var mongoDbSettings = new DatabaseSettings();
            applyConfiguration(mongoDbSettings);

            services.TryAddScoped<IDatabaseSettings>(_ => mongoDbSettings);

            return services;
        }

        ///// <summary>
        ///// Registers the unit of work given context as a service in the <see cref="IServiceCollection"/>.
        ///// </summary>
        ///// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        ///// <returns>The same service collection so that multiple calls can be chained.</returns>
        ///// <remarks>
        ///// This method only support one db context, if been called more than once, will throw exception.
        ///// </remarks>
        public static IServiceCollection AddUnitOfWork(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.TryAddScoped<IUnitOfWork, UnitOfWork>();
            services.TryAddScoped<IDatabaseBuilder, DatabaseBuilder>();
            services.TryAddScoped<ICollectionNameSelector, DefaultCollectionNameSelector>();

            var mongoDbSettings = configurationSection.Get<DatabaseSettings>();

            if (mongoDbSettings == null)
            {
                throw new UnitOfWorkArgumentNullException(nameof(DatabaseSettings));
            }

            services.TryAddScoped<IDatabaseSettings>(_ => mongoDbSettings);

            return services;
        }
    }
}
