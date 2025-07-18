using System.Collections.Generic;
using System.Linq;

namespace ConstructionLine.CodingChallenge
{
    public class SearchEngine
    {
        private readonly List<Shirt> _shirts;

        public SearchEngine(List<Shirt> shirts)
        {
            _shirts = shirts;
        }
        
        public SearchResults Search(SearchOptions options)
        {
            // Note: Updated projects to target .NET 9 for my development environment compatibility.
            // The core logic remains unchanged from the original challenge requirements.
            
            // Quick performance notes:
            // 1. We scan all 50k shirts to filter them.
            // 2. For size counts, we do a separate scan for each of the 3 sizes (so 3 scans).
            // 3. For color counts, we do another scan for each of the 5 colors (5 scans).
            // In total, that's 9 full passes through the shirts list—about 450,000 operations per search.
            //
            // This is not a good way to do this, I should pre-compute dictionaries with indexes for the aggregations and filters.
            // In a real production system, you'd use technology that implements it much better than I ever could:
            //  - Relational db with indexing (SQL Server or PostgreSQL).
            //  - Elasticsearch for full-text search with faceted aggregations
            //  - Redis with sorted sets for real-time filtering
            //  - Even LINQ providers like Entity Framework convert these expressions to optimized SQL
            //    via expression trees, so the database does the heavy lifting with proper indexes
            // 
            // Also worth noting: LINQ uses expression trees, so this exact same code could be backed by
            // completely different execution engines (SQL, NoSQL, search engines, etc.) without changing
            // the business logic. The abstraction layer means you can start simple and evolve the backend.
            // 
            // For this coding challenge, you could optimize with in-memory dictionaries, but honestly,
            // this O(n) approach is fine for showing the business logic.
            
            var filteredShirts = _shirts
                .Where(x => (options.Colors.Count <= 0 || options.Colors.Contains(x.Color)) &&
                            (options.Sizes.Count <= 0 || options.Sizes.Contains(x.Size)))
                .ToList();
            
            var sizeAggregate = Size.All
                .Select(size => new SizeCount
                {
                    Size = size,
                    Count = _shirts.Count(s =>
                        (options.Colors.Count <= 0 || options.Colors.Contains(s.Color)) &&
                        s.Size == size)
                })
                .ToList();
            
            var colorAggregate = Color.All
                .Select(color => new ColorCount
                {
                    Color = color,
                    Count = _shirts.Count(s =>
                        (options.Sizes.Count <= 0 || options.Sizes.Contains(s.Size)) &&
                        s.Color == color)
                })
                .ToList();
            
            // Initially, based on the readme I interpreted the aggregates to be aggregates of the filtered result but the test failed so either my grasp of English is not as good as I think it is or it's a mining canary for people who use AI.
            // I'm going to assume the test is the version of the truth and aggregate based on all shirts and not just those that match the query options.
            return new SearchResults
            {
                Shirts = filteredShirts,
                SizeCounts = sizeAggregate,
                ColorCounts = colorAggregate
            };
        }
    }
}