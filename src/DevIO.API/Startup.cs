using DevIO.API.Configuration;
using DevIO.Data.Context;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthChecks.UI.Core;
using DevIO.API.Extensions;
using System.Text.Json;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;

namespace DevIO.API
{
   public class Startup
   {
      public Startup(IConfiguration configuration)
      {
         Configuration = configuration;
      }

      public IConfiguration Configuration { get; }

      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services)
      {

         services.AddDbContext<MeuDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

         services.AddIdentityConfiguration(Configuration);

         services.WebApiConfig();
         services.AddSwaggerConfig();         
         services.AddAutoMapper(typeof(Startup));

         services.AddHealthChecks()
           .AddCheck("Users", new SqlServerHealthCheck(Configuration.GetConnectionString("DefaultConnection")))
           .AddSqlServer(Configuration.GetConnectionString("DefaultConnection"), name: "BancoSQL");

         services.AddHealthChecksUI()
         .AddSqlServerStorage(Configuration.GetConnectionString("DefaultConnection"));

         services.ResolveDependencies();
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
      {
         if (env.IsDevelopment())
         {
            app.UseCors("Development");
            app.UseDeveloperExceptionPage();
           
         } else {
            app.UseCors("Production");
            app.UseHsts();
         }

         app.UseAuthentication();
         app.UseApiConfiguration();
         app.UseSwaggerConfig(provider);

         app.UseHealthChecks("/api/hc", new HealthCheckOptions()
         { 
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
         });

         app.UseHealthChecksUI(opt => 
         {
            opt.UIPath = "/api/hc-ui";
         
         });
         
      }
   }
}
