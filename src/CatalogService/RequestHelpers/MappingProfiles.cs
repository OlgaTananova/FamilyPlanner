using System;
using AutoMapper;
using CatalogService.DTOs;
using CatalogService.Entities;
using Contracts.Catalog;
using Microsoft.OpenApi.Writers;

namespace CatalogService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Category mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items.Where(i => !i.IsDeleted)));

        CreateMap<CategoryDto, Category>()
            .ForMember(dest => dest.Items, opt => opt.Ignore()); // Ignore Items during reverse mapping

        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SKU, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Items, opt => opt.Ignore()); // No items initially in a new category


        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id should remain unchanged
            .ForMember(dest => dest.SKU, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // Optional: preserve current IsDeleted state
            .ForMember(dest => dest.Items, opt => opt.Ignore()); // Items should be managed separately

        // Item mappings
        CreateMap<Item, ItemDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.CategorySKU, opt => opt.MapFrom(src => src.Category.SKU));
        CreateMap<ItemDto, Item>()
            .ForMember(dest => dest.Category, opt => opt.Ignore()); // category will be linked manually

        CreateMap<CreateItemDto, Item>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id is typically generated by the database
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // Optional: set default value or leave it as configured
            .ForMember(dest => dest.Category, opt => opt.Ignore()); // Category is linked through CategoryId

        CreateMap<UpdateItemDto, Item>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id should remain unchanged
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // Optional: preserve current IsDeleted state
            .ForMember(dest => dest.Category, opt => opt.Ignore()); // Category is linked through CategoryId

        CreateMap<ItemDto, CatalogItemCreated>();

        CreateMap<Item, UpdatedItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId));

        CreateMap<ItemDto, CatalogItemDeleted>();

        CreateMap<CategoryDto, CatalogCategoryCreated>()
            .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.SKU));

        CreateMap<CategoryDto, CatalogCategoryDeleted>()
        .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.SKU));

        CreateMap<CategoryDto, CatalogCategoryUpdated>()
            .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.SKU));



    }

}
