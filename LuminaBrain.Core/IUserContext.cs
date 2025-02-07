﻿namespace LuminaBrain.Core;

public interface IUserContext
{
    string UserId { get; }
    
    string UserName { get; }
    
    bool IsAuthenticated { get; }
    
    string[] Roles { get; }
}