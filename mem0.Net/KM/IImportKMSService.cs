﻿using LuminaBrain.Application.KM.Dto;

namespace mem0.Net.KM;

public interface IImportKmsService
{
    void ImportKmsTask(ImportKmsTaskReq req);
}