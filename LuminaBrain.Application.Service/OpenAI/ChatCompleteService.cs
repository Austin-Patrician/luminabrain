using FastWiki.Application.Contract.OpenAI;
using FastWiki.Application.Contract.OpenAI.Dto;
using LuminaBrain.Core.Exceptions;

namespace LuminaBrain.Application.Service.OpenAI;

public class ChatCompleteService(
    IHttpContextAccessor httpContextAccessor,
    IAgentService agentService) : IChatCompleteService
{
    public async Task ChatCompleteAsync(ChatCompleteInput input)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var agent = await agentService.GetAsync(input.AgentId);

        if (agent == null)
        {
            throw new BusinessException("未找到对应的Agent");
        }
        
        
    }
}