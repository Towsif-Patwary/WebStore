using CloudPos_TWebStore.Application.Map;
using CloudPos_TWebStore.Application.Services;
using CloudPos_WebStore.Application.Interfaces;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CloudPos_TWebStore.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            //MongoDB Configuration
            string mongoConnectionString = configuration["MongoDbSettings:ConnectionString"];
            string mongoDatabaseName = configuration["MongoDbSettings:DatabaseName"];

            services.AddAutoMapper(typeof(MappingProfile));

            //services.AddScoped<CustomerService>();
            services.AddScoped<ICustomerService, CustomerService>();


            // Add Hangfire services
            services.AddHangfire(config => config.UseMongoStorage(mongoConnectionString, mongoDatabaseName)); // Use MongoDB for Hangfire
            services.AddHangfireServer();

            return services;
        }
    }
}