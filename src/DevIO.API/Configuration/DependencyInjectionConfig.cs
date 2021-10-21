﻿using DevIO.API.Extensions;
using DevIO.Business.Intefaces;
using DevIO.Business.Notificacoes;
using DevIO.Business.Services;
using DevIO.Data.Context;
using DevIO.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.API.Configuration
{
   public static class DependencyInjectionConfig
   {
      public static IServiceCollection ResolveDependencies(this IServiceCollection services)
      {
         services.AddScoped<MeuDbContext>();
         services.AddScoped<IFornecedorRepository, FornecedorRepository>();
         services.AddScoped<IFornecedorService, FornecedorService>();

         services.AddScoped<IEnderecoRepository, EnderecoRepository>();

         services.AddScoped<IProdutoRepository, ProdutoRepository>();
         services.AddScoped<IProdutoService, ProdutoService>();

         services.AddScoped<INotificador, Notificador>();

         //Singleton pois é para aplicação toda, não irá confundir os usuarios
         services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

         services.AddScoped<IUser, AspNetUser>();

         services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

         return services;
      }
   }
}
