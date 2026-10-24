/**
 * Các tab của Trang cá nhân. Mỗi tab là một hàm (container, ctx) và tự tải dữ liệu của chính mình.
 * ctx.hocKys: học kỳ của sinh viên, lấy từ bảng điểm (giảng viên không có quyền xem danh mục học kỳ).
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { escapeHtml } from '../core/dom.js';
import { formatDate, formatNumber } from '../core/format.js';
import { Badge } from '../components/badge.js';
import { Button } from '../components/button.js';
import { EmptyState } from '../components/empty-state.js';
import { Modal } from '../components/modal.js';
import { Pagination } from '../components/pagination.js';
import { Select } from '../components/select.js';
import { Skeleton } from '../components/skeleton.js';
import { Table } from '../components/table.js';
import { Toast } from '../components/toast.js';
import { openFormModal } from './graduation-forms.js';

// Giá trị enum được backend trả về dạng chuỗi (xem các DTO trong Modules/*/Dtos).
const TRANG_THAI_SINH_VIEN = { DangHoc: ['success', 'Đang học'], BaoLuu: ['warning', 'Bảo lưu'], ThoiHoc: ['destructive', 'Thôi học'], DaTotNghiep: ['info', 'Đã tốt nghiệp'] };
const PHUC_KHAO = { ChoXuLy: ['warning', 'Chờ xử lý'], DaChapNhan: ['success', 'Đã chấp nhận'], TuChoi: ['destructive', 'Từ chối'] };
const LOP_HOC_PHAN = { MoDangKy: ['info', 'Mở đăng ký'], DangHoc: ['success', 'Đang học'], DaKetThuc: ['neutral', 'Đã kết thúc'], DaHuy: ['destructive', 'Đã hủy'] };
const KHEN_THUONG_KY_LUAT = { KhenThuong: ['success', 'Khen thưởng'], KyLuat: ['destructive', 'Kỷ luật'] };
const CANH_BAO = { CanhBao: ['destructive', 'Cảnh báo học vụ'], KhongCanhBao: ['success', 'Không cảnh báo'], ChuaDuDuLieu: ['neutral', 'Chưa đủ dữ liệu'] };

/** @returns {string} */
function badgeOf(map, key) {
  const [variant, label] = map[key] ?? ['neutral', key ?? '—'];
  return Badge.render({ label, variant });
}

/** Chuỗi đã escape, hoặc "—" khi không có dữ liệu. */
function dash(value) {
  return value === null || value === undefined || value === '' ? '—' : escapeHtml(value);
}

/** Số đã định dạng, hoặc "—" khi không có dữ liệu. */
function number(value, decimals) {
  return escapeHtml(formatNumber(value, decimals) || '—');
}

/**
 * Bộ lọc một ô chọn phía trên tab. onChange được gọi khi người dùng đổi lựa chọn.
 * @returns {{ body: HTMLElement, value: () => string|number|undefined }}
 */
function filterBar(container, { name, label, options, value = '', numeric = false, onChange }) {
  let current = value;
  container.innerHTML = `<div class="filter-bar">${Select.render({ name, label, options, value })}</div><div class="filter-body"></div>`;
  container.querySelector(`select[name="${name}"]`).addEventListener('change', (event) => {
    current = event.target.value;
    onChange();
  });
  return {
    body: container.querySelector('.filter-body'),
    value: () => (current === '' ? undefined : numeric ? Number(current) : current),
  };
}

function hocKyOptions(hocKys, includeAll) {
  const all = includeAll ? [{ value: '', label: 'Tất cả học kỳ' }] : [];
  return [...all, ...hocKys.map((hk) => ({ value: hk.id, label: `${hk.ten} · ${hk.tenNamHoc}` }))];
}

/**
 * Bảng có phân trang. load({ page, pageSize }) trả về { data, total, page, pageSize }.
 * Trả về reload() để vẽ lại từ trang 1, dùng khi đổi bộ lọc.
 */
