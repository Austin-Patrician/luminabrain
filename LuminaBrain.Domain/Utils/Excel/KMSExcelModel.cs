namespace LuminaBrain.Domain.Utils.Excel
{
    public class KmsExcelModel
    {
        [ExecelProperty("问题", 0)] public string Question { get; set; }

        [ExecelProperty("答案", 1)] public string Answer { get; set; }
    }
}