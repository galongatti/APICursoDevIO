using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.API.Configuration
{
   public static class APIConfig
   {
      public static IServiceCollection WebApiConfig(this IServiceCollection services)
      {
         services.AddControllers();

         services.AddApiVersioning(opt =>
         {
            opt.AssumeDefaultVersionWhenUnspecified = true;
            opt.DefaultApiVersion = new ApiVersion(1, 0);
            opt.ReportApiVersions = true;
         });

         services.AddVersionedApiExplorer(opt =>
         {
            opt.GroupNameFormat = "'v'VVV";
            opt.SubstituteApiVersionInUrl = true;
         });


         services.Configure<ApiBehaviorOptions>(opt => {
            opt.SuppressModelStateInvalidFilter = true;
         });

         services.AddCors(options =>
         {
            options.AddDefaultPolicy(builder =>
               builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());

            //options.AddPolicy("Development", builder =>
            //builder.AllowAnyOrigin()
            //.AllowAnyMethod()
            //.AllowAnyHeader());

            //options.AddPolicy("Production", builder =>
            //builder
            //   .WithMethods("GET", "POST")
            //   .WithOrigins("Http://desenvolvedor.io")
            //   .SetIsOriginAllowedToAllowWildcardSubdomains()
            //   .AllowAnyHeader());
         });

         return services;
      }

      public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app)
      {
         app.UseHttpsRedirection();         

         app.UseRouting();

         app.UseAuthorization();        

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllers();                    
         });

         return app;
      }
   }
}
