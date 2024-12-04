using System;
using AutoMapper;
using Contracts.Catalog;
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
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit.ToString()))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CatalogItem.CategoryName))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CatalogItem.CategoryId));

        // ShoppingList -> ShoppingListDto
        CreateMap<ShoppingList, ShoppingListDto>();

        // ShoppingListItemDto -> ShoppingListItem
        CreateMap<ShoppingListItemDto, ShoppingListItem>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<Status>(src.Status)))
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => Enum.Parse<Units>(src.Unit)));

        // ShoppingListDto -> ShoppingList
        CreateMap<ShoppingListDto, ShoppingList>();
       

        //  CatalogItemCreated -> CatalogItem
        CreateMap<CatalogItemCreated, CatalogItem>();
    }
}
