using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Calabonga.UnitOfWork.MongoDb
{
    public static class UnitOfWorkServiceCollectionExtensions
    {
        ///// <summary>
        ///// Registers the unit of work given context as a service in the <see cref="IServiceCollection"/>.
        ///// </summary>
        ///// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        ///// <returns>The same service collection so that multiple calls can be chained.</returns>
        ///// <remarks>
        ///// This method only support one db context, if been called more than once, will throw exception.
        ///// </remarks>
        public static void AddUnitOfWork(this IServiceCollection services, Action<MongoDbSettings> applyConfiguration)
        {
            services.TryAddScoped<IUnitOfWork, UnitOfWork>();
            services.TryAddScoped<IDatabaseBuilder, DatabaseBuilder>();
            services.TryAddScoped<ICollectionNameSelector, CollectionNameSelector>();

            var mongoDbSettings = new MongoDbSettings();
            applyConfiguration(mongoDbSettings);

            services.TryAddScoped<IMongoDbSettings>(_ => mongoDbSettings);
        }

        ///// <summary>
        ///// Registers the unit of work given context as a service in the <see cref="IServiceCollection"/>.
        ///// </summary>
        ///// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        ///// <returns>The same service collection so that multiple calls can be chained.</returns>
        ///// <remarks>
        ///// This method only support one db context, if been called more than once, will throw exception.
        ///// </remarks>
        public static void AddUnitOfWork(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.TryAddScoped<IUnitOfWork, UnitOfWork>();
            services.TryAddScoped<IDatabaseBuilder, DatabaseBuilder>();
            services.TryAddScoped<ICollectionNameSelector, CollectionNameSelector>();

            var mongoDbSettings = configurationSection.Get<MongoDbSettings>();

            if (mongoDbSettings == null)
            {
                throw new ArgumentNullException(nameof(MongoDbSettings));
            }

            services.TryAddScoped<IMongoDbSettings>(_ => mongoDbSettings);
        }
    }
}
