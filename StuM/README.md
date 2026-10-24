# StuM - Hệ thống Quản lý Sinh viên & Đào tạo

Giao diện quản trị (enterprise dashboard) viết bằng HTML5, CSS3 thuần và Vanilla JavaScript ES6 Modules.
Không framework, không build tool, không `node_modules`. Chỉ phụ thuộc Google Fonts (Inter) và Lucide Icons qua CDN.

## Chạy ứng dụng

ES Modules không chạy khi mở `file://` trực tiếp (lỗi CORS), nên cần một web server tĩnh.

**Cách 1 - Python (khuyến nghị):**

```bash
cd StuM
python -m http.server 8000
```

Rồi mở `http://localhost:8000`.

**Cách 2 - VS Code:** cài extension *Live Server*, chuột phải `index.html` → *Open with Live Server*.

Tài khoản demo: `admin` / `admin123`.

## Các trang

| File | Nội dung |
| --- | --- |
| `index.html` | Đăng nhập (layout 2 cột trên desktop, chỉ form trên mobile) |
| `dashboard.html` | 4 KPI, biểu đồ đường SVG vẽ thủ công, việc cần làm, sinh viên mới nhất |
| `students.html` | CRUD sinh viên: tìm kiếm (debounce 300ms), lọc khoa/trạng thái, sắp xếp, phân trang, xem/sửa/xóa |
| `courses.html` | CRUD môn học. Không cho xóa môn đang có lớp học phần |
| `classes.html` | CRUD lớp học phần. Chọn môn từ danh sách, kiểm tra sĩ số |
| `grades.html` | Xem điểm và nhập điểm nhanh (sửa trực tiếp QT/CK trên bảng) |
| `ui-kit.html` | Showcase toàn bộ component ở cả light và dark theme |

## Cấu trúc

```
StuM/
├── index.html, dashboard.html, students.html, courses.html, classes.html, grades.html, ui-kit.html
├── assets/
│   ├── css/       tokens.css (design tokens), base.css (reset, typography, utilities),
│   │              layout.css (sidebar, topbar), components.css (component), pages.css (riêng từng trang)
│   ├── js/
│   │   ├── core/        api.js (mock API), router.js, dom.js, format.js, storage.js, theme.js, async-view.js
│   │   ├── components/  button, input, select, card, badge, table, pagination, dropdown,
│   │   │                modal, toast, empty-state, skeleton, line-chart
│   │   ├── layout/      sidebar.js, topbar.js, shell.js
│   │   ├── pages/       login, dashboard, students (+ students-form), courses, classes, grades, ui-kit
│   │   └── main.js      entry point: theme, bảo vệ route, dựng shell, nạp module trang
│   └── img/
└── README.md
```

## Dữ liệu và trạng thái

- Mock API (`assets/js/core/api.js`) trả về Promise sau 500ms. Dữ liệu được lưu trong `localStorage` với prefix `stum:`.
- Xóa `localStorage` của trang để đưa dữ liệu về mẫu ban đầu.
- Thêm `?error` vào URL (ví dụ `students.html?error=1`) để mọi request thất bại và kiểm tra trạng thái lỗi.
- Điểm tổng kết = 50% quá trình + 50% cuối kỳ. Xếp loại: ≥9 Xuất sắc, ≥8 Giỏi, ≥6.5 Khá, ≥5 Trung bình, còn lại Yếu.

## Quy ước

- Mọi màu lấy từ token `hsl(var(--...))`, không hardcode mã màu.
- Mọi dữ liệu đưa vào HTML đi qua `escapeHtml()`.
- Mỗi khu vực dữ liệu có đủ loading (skeleton), empty và error (`core/async-view.js`).
- Xóa, khóa, hủy luôn qua `Modal.confirm()`. Không dùng `alert`, `confirm`, `prompt` của trình duyệt.
- Phím tắt: `Tab` di chuyển focus, `Enter` gửi form, `Esc` đóng modal, drawer và menu. Mũi tên lên/xuống chọn mục trong menu.

## Responsive

- Dưới 768px: sidebar ẩn, mở bằng nút hamburger (drawer). Bảng chuyển thành danh sách card.
- 768–1024px: sidebar thu gọn mặc định.
- Trên 1024px: sidebar mở rộng.

## Kiểm thử

Đã kiểm thử tự động bằng Playwright trên Microsoft Edge (headless): đăng nhập, lọc, thêm/sửa/xóa, lỗi 409, chặn xóa môn đang dùng, nhập điểm nhanh, đổi theme, drawer mobile, không tràn ngang ở 390px.
