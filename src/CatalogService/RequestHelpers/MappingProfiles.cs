using System;
using AutoMapper;
using CatalogService.DTOs;
using CatalogService.Entities;

namespace CatalogService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Category, CategoryDto>().IncludeMembers(c => c.Items);
        CreateMap<Item, ItemDto>().IncludeMembers(c => c.Category);
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<CreateItemDto, Item>();
        CreateMap<UpdateItemDto, Item>().ForMember(c => c.Category, o => o.MapFrom(s => s));
    }

}
