using EasyLife.AutoInject.Attributes;
using LuminaBrain.Application.Users;
using LuminaBrain.Application.Users.Dto;
using LuminaBrain.Application.Users.Input;
using LuminaBrain.Core;
using LuminaBrain.Core.Exceptions;
using LuminaBrain.Core.Model;
using LuminaBrain.Domain.Users.Aggregates;
using LuminaBrain.Domain.Users.Repositories;
using MapsterMapper;

namespace LuminaBrain.Application.Service.Users;

[AutoInject<IUserService>]
public class UserService(IUserRepository userRepository, IMapper mapper, IUserContext userContext)
    : IUserService
{
    public async Task<string> CreateAsync(CreateUserInput input)
    {
        var user = mapper.Map<User>(input);

        user.SetPassword(input.Password);
        user.SetEmail(input.Email);

        user = await userRepository.CreateAsync(user);

        return user.Id;
    }

    public async Task UpdateAsync(string id, UpdateUserInput input)
    {
        var user = await userRepository.GetAsync(id);
        if (user == null)
        {
            throw new UserFriendlyException("用户不存在");
        }

        user.SetEmail(input.Email);
        user.SetName(input.Name);
        user.SetIntroduction(input.Introduction);

        await userRepository.UpdateAsync(user);
    }

    public async Task DeleteAsync(string id)
    {
        var user = await userRepository.GetAsync(id);
        if (user == null)
        {
            throw new UserFriendlyException("用户不存在");
        }

        await userRepository.DeleteAsync(user);
    }

    public async Task<PagedResultDto<UserDto>> GetListAsync(string? keyword, int page, int pageSize)
    {
        var users = await userRepository.GetListAsync(keyword, page, pageSize);

        var count = await userRepository.GetCountAsync(keyword);

        return new PagedResultDto<UserDto>(count, users.Select(mapper.Map<UserDto>).ToList());
    }

    public async Task<UserDto> GetCurrentAsync()
    {
        var user = await userRepository.GetAsync(userContext.UserId);

        return mapper.Map<UserDto>(user);
    }
}