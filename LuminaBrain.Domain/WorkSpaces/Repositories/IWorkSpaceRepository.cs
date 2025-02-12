using FastWiki.Domain.WorkSpaces.Aggregates;
using LuminaBrain.Data.Repositories;

namespace LuminaBrain.Domain.WorkSpaces.Repositories;

public interface IWorkSpaceRepository : IRepository<WorkSpace>
{
    /// <summary>
    /// 创建工作空间
    /// </summary>
    /// <param name="workSpace"></param>
    /// <returns></returns>
    Task CreateAsync(FastWiki.Domain.WorkSpaces.Aggregates.WorkSpace workSpace);

    /// <summary>
    /// 删除工作空间
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteAsync(long id);

    /// <summary>
    /// 更新工作空间
    /// </summary>
    /// <returns></returns>
    Task UpdateAsync(FastWiki.Domain.WorkSpaces.Aggregates.WorkSpace workSpace);

    /// <summary>
    /// 获取用户的工作空间
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<FastWiki.Domain.WorkSpaces.Aggregates.WorkSpace>> GetListAsync(string userId);
}