using Microsoft.KernelMemory;

namespace mem0.Net.KM;

public interface IKMService
{
    MemoryServerless GetMemoryByKms();
    
    bool BeforeUpload();
}