using AutoMapper;
using FiapGamesService.Application.Services;
using FiapGamesService.Domain.Interfaces;
using FiapGamesService.Infrastructure.Search;
using FluentAssertions;
using Moq;

namespace FiapGamesService.UnitTests.Games
{
    public class GameServiceTest
    {
        private static GameSearchDocument Doc(
            Guid? id = null, string? name = null, string? desc = null,
            decimal? price = null, string? genre = null, DateTime? createdAt = null)
            => new GameSearchDocument
            {
                Id = id ?? Guid.NewGuid(),
                Name = name ?? "Game X",
                Description = desc ?? "Desc",
                Price = price ?? 99.90m,
                Genre = genre ?? "Action",
                CreatedAt = createdAt ?? DateTime.UtcNow
            };

        private static GameService BuildService(
            Mock<IElasticClient> esMock,
            Mock<IGameCreatedEventRepository>? createdRepoMock = null,
            Mock<IGameChangedEventRepository>? changedRepoMock = null,
            Mock<IMapper>? mapperMock = null)
        {
            createdRepoMock ??= new Mock<IGameCreatedEventRepository>(MockBehavior.Strict);
            changedRepoMock ??= new Mock<IGameChangedEventRepository>(MockBehavior.Strict);
            mapperMock ??= new Mock<IMapper>(MockBehavior.Loose);

            return new GameService(
                createdRepoMock.Object,
                changedRepoMock.Object,
                mapperMock.Object,
                esMock.Object
            );
        }

        [Fact(DisplayName = "SearchEsAsync normaliza paginação (page<=0→1, pageSize<=0→20, cap=100)")]
        public async Task SearchEsAsync_NormalizesPaginationAndCapsPageSize()
        {
            var docs = Enumerable.Range(1, 5).Select(_ => Doc()).ToList();
            long total = 250;

            var es = new Mock<IElasticClient>(MockBehavior.Strict);
            es.Setup(x => x.SearchAsync("doom", "Action", 1, 20, It.IsAny<CancellationToken>()))
              .ReturnsAsync((docs, total));
            es.Setup(x => x.SearchAsync("doom", "Action", 1, 100, It.IsAny<CancellationToken>()))
              .ReturnsAsync((docs, total));

            var svc = BuildService(es);

            var r1 = await svc.SearchEsAsync("doom", "Action", 0, 0, CancellationToken.None);
            var r2 = await svc.SearchEsAsync("doom", "Action", -5, 1000, CancellationToken.None);

            r1.Page.Should().Be(1);
            r1.PageSize.Should().Be(20);
            r1.ItemsCount.Should().Be((int)total);
            r1.TotalPages.Should().Be((int)Math.Ceiling(total / 20.0));
            r1.Data.Should().HaveCount(docs.Count);

            r2.Page.Should().Be(1);
            r2.PageSize.Should().Be(100);
            r2.ItemsCount.Should().Be((int)total);
            r2.TotalPages.Should().Be((int)Math.Ceiling(total / 100.0));
            r2.Data.Should().HaveCount(docs.Count);

            es.VerifyAll();
        }

        [Fact(DisplayName = "SearchEsAsync mapeia documentos e contagens corretamente")]
        public async Task SearchEsAsync_MapsDocsAndCounts()
        {
            var d1 = Doc(name: "Forza", genre: "Racing", price: 199.90m);
            var d2 = Doc(name: "Halo", genre: "FPS", price: 149.90m);
            var docs = new List<GameSearchDocument> { d1, d2 };
            long total = 2;

            var es = new Mock<IElasticClient>(MockBehavior.Strict);
            es.Setup(x => x.SearchAsync(null, null, 1, 20, It.IsAny<CancellationToken>()))
              .ReturnsAsync((docs, total));

            var svc = BuildService(es);

            var r = await svc.SearchEsAsync(null, null, 1, 20, CancellationToken.None);

            r.ItemsCount.Should().Be((int)total);
            r.TotalPages.Should().Be((int)Math.Ceiling(total / 20.0));
            r.Data.Should().HaveCount(2);
            r.Data.Select(i => i.Name).Should().Contain(new[] { "Forza", "Halo" });
            r.Data.Select(i => i.Genre).Should().Contain(new[] { "Racing", "FPS" });

            es.VerifyAll();
        }

        [Fact(DisplayName = "SearchEsAsync passa 'genre' exatamente como recebido (case-sensitive)")]
        public async Task SearchEsAsync_PassesGenreExactly()
        {
            var es = new Mock<IElasticClient>(MockBehavior.Strict);
            es.Setup(x => x.SearchAsync("mario", "rpg", 2, 10, It.IsAny<CancellationToken>()))
              .ReturnsAsync((new List<GameSearchDocument>(), 0L));

            var svc = BuildService(es);

            var r = await svc.SearchEsAsync("mario", "rpg", 2, 10, CancellationToken.None);

            r.Data.Should().BeEmpty();
            r.ItemsCount.Should().Be(0);
            r.TotalPages.Should().Be(0);

            es.Verify(x => x.SearchAsync("mario", "rpg", 2, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "SearchEsAsync com zero resultados retorna lista vazia e contagens zeradas")]
        public async Task SearchEsAsync_NoResults_ReturnsEmpty()
        {
            var es = new Mock<IElasticClient>(MockBehavior.Strict);
            es.Setup(x => x.SearchAsync("nada", "Nada", 1, 20, It.IsAny<CancellationToken>()))
              .ReturnsAsync((new List<GameSearchDocument>(), 0L));

            var svc = BuildService(es);

            var r = await svc.SearchEsAsync("nada", "Nada", 1, 20, CancellationToken.None);

            r.Data.Should().BeEmpty();
            r.ItemsCount.Should().Be(0);
            r.TotalPages.Should().Be(0);
            r.Page.Should().Be(1);
            r.PageSize.Should().Be(20);

            es.VerifyAll();
        }

        [Fact(DisplayName = "TopGenresAsync respeita cap=50 e delega ao IElasticClient")]
        public async Task TopGenresAsync_DelegatesAndCaps()
        {
            var expected = new Dictionary<string, long> { ["Action"] = 120, ["RPG"] = 80 };
            var es = new Mock<IElasticClient>(MockBehavior.Strict);
            es.Setup(x => x.TopGenresAsync(50, It.IsAny<CancellationToken>()))
              .ReturnsAsync(expected);

            var svc = BuildService(es);

            var r = await svc.TopGenresAsync(500, CancellationToken.None);

            r.Should().BeEquivalentTo(expected);
            es.Verify(x => x.TopGenresAsync(50, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