function pagedTable(container, { load, columns, caption, actions = [], onAction, empty }) {
  let page = 1;
  let pageSize = 10;
  let rows = [];

  const draw = () =>
    renderAsync(container, {
      load: async () => {
        const result = await load({ page, pageSize });
        rows = result.data;
        return result;
      },
      skeleton: () => Skeleton.table(5, columns.length),
      isEmpty: (result) => result.total === 0,
      empty: () =>
        empty ?? EmptyState.render({ icon: 'inbox', title: 'Chưa có dữ liệu', description: 'Dữ liệu sẽ hiển thị khi có phát sinh.' }),
      render: (result) =>
        Table.render({
          columns,
          rows: result.data,
          rowKey: 'id',
          caption,
          actions,
          actionLabel: (row) => `Thao tác cho dòng ${row.id}`,
          footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
        }),
      errorTitle: 'Không tải được dữ liệu',
    });

  Table.init(container, {
    onAction: (action, rowId) => {
      const row = rows.find((item) => String(item.id) === String(rowId));
      if (row) onAction?.(action, row);
    },
  });
  Pagination.init(container, {
    onChange: (next) => {
      page = next.page;
      pageSize = next.pageSize;
      draw();
    },
  });
  draw();
  return {
    reload: () => {
      page = 1;
      draw();
    },
  };
}

/* ----- Hồ sơ ----- */

function studentProfile(data) {
  return `<dl class="detail-list">
    <dt>MSSV</dt><dd>${dash(data.mssv)}</dd>
    <dt>Họ và tên</dt><dd>${dash(data.hoTen)}</dd>
    <dt>Ngày sinh</dt><dd>${dash(formatDate(data.ngaySinh))}</dd>
    <dt>Email</dt><dd>${dash(data.email)}</dd>
    <dt>Điện thoại</dt><dd>${dash(data.soDienThoai)}</dd>
    <dt>Khoa</dt><dd>${dash(data.tenKhoa)}</dd>
    <dt>Ngành</dt><dd>${dash(data.tenNganh)}</dd>
    <dt>Lớp hành chính</dt><dd>${dash(data.tenLop)}</dd>
    <dt>Cố vấn học tập</dt><dd>${dash(data.tenCoVan)}</dd>
    <dt>Trạng thái</dt><dd>${badgeOf(TRANG_THAI_SINH_VIEN, data.trangThai)}</dd>
  </dl>`;
}

function lecturerProfile(data) {
  return `<dl class="detail-list">
    <dt>Mã giảng viên</dt><dd>${dash(data.ma)}</dd>
    <dt>Họ và tên</dt><dd>${dash(data.hoTen)}</dd>
    <dt>Học vị</dt><dd>${dash(data.hocVi)}</dd>
    <dt>Bộ môn</dt><dd>${dash(data.tenBoMon)}</dd>
    <dt>Email</dt><dd>${dash(data.email)}</dd>
    <dt>Điện thoại</dt><dd>${dash(data.soDienThoai)}</dd>
  </dl>`;
}

/** @param {'SinhVien'|'GiangVien'} role */
export function profileTab(container, role) {
  const isStudent = role === 'SinhVien';
  return renderAsync(container, {
    load: () => (isStudent ? api.cuaToi.sinhVien() : api.cuaToi.giangVien()),
    skeleton: () => Skeleton.text(6),
    isEmpty: (data) => !data,
    empty: () =>
      EmptyState.render({ icon: 'user-round', title: 'Chưa liên kết hồ sơ', description: 'Liên hệ phòng đào tạo để liên kết tài khoản với hồ sơ.' }),
    render: (data) => (isStudent ? studentProfile(data) : lecturerProfile(data)),
    errorTitle: 'Không tải được hồ sơ',
  });
}

/* ----- Học tập ----- */

