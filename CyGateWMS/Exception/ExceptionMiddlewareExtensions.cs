﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CyGateWMS.Exception
{
    public static class ExceptionMiddlewareExtensions
    {
        //public static class ExceptionMiddlewareExtensions
        //{
        //    public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger logger)
        //    {
        //        app.UseExceptionHandler(appError =>
        //        {
        //            appError.Run(async context =>
        //            {
        //                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        //                context.Response.ContentType = "application/json";

        //                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        //                if (contextFeature != null)
        //                {
        //                    logger.LogError($"Something went wrong: {contextFeature.Error}");

        //                    (new ErrorDetails()
        //                    {
        //                        StatusCode = context.Response.StatusCode,
        //                        Message = "Internal Server Error."
        //                    }.ToString());
        //                }
        //            });
        //        });
        //    }
        //}
    }
}
