using System.Text;
using LuminaBrain.Application.KM.Dto;
using LuminaBrain.Domain.Model.Constant;
using LuminaBrain.Domain.Model.Enum;
using LuminaBrain.Domain.Utils.Excel;
using mem0.Net.KM;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Handlers;

namespace mem0.Net.Qdrant.Services;

public class ImportKmsService(IKMService _kMService, ILogger<ImportKmsService> _logger) : IImportKmsService
{
    public void ImportKmsTask(ImportKmsTaskReq req)
    {
        try
        {
            var memory = _kMService.GetMemoryByKms();
            string fileid = req.KmsDetail.Id;
            List<string> step = new List<string>();
            if (req.IsQA)
            {
                memory.Orchestrator.AddHandler<TextExtractionHandler>("extract_text");
                memory.Orchestrator.AddHandler<QAHandler>("handle_qa");
                memory.Orchestrator.AddHandler<GenerateEmbeddingsHandler>("generate_embeddings");
                memory.Orchestrator.AddHandler<SaveRecordsHandler>("save_memory_records");
                step.Add("extract_text");
                step.Add("handle_qa");
                step.Add("generate_embeddings");
                step.Add("save_memory_records");
            }

            switch (req.ImportType)
            {
                case ImportType.File:
                {
                    //导入文件
                    if (req.IsQA)
                    {
                        var importResult = memory.ImportDocumentAsync(new Document(fileid)
                                .AddFile(req.FilePath)
                                .AddTag(KmsConstantcs.KmsIdTag, req.KmsId)
                            , index: KmsConstantcs.KmsIndex, steps: step.ToArray()).Result;
                    }
                    else
                    {
                        var importResult = memory.ImportDocumentAsync(new Document(fileid)
                                .AddFile(req.FilePath)
                                .AddTag(KmsConstantcs.KmsIdTag, req.KmsId)
                            , index: KmsConstantcs.KmsIndex).Result;
                    }

                    //查询文档数量
                    var docTextList = _kMService.GetDocumentByFileID(fileid).Result;
                    string fileGuidName = Path.GetFileName(req.FilePath);
                    req.KmsDetail.FileName = req.FileName;
                    req.KmsDetail.FileGuidName = fileGuidName;
                    req.KmsDetail.DataCount = docTextList.Count;
                }
                    break;
                case ImportType.Url:
                {
                    //导入url                  
                    if (req.IsQA)
                    {
                        var importResult = memory.ImportWebPageAsync(req.Url, fileid,
                            new TagCollection() { { KmsConstantcs.KmsIdTag, req.KmsId } }
                            , index: KmsConstantcs.KmsIndex, steps: step.ToArray()).Result;
                    }
                    else
                    {
                        var importResult = memory.ImportWebPageAsync(req.Url, fileid,
                            new TagCollection() { { KmsConstantcs.KmsIdTag, req.KmsId } }
                            , index: KmsConstantcs.KmsIndex).Result;
                    }

                    //查询文档数量
                    var docTextList = _kMService.GetDocumentByFileID(fileid).Result;
                    req.KmsDetail.Url = req.Url;
                    req.KmsDetail.DataCount = docTextList.Count;
                }
                    break;
                case ImportType.Text:
                    //导入文本
                {
                    if (req.IsQA)
                    {
                        var importResult = memory.ImportTextAsync(req.Text, fileid,
                            new TagCollection() { { KmsConstantcs.KmsIdTag, req.KmsId } }
                            , index: KmsConstantcs.KmsIndex, steps: step.ToArray()).Result;
                    }
                    else
                    {
                        var importResult = memory.ImportTextAsync(req.Text, fileid,
                            new TagCollection() { { KmsConstantcs.KmsIdTag, req.KmsId } }
                            , index: KmsConstantcs.KmsIndex).Result;
                    }

                    //查询文档数量
                    var docTextList = _kMService.GetDocumentByFileID(fileid).Result;
                    req.KmsDetail.Url = req.Url;
                    req.KmsDetail.DataCount = docTextList.Count;
                }
                    break;
                case ImportType.Excel:
                    using (var fs = File.OpenRead(req.FilePath))
                    {
                        var excelList = ExecelHelper.ExcelToList<KmsExcelModel>(fs);
                        memory.Orchestrator.AddHandler<TextExtractionHandler>("extract_text");
                        memory.Orchestrator.AddHandler<KmExcelHandler>("antsk_excel_split");
                        memory.Orchestrator.AddHandler<GenerateEmbeddingsHandler>("generate_embeddings");
                        memory.Orchestrator.AddHandler<SaveRecordsHandler>("save_memory_records");

                        StringBuilder text = new StringBuilder();
                        foreach (var item in excelList)
                        {
                            text.AppendLine(
                                @$"Question:{item.Question}{Environment.NewLine}Answer:{item.Answer}{KmsConstantcs.KMExcelSplit}");
                        }

                        var importResult = memory.ImportTextAsync(text.ToString(), fileid,
                            new TagCollection() { { KmsConstantcs.KmsIdTag, req.KmsId } }
                            , index: KmsConstantcs.KmsIndex,
                            steps: new[]
                            {
                                "extract_text",
                                "antsk_excel_split",
                                "generate_embeddings",
                                "save_memory_records"
                            }
                        ).Result;
                        req.KmsDetail.FileName = req.FileName;
                        string fileGuidName = Path.GetFileName(req.FilePath);
                        req.KmsDetail.FileGuidName = fileGuidName;
                        req.KmsDetail.DataCount = excelList.Count();
                    }

                    break;
            }

            req.KmsDetail.Status = ImportKmsStatus.Success;
            //_kmsDetails_Repositories.Update(req.KmsDetail);
            //_kmsDetails_Repositories.GetList(p => p.KmsId == req.KmsId);
            _logger.LogInformation("后台导入任务成功:" + req.KmsDetail.DataCount);
        }
        catch (Exception ex)
        {
            req.KmsDetail.Status = ImportKmsStatus.Fail;
            //_kmsDetails_Repositories.Update(req.KmsDetail);
            _logger.LogError("后台导入任务异常:" + ex.Message);
        }
    }
}