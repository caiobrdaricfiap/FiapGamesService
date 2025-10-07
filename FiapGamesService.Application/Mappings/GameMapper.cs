using AutoMapper;
using FiapGamesService.Application.DTOs;
using FiapGamesService.Domain.Entities;
using FiapGamesService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Application.Mappings
{
    public class GameMapper : Profile
    {
        public GameMapper()
        {
            CreateMap<GameCreateDto, GameCreatedEvent>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

            CreateMap<GameUpdateDto, GameChangedEvent>()
                .ForMember(d => d.GameId, o => o.MapFrom((_, __, ___, ctx) => (int)ctx.Items["GameId"]))
                .ForMember(d => d.ChangedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.OldName, o => o.MapFrom((s, _, __, ctx) => ((GameDto)ctx.Items["Current"]).Name))
                .ForMember(d => d.NewName, o => o.MapFrom(s => s.Name.Trim()))
                .ForMember(d => d.OldDescription, o => o.MapFrom((s, _, __, ctx) => ((GameDto)ctx.Items["Current"]).Description))
                .ForMember(d => d.NewDescription, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.Description) ? null : s.Description.Trim()))
                .ForMember(d => d.OldGenre, o => o.MapFrom((s, _, __, ctx) => ((GameDto)ctx.Items["Current"]).Genre))
                .ForMember(d => d.NewGenre, o => o.MapFrom(s => s.Genre.Trim()))
                .ForMember(d => d.OldPrice, o => o.MapFrom((s, _, __, ctx) => ((GameDto)ctx.Items["Current"]).Price))
                .ForMember(d => d.NewPrice, o => o.MapFrom(s => decimal.Round(s.Price, 2)))
                .ForMember(d => d.ChangeType, o => o.MapFrom((s, _, __, ctx) =>
                {
                    var cur = (GameDto)ctx.Items["Current"];
                    var newPrice = decimal.Round(s.Price, 2);
                    if (cur.Price != newPrice) return GameChangeType.PriceChanged;
                    if (!string.Equals(cur.Name, s.Name, StringComparison.Ordinal)) return GameChangeType.Renamed;
                    if (!string.Equals(cur.Genre, s.Genre, StringComparison.Ordinal)) return GameChangeType.GenreChanged;
                    if (!string.Equals(cur.Description ?? "", s.Description ?? "", StringComparison.Ordinal)) return GameChangeType.DescriptionChanged;
                    return GameChangeType.Updated;
                }));

            CreateMap<GameState, GameDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Created.Id))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.LastChange != null && s.LastChange.NewName != null ? s.LastChange.NewName : s.Created.Name))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.LastChange != null && s.LastChange.NewDescription != null ? s.LastChange.NewDescription : s.Created.Description))
                .ForMember(d => d.Genre, o => o.MapFrom(s => s.LastChange != null && s.LastChange.NewGenre != null ? s.LastChange.NewGenre : s.Created.Genre))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.LastChange != null && s.LastChange.NewPrice != null ? s.LastChange.NewPrice.Value : s.Created.Price))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.Created.CreatedAt));
        }
    }

    public class GameState
    {
        public GameCreatedEvent Created { get; set; } = default!;
        public GameChangedEvent? LastChange { get; set; }
    }
}
