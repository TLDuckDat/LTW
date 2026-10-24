# StuM - Giao diện Quản lý Sinh viên & Đào tạo

Frontend HTML5, CSS3 thuần và Vanilla JavaScript ES6 Modules, không framework và không build tool.
Dữ liệu lấy trực tiếp từ backend **StudentManagement** (ASP.NET Core + SQL Server), không còn dữ liệu mẫu.

## Chạy hệ thống

Cần chạy **backend** trước, rồi **frontend**.

### 1. Backend (thư mục gốc của repo)

```bash
dotnet run --launch-profile http
```

Backend chạy ở `http://localhost:5235`. Ở môi trường Development, backend tự migrate và seed dữ liệu vào SQL Server LocalDB
(`StudentManagementDb`). Swagger: http://localhost:5235/swagger.

### 2. Frontend (thư mục `StuM/`)

```bash
cd StuM
python -m http.server 8000
```

Mở `http://localhost:8000`. Cổng 8000 đã nằm trong `Cors:AllowedOrigins` của `appsettings.Development.json`.
Nếu đổi cổng frontend, thêm origin mới vào file này rồi khởi động lại backend.

Địa chỉ backend nằm trong `assets/js/core/config.js` (`API_BASE_URL`).

### Đăng nhập

Tài khoản seed của backend (xem `README.md` ở thư mục gốc để có danh sách đầy đủ):

| Tài khoản | Mật khẩu | Vai trò |
|---|---|---|
| `admin` | `Admin@123` | Admin |
| `pdt` | `Pdt@1234` | PhongDaoTao |

Frontend dùng JWT của backend: access token 60 phút, refresh token 7 ngày. Khi access token hết hạn, frontend tự
làm mới phiên một lần (các request đồng thời chờ cùng kết quả) rồi gửi lại. Phiên được lưu trong `localStorage`
(khóa `stum:session`).

## Các trang và API backend tương ứng

| Trang | Dữ liệu | Endpoint chính |
|---|---|---|
| `index.html` | Đăng nhập | `POST /api/auth/login` |
| `dashboard.html` | KPI, biểu đồ theo khoa, sinh viên mới, thông báo | `GET /api/bao-cao/tong-quan`, `GET /api/sinh-vien`, `GET /api/thong-bao` |
| `students.html` | CRUD sinh viên, lọc theo khoa/trạng thái, bảo lưu, thôi học, quay lại học | `/api/sinh-vien`, `/api/sinh-vien/{id}/bao-luu`, `/thoi-hoc`, `/quay-lai-hoc` |
| `courses.html` | CRUD môn học, môn tiên quyết | `/api/mon-hoc`, `/api/mon-hoc/{id}/tien-quyet` |
| `classes.html` | CRUD lớp học phần, phân giảng viên, lịch học, bắt đầu/kết thúc/hủy lớp | `/api/lop-hoc-phan`, `/{id}/giang-vien`, `/{id}/lich-hoc`, `/{id}/bat-dau-hoc`... |
| `grades.html` | Nhập điểm theo lớp, tính điểm, khóa điểm | `/api/diem/lop-hoc-phan/{id}`, `/tinh-diem`, `/khoa` |
| `ui-kit.html` | Showcase component (không gọi API) | - |

Danh mục dùng cho ô chọn (khoa, ngành, lớp hành chính, học kỳ, giảng viên, phòng) lấy từ các endpoint `GET` tương ứng.

## Thay đổi so với bản dữ liệu mẫu

- **Dashboard** dùng endpoint mới `GET /api/bao-cao/tong-quan` (mô tả trong Swagger, nhóm Reports): gom KPI, phân bố theo khoa,
  GPA trung bình và học kỳ mốc trong một lần gọi. Thay cho biểu đồ theo tháng và danh sách việc cần làm (không có dữ liệu thật tương ứng),
  dashboard hiện thông báo thật từ `/api/thong-bao`.
- **Sinh viên** không còn cột GPA trong danh sách (backend không trả GPA trong DTO danh sách). Khoa lọc theo `KhoaId` phía server.
- **Điểm** nhập theo lớp học phần, gồm điểm quá trình, giữa kỳ và cuối kỳ. Điểm tổng kết và điểm chữ do backend tính.
  Lớp đã khóa điểm thì ô điểm bị vô hiệu hóa.
- **Danh sách** (sinh viên, môn học, lớp, điểm) được lọc và phân trang phía server. Không còn sắp xếp theo cột vì
  backend không hỗ trợ sắp xếp danh sách.
- Mọi thông báo lỗi là thông báo từ backend (tiếng Việt), ví dụ sai mật khẩu hoặc môn đang được sử dụng nên không xóa được.

## Cấu trúc

```
StuM/
├── index.html, dashboard.html, students.html, courses.html, classes.html, grades.html, ui-kit.html
├── assets/
│   ├── css/       tokens.css, base.css, layout.css, components.css, pages.css
│   └── js/
│       ├── core/        config.js (địa chỉ backend), http.js (JWT, làm mới phiên, ApiResponse),
│       │                api.js (các hàm gọi endpoint), router.js, theme.js, async-view.js, dom.js, format.js, storage.js
│       ├── components/  button, input, select, card, badge, table, pagination, dropdown, modal, toast,
│       │                empty-state, skeleton, line-chart, bar-chart
│       ├── layout/      sidebar.js, topbar.js, shell.js
│       ├── pages/       login, dashboard, students (+ students-form), courses, classes (+ classes-modals), grades, ui-kit
│       └── main.js      entry point: theme, bảo vệ route, dựng shell, nạp module trang
└── README.md
```

## Quy ước

- Mọi màu lấy từ token `hsl(var(--...))`. Mọi dữ liệu đưa vào HTML đi qua `escapeHtml()`.
- Mỗi khu vực dữ liệu có loading (skeleton), empty và error (`core/async-view.js`). Thêm `?error` vào URL để mọi request
  báo lỗi máy chủ, dùng để kiểm thử trạng thái lỗi.
- Xóa, khóa, hủy đều qua `Modal.confirm()`. Không dùng `alert`, `confirm`, `prompt` của trình duyệt.

## Kiểm thử

Bộ kiểm thử tích hợp chạy Playwright trên Microsoft Edge với backend và DB thật: đăng nhập sai và đúng, dashboard, lọc và
phân trang phía server, tạo rồi xóa sinh viên tạm (có bảo lưu và quay lại học), môn tiên quyết, phân giảng viên, khóa điểm,
trạng thái lỗi, làm mới token tự động, đăng xuất. Mọi icon đều render được ở cả 5 trang.
