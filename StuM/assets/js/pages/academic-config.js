/**
 * Cấu hình các danh mục học vụ (module Academic của backend): cột, trường form, bộ lọc.
 * Trang academic.js đọc cấu hình này để dựng bảng, form và bộ lọc cho từng tab.
 *
 * Trường (field):
 *   name, label, kind: 'text' | 'int' | 'decimal' | 'date' | 'bool' | 'ref' (chọn theo danh mục khác)
 *   required, hint, refKey (khóa trong refs khi kind = 'ref')
 * Bộ lọc (filter): name, label, param (tham số query của backend), refKey
 */

import { api } from '../core/api.js';
import { escapeHtml } from '../core/dom.js';
import { formatDate, formatNumber } from '../core/format.js';

const yesNo = (value) => (value ? 'Có' : 'Không');

export const DANH_MUC = {
  khoa: {
    label: 'Khoa',
    title: 'Khoa',
    api: () => api.khoa,
    columns: [
      { key: 'ma', label: 'Mã', primary: true },
      { key: 'ten', label: 'Tên' },
      { key: 'moTa', label: 'Mô tả' },
    ],
    fields: [
      { name: 'ma', label: 'Mã khoa', kind: 'text', required: true },
      { name: 'ten', label: 'Tên khoa', kind: 'text', required: true },
      { name: 'moTa', label: 'Mô tả', kind: 'text' },
    ],
    filters: [],
  },
  boMon: {
    label: 'Bộ môn',
    title: 'Bộ môn',
    api: () => api.boMon,
    columns: [
      { key: 'ma', label: 'Mã', primary: true },
      { key: 'ten', label: 'Tên' },
      { key: 'tenKhoa', label: 'Khoa' },
    ],
    fields: [
      { name: 'ma', label: 'Mã bộ môn', kind: 'text', required: true },
      { name: 'ten', label: 'Tên bộ môn', kind: 'text', required: true },
      { name: 'khoaId', label: 'Khoa', kind: 'ref', refKey: 'khoa', required: true },
    ],
    filters: [{ name: 'khoaId', label: 'Khoa', param: 'KhoaId', refKey: 'khoa' }],
  },
  nganh: {
    label: 'Ngành',
    title: 'Ngành đào tạo',
    api: () => api.nganh,
    columns: [
      { key: 'ma', label: 'Mã', primary: true },
      { key: 'ten', label: 'Tên' },
      { key: 'tenKhoa', label: 'Khoa' },
    ],
    fields: [
      { name: 'ma', label: 'Mã ngành', kind: 'text', required: true },
      { name: 'ten', label: 'Tên ngành', kind: 'text', required: true },
      { name: 'khoaId', label: 'Khoa', kind: 'ref', refKey: 'khoa', required: true },
    ],
    filters: [{ name: 'khoaId', label: 'Khoa', param: 'KhoaId', refKey: 'khoa' }],
  },
  chuyenNganh: {
    label: 'Chuyên ngành',
    title: 'Chuyên ngành',
    api: () => api.chuyenNganh,
    columns: [
      { key: 'ma', label: 'Mã', primary: true },
      { key: 'ten', label: 'Tên' },
      { key: 'tenNganh', label: 'Ngành' },
    ],
    fields: [
      { name: 'ma', label: 'Mã chuyên ngành', kind: 'text', required: true },
      { name: 'ten', label: 'Tên chuyên ngành', kind: 'text', required: true },
      { name: 'nganhId', label: 'Ngành', kind: 'ref', refKey: 'nganh', required: true },
    ],
    filters: [{ name: 'nganhId', label: 'Ngành', param: 'NganhId', refKey: 'nganh' }],
  },
  namHoc: {
    label: 'Năm học',
    title: 'Năm học',
    api: () => api.namHoc,
    columns: [
      { key: 'ten', label: 'Năm học', primary: true },
      { key: 'ngayBatDau', label: 'Bắt đầu', render: (row) => formatDate(row.ngayBatDau) },
      { key: 'ngayKetThuc', label: 'Kết thúc', render: (row) => formatDate(row.ngayKetThuc) },
    ],
    fields: [
      { name: 'ten', label: 'Tên năm học (ví dụ 2025-2026)', kind: 'text', required: true },
      { name: 'ngayBatDau', label: 'Ngày bắt đầu', kind: 'date', required: true },
      { name: 'ngayKetThuc', label: 'Ngày kết thúc', kind: 'date', required: true },
    ],
    filters: [],
  },
  hocKy: {
    label: 'Học kỳ',
    title: 'Học kỳ',
    api: () => api.hocKy,
    columns: [
      { key: 'ten', label: 'Học kỳ', primary: true },
      { key: 'soThuTu', label: 'Thứ tự', align: 'right' },
      { key: 'tenNamHoc', label: 'Năm học' },
      { key: 'ngayBatDau', label: 'Bắt đầu', render: (row) => formatDate(row.ngayBatDau) },
      { key: 'ngayKetThuc', label: 'Kết thúc', render: (row) => formatDate(row.ngayKetThuc) },
    ],
    fields: [
      { name: 'ten', label: 'Tên học kỳ', kind: 'text', required: true },
      { name: 'soThuTu', label: 'Số thứ tự', kind: 'int', required: true },
      { name: 'namHocId', label: 'Năm học', kind: 'ref', refKey: 'namHoc', required: true },
      { name: 'ngayBatDau', label: 'Ngày bắt đầu', kind: 'date', required: true },
      { name: 'ngayKetThuc', label: 'Ngày kết thúc', kind: 'date', required: true },
    ],
    filters: [{ name: 'namHocId', label: 'Năm học', param: 'NamHocId', refKey: 'namHoc' }],
  },
  quyCheHocVu: {
    label: 'Quy chế',
    title: 'Quy chế học vụ (tham số nghiệp vụ)',
    api: () => api.quyCheHocVu,
    columns: [
      { key: 'ma', label: 'Mã', primary: true },
      { key: 'ten', label: 'Tên' },
      { key: 'giaTri', label: 'Giá trị', render: (row) => `${row.giaTri}${row.donVi ? ` ${row.donVi}` : ''}` },
      { key: 'apDungTuKhoa', label: 'Áp dụng từ khóa', align: 'right' },
    ],
    fields: [
      { name: 'ma', label: 'Mã quy chế', kind: 'text', required: true, hint: 'Mã dùng trong hệ thống, ví dụ TC_TOI_DA' },
      { name: 'ten', label: 'Tên', kind: 'text', required: true },
      { name: 'giaTri', label: 'Giá trị', kind: 'text', required: true },
      { name: 'donVi', label: 'Đơn vị', kind: 'text' },
      { name: 'apDungTuKhoa', label: 'Áp dụng từ khóa', kind: 'int' },
      { name: 'moTa', label: 'Mô tả', kind: 'text' },
    ],
    filters: [],
    extraRowActions: [{ action: 'traCuu', label: 'Tra cứu theo mã', icon: 'search' }],
  },
  chuongTrinhDaoTao: {
    label: 'Chương trình đào tạo',
    title: 'Chương trình đào tạo',
    api: () => api.chuongTrinhDaoTao,
    columns: [
      { key: 'ma', label: 'Mã', primary: true },
      { key: 'ten', label: 'Tên' },
      { key: 'tenNganh', label: 'Ngành' },
      { key: 'khoaTuyen', label: 'Khóa tuyển', align: 'right' },
      { key: 'tongSoTinChi', label: 'Tổng TC', align: 'right' },
      { key: 'soLuongMonHoc', label: 'Số môn', align: 'right' },
    ],
    fields: [
      { name: 'ma', label: 'Mã CTĐT', kind: 'text', required: true },
      { name: 'ten', label: 'Tên chương trình', kind: 'text', required: true },
      { name: 'nganhId', label: 'Ngành', kind: 'ref', refKey: 'nganh', required: true },
      { name: 'khoaTuyen', label: 'Khóa tuyển', kind: 'int', required: true },
      { name: 'tongSoTinChi', label: 'Tổng số tín chỉ', kind: 'int', required: true },
      { name: 'cpaToiThieu', label: 'CPA tối thiểu tốt nghiệp', kind: 'decimal' },
      { name: 'yeuCauThucTap', label: 'Yêu cầu thực tập', kind: 'bool' },
      { name: 'diemThucTapToiThieu', label: 'Điểm thực tập tối thiểu', kind: 'decimal' },
      { name: 'yeuCauKhoaLuan', label: 'Yêu cầu khóa luận', kind: 'bool' },
      { name: 'diemBaoVeToiThieu', label: 'Điểm bảo vệ tối thiểu', kind: 'decimal' },
    ],
    filters: [{ name: 'nganhId', label: 'Ngành', param: 'NganhId', refKey: 'nganh' }],
    extraRowActions: [{ action: 'monHoc', label: 'Môn học trong CTĐT', icon: 'book-open' }],
  },
  giangVien: {
    label: 'Giảng viên',
    title: 'Giảng viên',
    api: () => api.giangVien,
    columns: [
      { key: 'ma', label: 'Mã', primary: true },
      { key: 'hoTen', label: 'Họ và tên' },
      { key: 'hocVi', label: 'Học vị' },
      { key: 'tenBoMon', label: 'Bộ môn' },
    ],
    fields: [
      { name: 'ma', label: 'Mã giảng viên', kind: 'text', required: true },
      { name: 'hoTen', label: 'Họ và tên', kind: 'text', required: true },
      { name: 'hocVi', label: 'Học vị', kind: 'text' },
      { name: 'email', label: 'Email', kind: 'text' },
      { name: 'soDienThoai', label: 'Điện thoại', kind: 'text' },
      { name: 'boMonId', label: 'Bộ môn', kind: 'ref', refKey: 'boMon', required: true },
    ],
    filters: [],
  },
  lopHanhChinh: {
    label: 'Lớp hành chính',
    title: 'Lớp hành chính',
    api: () => api.lopHanhChinh,
    columns: [
      { key: 'ma', label: 'Mã lớp', primary: true },
      { key: 'ten', label: 'Tên lớp' },
      { key: 'tenNganh', label: 'Ngành' },
      { key: 'tenCoVan', label: 'Cố vấn', render: (row) => escapeHtml(row.tenCoVan ?? 'Chưa gán') },
      { key: 'soSinhVien', label: 'Sĩ số', align: 'right' },
    ],
    fields: [
      { name: 'ma', label: 'Mã lớp', kind: 'text', required: true },
      { name: 'ten', label: 'Tên lớp', kind: 'text', required: true },
      { name: 'nganhId', label: 'Ngành', kind: 'ref', refKey: 'nganh', required: true },
      { name: 'khoaTuyen', label: 'Khóa tuyển', kind: 'int', required: true },
      { name: 'coVanId', label: 'Cố vấn học tập', kind: 'ref', refKey: 'giangVien' },
    ],
    filters: [],
    extraRowActions: [
      { action: 'phanLop', label: 'Phân sinh viên vào lớp', icon: 'user-plus' },
      { action: 'coVan', label: 'Gán cố vấn', icon: 'user-round' },
    ],
  },
  phongHoc: {
    label: 'Phòng học',
    title: 'Phòng học',
    api: () => api.phongHoc,
    columns: [
      { key: 'ma', label: 'Mã phòng', primary: true },
      { key: 'sucChua', label: 'Sức chứa', align: 'right' },
    ],
    fields: [
      { name: 'ma', label: 'Mã phòng', kind: 'text', required: true },
      { name: 'sucChua', label: 'Sức chứa', kind: 'int', required: true },
    ],
    filters: [],
  },
  thongBao: {
    label: 'Thông báo',
    title: 'Thông báo',
    api: () => api.thongBao,
    columns: [
      { key: 'tieuDe', label: 'Tiêu đề', primary: true },
      { key: 'ngayDang', label: 'Ngày đăng', render: (row) => formatDate(row.ngayDang) },
      { key: 'tenNguoiTao', label: 'Người đăng', render: (row) => escapeHtml(row.tenNguoiTao ?? '—') },
    ],
    fields: [
      { name: 'tieuDe', label: 'Tiêu đề', kind: 'text', required: true },
      { name: 'noiDung', label: 'Nội dung', kind: 'text', required: true },
      { name: 'ngayDang', label: 'Ngày đăng', kind: 'date', required: true },
    ],
    filters: [],
  },
};

export { formatNumber, yesNo };

/**
 * Chuyển giá trị ô form sang kiểu backend cần.
 * @param {{ kind: string }} field
 * @param {string} raw
 */
export function toPayloadValue(field, raw) {
  const value = String(raw ?? '').trim();
  if (value === '') return null;
  switch (field.kind) {
    case 'int':
    case 'ref':
      return Number(value);
    case 'decimal':
      return Number(value);
    case 'bool':
      return value === 'true';
    default:
      return value;
  }
}
