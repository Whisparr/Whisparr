using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Indexers.Newznab
{
    public static class NewznabCategoryFieldOptionsConverter
    {
        public static List<FieldSelectOption> GetFieldSelectOptions(List<NewznabCategory> categories)
        {
            // Categories not relevant for Whisparr
            var ignoreCategories = new[] { 1000, 3000, 4000, 7000 };

            // And maybe relevant for specific users
            var unimportantCategories = new[] { 0, 2000, 5000 };

            var result = new List<FieldSelectOption>();

            if (categories == null)
            {
                // Fetching categories failed, use default Newznab categories
                categories = new List<NewznabCategory>();
                categories.Add(new NewznabCategory
                {
                    Id = 6000,
                    Name = "XXX",
                    Subcategories = new List<NewznabCategory>
                    {
                        new NewznabCategory { Id = 6010, Name = "DVD" },
                        new NewznabCategory { Id = 6020, Name = "WMV" },
                        new NewznabCategory { Id = 6030, Name = "XVid" },
                        new NewznabCategory { Id = 6040, Name = "x264" },
                        new NewznabCategory { Id = 6050, Name = "Pack" },
                        new NewznabCategory { Id = 6070, Name = "Other" }
                    }
                });
            }

            foreach (var category in categories.Where(cat => !ignoreCategories.Contains(cat.Id)).OrderBy(cat => unimportantCategories.Contains(cat.Id)).ThenBy(cat => cat.Id))
            {
                result.Add(new FieldSelectOption
                {
                    Value = category.Id,
                    Name = category.Name,
                    Hint = $"({category.Id})"
                });

                if (category.Subcategories != null)
                {
                    foreach (var subcat in category.Subcategories.OrderBy(cat => cat.Id))
                    {
                        result.Add(new FieldSelectOption
                        {
                            Value = subcat.Id,
                            Name = subcat.Name,
                            Hint = $"({subcat.Id})",
                            ParentValue = category.Id
                        });
                    }
                }
            }

            return result;
        }
    }
}
