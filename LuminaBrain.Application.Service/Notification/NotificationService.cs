using EasyLife.AutoInject.Attributes;
using FastWiki.Application.Contract.Notification;
using FastWiki.Application.Contract.Notification.Dto;
using Lazy.Captcha.Core;

namespace LuminaBrain.Application.Service.Notification;

[AutoInject<INotificationService>]
public class NotificationService(ICaptcha captcha) : INotificationService
{
    public Task<VerificationDto> GetLoginVerificationCodeAsync()
    {
        var uuid = "login:" + Guid.NewGuid().ToString("N");

        var code = captcha.Generate(uuid, 240);

        return Task.FromResult(new VerificationDto
        {
            Key = uuid,
            Code = "data:image/png;base64," + code.Base64
        });
    }

    public Task<string> GetRegisterVerificationCodeAsync(string account)
    {
        var uuid = "register:" + account;

        var code = captcha.Generate(uuid, 240);

        return Task.FromResult(code.Base64);
    }
}