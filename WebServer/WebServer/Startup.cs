using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebServer.Controllers;
using WebServer.Models;

namespace WebServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.WriteLine($"Server IP is: {ip.ToString()}");
                }
            }
        }

        public IConfiguration Configuration { get; }

        
        public void ConfigureServices(IServiceCollection services)
        {
            
            var builder = new ConfigurationBuilder().SetBasePath(ApplicationEnvironment.ApplicationBasePath)
                .AddJsonFile("appsettings.json");

            //services.AddSingleton<IConfiguration>(builder.Build());
            services.AddSingleton<ITokens, Tokens>();
            services.AddSingleton<MessageData>();


            services.AddCors();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Not sure what this stuff really does
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(
                options => options.WithOrigins("*").AllowAnyMethod()
            );
            
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}