using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ShoppingListService.Helpers;

public class TsVectorConverter : ValueConverter<string, string>
{
    public TsVectorConverter()
        : base(v => v, v => v) // Identity conversion as EF Core only stores `tsvector` as-is
    {
    }
}
