﻿using LuminaBrain.Data.Auditing;
using LuminaBrain.Domain.Shared.WorkSpace;
using LuminaBrain.Domain.Users.Aggregates;

namespace FastWiki.Domain.WorkSpaces.Aggregates;

/// <summary>
/// 工作空间成员列表
/// </summary>
public class WorkSpaceMember : AuditEntity<long>
{
    public long WorkSpaceId { get; private set; }

    public string UserId { get; private set; }

    public WorkSpaceRoleType RoleType { get; private set; }

    public User User { get; set; }

    public WorkSpaces.Aggregates.WorkSpace WorkSpace { get; set; }

    public WorkSpaceMember(long workSpaceId, string userId, WorkSpaceRoleType roleType)
    {
        WorkSpaceId = workSpaceId;
        UserId = userId;
        RoleType = roleType;
    }

    protected WorkSpaceMember()
    {

    }
}
