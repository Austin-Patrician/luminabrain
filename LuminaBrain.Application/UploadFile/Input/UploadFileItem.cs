using Microsoft.AspNetCore.Http;

namespace LuminaBrain.Application.UploadFile.Input;

public class UploadFileItem
{
    public int WorkSpaceId { get; set; }
    public IFormFile File { get; set; }
}