function semesterMarkup(hk) {
  const table = Table.render({
    caption: `Điểm ${hk.tenHocKy}`,
    columns: [
      { key: 'tenMonHoc', label: 'Môn học', primary: true, render: (r) => dash(`${r.maMonHoc} · ${r.tenMonHoc}`) },
      { key: 'soTinChi', label: 'Tín chỉ', align: 'right' },
      { key: 'diemTongKet', label: 'Điểm', align: 'right', render: (r) => number(r.diemTongKet) },
      { key: 'diemChu', label: 'Điểm chữ', render: (r) => dash(r.diemChu) },
    ],
    rows: hk.danhSachMonHoc,
    rowKey: 'dangKyHocPhanId',
  });
  return `<h3 class="h3 mt-md">${dash(hk.tenHocKy)} · ${dash(hk.tenNamHoc)}</h3>
    <p class="form-hint">GPA ${number(hk.gpa, 2)}${hk.gpaTamTinh ? ' (tạm tính)' : ''} · CPA ${number(hk.cpa, 2)}</p>
    ${table}`;
}

export function bangDiemTab(container) {
  return renderAsync(container, {
    load: () => api.cuaToi.bangDiem(),
    skeleton: () => Skeleton.table(5, 4),
    isEmpty: (data) => data.danhSachHocKy.length === 0,
    empty: () => EmptyState.render({ icon: 'clipboard-list', title: 'Chưa có kết quả học tập' }),
    render: (data) => `<p class="form-hint">CPA ${number(data.cpa, 2)} · Tín chỉ tích lũy đạt ${dash(data.tongTinChiTichLuyDat)}</p>
      ${data.danhSachHocKy.map(semesterMarkup).join('')}`,
    errorTitle: 'Không tải được bảng điểm',
  });
}

export function canhBaoTab(container, ctx) {
  const load = (body, hocKyId) =>
    renderAsync(body, {
      load: () => api.cuaToi.canhBao(hocKyId),
      skeleton: () => Skeleton.text(4),
      isEmpty: (data) => !data,
      empty: () => EmptyState.render({ icon: 'shield-check', title: 'Không có dữ liệu cảnh báo' }),
      render: (data) => `<dl class="detail-list">
        <dt>Học kỳ</dt><dd>${dash(data.tenHocKy)}</dd>
        <dt>GPA / CPA</dt><dd>${number(data.gpa, 2)} / ${number(data.cpa, 2)}</dd>
        <dt>Tín chỉ đăng ký</dt><dd>${dash(data.tongTinChiDangKy)}</dd>
        <dt>Tín chỉ không đạt</dt><dd>${dash(data.soTinChiKhongDat)}</dd>
        <dt>Đánh giá</dt><dd>${badgeOf(CANH_BAO, data.trangThaiCanhBao)}</dd>
        <dt>Lý do</dt><dd>${dash((data.danhSachLyDo ?? []).join('; '))}</dd>
      </dl>`,
      errorTitle: 'Không tải được cảnh báo',
    });

  const filter = filterBar(container, {
    name: 'hocKyId',
    label: 'Học kỳ',
    options: hocKyOptions(ctx.hocKys, false),
    value: ctx.hocKys[0]?.id ?? '',
    numeric: true,
    onChange: () => load(filter.body, filter.value()),
  });
  if (filter.value()) load(filter.body, filter.value());
  else filter.body.innerHTML = EmptyState.render({ icon: 'calendar', title: 'Chưa có học kỳ nào' });
}

export function lichSuTab(container) {
  return renderAsync(container, {
    load: () => api.cuaToi.lichSuHocTap(),
    skeleton: () => Skeleton.table(4, 4),
    empty: () => EmptyState.render({ icon: 'history', title: 'Chưa có lịch sử học tập', description: 'Thay đổi trạng thái học tập sẽ được ghi tại đây.' }),
    render: (rows) =>
      Table.render({
        caption: 'Lịch sử trạng thái học tập',
        rows,
        rowKey: 'id',
        columns: [
          { key: 'ngayHieuLuc', label: 'Ngày hiệu lực', primary: true, render: (r) => dash(formatDate(r.ngayHieuLuc)) },
          { key: 'trangThaiCu', label: 'Trạng thái cũ', render: (r) => badgeOf(TRANG_THAI_SINH_VIEN, r.trangThaiCu) },
          { key: 'trangThaiMoi', label: 'Trạng thái mới', render: (r) => badgeOf(TRANG_THAI_SINH_VIEN, r.trangThaiMoi) },
          { key: 'soQuyetDinh', label: 'Số quyết định', render: (r) => dash(r.soQuyetDinh) },
          { key: 'lyDo', label: 'Lý do', render: (r) => dash(r.lyDo) },
        ],
      }),
    errorTitle: 'Không tải được lịch sử',
  });
}

