namespace mem0.Net.Model;

public class VectorData
{
    public Guid Id { get; set; } // 向量 ID

    public float Score { get; set; } // 向量

    public IReadOnlyList<float> Vector { get; set; }

    public Dictionary<string, string> MetaData { get; set; } // 有效载荷

    public string? Text
    {
        get
        {
            return MetaData["data"];
        }
    } // 文本
}