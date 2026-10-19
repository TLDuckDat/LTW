using Microsoft.Data.SqlClient;

namespace StudentManagement.Common;

/// <summary>
/// Nhận diện lỗi ràng buộc của SQL Server. Nhận cả <c>DbUpdateException</c> (từ SaveChanges,
/// lỗi SQL nằm trong InnerException) lẫn <c>SqlException</c> (từ ExecuteUpdate).
/// </summary>
public static class SqlErrors
{
    private const int ConstraintConflict = 547;  // vi phạm khóa ngoại hoặc CHECK constraint
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public static bool IsForeignKeyViolation(this Exception ex) =>
        FindSqlException(ex)?.Number == ConstraintConflict;

    public static bool IsUniqueViolation(this Exception ex) =>
        FindSqlException(ex)?.Number is UniqueIndexViolation or UniqueConstraintViolation;

    private static SqlException? FindSqlException(Exception ex) =>
        ex as SqlException ?? ex.InnerException as SqlException;
}
