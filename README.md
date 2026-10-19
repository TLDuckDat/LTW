# Quản lý Sinh viên – StudentManagement

ASP.NET Core Web API (.NET 10) + SQL Server, kiến trúc **Modular Monolith đơn giản**: một project, mỗi module là một thư mục trong `Modules/`.

## 1. Chạy dự án

Yêu cầu: .NET SDK 10, SQL Server LocalDB (đi kèm Visual Studio), công cụ EF:

```bash
dotnet tool install --global dotnet-ef   # chỉ cần một lần
```

```bash
cd StudentManagement
dotnet run --launch-profile http
```

Mở Swagger: http://localhost:5235/swagger

Ở môi trường **Development**, khi khởi động ứng dụng sẽ tự chạy migration và seed dữ liệu mẫu (`Data/DbSeeder.cs`).
Chuỗi kết nối nằm trong `appsettings.json` → `ConnectionStrings:DefaultConnection`.

**JWT secret** không lưu trong repo. Ở Development, nếu để trống, app tự sinh khóa ngẫu nhiên mỗi lần chạy (token cũ mất hiệu lực khi khởi động lại). Muốn giữ khóa cố định:

```bash
dotnet user-secrets set "Jwt:Secret" "<chuỗi ngẫu nhiên tối thiểu 32 byte>"
```

Môi trường khác bắt buộc cấu hình qua biến môi trường `Jwt__Secret`.

Tài khoản mẫu do seed tạo (Admin tạo thêm tài khoản qua `POST /api/auth/accounts`):

| Tài khoản | Mật khẩu | Vai trò | Ghi chú |
|---|---|---|---|
| `admin` | `Admin@123` | Admin | |
| `pdt` | `Pdt@1234` | PhongDaoTao | |
| `gv001` | `Gv@12345` | GiangVien | Dạy lớp IT002-01 học kỳ hiện tại |
| `sv001` | `Sv@12345` | SinhVien | Đã đạt IT001 → đăng ký được IT002 |
| `sv002` | `Sv@12345` | SinhVien | Trượt IT001 → bị chặn IT002 (tiên quyết), học lại IT001 |

Ngày tháng trong seed tính theo ngày chạy: học kỳ hiện tại có đợt đăng ký **đang mở**, học kỳ trước có điểm đã khóa.
Seed chỉ chạy khi CSDL còn trống; muốn seed lại thì xóa CSDL (`dotnet ef database drop`) rồi chạy app.

**CORS**: origin của frontend khai báo trong `Cors:AllowedOrigins`. Development đã cho phép `http://localhost:3000`, `:5173`, `:4200`.

### Đăng nhập, refresh token và nhật ký

- `POST /api/auth/login` trả `token` (access token, **60 phút**, `Jwt:ExpiryMinutes`) và `refreshToken` (**7 ngày**, `Jwt:RefreshTokenDays`).
  Gửi `Authorization: Bearer <token>` cho mọi API.
- Khi access token hết hạn (API trả 401): gọi `POST /api/auth/refresh` với `{ "refreshToken": "..." }` để nhận cặp token **mới**.
  Refresh token chỉ dùng được **một lần**: luôn lưu lại cặp mới. Dùng lại token cũ bị coi là token bị đánh cắp và **mọi phiên**
  của tài khoản bị thu hồi, nên frontend chỉ được gọi refresh một lần tại một thời điểm (các request khác chờ kết quả).
- `POST /api/auth/logout` (gửi refresh token) đăng xuất phiên hiện tại; `POST /api/auth/logout-all` đăng xuất mọi thiết bị.
  Đổi/đặt lại mật khẩu, đổi vai trò hoặc bị khóa tài khoản đều làm mọi token cũ hết hiệu lực.
- **Chống dò mật khẩu**: sai quá **5 lần trong 30 phút** với cùng một tên đăng nhập thì bị chặn (429), kể cả khi nhập đúng,
  đến khi lần sai cũ nhất trong 5 lần quá 30 phút. Đăng nhập đúng xóa bộ đếm; Admin đặt lại mật khẩu hoặc khóa rồi mở khóa
  tài khoản thì gỡ chặn ngay.
