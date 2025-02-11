using System.Text.Json;
using LuminaBrain.Core.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LuminaBrain.HttpApi.Middleware;

public class HandlingExceptionMiddleware(ILogger<HandlingExceptionMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "在处理 {Path} 时发生了错误", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(ResponseModel.CreateError("抱歉，服务器发生了错误")));
        }
    }
}