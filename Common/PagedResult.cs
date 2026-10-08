namespace StudentManagement.Common;

/// <summary>Kết quả phân trang.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>Tham số truy vấn phân trang + tìm kiếm dùng chung.</summary>
public class PagedQuery
{
    private int _page = 1;
    private int _pageSize = 20;

    public int Page { get => _page; set => _page = value < 1 ? 1 : value; }
    public int PageSize { get => _pageSize; set => _pageSize = value is < 1 or > 100 ? 20 : value; }
    public string? Search { get; set; }
}