- **Nhật ký** (`GET /api/auth/activity-logs`, chỉ Admin): đăng nhập, làm mới phiên, đăng xuất và **mọi thao tác ghi**
  (POST/PUT/PATCH/DELETE) của người đã đăng nhập, kể cả bị từ chối (403) hay lỗi (500): người thực hiện, hành động
  (`SinhVien.Delete`...), đường dẫn, tham số, mã phản hồi, IP, chi tiết nghiệp vụ. Không lưu body request (mật khẩu).
  Lọc theo `taiKhoanId`, `hanhDong` (tiền tố, ví dụ `HocPhi.`), `thanhCong`, `tuNgay`, `denNgay`, `search`.
  Service ghi thêm chi tiết qua `NhatKyContext.Ghi("...")` (ví dụ "vai trò SinhVien -> GiangVien").

## 2. Migration

```bash
# Tạo migration mới sau khi thêm/sửa Entity hoặc Configuration
dotnet ef migrations add <TenMigration> -o Data/Migrations

# Áp dụng vào CSDL (hoặc chỉ cần chạy app ở Development)
dotnet ef database update

# Xóa migration cuối nếu chưa áp dụng
dotnet ef migrations remove
```

> Đặt tên migration theo task, ví dụ `REG03_ThemCotGhiChu`. Trước khi tạo migration hãy `git pull` để tránh hai người cùng sửa model snapshot.

## 3. Module → Epic Jira → Thực thể

| Module | Epic | Route chính | Thực thể |
|---|---|---|---|
| Auth | AUTH | `/api/auth` | TaiKhoan, enum VaiTro |
| Academic | ACA | `/api/khoa`, `/api/bo-mon`, ... | Khoa (**mẫu CRUD**), BoMon, Nganh, ChuyenNganh, NamHoc, HocKy, ChuongTrinhDaoTao |
| Students | STU | `/api/sinh-vien`, `/api/giang-vien`, `/api/lop-hanh-chinh` | SinhVien, GiangVien, LopHanhChinh, enum TrangThaiSinhVien |
| Courses | CRS | `/api/mon-hoc`, `/api/phong-hoc`, `/api/lop-hoc-phan` | MonHoc, MonHocTienQuyet, PhongHoc, LopHocPhan, LichHoc, enum TrangThaiLopHocPhan |
| Registration | REG | `/api/dang-ky-hoc-phan`, `/api/dot-dang-ky` | DotDangKy, DangKyHocPhan, enum TrangThaiDotDangKy, LoaiDangKy, TrangThaiDangKy |
| Grades | GRD | `/api/diem`, `/api/phuc-khao` | DiemHocPhan, PhucKhao, enum TrangThaiPhucKhao |
| StudentAffairs | SA | `/api/cong-tac-sinh-vien` | DiemRenLuyen, HocPhi, MienGiam, HocBong, KhenThuongKyLuat, enum LoaiKhenThuongKyLuat |
| Graduation | GRAD | `/api/tot-nghiep` | ThucTap, DeTaiKhoaLuan, HoiDongBaoVe, XetTotNghiep, enum KetQuaXetTotNghiep |
| Reports | RPT | `/api/bao-cao`, `/api/thong-bao` | ThongBao (báo cáo không có bảng riêng) |

Các API tự phục vụ của sinh viên/giảng viên nằm dưới `.../toi` (ví dụ `/api/diem/toi/bang-diem`, `/api/giang-vien/toi/lop-hoc-phan`).

Tham số nghiệp vụ (đơn giá tín chỉ, tín chỉ tối đa, ngưỡng cảnh báo, ngưỡng học bổng...) đọc từ bảng quy chế `/api/quy-che`;
danh sách mã và giá trị mặc định ở `Modules/Academic/Services/QuyCheThamSo.cs` (`MaQuyChe`).

## 4. Cấu trúc một module

```
Modules/<Module>/
├── Entities/         Lớp thực thể (kế thừa BaseEntity) và enum
├── Configurations/   IEntityTypeConfiguration<T> – bảng, độ dài, index, khóa ngoại
├── Dtos/             Request/Response record
├── Services/         IXxxService + XxxService (dùng AppDbContext trực tiếp)
├── Controllers/      Chỉ gọi Service, trả về ApiResponse<T>
└── <Module>Module.cs AddXxxModule(this IServiceCollection) – đăng ký DI
```

