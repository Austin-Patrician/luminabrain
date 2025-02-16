namespace mem0.Net.Model;

public class SearchHit
{
    public Guid Id { get; set; } // 向量 ID
    public float Score { get; set; } // 相似度分数
    public Dictionary<string, string> Payload { get; set; } // 有效载荷

    public Dictionary<string, object> MetaData { get; set; }

    public string? Text => Payload["data"];
}