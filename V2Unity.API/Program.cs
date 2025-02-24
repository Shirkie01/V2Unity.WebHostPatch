
using V2Unity.Model;
using V2Unity.API.Persistence;
using LiteDB;
using Microsoft.AspNetCore.HttpLogging;

namespace V2Unity.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddHttpLogging(options =>
            {
                options.LoggingFields = HttpLoggingFields.Request;
            });

            // Add services to the container.

            AddLiteDB(builder.Services, "v2unity.db");


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseHttpLogging();

            app.MapControllers();

            app.Run();
        }


        public static IServiceCollection AddLiteDB(IServiceCollection services, string connectionString)
        {
            // Put the database in DI so it gets disposed when the service ends
            services.AddSingleton<ILiteDatabase, LiteDatabase>(s => new LiteDatabase(connectionString));

            services.AddSingleton<IRepository<User>, LiteDBRepository<User>>();
            services.AddSingleton<IRepository<Record>, LiteDBRepository<Record>>();

            return services;
        }
    }
}
