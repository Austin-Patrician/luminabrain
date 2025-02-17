using LuminaBrain.Application.UploadFile.Input;

namespace LuminaBrain.Application.UploadFile;

public interface IUploadService
{
    Task<string> Upload(UploadFileItem model);
}