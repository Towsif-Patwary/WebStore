using CloudPos_TWebStore.Domain.Repositories;
using CloudPos_TWebStore.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace CloudPos_TWebStore.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            //MongoDB Configuration
            string mongoConnectionString = configuration["MongoDbSettings:ConnectionString"];
            string mongoDatabaseName = configuration["MongoDbSettings:DatabaseName"];
            services.AddScoped<IMongoClient>(sp => new MongoClient(mongoConnectionString));
            services.AddScoped<ICustomerRepository, CustomerRepository>(sp =>
                new CustomerRepository(sp.GetRequiredService<IMongoClient>(), mongoDatabaseName));

            return services;
        
        }
    }
}
