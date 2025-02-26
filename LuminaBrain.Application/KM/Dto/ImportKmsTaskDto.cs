using LuminaBrain.Domain.KM.Aggregates;
using LuminaBrain.Domain.Model.Enum;

namespace LuminaBrain.Application.KM.Dto;

public class ImportKmsTaskDto
{
    public ImportType ImportType { get; set; }

    public string KmsId { get; set; }

    public string Url { get; set; } = "";


    public string Text { get; set; } = "";

    public string FilePath { get; set; } = "";

    public string FileName { get; set; } = "";

    public bool IsQA { get; set; } = false;
}

public class ImportKmsTaskReq : ImportKmsTaskDto
{
    public bool IsQA { get; set; } = false;
    public KmsDetails KmsDetail { get; set; } = new KmsDetails();
}