### Quy ước
- Controller **chỉ** gọi Service; Service dùng `AppDbContext` trực tiếp (`_db.Set<T>()`), không có Repository.
- Route: `/api/<tai-nguyen>` (chữ thường, gạch nối).
- Enum lưu dạng chuỗi: `.HasConversion<string>().HasMaxLength(30)`.
- Thực thể đặt tên tiếng Việt không dấu; lớp kỹ thuật tiếng Anh (`KhoaService`, `KhoaDto`...).
- Tham chiếu thực thể **module khác chỉ bằng Id**, không có navigation property. Khóa ngoại vẫn tạo trong CSDL:
  `builder.HasOne<SinhVien>().WithMany().HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Restrict);`
- Mọi phản hồi bọc trong `ApiResponse<T>` (`Success`, `Message`, `Data`); danh sách dùng `PagedQuery` + `PagedResult<T>`.
- Xử lý lỗi: Controller kế thừa `ApiControllerBase`, mỗi action là `Execute(() => service.XxxAsync(...))`.
  Service ném `AppException.NotFound/BadRequest/Conflict/Forbidden(...)` cho lỗi nghiệp vụ; **không** trả `null`/`false`
  và không dùng `InvalidOperationException`. Lỗi khác (bug) thành 500, vẫn đúng định dạng `ApiResponse` nhờ `GlobalExceptionHandler`.

## 5. Thêm một chức năng mới theo mẫu Khoa

Ví dụ làm CRUD cho **BoMon** (task ACA-02):

1. **Entity** – đã có trong `Modules/Academic/Entities/BoMon.cs`. Nếu thêm cột: sửa entity + configuration.
2. **DTO** – tạo `Dtos/BoMonDtos.cs` theo `Dtos/KhoaDtos.cs`: `BoMonDto` (trả về) và `BoMonRequest` (tạo/sửa, có `[Required]`, `[MaxLength]`).
3. **Service** – tạo `IBoMonService` + `BoMonService` theo `KhoaService`:
   - `GetPagedAsync(PagedQuery)` → `AsNoTracking`, lọc `Search`, `Skip/Take`, `Select` sang DTO.
   - `GetByIdAsync`, `CreateAsync` (kiểm tra trùng mã), `UpdateAsync`, `DeleteAsync` (bắt `DbUpdateException` khi bị Restrict).
4. **Controller** – tạo `BoMonController` với `[Route("api/bo-mon")]` theo `KhoaController`: 404 khi không thấy, 409 khi trùng/ràng buộc, 201 khi tạo.
5. **Đăng ký DI** – thêm `services.AddScoped<IBoMonService, BoMonService>();` vào `AcademicModule.cs`. **Không sửa Program.cs.**
6. Xóa phương thức stub tương ứng trong `AcademicService` / `AcademicController` (nếu đã thay thế).
7. Nếu đổi model: `dotnet ef migrations add ACA02_...`, chạy app, test trên Swagger.

## 6. Quy tắc làm nhóm

- Mỗi người làm trong **thư mục module của mình** (`Modules/<Module>/`). Không sửa file module khác; cần thay đổi chung (`Common/`, `Data/`, `Program.cs`) thì báo nhóm trước.
- Cần dữ liệu module khác: truy vấn qua `_db.Set<T>()` theo Id, không thêm navigation sang module khác.
- Branch đặt theo mã Jira: `feature/REG-03`, sửa lỗi: `bugfix/GRD-02`.
- Commit message bắt đầu bằng mã task: `REG-03 Kiểm tra trùng lịch khi đăng ký`.
- Tạo Pull Request vào `develop`, cần ít nhất 1 người review; `dotnet build` phải thành công.
- Migration: mỗi PR tối đa một migration, rebase lên `develop` mới nhất trước khi tạo.
  **Không sửa tay `AppDbContextModelSnapshot.cs`**: thay đổi model luôn đi qua `dotnet ef migrations add`, nếu không CSDL sẽ thiếu bảng/cột
  mà `has-pending-model-changes` vẫn báo không có gì thay đổi.
- Chạy `dotnet test` trước khi tạo PR.

## 7. Kiểm thử

```bash
dotnet test
```

Project `tests/StudentManagement.Tests`:
- `Unit/`: hàm tính toán thuần (quy đổi điểm, GPA, xếp loại, phân trang).
- `Integration/`: chạy app thật trên một CSDL LocalDB riêng tên ngẫu nhiên (tự migrate + seed, xóa khi xong), gọi API như client.
  Mỗi test tự tạo dữ liệu riêng nên không phụ thuộc thứ tự chạy. Không có LocalDB thì đặt biến môi trường `TEST_SQL_SERVER`,
  ví dụ `localhost,1433;User Id=sa;Password=...` cho SQL Server trong Docker.
