﻿using System.Text.RegularExpressions;
using LuminaBrain.Domain.Utils;
using mem0.NET;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.Configuration;
using Microsoft.KernelMemory.Diagnostics;
using Microsoft.KernelMemory.Pipeline;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Text;

namespace mem0.Net;

public class QAHandler : IPipelineStepHandler
{
    private readonly TextPartitioningOptions _options;
    private readonly IPipelineOrchestrator _orchestrator;
    private readonly ILogger<QAHandler> _log;

#pragma warning disable
    private readonly TiktokenTokenizer _tokenCounter;
    private readonly Kernel _kernel;

    public QAHandler(
        string stepName,
        IPipelineOrchestrator orchestrator,
        TextPartitioningOptions? options = null,
        ILogger<QAHandler>? log = null
    )
    {
        this.StepName = stepName;
        this._orchestrator = orchestrator;
        this._options = options ?? new TextPartitioningOptions();
        this._options.Validate();

        
        this._log = log ?? DefaultLogger<QAHandler>.Instance;
        this._tokenCounter = new TiktokenTokenizer("");;

        var handler = new OpenAIHttpClientHandler("");
        var chatHttpClient = new HttpClient(handler);
        chatHttpClient.Timeout = TimeSpan.FromMinutes(10);
        _kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: "", apiKey: "", httpClient: chatHttpClient)
            .Build();
    }

    /// <inheritdoc />
    public string StepName { get; }

    /// <inheritdoc />
    public async Task<(ReturnType returnType, DataPipeline updatedPipeline)> InvokeAsync(
        DataPipeline pipeline, CancellationToken cancellationToken = default)
    {
        this._log.LogDebug("Partitioning text, pipeline '{0}/{1}'", pipeline.Index, pipeline.DocumentId);

        if (pipeline.Files.Count == 0)
        {
            this._log.LogWarning("Pipeline '{0}/{1}': there are no files to process, moving to next pipeline step.",
                pipeline.Index, pipeline.DocumentId);
            return (ReturnType.Success, pipeline);
        }

        foreach (DataPipeline.FileDetails uploadedFile in pipeline.Files)
        {
            // Track new files being generated (cannot edit originalFile.GeneratedFiles while looping it)
            Dictionary<string, DataPipeline.GeneratedFileDetails> newFiles = new();

            foreach (KeyValuePair<string, DataPipeline.GeneratedFileDetails> generatedFile in uploadedFile
                         .GeneratedFiles)
            {
                var file = generatedFile.Value;
                if (file.AlreadyProcessedBy(this))
                {
                    this._log.LogTrace("File {0} already processed by this handler", file.Name);
                    continue;
                }

                // Partition only the original text
                if (file.ArtifactType != DataPipeline.ArtifactTypes.ExtractedText)
                {
                    this._log.LogTrace("Skipping file {0} (not original text)", file.Name);
                    continue;
                }

                // Use a different partitioning strategy depending on the file type
                List<string> partitions;
                List<string> sentences;
                BinaryData partitionContent = await this._orchestrator
                    .ReadFileAsync(pipeline, file.Name, cancellationToken).ConfigureAwait(false);

                // Skip empty partitions. Also: partitionContent.ToString() throws an exception if there are no bytes.
                if (partitionContent.ToArray().Length == 0)
                {
                    continue;
                }

                switch (file.MimeType)
                {
                    case MimeTypes.PlainText:
                    case MimeTypes.MarkDown:
                    {
                        this._log.LogDebug("Partitioning text file {0}", file.Name);
                        string content = partitionContent.ToString();

                        var lines = TextChunker.SplitPlainTextLines(content, 299);
                        var paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 3000);
                        KernelFunction jsonFun = _kernel.Plugins.GetFunction("KMSPlugin", "QA");

                        List<string> qaList = new List<string>();
                        foreach (var para in paragraphs)
                        {
                            var qaresult = await _kernel.InvokeAsync(function: jsonFun,
                                new KernelArguments { ["input"] = para }, cancellationToken: cancellationToken);
                            var qaListStr = qaresult.GetValue<string>().ConvertToString();

                            string pattern = @"Q\d+:.*?A\d+:.*?(?=(Q\d+:|$))";
                            RegexOptions options = RegexOptions.Singleline;

                            foreach (Match match in Regex.Matches(qaListStr, pattern, options))
                            {
                                qaList.Add(match.Value.Trim()); // Trim用于删除可能的首尾空格
                            }
                        }

                        sentences = qaList;
                        partitions = qaList;
                        break;
                    }
                    default:
                        this._log.LogWarning("File {0} cannot be partitioned, type '{1}' not supported", file.Name,
                            file.MimeType);
                        // Don't partition other files
                        continue;
                }

                if (partitions.Count == 0)
                {
                    continue;
                }

                this._log.LogDebug("Saving {0} file partitions", partitions.Count);
                for (int partitionNumber = 0; partitionNumber < partitions.Count; partitionNumber++)
                {
                    // TODO: turn partitions in objects with more details, e.g. page number
                    string text = partitions[partitionNumber];
                    int sectionNumber = 0; // TODO: use this to store the page number (if any)
                    BinaryData textData = new(text);

                    int tokenCount = this._tokenCounter.CountTokens(text);
                    this._log.LogDebug("Partition size: {0} tokens", tokenCount);

                    var destFile = uploadedFile.GetPartitionFileName(partitionNumber);
                    await this._orchestrator.WriteFileAsync(pipeline, destFile, textData, cancellationToken)
                        .ConfigureAwait(false);

                    var destFileDetails = new DataPipeline.GeneratedFileDetails
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        ParentId = uploadedFile.Id,
                        Name = destFile,
                        Size = text.Length,
                        MimeType = MimeTypes.PlainText,
                        ArtifactType = DataPipeline.ArtifactTypes.TextPartition,
                        PartitionNumber = partitionNumber,
                        SectionNumber = sectionNumber,
                        Tags = pipeline.Tags,
                        ContentSHA256 = textData.CalculateSha256(),
                    };
                    newFiles.Add(destFile, destFileDetails);
                    destFileDetails.MarkProcessedBy(this);
                }

                file.MarkProcessedBy(this);
            }

            // Add new files to pipeline status
            foreach (var file in newFiles)
            {
                uploadedFile.GeneratedFiles.Add(file.Key, file.Value);
            }
        }

        return (ReturnType.Success, pipeline);
    }
}