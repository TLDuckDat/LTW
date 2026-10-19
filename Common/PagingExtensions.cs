using Microsoft.EntityFrameworkCore;

namespace StudentManagement.Common;

public static class PagingExtensions
{
    /// <summary>Đếm tổng số dòng rồi lấy đúng trang theo <paramref name="query"/>.
    /// <paramref name="source"/> phải được sắp xếp sẵn để kết quả phân trang ổn định.</summary>
    public static Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> source, PagedQuery query) =>
        source.ToPagedResultAsync(query, page => page);

    /// <summary>Như trên, nhưng chỉ chạy <paramref name="project"/> (join, chọn cột) trên các dòng
    /// của trang hiện tại thay vì toàn bộ dữ liệu.</summary>
    public static async Task<PagedResult<TResult>> ToPagedResultAsync<TSource, TResult>(
        this IQueryable<TSource> source, PagedQuery query,
        Func<IQueryable<TSource>, IQueryable<TResult>> project)
    {
        var total = await source.CountAsync();
        var page = source.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);
        var items = await project(page).ToListAsync();
        return new PagedResult<TResult>
            { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    /// <summary>Phân trang một danh sách đã nằm sẵn trong bộ nhớ. Chỉ dùng khi phải tính toán xong
    /// (bằng C#, không làm được trong SQL) mới biết dòng nào cần lấy.</summary>
    public static PagedResult<T> ToPagedResult<T>(this IReadOnlyList<T> items, PagedQuery query) => new()
    {
        Items = items.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList(),
        Page = query.Page,
        PageSize = query.PageSize,
        TotalCount = items.Count
    };
}
