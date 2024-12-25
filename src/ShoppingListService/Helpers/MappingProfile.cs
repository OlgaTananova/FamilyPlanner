using System;
using AutoMapper;
using Contracts.Catalog;
using Contracts.ShoppingLists;
using MassTransit.Scheduling;
using ShoppingListService.DTOs;
using ShoppingListService.Entities;

namespace ShoppingListService.Helpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ShoppingListItem -> ShoppingListItemDto
        CreateMap<ShoppingListItem, DTOs.ShoppingListItemDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit.ToString()));

        // ShoppingList -> ShoppingListDto
        CreateMap<ShoppingList, ShoppingListDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items)); // Map nested items


        // ShoppingListItemDto -> ShoppingListItem
        CreateMap<DTOs.ShoppingListItemDto, ShoppingListItem>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<Status>(src.Status)))
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => Enum.Parse<Units>(src.Unit)));


        // ShoppingListDto -> ShoppingList
        CreateMap<ShoppingListDto, ShoppingList>();


        //  CatalogItemCreated -> CatalogItem
        CreateMap<CatalogItemCreated, CatalogItem>();

        CreateMap<UpdatedItem, CatalogItem>();

        // CatalogItem --> CatalogItem
        CreateMap<CatalogItem, CatalogItemDto>();

        CreateMap<CatalogItem, ShoppingListItem>()
            .ForMember(dest => dest.CatalogItemId, opt => opt.MapFrom(c => c.Id))
             .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<ShoppingListDto, ShoppingListCreated>()
        .ForMember(dest => dest.Items, opt => opt.MapFrom(s => s.Items));

        // Map ShoppingListItemDto to itself
        CreateMap<DTOs.ShoppingListItemDto, Contracts.ShoppingLists.ShoppingListItemDto>();

        CreateMap<ShoppingList, ShoppingListDeleted>();

        CreateMap<ShoppingListDto, ShoppingListUpdated>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<ShoppingList, ShoppingListUpdated>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        CreateMap<ShoppingListDto, ShoppingListItemUpdated>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));    
    }
}
