using LuminaBrain.Application.Authorization.Input;

namespace LuminaBrain.Application.Authorization;

public interface IAuthorizationService
{
    /// <summary>
    /// 获取Token
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<string> TokenAsync(TokenInput input);
}