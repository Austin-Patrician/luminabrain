using FastWiki.Application.Contract.Notification;
using LuminaBrain.Application.Notification;
using LuminaBrain.HttpApi.Filter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LuminaBrain.HttpApi.Endpoints;

// NotificationEndpoints.cs
public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoint)
    {
        var notification = endpoint.MapGroup("/api/v1/notifications")
            .WithTags("通知管理")
            .AddEndpointFilter<ResultFilter>();

        notification.MapPost("login-verification-code",
                [EndpointSummary("获取登录验证码"), EndpointDescription("获取登录验证码")]
                async (INotificationService service) => await service.GetLoginVerificationCodeAsync());

        notification.MapPost("register-verification-code",
                [EndpointSummary("获取注册验证码"), EndpointDescription("获取注册验证码")]
                async (INotificationService service, string account,IFormFile file) =>
                    await service.GetRegisterVerificationCodeAsync(account));

        return endpoint;
    }
}