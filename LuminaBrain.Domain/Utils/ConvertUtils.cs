using System.Security.Cryptography;

namespace LuminaBrain.Domain.Utils;

public static class ConvertUtils
{
    /// <summary>
    /// 将obj类型转换为string
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string ConvertToString(this object s)
    {
        if (s == null)
        {
            return "";
        }
        else
        {
            return Convert.ToString(s);
        }
    }
    
    public static string CalculateSha256(this BinaryData binaryData)
    {
        byte[] byteArray = SHA256.HashData(binaryData.ToMemory().Span);
        return Convert.ToHexString(byteArray).ToLowerInvariant();
    }
}