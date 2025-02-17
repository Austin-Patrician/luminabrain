using LuminaBrain.Application.OpenAI.Dto;

namespace LuminaBrain.Application.OpenAI;

public interface IChatCompleteService
{
    /// <summary>
    /// 知识库对话
    /// </summary>
    /// <returns></returns>
    Task ChatCompleteAsync(ChatCompleteInput input);
}