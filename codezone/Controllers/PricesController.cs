using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Data;
using ScrapWebsite.Models;
using ScrapWebsite.ViewModels;

namespace ScrapWebsite.Controllers;

public class PricesController : Controller
{
    private readonly AppDbContext _dbContext;

    public PricesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var categoryRows = await _dbContext.ScrapCategories
            .AsNoTracking()
            .Where(category => category.Status == "published")
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Id)
            .Select(category => new { category.Id, category.Name, category.Slug, category.Description })
            .ToListAsync(cancellationToken);

        var itemRows = await _dbContext.ScrapItems
            .AsNoTracking()
            .Where(item => item.Status == "published" && item.DeletedAt == null && item.ScrapCategoryId != null)
            .OrderByDescending(item => item.IsFeatured)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .Select(item => new
            {
                item.Id,
                item.Name,
                item.ShortDescription,
                item.PriceLabel,
                item.PriceFrom,
                item.Unit,
                item.ScrapCategoryId,
                Prices = item.Prices
                    .Where(price => price.DeletedAt == null)
                    .OrderBy(price => price.Id)
                    .Select(price => new { price.PriceLabel, price.PriceValue, price.Unit, price.EffectiveDate })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var groups = categoryRows
            .Select(category =>
            {
                var rows = new List<PriceBoardRow>();
                foreach (var item in itemRows.Where(item => item.ScrapCategoryId == category.Id))
                {
                    if (item.Prices.Count > 0)
                    {
                        foreach (var price in item.Prices)
                        {
                            rows.Add(new PriceBoardRow
                            {
                                ItemId = item.Id,
                                ItemName = item.Name,
                                ShortDescription = item.ShortDescription,
                                Label = price.PriceLabel ?? (item.Prices.Count == 1 ? item.PriceLabel : null),
                                Value = price.PriceValue ?? (item.Prices.Count == 1 ? item.PriceFrom : null),
                                Unit = string.IsNullOrWhiteSpace(price.Unit) ? "kg" : price.Unit,
                                EffectiveDate = price.EffectiveDate,
                                IsFirstOfItem = true,
                                RowSpan = 1
                            });
                        }
                    }
                    else
                    {
                        rows.Add(new PriceBoardRow
                        {
                            ItemId = item.Id,
                            ItemName = item.Name,
                            ShortDescription = item.ShortDescription,
                            Label = item.PriceLabel,
                            Value = item.PriceFrom,
                            Unit = string.IsNullOrWhiteSpace(item.Unit) ? "kg" : item.Unit
                        });
                    }
                }

                // Gộp ô theo loại phế liệu: đánh RowSpan cho dòng đầu của mỗi item.
                foreach (var groupItem in rows.GroupBy(row => row.ItemId))
                {
                    var list = groupItem.ToList();
                    list[0].RowSpan = list.Count;
                    for (var index = 1; index < list.Count; index++)
                    {
                        list[index].IsFirstOfItem = false;
                    }
                }

                return new PriceBoardGroup
                {
                    Id = category.Id,
                    Name = category.Name,
                    Slug = category.Slug,
                    Description = category.Description,
                    Rows = rows
                };
            })
            .Where(group => group.Rows.Count > 0)
            .ToList();

        return View(new PriceBoardViewModel { Groups = groups });
    }
}
