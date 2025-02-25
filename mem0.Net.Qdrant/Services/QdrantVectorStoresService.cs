﻿using System.Drawing;
using System.Text.Json;
using mem0.Net.Model;
using mem0.Net.VectorStores;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using QDistance = Qdrant.Client.Grpc.Distance;
using MDistance = mem0.Net.Distance;
using OptimizerStatus = mem0.Net.Model.OptimizerStatus;


namespace mem0.Net.Qdrant.Services;

public class QdrantVectorStoresService(QdrantClient client) : IVectorStoreService
{
    public async Task CreateColAsync(string name, ulong vectorSize, MDistance distance = MDistance.Cosine)
    {
        //判断集合是否存在
        if (await client.CollectionExistsAsync(name))
        {
            return;
        }

        //创建collection 根据相似度算法，以及集合向量坐标高度
        await client.CreateCollectionAsync(name, new VectorParams()
        {
            Size = vectorSize,
            Distance = ToQdrantDistance(distance)
        });
    }

    public async Task InsertAsync(string name, List<List<float>> vectors,
        List<Dictionary<string, object>> payloads = null,
        List<Guid> ids = null)
    {
        var points = vectors.Select((vector, index) =>
        {
            var item = new PointStruct
            {
                Vectors = vector.ToArray(),
            };

            if (ids != null && ids.Count > index)
            {
                item.Id = new PointId(ids[index]);
            }

            foreach (var payload in payloads[index])
            {
                switch (payload.Value)
                {
                    case string str:
                        item.Payload.Add(payload.Key, str);
                        break;
                    case float f:
                        item.Payload.Add(payload.Key, f);
                        break;
                    case int i:
                        item.Payload.Add(payload.Key, i);
                        break;
                    case bool b:
                        item.Payload.Add(payload.Key, b);
                        break;
                    case Color color:
                        item.Payload.Add(payload.Key, color.ToArgb());
                        break;
                    case DateTime dateTime:
                        item.Payload.Add(payload.Key, dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        break;
                    default:
                        item.Payload.Add(payload.Key, JsonSerializer.Serialize(payload.Value));
                        break;
                }
            }

            return item;
        }).ToList();

        await client.UpsertAsync(name, points);
    }

    private QDistance ToQdrantDistance(MDistance distance)
    {
        return distance switch
        {
            MDistance.Cosine => QDistance.Cosine,
            MDistance.UnknownDistance => QDistance.UnknownDistance,
            MDistance.Euclid => QDistance.Euclid,
            MDistance.Dot => QDistance.Dot,
            MDistance.Manhattan => QDistance.Manhattan,
            _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, null)
        };
    }

    public async Task<List<SearchHit>> SearchAsync(string name, float[] query, ulong limit = 5UL,
        Dictionary<string, object>? filters = null)
    {
        var filter = CreateFilter(filters);
        var hits = await client.SearchAsync(name, query.ToArray(), filter, limit: limit);

        return hits.Select(hit => new SearchHit
        {
            Id = Guid.Parse(hit.Id.Uuid),
            Score = hit.Score,
            Payload = hit.Payload.ToDictionary(x => x.Key, x => x.Value.StringValue),
        }).ToList();
    }

    private Filter CreateFilter(Dictionary<string, object>? filters)
    {
        var conditions = new List<Condition>();

        foreach (var filter in filters)
        {
            if (filter.Value is Dictionary<string, object> rangeDict &&
                rangeDict.ContainsKey("gte") && rangeDict.TryGetValue("lte", out var value))
            {
                conditions.Add(new Condition
                {
                    Field = new FieldCondition()
                    {
                        Key = filter.Key,
                        Match = new Match
                        {
                            Text = value.ToString()
                        }
                    },
                });
            }
            else
            {
                conditions.Add(new Condition()
                {
                    Field = new FieldCondition
                    {
                        Key = filter.Key,
                        Match = new Match
                        {
                            Text = filter.Value.ToString()
                        }
                    }
                });
            }
        }

        var filterValue = new Filter()
        {
            Must =
            {
                conditions
            }
        };
        return conditions.Any() ? filterValue : null;
    }

    public async Task DeleteAsync(string name, Guid vectorId)
    {
        await client.DeleteAsync(name, vectorId);
    }

    public async Task UpdateAsync(string name, Guid vectorId, List<float> vector = null,
        Dictionary<string, string> payload = null)
    {
        var point = new PointStruct()
        {
            Id = vectorId,
            Vectors = vector?.ToArray()
        };

        if (payload != null)
        {
            foreach (var item in payload)
            {
                point.Payload.Add(item.Key, item.Value);
            }
        }

        await client.UpsertAsync(name, new List<PointStruct> { point });
    }

    public async Task<VectorData> GetAsync(string name, Guid vectorId)
    {
        var result = (await client.RetrieveAsync(name, ids: new List<PointId>()
        {
            vectorId
        }, true, true)).FirstOrDefault();


        return new VectorData()
        {
            Id = new Guid(result.Id.Uuid),
            Vector = result.Vectors.Vector.Data.ToList(),
            MetaData = result.Payload.ToDictionary(x => x.Key, x => x.Value.StringValue)
        };
    }

    public async Task<IReadOnlyList<string>> ListColsAsync()
    {
        return await client.ListCollectionsAsync();
    }

    public async Task DeleteColAsync(string name)
    {
        await client.DeleteCollectionAsync(name);
    }

    public async Task<VectorInfo> ColInfoAsync(string name)
    {
        var result = await client.GetCollectionInfoAsync(name);

        return new VectorInfo()
        {
            HasVectorsCount = result.HasVectorsCount,
            Status = (VectorInfoStatus)((int)result.Status),
            OptimizerStatus = new OptimizerStatus()
            {
                Ok = result.OptimizerStatus.Ok,
                Error = result.OptimizerStatus.Error
            },
            HasPointsCount = result.HasPointsCount,
            PayloadSchema = result.PayloadSchema.ToDictionary(x => x.Key, x => (object)x.Value),
            PointsCount = result.PointsCount,
            SegmentsCount = result.SegmentsCount
        };
    }

    public async Task<List<VectorData>> GetListAsync(string name, Dictionary<string, object>? filters = null,
        uint limit = 100U)
    {
        var filter = CreateFilter(filters);

        var result = await client.ScrollAsync(name, filter, limit: limit, vectorsSelector: new WithVectorsSelector()
        {
            Enable = true,
        });

        return result.Result.Select(hit => new VectorData()
        {
            Id = Guid.Parse(hit.Id.Uuid),
            Vector = hit.Vectors.Vector.Data,
            MetaData = hit.Payload.ToDictionary(x => x.Key, x => x.Value.StringValue)
        }).ToList();
    }
}