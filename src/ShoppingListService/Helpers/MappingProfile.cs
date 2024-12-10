using System;
using AutoMapper;
using Contracts.Catalog;
using MassTransit.Scheduling;
using ShoppingListService.DTOs;
using ShoppingListService.Entities;

namespace ShoppingListService.Helpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ShoppingListItem -> ShoppingListItemDto
        CreateMap<ShoppingListItem, ShoppingListItemDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit.ToString()));

        // ShoppingList -> ShoppingListDto
        CreateMap<ShoppingList, ShoppingListDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items)); // Map nested items


        // ShoppingListItemDto -> ShoppingListItem
        CreateMap<ShoppingListItemDto, ShoppingListItem>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<Status>(src.Status)))
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => Enum.Parse<Units>(src.Unit)));


        // ShoppingListDto -> ShoppingList
        CreateMap<ShoppingListDto, ShoppingList>();


        //  CatalogItemCreated -> CatalogItem
        CreateMap<CatalogItemCreated, CatalogItem>();
        CreateMap<CatalogItemUpdated, CatalogItem>();

        // CatalogItem --> CatalogItem
        CreateMap<CatalogItem, CatalogItemDto>();

        CreateMap<CatalogItem, ShoppingListItem>()
            .ForMember(dest => dest.CatalogItemId, opt => opt.MapFrom(c => c.Id))
             .ForMember(dest => dest.Id, opt => opt.Ignore());


    }
}
