using System;
using System.Collections.Generic;
using System.Text;
using Albelli.Correlation.Http.Server.Middleware;
using Albumprinter.CorrelationTracking;
using Microsoft.AspNetCore.Builder;

namespace Albelli.Correlation.Http.Server
{
    public static class UseExtensions
    {
        public static void UseCorrelation(this IApplicationBuilder app)
        {
            CorrelationTrackingConfiguration.Initialize();
            app.UseMiddleware<CorrelationTrackingMiddleware>();
            app.UseMiddleware<LogRequestMiddleware>();
            app.UseMiddleware<LogResponseMiddleware>();
        }
    }
}
