namespace FreelanceJobBoard.Application.Common;

public class PagedResult<T>
{
	public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
	{
		Items = items;
		TotalItemsCount = totalCount;
		PageNumber = pageNumber;
		TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
		ItemsFrom = pageSize * (pageNumber - 1) + 1;

		// Correctly handle the last page to prevent "ItemsTo" from exceeding the total count
		ItemsTo = Math.Min(ItemsFrom + pageSize - 1, totalCount);

		HasPreviousPage = pageNumber > 1;
		HasNextPage = pageNumber < TotalPages;
	}

	public IEnumerable<T> Items { get; set; }
	public int TotalPages { get; set; }
	public int PageNumber { get; set; }
	public int TotalItemsCount { get; set; }
	public int ItemsFrom { get; set; }
	public int ItemsTo { get; set; }

	public bool HasPreviousPage { get; }
	public bool HasNextPage { get; }
}