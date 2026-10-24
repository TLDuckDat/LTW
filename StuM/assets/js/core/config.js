/**
 * Cấu hình kết nối backend (StudentManagement, ASP.NET Core).
 * Mặc định là backend chạy ở môi trường Development: dotnet run --launch-profile http
 * Nếu đổi cổng backend thì sửa API_BASE_URL ở đây.
 * Frontend đứng ở origin khác (ví dụ http://localhost:8000) nên origin đó phải nằm trong
 * Cors:AllowedOrigins của appsettings.Development.json.
 */

export const API_BASE_URL = 'http://localhost:5235/api';
