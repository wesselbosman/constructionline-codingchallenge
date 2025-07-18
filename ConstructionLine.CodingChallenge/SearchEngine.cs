using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstructionLine.CodingChallenge;

public class SearchEngine
{
    private readonly List<Shirt> _shirts;
    private readonly ILookup<(Color, Size), Shirt> _shirtLookup;
    private readonly ILookup<Color, Shirt> _shirtLookupByColor;
    private readonly ILookup<Size, Shirt> _shirtLookupBySize;

    public SearchEngine(List<Shirt> shirts)
    {
        _shirts = shirts;
        
        // Use pre-computed indexes to speed up searches at the cost of wasting tons of memory
        // reducing the query from 7 ms to 2 ms at the expense of giving a cloud provider tons more money.
        _shirtLookup = shirts.ToLookup(s => (s.Color, s.Size));
        _shirtLookupByColor = shirts.ToLookup(s => s.Color);
        _shirtLookupBySize = shirts.ToLookup(s => s.Size);
    }

    public SearchResults Search(SearchOptions options)
    {
        // Turn the filter options into flags so it makes the switch expression more readable.
        var filterOptions = FilterOptions.None;
        if (options.Colors.Count > 0) filterOptions |= FilterOptions.HasColorFilter;
        if (options.Sizes.Count > 0) filterOptions |= FilterOptions.HasSizeFilter;

        var filteredShirts = filterOptions switch
        {
            FilterOptions.HasColorFilter | FilterOptions.HasSizeFilter => options.Colors
                .SelectMany(color => options.Sizes.Select(size => _shirtLookup[(color, size)]))
                .SelectMany(x => x)
                .ToList(),
            FilterOptions.HasColorFilter => options.Colors
                .SelectMany(color => _shirtLookupByColor[color]).ToList(),
            FilterOptions.HasSizeFilter => options.Sizes
                .SelectMany(size => _shirtLookupBySize[size]).ToList(),
            _ => _shirts.ToList()
        };

        var sizeAggregate = Size.All
            .Select(size => new SizeCount
            {
                Size = size,
                Count = filteredShirts.Count(s =>
                    (options.Colors.Count <= 0 || options.Colors.Contains(s.Color)) &&
                    s.Size == size)
            })
            .ToList();
            
        var colorAggregate = Color.All
            .Select(color => new ColorCount
            {
                Color = color,
                Count = filteredShirts.Count(s =>
                    (options.Sizes.Count <= 0 || options.Sizes.Contains(s.Size)) &&
                    s.Color == color)
            })
            .ToList();
            
        // Based on the readme the test is wrong since the second example indicates that it should aggregate counts based on filtered results.
        // Due to the readme being confirmed as the source of truth, I amended the code to do that and ignore the failing test.
        return new SearchResults
        {
            Shirts = filteredShirts,
            SizeCounts = sizeAggregate,
            ColorCounts = colorAggregate
        };
    }
        
    [Flags]
    private enum FilterOptions
    {
        None = 0,
        HasColorFilter = 1 << 0, // 1
        HasSizeFilter = 1 << 1   // 2
    }
}