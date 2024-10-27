using System;
using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    //the name has to be InvokeAsync
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message); //log out to the terminal

            //using the context to do something to the response
            context.Response.ContentType = "application/json"; //http response
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = env.IsDevelopment()
            ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace)
            : new ApiException(context.Response.StatusCode, ex.Message, "Internal Server Error");


            //just a configuration for the json
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            //the response will be in json format
            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}
