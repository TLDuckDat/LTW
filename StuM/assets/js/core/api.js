/**
 * Mock API: Promise + setTimeout giả lập độ trễ mạng, dữ liệu lưu trong localStorage.
 * Thêm ?error vào URL để mọi request thất bại (dùng để kiểm thử error state).
 */

import { Storage } from './storage.js';

const DELAY_MS = 500;
const FORCE_ERROR = new URLSearchParams(window.location.search).has('error');

/** Trọng số điểm tổng kết: 50% quá trình, 50% cuối kỳ (khớp dữ liệu mẫu). */
const WEIGHT_QT = 0.5;
const WEIGHT_CK = 0.5;

export class ApiError extends Error {
  /**
   * @param {string} message
   * @param {number} [status=500]
   */
  constructor(message, status = 500) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

const SEED = {
  students: [
    { id: 'st-1', mssv: 'SV001', email: 'sv001@sv.stum.edu.vn', hoTen: 'Nguyễn Văn A', ngaySinh: '2004-03-12', gioiTinh: 'Nam', khoa: 'CNTT', lop: 'CNTT-K20', gpa: 3.45, trangThai: 'active' },
    { id: 'st-2', mssv: 'SV002', email: 'sv002@sv.stum.edu.vn', hoTen: 'Trần Thị Bình', ngaySinh: '2003-07-25', gioiTinh: 'Nữ', khoa: 'CNTT', lop: 'CNTT-K19', gpa: 3.82, trangThai: 'graduated' },
    { id: 'st-3', mssv: 'SV003', email: 'sv003@sv.stum.edu.vn', hoTen: 'Lê Hoàng Cường', ngaySinh: '2004-11-02', gioiTinh: 'Nam', khoa: 'Kinh tế', lop: 'KT-K20', gpa: 2.9, trangThai: 'active' },
    { id: 'st-4', mssv: 'SV004', email: 'sv004@sv.stum.edu.vn', hoTen: 'Phạm Thị Dung', ngaySinh: '2005-01-18', gioiTinh: 'Nữ', khoa: 'Ngoại ngữ', lop: 'NN-K21', gpa: 3.15, trangThai: 'active' },
    { id: 'st-5', mssv: 'SV005', email: 'sv005@sv.stum.edu.vn', hoTen: 'Hoàng Minh Đức', ngaySinh: '2002-06-30', gioiTinh: 'Nam', khoa: 'Cơ khí', lop: 'CK-K18', gpa: 2.45, trangThai: 'suspended' },
    { id: 'st-6', mssv: 'SV006', email: 'sv006@sv.stum.edu.vn', hoTen: 'Vũ Thị Hà', ngaySinh: '2004-09-09', gioiTinh: 'Nữ', khoa: 'CNTT', lop: 'CNTT-K20', gpa: 3.64, trangThai: 'active' },
    { id: 'st-7', mssv: 'SV007', email: 'sv007@sv.stum.edu.vn', hoTen: 'Đặng Quốc Khánh', ngaySinh: '2003-04-21', gioiTinh: 'Nam', khoa: 'Kinh tế', lop: 'KT-K19', gpa: 3.31, trangThai: 'graduated' },
    { id: 'st-8', mssv: 'SV008', email: 'sv008@sv.stum.edu.vn', hoTen: 'Bùi Thu Lan', ngaySinh: '2005-12-05', gioiTinh: 'Nữ', khoa: 'Ngoại ngữ', lop: 'NN-K21', gpa: 3.92, trangThai: 'active' },
    { id: 'st-9', mssv: 'SV009', email: 'sv009@sv.stum.edu.vn', hoTen: 'Ngô Anh Tuấn', ngaySinh: '2004-02-14', gioiTinh: 'Nam', khoa: 'Cơ khí', lop: 'CK-K20', gpa: 2.1, trangThai: 'dropped' },
    { id: 'st-10', mssv: 'SV010', email: 'sv010@sv.stum.edu.vn', hoTen: 'Đỗ Ngọc Yến', ngaySinh: '2003-08-08', gioiTinh: 'Nữ', khoa: 'CNTT', lop: 'CNTT-K19', gpa: 3.58, trangThai: 'active' },
  ],
  courses: [
    { id: 'co-1', maMon: 'INT1001', tenMon: 'Lập trình Web', soTinChi: 3, khoa: 'CNTT', loaiMon: 'Bắt buộc' },
    { id: 'co-2', maMon: 'INT1002', tenMon: 'Cơ sở dữ liệu', soTinChi: 3, khoa: 'CNTT', loaiMon: 'Bắt buộc' },
    { id: 'co-3', maMon: 'INT1003', tenMon: 'Cấu trúc dữ liệu', soTinChi: 4, khoa: 'CNTT', loaiMon: 'Bắt buộc' },
    { id: 'co-4', maMon: 'INT1004', tenMon: 'Trí tuệ nhân tạo', soTinChi: 3, khoa: 'CNTT', loaiMon: 'Tự chọn' },
    { id: 'co-5', maMon: 'ECO1001', tenMon: 'Kinh tế vi mô', soTinChi: 3, khoa: 'Kinh tế', loaiMon: 'Bắt buộc' },
    { id: 'co-6', maMon: 'ECO1002', tenMon: 'Marketing căn bản', soTinChi: 2, khoa: 'Kinh tế', loaiMon: 'Tự chọn' },
    { id: 'co-7', maMon: 'ENG1001', tenMon: 'Tiếng Anh giao tiếp', soTinChi: 3, khoa: 'Ngoại ngữ', loaiMon: 'Bắt buộc' },
    { id: 'co-8', maMon: 'ENG1002', tenMon: 'Tiếng Anh chuyên ngành', soTinChi: 3, khoa: 'Ngoại ngữ', loaiMon: 'Tự chọn' },
    { id: 'co-9', maMon: 'MEC1001', tenMon: 'Cơ học kỹ thuật', soTinChi: 3, khoa: 'Cơ khí', loaiMon: 'Bắt buộc' },
    { id: 'co-10', maMon: 'MEC1002', tenMon: 'Vẽ kỹ thuật', soTinChi: 2, khoa: 'Cơ khí', loaiMon: 'Bắt buộc' },
  ],
  classes: [
    { id: 'cl-1', maLop: 'INT1001-01', maMon: 'INT1001', giangVien: 'ThS. Lê Thu Hà', hocKy: 'HK1-2026', siSo: 45, siSoToiDa: 50, lichHoc: 'Thứ 2, 7:00-9:30', phong: 'A101' },
    { id: 'cl-2', maLop: 'INT1002-01', maMon: 'INT1002', giangVien: 'TS. Phạm Quang Huy', hocKy: 'HK1-2026', siSo: 50, siSoToiDa: 50, lichHoc: 'Thứ 3, 9:45-12:15', phong: 'A102' },
    { id: 'cl-3', maLop: 'INT1003-01', maMon: 'INT1003', giangVien: 'ThS. Trần Đức Long', hocKy: 'HK1-2026', siSo: 38, siSoToiDa: 45, lichHoc: 'Thứ 4, 13:00-15:30', phong: 'B201' },
    { id: 'cl-4', maLop: 'INT1004-01', maMon: 'INT1004', giangVien: 'TS. Nguyễn Thị Mai', hocKy: 'HK1-2026', siSo: 27, siSoToiDa: 40, lichHoc: 'Thứ 5, 7:00-9:30', phong: 'B203' },
    { id: 'cl-5', maLop: 'ECO1001-01', maMon: 'ECO1001', giangVien: 'ThS. Võ Minh Tuấn', hocKy: 'HK1-2026', siSo: 60, siSoToiDa: 60, lichHoc: 'Thứ 2, 13:00-15:30', phong: 'C101' },
    { id: 'cl-6', maLop: 'ECO1002-01', maMon: 'ECO1002', giangVien: 'ThS. Đinh Thu Trang', hocKy: 'HK2-2025', siSo: 30, siSoToiDa: 40, lichHoc: 'Thứ 6, 9:45-12:15', phong: 'C203' },
    { id: 'cl-7', maLop: 'ENG1001-01', maMon: 'ENG1001', giangVien: 'ThS. Hoàng Anh Thư', hocKy: 'HK1-2026', siSo: 35, siSoToiDa: 40, lichHoc: 'Thứ 3, 7:00-9:30', phong: 'D105' },
    { id: 'cl-8', maLop: 'ENG1002-01', maMon: 'ENG1002', giangVien: 'ThS. Hoàng Anh Thư', hocKy: 'HK2-2025', siSo: 22, siSoToiDa: 35, lichHoc: 'Thứ 5, 13:00-15:30', phong: 'D106' },
    { id: 'cl-9', maLop: 'MEC1001-01', maMon: 'MEC1001', giangVien: 'TS. Bùi Quốc Anh', hocKy: 'HK1-2026', siSo: 42, siSoToiDa: 45, lichHoc: 'Thứ 4, 7:00-9:30', phong: 'E301' },
    { id: 'cl-10', maLop: 'MEC1002-01', maMon: 'MEC1002', giangVien: 'ThS. Đặng Hữu Phước', hocKy: 'HK2-2025', siSo: 18, siSoToiDa: 30, lichHoc: 'Thứ 7, 7:00-9:30', phong: 'E205' },
  ],
  grades: [
    { id: 'gr-1', mssv: 'SV001', maMon: 'INT1001', diemQT: 8.5, diemCK: 7.5, diemTK: 8, xepLoai: 'Giỏi' },
    { id: 'gr-2', mssv: 'SV002', maMon: 'INT1001', diemQT: 9, diemCK: 8.5, diemTK: 8.75, xepLoai: 'Giỏi' },
    { id: 'gr-3', mssv: 'SV003', maMon: 'ECO1001', diemQT: 6, diemCK: 5.5, diemTK: 5.75, xepLoai: 'Trung bình' },
    { id: 'gr-4', mssv: 'SV004', maMon: 'ENG1001', diemQT: 7.5, diemCK: 8, diemTK: 7.75, xepLoai: 'Khá' },
    { id: 'gr-5', mssv: 'SV005', maMon: 'MEC1001', diemQT: 4, diemCK: 4.5, diemTK: 4.25, xepLoai: 'Yếu' },
    { id: 'gr-6', mssv: 'SV006', maMon: 'INT1002', diemQT: 8, diemCK: 7, diemTK: 7.5, xepLoai: 'Khá' },
    { id: 'gr-7', mssv: 'SV007', maMon: 'ECO1002', diemQT: 7, diemCK: 6.5, diemTK: 6.75, xepLoai: 'Khá' },
    { id: 'gr-8', mssv: 'SV008', maMon: 'ENG1002', diemQT: 9.5, diemCK: 9, diemTK: 9.25, xepLoai: 'Xuất sắc' },
    { id: 'gr-9', mssv: 'SV009', maMon: 'MEC1002', diemQT: 3, diemCK: 4, diemTK: 3.5, xepLoai: 'Yếu' },
    { id: 'gr-10', mssv: 'SV010', maMon: 'INT1003', diemQT: 7, diemCK: 8.5, diemTK: 7.75, xepLoai: 'Khá' },
  ],
  todos: [
    { id: 'td-1', text: 'Duyệt danh sách đăng ký học phần HK1-2026', done: false },
    { id: 'td-2', text: 'Nhập điểm giữa kỳ môn Cơ sở dữ liệu', done: false },
    { id: 'td-3', text: 'Xác nhận hồ sơ tốt nghiệp của 12 sinh viên', done: true },
    { id: 'td-4', text: 'Gửi thông báo lịch thi cho giảng viên', done: false },
    { id: 'td-5', text: 'Cập nhật thời khóa biểu phòng B201', done: false },
  ],
};

const DASHBOARD_STATS = {
  totalStudents: { value: 1420, previous: 1366 },
  activeStudents: { value: 1187, previous: 1152 },
  graduationRate: { value: 0.642, previous: 0.618 },
  averageGpa: { value: 3.18, previous: 3.12 },
  enrollmentByMonth: [
    { label: 'T1', value: 820 }, { label: 'T2', value: 905 }, { label: 'T3', value: 960 },
    { label: 'T4', value: 930 }, { label: 'T5', value: 1010 }, { label: 'T6', value: 1080 },
    { label: 'T7', value: 1045 }, { label: 'T8', value: 1120 }, { label: 'T9', value: 1190 },
    { label: 'T10', value: 1240 }, { label: 'T11', value: 1305 }, { label: 'T12', value: 1420 },
  ],
};

const NOTIFICATIONS = [
  { id: 'nt-1', title: 'Hạn nhập điểm giữa kỳ là 30/10', time: '2 giờ trước' },
  { id: 'nt-2', title: 'Lớp INT1002-01 đã đầy sĩ số', time: 'Hôm qua' },
  { id: 'nt-3', title: '3 hồ sơ sinh viên đang chờ duyệt', time: '2 ngày trước' },
];

const CREDENTIALS = { username: 'admin', password: 'admin123' };

/**
 * Chuẩn hóa chuỗi tiếng Việt để tìm kiếm không phân biệt hoa thường và dấu.
 * @param {unknown} value
 * @returns {string}
 */
function normalizeText(value) {
  return String(value ?? '')
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .replace(/đ/g, 'd')
    .replace(/Đ/g, 'd')
    .toLowerCase();
}

/**
 * Tính điểm tổng kết và xếp loại theo trọng số cố định.
 * @param {number} diemQT
 * @param {number} diemCK
 * @returns {{ diemTK: number, xepLoai: string }}
 */
export function computeGrade(diemQT, diemCK) {
  const diemTK = Math.round((diemQT * WEIGHT_QT + diemCK * WEIGHT_CK) * 100) / 100;
  let xepLoai = 'Yếu';
  if (diemTK >= 9) xepLoai = 'Xuất sắc';
  else if (diemTK >= 8) xepLoai = 'Giỏi';
  else if (diemTK >= 6.5) xepLoai = 'Khá';
  else if (diemTK >= 5) xepLoai = 'Trung bình';
  return { diemTK, xepLoai };
}

const clone = (value) => JSON.parse(JSON.stringify(value));

function delay(ms = DELAY_MS) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

/**
 * Bọc mọi request: độ trễ giả lập + lỗi giả lập khi có ?error.
 * @template T
 * @param {() => T} task
 * @returns {Promise<T>}
 */
async function request(task) {
  await delay();
  if (FORCE_ERROR) {
    throw new ApiError('Không kết nối được máy chủ. Vui lòng thử lại.', 503);
  }
  return task();
}

function load(key) {
  const stored = Storage.get(key);
  if (Array.isArray(stored)) return stored;
  Storage.set(key, clone(SEED[key]));
  return clone(SEED[key]);
}

function save(key, rows) {
  Storage.set(key, rows);
}

let idCounter = 0;
function newId(prefix) {
  idCounter += 1;
  return `${prefix}-${Date.now().toString(36)}${idCounter}`;
}

/**
 * Lọc, tìm kiếm, sắp xếp và phân trang một danh sách.
 */
function queryRows(rows, { search = '', searchFields = [], filters = {}, sort, page = 1, pageSize = 10 }) {
  const needle = normalizeText(search).trim();
  let result = rows.filter((row) => {
    const matchFilters = Object.entries(filters).every(
      ([key, value]) => value === '' || value === null || value === undefined || String(row[key]) === String(value),
    );
    const matchSearch =
      !needle || searchFields.some((field) => normalizeText(row[field]).includes(needle));
    return matchFilters && matchSearch;
  });

  if (sort && sort.key) {
    const direction = sort.dir === 'desc' ? -1 : 1;
    result = [...result].sort((a, b) => {
      const left = a[sort.key];
      const right = b[sort.key];
      if (typeof left === 'number' && typeof right === 'number') return (left - right) * direction;
      return String(left ?? '').localeCompare(String(right ?? ''), 'vi') * direction;
    });
  }

  const total = result.length;
  const start = (page - 1) * pageSize;
  return { data: clone(result.slice(start, start + pageSize)), total, page, pageSize };
}

/**
 * Tạo resource CRUD dùng chung cho các trang danh mục.
 * @param {{ key: string, label: string, searchFields: string[], uniqueField?: string, prefix: string }} config
 */
function createResource({ key, label, searchFields, uniqueField, prefix }) {
  return {
    list(options = {}) {
      return request(() => queryRows(load(key), { ...options, searchFields }));
    },

    getAll() {
      return request(() => clone(load(key)));
    },

    get(id) {
      return request(() => {
        const record = load(key).find((row) => row.id === id);
        if (!record) throw new ApiError(`${label} không tồn tại`, 404);
        return clone(record);
      });
    },

    create(payload) {
      return request(() => {
        const rows = load(key);
        if (uniqueField && rows.some((row) => row[uniqueField] === payload[uniqueField])) {
          throw new ApiError(`${label} với mã "${payload[uniqueField]}" đã tồn tại`, 409);
        }
        const record = { ...payload, id: newId(prefix) };
        save(key, [record, ...rows]);
        return clone(record);
      });
    },

    update(id, patch) {
      return request(() => {
        const rows = load(key);
        const index = rows.findIndex((row) => row.id === id);
        if (index === -1) throw new ApiError(`${label} không tồn tại`, 404);
        if (uniqueField && patch[uniqueField] !== undefined) {
          const duplicate = rows.some((row) => row.id !== id && row[uniqueField] === patch[uniqueField]);
          if (duplicate) throw new ApiError(`${label} với mã "${patch[uniqueField]}" đã tồn tại`, 409);
        }
        const record = { ...rows[index], ...patch, id };
        rows[index] = record;
        save(key, rows);
        return clone(record);
      });
    },

    remove(id) {
      return request(() => {
        const rows = load(key);
        const next = rows.filter((row) => row.id !== id);
        if (next.length === rows.length) throw new ApiError(`${label} không tồn tại`, 404);
        save(key, next);
        return true;
      });
    },
  };
}

const students = createResource({
  key: 'students',
  label: 'Sinh viên',
  prefix: 'st',
  uniqueField: 'mssv',
  searchFields: ['mssv', 'hoTen', 'email', 'lop'],
});

const courses = createResource({
  key: 'courses',
  label: 'Môn học',
  prefix: 'co',
  uniqueField: 'maMon',
  searchFields: ['maMon', 'tenMon'],
});

const classes = createResource({
  key: 'classes',
  label: 'Lớp học phần',
  prefix: 'cl',
  uniqueField: 'maLop',
  searchFields: ['maLop', 'giangVien', 'phong'],
});

const grades = createResource({
  key: 'grades',
  label: 'Điểm',
  prefix: 'gr',
  searchFields: ['mssv', 'maMon'],
});

export const api = {
  students,
  courses,
  classes,
  grades,

  /** Cập nhật điểm và tự tính lại điểm tổng kết, xếp loại. */
  updateGrade(id, { diemQT, diemCK }) {
    return grades.update(id, { diemQT, diemCK, ...computeGrade(diemQT, diemCK) });
  },

  getDashboard() {
    return request(() => clone({ ...DASHBOARD_STATS, updatedAt: new Date().toISOString() }));
  },

  getTodos() {
    return request(() => clone(Storage.get('todos', SEED.todos)));
  },

  setTodoDone(id, done) {
    return request(() => {
      const todos = Storage.get('todos', clone(SEED.todos));
      const next = todos.map((todo) => (todo.id === id ? { ...todo, done } : todo));
      Storage.set('todos', next);
      return clone(next.find((todo) => todo.id === id));
    });
  },

  getNotifications() {
    return request(() => clone(NOTIFICATIONS));
  },

  /**
   * Đăng nhập giả lập. Tài khoản demo: admin / admin123.
   * @param {{ username: string, password: string }} credentials
   */
  login({ username, password }) {
    return request(() => {
      if (username !== CREDENTIALS.username || password !== CREDENTIALS.password) {
        throw new ApiError('Tên đăng nhập hoặc mật khẩu không đúng', 401);
      }
      const session = { username, name: 'Quản trị viên', token: `mock-${Date.now().toString(36)}`, loginAt: Date.now() };
      Storage.set('auth', session);
      return clone(session);
    });
  },

  logout() {
    Storage.remove('auth');
  },

  getSession() {
    return Storage.get('auth', null);
  },
};
