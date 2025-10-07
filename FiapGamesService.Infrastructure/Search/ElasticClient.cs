using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace FiapGamesService.Infrastructure.Search
{
    public interface IElasticClient
    {
        Task IndexAsync(GameSearchDocument doc, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<(IReadOnlyList<GameSearchDocument> Docs, long Total)> SearchAsync(
            string? q, string? genre, int page, int size, CancellationToken ct = default);
        Task<Dictionary<string, long>> TopGenresAsync(int size = 10, CancellationToken ct = default);
    }

    public class ElasticClient : IElasticClient
    {
        private readonly ElasticsearchClient _es;
        private readonly string _index;

        public ElasticClient(ElasticsearchClient es, IElasticSettings settings)
        {
            _es = es;
            _index = string.IsNullOrWhiteSpace(settings.Index) ? "games" : settings.Index;
        }

        public async Task IndexAsync(GameSearchDocument doc, CancellationToken ct = default)
        {
            await _es.IndexAsync(doc, i => i.Index(_index).Id(doc.Id), ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            await _es.DeleteAsync<GameSearchDocument>(id.ToString(), d => d.Index(_index), ct);
        }

        public async Task<(IReadOnlyList<GameSearchDocument> Docs, long Total)> SearchAsync(
            string? q, string? genre, int page, int size, CancellationToken ct = default)
        {
            var from = Math.Max(0, (page - 1) * size);

            var must = new List<Query>();
            if (!string.IsNullOrWhiteSpace(q))
            {
                must.Add(new MultiMatchQuery
                {
                    Query = q,
                    Fields = (Fields)"name,description",
                    Fuzziness = new Fuzziness("AUTO")
                });
            }

            var filter = new List<Query>();
            if (!string.IsNullOrWhiteSpace(genre))
            {
                filter.Add(new TermQuery
                {
                    Field = "genre",
                    Value = genre
                });
            }

            var sort = new List<SortOptions>
            {
                new FieldSort { Field = "createdAt",      Order = SortOrder.Desc },
                new FieldSort { Field = "name.keyword",   Order = SortOrder.Asc }
            };

            var req = new SearchRequest<GameSearchDocument>(_index)
            {
                From = from,
                Size = size,
                Sort = sort,
                Query = new BoolQuery
                {
                    Must = must.Count > 0 ? must : null,
                    Filter = filter.Count > 0 ? filter : null
                }
            };

            var resp = await _es.SearchAsync<GameSearchDocument>(req, ct);

            if (!resp.IsValidResponse)
            {
                return (Array.Empty<GameSearchDocument>(), 0);
            }

            return (resp.Documents.ToList(), resp.Total);
        }

        public async Task<Dictionary<string, long>> TopGenresAsync(int size = 10, CancellationToken ct = default)
        {
            var resp = await _es.SearchAsync<GameSearchDocument>(s => s
                .Indices(_index)
                .Size(0)
                .Aggregations(a => a.Add("genres", new Aggregation
                {
                    Terms = new TermsAggregation
                    {
                        Field = new Field("genre"),
                        Size = Math.Min(size, 50)
                    }
                })),
                ct);

            if (!resp.IsValidResponse)
                return new Dictionary<string, long>();

            var terms = resp.Aggregations.GetStringTerms("genres");
            if (terms?.Buckets is null)
                return new Dictionary<string, long>();

            return terms.Buckets.ToDictionary(
                b =>
                {
                    if (b.Key.IsString && b.Key.TryGetString(out var str))
                        return str ?? "";
                    return b.Key.ToString() ?? "";
                },
                b => b.DocCount
            );
        }
    }
}
