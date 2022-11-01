using FluentValidation.AspNetCore;
using Library.Context;
using Library.Models;
using Library.RepositoryPattern.Base;
using Library.RepositoryPattern.Concrete;
using Library.RepositoryPattern.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library
{
    public class Startup
    {
        IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MyDbContext>(options=>options.UseSqlServer(_configuration
                ["ConnectionStrings:Mssql"]));
            services.AddControllersWithViews().AddFluentValidation(fv=>fv.RegisterValidatorsFromAssemblyContaining<Startup>());
            //services.AddScoped<IRepository<BookType>, Repository<BookType>>();
            //services.AddScoped<IRepository<Authors>, Repository<Authors>>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<IRepository<AppUser>, Repository<AppUser>>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IBookTypeRepository, BookTypeRepository>();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(
                options => { options.LoginPath = "/Auth/Login";
                    options.Cookie.Name = "UserDetail";
                    options.AccessDeniedPath = "/Auth/Login";
                }); 
            // �erezleri kullanarak bu giri� kontrol�ne ata,
            // AddAuthentication i�ine direkt parametre olarak Cookie olarak yazsan da olurdu.
            // addCookie i�ine yaz�lan parametre ise e�er b�yle bir yetkilendirme yoksa login sayfas�na g�nder.


            services.AddAuthorization(options => 
            {
                options.AddPolicy("AdminPolicy",policy=>policy.RequireClaim("role","admin"));
                options.AddPolicy("UserPolicy",policy=>policy.RequireClaim("role","admin","user"));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MyDbContext context)
        {
            context.Database.Migrate(); // son migration �zerinden databaseyi yeniler projeyi birine yollarken kullan.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles(); // wwwroot dosyas�n� kullanabilmek i�in. statik dosyalara eri�im sa�lamak i�in.
            app.UseRouting();
            app.UseAuthentication(); // giri� i�in 
            app.UseAuthorization(); // giri� kontr�l� i�in 

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                   name: "DefaultArea",
                   pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                   );
                endpoints.MapControllerRoute(
                    name:"Default",
                    pattern:"{controller=Auth}/{Action=Login}/{id?}"
                    );
               
            });
        }
    }
}
