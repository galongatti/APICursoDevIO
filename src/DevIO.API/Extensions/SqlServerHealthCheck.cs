using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevIO.API.Extensions
{
   public class SqlServerHealthCheck : IHealthCheck
   {

      private readonly string _connection;
      public SqlServerHealthCheck(string connection)
      {
         _connection = connection;
      }

      public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
      {
         try
         {
            using(var connection = new SqlConnection(_connection))
            {
               await connection.OpenAsync(cancellationToken);
               var command = connection.CreateCommand();
               command.CommandText = "SELECT count(id) from AspNetUsers";

               int count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
               HealthCheckResult result = count  > 0 ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();

               return result;
            }
         }
         catch (Exception)
         {

            return HealthCheckResult.Unhealthy();
         }
      }
   }
}
