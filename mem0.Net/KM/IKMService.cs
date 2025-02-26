using LuminaBrain.Domain.Chat.Aggregates;
using Microsoft.KernelMemory;

namespace mem0.Net.KM;

public interface IKMService
{
    MemoryServerless GetMemoryByKms();

    bool BeforeUpload();

    Task<List<KMFile>> GetDocumentByFileID(string fileId);
}