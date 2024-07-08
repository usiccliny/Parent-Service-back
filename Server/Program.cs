using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Server.Contexts;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<DatabaseContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseContext")));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder.WithOrigins("https://tgwatcher-fe.qpilipp.ru")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });


            var app = builder.Build();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseCors("AllowAll");
                app.UseHttpsRedirection();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseForwardedHeaders();
                app.UseHsts();
                app.UseCors("AllowSpecificOrigin");
            }

            app.UseAuthorization();
            app.UseDefaultFiles();

            app.MapControllers();

            app.Run();
        }
    }
}