/* ----- Học phí, học bổng, rèn luyện, khen thưởng ----- */

async function showPayments(row) {
  try {
    const payments = await api.cuaToi.lichSuThanhToan(row.id);
    const handle = Modal.open({
      title: `Thanh toán · ${row.tenHocKy}`,
      size: 'lg',
      content: payments.length
        ? Table.render({
            caption: 'Lịch sử thanh toán',
            rows: payments,
            rowKey: 'id',
            columns: [
              { key: 'ngayThanhToan', label: 'Ngày', primary: true, render: (r) => dash(formatDate(r.ngayThanhToan)) },
              { key: 'soTien', label: 'Số tiền', align: 'right', render: (r) => number(r.soTien) },
              { key: 'hinhThuc', label: 'Hình thức', render: (r) => dash(r.hinhThuc) },
            ],
          })
        : '<p class="form-hint">Chưa có lần thanh toán nào.</p>',
      footer: Button.render({ label: 'Đóng', variant: 'primary', action: 'close' }),
    });
    handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);
  } catch (error) {
    Toast.error(error.message);
  }
}

export function hocPhiTab(container) {
  pagedTable(container, {
    load: (params) => api.cuaToi.hocPhi(params),
    caption: 'Học phí',
    columns: [
      { key: 'tenHocKy', label: 'Học kỳ', primary: true },
      { key: 'soTien', label: 'Phải nộp', align: 'right', render: (r) => number(r.soTien) },
      { key: 'soTienDaNop', label: 'Đã nộp', align: 'right', render: (r) => number(r.soTienDaNop) },
      { key: 'soConNo', label: 'Còn nợ', align: 'right', render: (r) => number(r.soConNo) },
      { key: 'daThanhToan', label: 'Trạng thái', render: (r) => (r.daThanhToan ? Badge.render({ label: 'Đã nộp đủ', variant: 'success' }) : Badge.render({ label: 'Còn nợ', variant: 'warning' })) },
    ],
    actions: [{ action: 'lichSu', label: 'Lịch sử thanh toán', icon: 'history' }],
    onAction: (action, row) => {
      if (action === 'lichSu') showPayments(row);
    },
  });
}

export function hocBongTab(container, ctx) {
  const filter = filterBar(container, {
    name: 'hocKyId',
    label: 'Học kỳ',
    options: hocKyOptions(ctx.hocKys, true),
    numeric: true,
    onChange: () => table.reload(),
  });
  const table = pagedTable(filter.body, {
    load: (params) => api.cuaToi.hocBong({ ...params, hocKyId: filter.value() }),
    caption: 'Học bổng',
    columns: [
      { key: 'ten', label: 'Học bổng', primary: true },
      { key: 'tenHocKy', label: 'Học kỳ', render: (r) => dash(r.tenHocKy) },
      { key: 'soTien', label: 'Số tiền', align: 'right', render: (r) => number(r.soTien) },
    ],
  });
}

export function diemRenLuyenTab(container, ctx) {
  const filter = filterBar(container, {
    name: 'hocKyId',
    label: 'Học kỳ',
    options: hocKyOptions(ctx.hocKys, true),
    numeric: true,
    onChange: () => table.reload(),
  });
  const table = pagedTable(filter.body, {
    load: (params) => api.cuaToi.diemRenLuyen({ ...params, hocKyId: filter.value() }),
    caption: 'Điểm rèn luyện',
    columns: [
      { key: 'tenHocKy', label: 'Học kỳ', primary: true },
      { key: 'diem', label: 'Điểm', align: 'right' },
      { key: 'xepLoai', label: 'Xếp loại', render: (r) => dash(r.xepLoai) },
    ],
  });
}

