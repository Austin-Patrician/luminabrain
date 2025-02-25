using mem0.NET;
using mem0.Net.KM;
using mem0.Net.OCR;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;

namespace mem0.Net.Qdrant.Services;

public class KMService : IKMService
{
    public MemoryServerless GetMemoryByKms()
    {
        var memoryBuild = new KernelMemoryBuilder().WithOpenAIDefaults("")
            .WithCustomTextPartitioningOptions(new TextPartitioningOptions());
        
        var handler = new OpenAIHttpClientHandler("");
        var httpClient = new HttpClient(handler);
        httpClient.Timeout = TimeSpan.FromMinutes(10);
        
        WithOcr(memoryBuild);
        //加载会话模型
        WithTextGenerationByAiType(memoryBuild,httpClient);
        //加载向量模型
        WithTextEmbeddingGenerationByAiType(memoryBuild,httpClient);
        //加载向量库
        WithMemoryDbByVectorDb(memoryBuild);

        return memoryBuild.Build<MemoryServerless>();
    }

    private void WithTextGenerationByAiType(IKernelMemoryBuilder memory,HttpClient chatHttpClient)
    {
        memory.WithOpenAITextGeneration(new OpenAIConfig
        {
            APIKey = "",
            TextModel = ""
        }, null, chatHttpClient);
    }

    private void WithMemoryDbByVectorDb(IKernelMemoryBuilder memory)
    {
        memory.WithQdrantMemoryDb("");
    }

    private void WithTextEmbeddingGenerationByAiType(IKernelMemoryBuilder memory,HttpClient embeddingHttpClient)
    {
        memory.WithOpenAITextEmbeddingGeneration(new OpenAIConfig()
        {
            APIKey = "",
            EmbeddingModel = "",
        },null, false, embeddingHttpClient);
    }


    private static void WithOcr(IKernelMemoryBuilder memoryBuild)
    {
        memoryBuild.WithCustomImageOcr(new AntSkOcrEngine());
    }

    public bool BeforeUpload()
    {
        List<string> types = new List<string>() {
            "text/plain",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "application/pdf",
            "application/json",
            "text/x-markdown",
            "text/markdown",
            "image/jpeg",
            "image/png",
            "image/tiff"
        };

        string[] exceptExts = [".md", ".pdf"];
        // var validTypes = types.Contains(file.Type) || exceptExts.Contains(file.Ext);
        // if (!validTypes && file.Ext != ".md")
        // {
        //     _message.Error("文件格式错误,请重新选择!");
        // }
        // var IsLt500K = file.Size < 1024 * 1024 * 100;
        // if (!IsLt500K)
        // {
        //     _message.Error("文件需不大于100MB!");
        // }
        //
        // return validTypes && IsLt500K;

        return true;
    }
}