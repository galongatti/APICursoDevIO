using Microsoft.AspNetCore.Builder;
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
         services.Configure<ApiBehaviorOptions>(opt => {
            opt.SuppressModelStateInvalidFilter = true;
         });

         services.AddCors(options =>
         {
            options.AddPolicy("Development", builder =>
            builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
         });

         return services;
      }

      public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app)
      {


         app.UseHttpsRedirection();

         app.UseRouting();

         app.UseAuthorization();

         app.UseCors("Development");

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllers();
         });

         return app;
      }
   }
}
