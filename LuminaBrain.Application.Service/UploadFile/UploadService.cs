using LuminaBrain.Application.UploadFile;
using LuminaBrain.Application.UploadFile.Input;

namespace LuminaBrain.Application.Service.UploadFile;

public class UploadService(IServiceProvider serviceProvider) : IUploadService
{
    private const long MaxFileSize = 2 * 1024 * 1024; // 限制文件大小为 2MB
    
    public async Task<string> Upload(UploadFileItem model)
    {
        if (model.File == null || model.File.Length == 0)
        {
            return "未选择文件。";
        }

        // 检查文件大小
        if (model.File.Length > MaxFileSize)
        {
            return "文件大小超过限制 2MB。";
        }

        // 处理文件（保存到服务器等）
        //这里去处理文件embedding到数据库里面
        var filePath = Path.Combine("uploads", model.File.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await model.File.CopyToAsync(stream);
        }

        return "文件上传成功。";
    }
}