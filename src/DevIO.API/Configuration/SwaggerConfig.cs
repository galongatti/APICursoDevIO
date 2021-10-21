using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.Swagger;
using Microsoft.OpenApi.Models;
using System;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using static DevIO.API.Configuration.SwaggerDefaultValues;

namespace DevIO.API.Configuration
{
   public static class SwaggerConfig
   {

      public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
      {
         services.AddSwaggerGen(c =>
         {
            c.OperationFilter<SwaggerDefaultValues>();

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
               Description = "Insira o token JWT dessa maneira: Bearer {seu token}",
               Name = "Authorization",
               In = ParameterLocation.Header,
               Type = SecuritySchemeType.ApiKey,
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
              new OpenApiSecurityScheme
              {
                Reference = new OpenApiReference
                {
                  Type = ReferenceType.SecurityScheme,
                  Id = "Bearer"
                }
               },
               Array.Empty<string>()
             }
           });
         });

         return services;
      }

      public static IApplicationBuilder UseSwaggerConfig(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
      {
         //app.UseMiddleware<SwaggerAuthorizeMiddleware>();
         app.UseSwagger();
         app.UseSwaggerUI(opt => 
         { 
            foreach(ApiVersionDescription desc in provider.ApiVersionDescriptions)
            {
               opt.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
            }
         });

         return app;
      }
   }

   public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
   {
      readonly IApiVersionDescriptionProvider provider;

      public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
      {
         this.provider = provider;
      }

      public void Configure(SwaggerGenOptions options)
      {
         foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
         {
            options.SwaggerGeneratorOptions.SwaggerDocs.Add(description.GroupName, CreateInfoApiVersion(description));
         }
      }

      static OpenApiInfo CreateInfoApiVersion(ApiVersionDescription description)
      {
         OpenApiInfo info = new OpenApiInfo()
         {
            Title = "API - desenvolvedor.io",
            Version = description.ApiVersion.ToString(),
            Description = "Esta API faz parte do curso REST com ASP.NET Core WebAPI",
            Contact = new OpenApiContact { Name = "Gabriel Longatti", Email = "gabriel@teste.com.br" },
            TermsOfService = new Uri("https://opensource/licenses/MIT"),
            License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource/licenses/MIT") }

         };

         if (description.IsDeprecated)
         {
            info.Description += " Esta versão está obsoleta!";
         }

         return info;

      }
   }

   public class SwaggerDefaultValues : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
   {
      public void Apply(OpenApiOperation operation, OperationFilterContext context)
      {
         ApiDescription apiDescription = context.ApiDescription;
         operation.Deprecated = apiDescription.IsDeprecated();

         if(operation.Parameters == null)
         {
            return;
         }

         foreach(OpenApiParameter parameter in operation.Parameters)
         {
            int countParam = apiDescription.ParameterDescriptions.Count;
            ApiParameterDescription desc = new ApiParameterDescription();
            for(int i = 0; i < countParam; i++)
            {

               if(apiDescription.ParameterDescriptions[i].Name.Equals(parameter.Name))
               {
                  desc = apiDescription.ParameterDescriptions[i];
                  break;
               }               
            }

            if (parameter.Description == null)
            {
               parameter.Description = desc.ModelMetadata?.Description;
            }

            parameter.Required |= desc.IsRequired;

            
         }

      }

      public class SwaggerAuthorizeMiddleware
      {
         private readonly RequestDelegate _next;

         public SwaggerAuthorizeMiddleware(RequestDelegate next)
         {
            _next = next;
         }

         public async Task Invoke(HttpContext context)
         {
            if (context.Request.Path.StartsWithSegments("/swagger") && !context.User.Identity.IsAuthenticated)
            {
               context.Response.StatusCode = StatusCodes.Status401Unauthorized;
               return;
            }

            await _next.Invoke(context);
         }



      }



   }
}