export function khenThuongTab(container) {
  const filter = filterBar(container, {
    name: 'loai',
    label: 'Loại',
    options: [
      { value: '', label: 'Tất cả' },
      { value: 'KhenThuong', label: 'Khen thưởng' },
      { value: 'KyLuat', label: 'Kỷ luật' },
    ],
    onChange: () => table.reload(),
  });
  const table = pagedTable(filter.body, {
    load: (params) => api.cuaToi.khenThuong({ ...params, loai: filter.value() }),
    caption: 'Khen thưởng, kỷ luật',
    columns: [
      { key: 'loai', label: 'Loại', primary: true, render: (r) => badgeOf(KHEN_THUONG_KY_LUAT, r.loai) },
      { key: 'soQuyetDinh', label: 'Số quyết định', render: (r) => dash(r.soQuyetDinh) },
      { key: 'ngayQuyetDinh', label: 'Ngày', render: (r) => dash(formatDate(r.ngayQuyetDinh)) },
      { key: 'noiDung', label: 'Nội dung', render: (r) => dash(r.noiDung) },
    ],
  });
}

/* ----- Phúc khảo ----- */

export function phucKhaoTab(container) {
  pagedTable(container, {
    load: (params) => api.cuaToi.phucKhao(params),
    caption: 'Yêu cầu phúc khảo',
    columns: [
      { key: 'lyDo', label: 'Lý do', primary: true, render: (r) => dash(r.lyDo) },
      { key: 'diemCu', label: 'Điểm cũ', align: 'right', render: (r) => number(r.diemCu) },
      { key: 'diemMoi', label: 'Điểm mới', align: 'right', render: (r) => number(r.diemMoi) },
      { key: 'trangThai', label: 'Trạng thái', render: (r) => badgeOf(PHUC_KHAO, r.trangThai) },
    ],
  });
}

/**
 * Form gửi phúc khảo. Chỉ liệt kê môn đã khóa điểm vì backend chỉ nhận phúc khảo cho điểm đã khóa.
 * @param {() => void} onDone
 */
export async function openPhucKhaoForm(onDone) {
  let bangDiem;
  try {
    bangDiem = await api.cuaToi.bangDiem();
  } catch (error) {
    Toast.error(error.message);
    return;
  }

  const options = bangDiem.danhSachHocKy.flatMap((hk) =>
    hk.danhSachMonHoc
      .filter((mon) => mon.diemHocPhanId && mon.daKhoa)
      .map((mon) => ({
        value: mon.diemHocPhanId,
        label: `${mon.maMonHoc} · ${mon.tenMonHoc} (${hk.tenHocKy}, điểm ${mon.diemTongKet ?? '—'})`,
      })),
  );
  if (options.length === 0) {
    Toast.error('Chưa có môn nào đã khóa điểm để phúc khảo.');
    return;
  }

  openFormModal({
    title: 'Yêu cầu phúc khảo điểm',
    fields: [
      { name: 'diemHocPhanId', label: 'Môn học', kind: 'select', numeric: true, required: true, options },
      { name: 'lyDo', label: 'Lý do phúc khảo', kind: 'text', required: true, hint: 'Tối đa 500 ký tự.' },
    ],
    submitText: 'Gửi yêu cầu',
    successMessage: 'Đã gửi yêu cầu phúc khảo.',
    onSubmit: (payload) => api.cuaToi.taoPhucKhao(payload),
    onDone,
  });
}

/* ----- Giảng viên ----- */

export function lopDayTab(container) {
  pagedTable(container, {
    load: (params) => api.cuaToi.lopDay(params),
    caption: 'Lớp học phần giảng dạy',
    columns: [
      { key: 'ma', label: 'Mã lớp', primary: true },
      { key: 'tenMonHoc', label: 'Môn học', render: (r) => dash(`${r.maMonHoc} · ${r.tenMonHoc}`) },
      { key: 'siSoHienTai', label: 'Sĩ số', align: 'right', render: (r) => `${r.siSoHienTai} / ${r.siSoToiDa}` },
      { key: 'trangThai', label: 'Trạng thái', render: (r) => badgeOf(LOP_HOC_PHAN, r.trangThai) },
    ],
  });
}
