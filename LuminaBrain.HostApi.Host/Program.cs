using LuminaBrain.HostApi.Host.Extensions;
using LuminaBrain.HttpApi.Extensions;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using tusdotnet;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Luminabrain",
            Version = "V1",
            Contact = new OpenApiContact(){Name = "AUSTIN_Zhang"},
            Description = "Smart RAG Platform"
        };
        return Task.CompletedTask;
    });
});

builder.Services.AddLuminaBrain(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();//映射Scalar的API参考文档路径
    app.MapOpenApi();//映射OpenApi文档路径
}



//file upload interface to interact the wiki file.
app.MapTus("/files", async httpContext => new()
{
    // This method is called on each request so different configurations can be returned per user, domain, path etc.
    // Return null to disable tusdotnet for the current request.
    // Where to store data?
    Store = new tusdotnet.Stores.TusDiskStore(@"F:\tusfiles\"),
    Events = new()
    {
        // What to do when file is completely uploaded?
        OnFileCompleteAsync = async eventContext =>
        {
            tusdotnet.Interfaces.ITusFile file = await eventContext.GetFileAsync();
            Dictionary<string, tusdotnet.Models.Metadata> metadata =
                await file.GetMetadataAsync(eventContext.CancellationToken);
            await using Stream content = await file.GetContentAsync(eventContext.CancellationToken);
            //herer to do something else.
        }
    }
});

await app.UseLuminaBrain(builder.Configuration);

app.MapApis();

app.UseHttpsRedirection();

app.Run();

