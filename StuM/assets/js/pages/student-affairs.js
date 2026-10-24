/**
 * Trang Công tác sinh viên (module StudentAffairs) và Phúc khảo điểm (module Grades).
 * Sáu tab: Học phí, Học bổng, Miễn giảm, Điểm rèn luyện, Khen thưởng/Kỷ luật, Phúc khảo.
 * Mỗi tab khai báo cột, bộ lọc, thao tác và hành động tạo; bảng, phân trang và hộp thoại dùng chung.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { debounce, escapeHtml } from '../core/dom.js';
import { formatDate, formatNumber } from '../core/format.js';
import { Badge } from '../components/badge.js';
import { Button } from '../components/button.js';
import { EmptyState } from '../components/empty-state.js';
import { Input } from '../components/input.js';
import { Modal } from '../components/modal.js';
import { Pagination } from '../components/pagination.js';
import { Select } from '../components/select.js';
import { Skeleton } from '../components/skeleton.js';
import { Table } from '../components/table.js';
import { Toast } from '../components/toast.js';
import { renderPageHeader } from '../layout/shell.js';
import { openDetailModal, openFormModal } from './graduation-forms.js';

const PHUC_KHAO_STATUS = {
  ChoXuLy: ['neutral', 'Chờ xử lý'],
  DaChapNhan: ['success', 'Chấp nhận'],
  TuChoi: ['destructive', 'Từ chối'],
};
const LOAI_KTKL = { KhenThuong: ['success', 'Khen thưởng'], KyLuat: ['destructive', 'Kỷ luật'] };
const badgeOf = (map, key) => {
  const [variant, label] = map[key] ?? ['neutral', key ?? '—'];
  return Badge.render({ label, variant });
};

const state = { tab: 'hocPhi', page: 1, pageSize: 10, search: '', filters: {}, rows: [] };
let refs = { hocKys: [], students: [] };

const hocKyOptions = () => [{ value: '', label: 'Tất cả học kỳ' }, ...refs.hocKys.map((hk) => ({ value: hk.id, label: `${hk.ten} (${hk.tenNamHoc})` }))];
const sinhVienOptions = () => refs.students.map((sv) => ({ value: sv.id, label: `${sv.mssv} - ${sv.hoTen}` }));
const hocKySelect = (name = 'hocKyId') => ({ name, label: 'Học kỳ', kind: 'select', numeric: true, required: true, options: refs.hocKys.map((hk) => ({ value: hk.id, label: `${hk.ten} (${hk.tenNamHoc})` })) });
const sinhVienSelect = () => ({ name: 'sinhVienId', label: 'Sinh viên', kind: 'select', numeric: true, required: true, searchable: true, options: sinhVienOptions() });

/** Cấu hình các tab. */
const TABS = {
  hocPhi: {
    label: 'Học phí',
    title: 'Học phí sinh viên',
    rowKey: 'id',
    filters: () => [
      { name: 'HocKyId', label: 'Học kỳ', options: hocKyOptions() },
      { name: 'ConNo', label: 'Tình trạng', options: [{ value: '', label: 'Tất cả' }, { value: 'true', label: 'Còn nợ' }, { value: 'false', label: 'Đã thanh toán' }] },
    ],
    load: (p) => api.congTacSinhVien.hocPhi.list({ page: p.page, pageSize: p.pageSize, search: p.search, hocKyId: p.HocKyId, conNo: p.ConNo }),
    columns: [
      { key: 'mssv', label: 'MSSV', primary: true },
      { key: 'hoTen', label: 'Họ và tên' },
      { key: 'tenHocKy', label: 'Học kỳ' },
      { key: 'soTien', label: 'Phải nộp', align: 'right', render: (r) => formatNumber(r.soTien) },
      { key: 'soTienDaNop', label: 'Đã nộp', align: 'right', render: (r) => formatNumber(r.soTienDaNop) },
      { key: 'soConNo', label: 'Còn nợ', align: 'right', render: (r) => formatNumber(r.soConNo) },
      { key: 'daThanhToan', label: 'Trạng thái', render: (r) => (r.daThanhToan ? Badge.render({ label: 'Đã đóng đủ', variant: 'success' }) : Badge.render({ label: 'Còn nợ', variant: 'destructive' })) },
    ],
    createLabel: 'Tính học phí học kỳ',
    create: () =>
      openFormModal({
        title: 'Tính học phí cho học kỳ',
        fields: [hocKySelect()],
        submitText: 'Tính học phí',
        successMessage: 'Đã tính học phí',
        onSubmit: async (payload) => {
          const r = await api.congTacSinhVien.hocPhi.tinh(payload.hocKyId);
          Toast.info(`Tạo mới ${r.soTaoMoi}, cập nhật ${r.soCapNhat}, tổng phải thu ${formatNumber(r.tongPhaiThu)}`);
        },
        onDone: () => loadTab(),
      }),
    actions: [
      { action: 'detail', label: 'Chi tiết', icon: 'eye' },
      { action: 'thanhToan', label: 'Ghi nhận thanh toán', icon: 'wallet' },
      { action: 'lichSu', label: 'Lịch sử thanh toán', icon: 'history' },
    ],
    onAction: async (action, row) => {
      if (action === 'detail') {
        const d = await api.congTacSinhVien.hocPhi.get(row.id);
        openDetailModal({
          title: `Học phí - ${d.mssv}`,
          rows: [
            ['Họ và tên', escapeHtml(d.hoTen)],
            ['Học kỳ', escapeHtml(d.tenHocKy)],
            ['Phải nộp', formatNumber(d.soTien)],
            ['Đã nộp', formatNumber(d.soTienDaNop)],
            ['Còn nợ', formatNumber(d.soConNo)],
            ['Tiền thừa', formatNumber(d.soTienThua)],
          ],
        });
      }
      if (action === 'thanhToan') {
        openFormModal({
          title: `Ghi nhận thanh toán: ${row.mssv}`,
          fields: [
            { name: 'soTien', label: 'Số tiền', kind: 'number', required: true, min: 1 },
            { name: 'hinhThuc', label: 'Hình thức', kind: 'text', required: true, hint: 'Ví dụ: Tiền mặt, Chuyển khoản' },
            { name: 'ghiChu', label: 'Ghi chú', kind: 'text' },
          ],
          submitText: 'Ghi nhận',
          successMessage: 'Đã ghi nhận thanh toán',
          onSubmit: (payload) => api.congTacSinhVien.hocPhi.thanhToan(row.id, payload),
          onDone: () => loadTab(),
        });
      }
      if (action === 'lichSu') {
        const list = await api.congTacSinhVien.hocPhi.lichSuThanhToan(row.id);
        openDetailModal({
          title: `Lịch sử thanh toán - ${row.mssv}`,
          rows: [],
          extra: list.length
            ? Table.render({
                columns: [
                  { key: 'ngayThanhToan', label: 'Ngày', render: (r) => formatDate(r.ngayThanhToan) },
                  { key: 'soTien', label: 'Số tiền', align: 'right', render: (r) => formatNumber(r.soTien) },
                  { key: 'hinhThuc', label: 'Hình thức', render: (r) => escapeHtml(r.hinhThuc ?? '—') },
                  { key: 'tenNguoiThu', label: 'Người thu', render: (r) => escapeHtml(r.tenNguoiThu ?? '—') },
                ],
                rows: list,
                rowKey: 'id',
                caption: 'Lịch sử thanh toán',
              })
            : '<p class="form-hint">Chưa có thanh toán.</p>',
        });
      }
    },
  },

  hocBong: {
    label: 'Học bổng',
    title: 'Học bổng',
    rowKey: 'id',
    filters: () => [{ name: 'HocKyId', label: 'Học kỳ', options: hocKyOptions() }],
    load: (p) => api.congTacSinhVien.hocBong.list({ page: p.page, pageSize: p.pageSize, search: p.search, hocKyId: p.HocKyId }),
    columns: [
      { key: 'mssv', label: 'MSSV', primary: true },
      { key: 'hoTen', label: 'Họ và tên' },
      { key: 'tenHocKy', label: 'Học kỳ' },
      { key: 'ten', label: 'Học bổng' },
      { key: 'soTien', label: 'Số tiền', align: 'right', render: (r) => formatNumber(r.soTien) },
    ],
    createLabel: 'Xét học bổng học kỳ',
    create: () =>
      openFormModal({
        title: 'Xét học bổng cho học kỳ',
        fields: [hocKySelect()],
        submitText: 'Xét học bổng',
        successMessage: 'Đã xét học bổng',
        onSubmit: async (payload) => {
          const r = await api.congTacSinhVien.hocBong.xet(payload.hocKyId);
          Toast.info(`Xét ${r.soSinhVienXet} sinh viên: mới cấp ${r.soMoiCap}, đã có trước đó ${r.soDaCoTruocDo}`);
        },
        onDone: () => loadTab(),
      }),
    actions: [{ action: 'delete', label: 'Thu hồi học bổng', icon: 'trash-2', danger: true }],
    onAction: (action, row) => {
      if (action === 'delete') confirmRemove(`Thu hồi học bổng của ${row.mssv}?`, () => api.congTacSinhVien.hocBong.remove(row.id));
    },
  },

  mienGiam: {
    label: 'Miễn giảm',
    title: 'Miễn giảm học phí',
    rowKey: 'id',
    filters: () => [{ name: 'HocKyId', label: 'Học kỳ', options: hocKyOptions() }],
    load: (p) => api.congTacSinhVien.mienGiam.list({ page: p.page, pageSize: p.pageSize, search: p.search, HocKyId: p.HocKyId }),
    columns: [
      { key: 'mssv', label: 'MSSV', primary: true },
      { key: 'hoTen', label: 'Họ và tên' },
      { key: 'tenHocKy', label: 'Học kỳ' },
      { key: 'tyLe', label: 'Tỷ lệ miễn giảm', align: 'right', render: (r) => formatNumber(r.tyLe) },
      { key: 'lyDo', label: 'Lý do' },
    ],
    createLabel: 'Thêm miễn giảm',
    create: () => miemGiamForm(null),
    actions: [
      { action: 'edit', label: 'Chỉnh sửa', icon: 'pencil' },
      { action: 'delete', label: 'Xóa', icon: 'trash-2', danger: true },
    ],
    onAction: (action, row) => {
      if (action === 'edit') miemGiamForm(row);
      if (action === 'delete') confirmRemove(`Xóa miễn giảm của ${row.mssv}?`, () => api.congTacSinhVien.mienGiam.remove(row.id));
    },
  },

  diemRenLuyen: {
    label: 'Điểm rèn luyện',
    title: 'Điểm rèn luyện',
    rowKey: 'id',
    filters: () => [{ name: 'HocKyId', label: 'Học kỳ', options: hocKyOptions() }],
    load: (p) => api.congTacSinhVien.diemRenLuyen.list({ page: p.page, pageSize: p.pageSize, search: p.search, HocKyId: p.HocKyId }),
    columns: [
      { key: 'mssv', label: 'MSSV', primary: true },
      { key: 'hoTen', label: 'Họ và tên' },
      { key: 'tenHocKy', label: 'Học kỳ' },
      { key: 'diem', label: 'Điểm', align: 'right' },
      { key: 'xepLoai', label: 'Xếp loại', render: (r) => escapeHtml(r.xepLoai ?? '—') },
    ],
    createLabel: 'Nhập điểm rèn luyện',
    create: () => diemRenLuyenForm(null),
    actions: [
      { action: 'edit', label: 'Sửa điểm', icon: 'pencil' },
      { action: 'delete', label: 'Xóa', icon: 'trash-2', danger: true },
    ],
    onAction: (action, row) => {
      if (action === 'edit') diemRenLuyenForm(row);
      if (action === 'delete') confirmRemove(`Xóa điểm rèn luyện của ${row.mssv}?`, () => api.congTacSinhVien.diemRenLuyen.remove(row.id));
    },
  },

  khenThuong: {
    label: 'Khen thưởng, kỷ luật',
    title: 'Khen thưởng và kỷ luật',
    rowKey: 'id',
    filters: () => [{ name: 'Loai', label: 'Loại', options: [{ value: '', label: 'Tất cả' }, { value: 'KhenThuong', label: 'Khen thưởng' }, { value: 'KyLuat', label: 'Kỷ luật' }] }],
    load: (p) => api.congTacSinhVien.khenThuongKyLuat.list({ page: p.page, pageSize: p.pageSize, search: p.search, Loai: p.Loai }),
    columns: [
      { key: 'mssv', label: 'MSSV', primary: true },
      { key: 'hoTen', label: 'Họ và tên' },
      { key: 'loai', label: 'Loại', render: (r) => badgeOf(LOAI_KTKL, r.loai) },
      { key: 'soQuyetDinh', label: 'Số quyết định' },
      { key: 'ngayQuyetDinh', label: 'Ngày QĐ', render: (r) => formatDate(r.ngayQuyetDinh) },
      { key: 'noiDung', label: 'Nội dung' },
    ],
    createLabel: 'Thêm khen thưởng / kỷ luật',
    create: () => khenThuongForm(null),
    actions: [
      { action: 'edit', label: 'Chỉnh sửa', icon: 'pencil' },
      { action: 'delete', label: 'Xóa', icon: 'trash-2', danger: true },
    ],
    onAction: (action, row) => {
      if (action === 'edit') khenThuongForm(row);
      if (action === 'delete') confirmRemove(`Xóa quyết định của ${row.mssv}?`, () => api.congTacSinhVien.khenThuongKyLuat.remove(row.id));
    },
  },

  phucKhao: {
    label: 'Phúc khảo điểm',
    title: 'Phúc khảo điểm',
    rowKey: 'id',
    filters: () => [{ name: 'trangThai', label: 'Trạng thái', options: [{ value: '', label: 'Tất cả' }, ...Object.entries(PHUC_KHAO_STATUS).map(([v, [, l]]) => ({ value: v, label: l }))] }],
    load: (p) => api.phucKhao.list({ page: p.page, pageSize: p.pageSize, search: p.search, trangThai: p.trangThai }),
    columns: [
      { key: 'mssv', label: 'MSSV', primary: true },
      { key: 'hoTen', label: 'Họ và tên' },
      { key: 'lyDo', label: 'Lý do', render: (r) => escapeHtml(r.lyDo ?? '—') },
      { key: 'diemCu', label: 'Điểm cũ', align: 'right' },
      { key: 'diemMoi', label: 'Điểm mới', align: 'right', render: (r) => (r.diemMoi ?? '—') },
      { key: 'trangThai', label: 'Trạng thái', render: (r) => badgeOf(PHUC_KHAO_STATUS, r.trangThai) },
    ],
    createLabel: 'Tạo yêu cầu phúc khảo',
    create: () =>
      openFormModal({
        title: 'Tạo yêu cầu phúc khảo',
        fields: [
          { name: 'diemHocPhanId', label: 'Mã điểm học phần', kind: 'number', required: true, numeric: true, hint: 'Mã bản ghi điểm (diemHocPhanId) của sinh viên' },
          { name: 'lyDo', label: 'Lý do phúc khảo', kind: 'text', required: true },
        ],
        successMessage: 'Đã tạo yêu cầu phúc khảo',
        submitText: 'Tạo yêu cầu',
        onSubmit: (payload) => api.phucKhao.create(payload),
        onDone: () => loadTab(),
      }),
    actions: [{ action: 'xuLy', label: 'Xử lý yêu cầu', icon: 'gavel' }],
    onAction: (action, row) => {
      if (action !== 'xuLy') return;
      openFormModal({
        title: `Xử lý phúc khảo: ${row.mssv}`,
        fields: [
          { name: 'chapNhan', label: 'Quyết định', kind: 'select', required: true, options: [{ value: 'true', label: 'Chấp nhận (cập nhật điểm)' }, { value: 'false', label: 'Từ chối' }] },
          { name: 'diemMoi', label: 'Điểm mới (nếu chấp nhận)', kind: 'number', min: 0, max: 10, step: 0.25 },
        ],
        submitText: 'Xử lý',
        successMessage: 'Đã xử lý phúc khảo',
        onSubmit: (payload) =>
          api.phucKhao.xuLy(row.id, { chapNhan: payload.chapNhan === 'true', diemMoi: payload.diemMoi ?? null }),
        onDone: () => loadTab(),
      });
    },
  },
};

/* ----- Hộp thoại dùng chung ----- */

function confirmRemove(message, run) {
  Modal.confirm({
    title: 'Xác nhận',
    message,
    confirmText: 'Xóa',
    danger: true,
    onConfirm: async () => {
      try {
        await run();
        Toast.success('Đã xóa');
        loadTab();
      } catch (error) {
        Toast.error(error.message);
        throw error;
      }
    },
  }).catch(() => {});
}

function miemGiamForm(record) {
  openFormModal({
    title: record ? 'Chỉnh sửa miễn giảm' : 'Thêm miễn giảm',
    initial: record ?? {},
    fields: record
      ? [
          { name: 'tyLe', label: 'Tỷ lệ miễn giảm', kind: 'number', required: true, min: 0, max: 100, step: 1 },
          { name: 'lyDo', label: 'Lý do', kind: 'text', required: true },
        ]
      : [sinhVienSelect(), hocKySelect(), { name: 'tyLe', label: 'Tỷ lệ miễn giảm', kind: 'number', required: true, min: 0, max: 100, step: 1 }, { name: 'lyDo', label: 'Lý do', kind: 'text', required: true }],
    onSubmit: (payload) => (record ? api.congTacSinhVien.mienGiam.update(record.id, payload) : api.congTacSinhVien.mienGiam.create(payload)),
    onDone: () => loadTab(),
  });
}

function diemRenLuyenForm(record) {
  openFormModal({
    title: record ? 'Sửa điểm rèn luyện' : 'Nhập điểm rèn luyện',
    initial: record ?? {},
    fields: record
      ? [{ name: 'diem', label: 'Điểm (0 - 100)', kind: 'number', required: true, min: 0, max: 100, step: 1 }]
      : [sinhVienSelect(), hocKySelect(), { name: 'diem', label: 'Điểm (0 - 100)', kind: 'number', required: true, min: 0, max: 100, step: 1 }],
    onSubmit: (payload) => (record ? api.congTacSinhVien.diemRenLuyen.update(record.id, { diem: payload.diem }) : api.congTacSinhVien.diemRenLuyen.create(payload)),
    onDone: () => loadTab(),
  });
}

function khenThuongForm(record) {
  openFormModal({
    title: record ? 'Chỉnh sửa khen thưởng / kỷ luật' : 'Thêm khen thưởng / kỷ luật',
    size: 'lg',
    initial: record ?? {},
    fields: [
      ...(record ? [] : [sinhVienSelect()]),
      { name: 'loai', label: 'Loại', kind: 'select', required: true, options: [{ value: 'KhenThuong', label: 'Khen thưởng' }, { value: 'KyLuat', label: 'Kỷ luật' }] },
      { name: 'soQuyetDinh', label: 'Số quyết định', kind: 'text', required: true },
      { name: 'ngayQuyetDinh', label: 'Ngày quyết định', kind: 'date', required: true },
      { name: 'noiDung', label: 'Nội dung', kind: 'text', required: true },
    ],
    onSubmit: (payload) => (record ? api.congTacSinhVien.khenThuongKyLuat.update(record.id, payload) : api.congTacSinhVien.khenThuongKyLuat.create(payload)),
    onDone: () => loadTab(),
  });
}

/* ----- Bảng và tab ----- */

function filterDefs(tab) {
  return typeof tab.filters === 'function' ? tab.filters() : (tab.filters ?? []);
}

function filtersMarkup(tab) {
  const filters = filterDefs(tab).map((f) => Select.render({ name: f.name, label: f.label, options: f.options, value: state.filters[f.name] ?? '' }));
  return `<div class="filter-bar" role="search">
    ${Input.render({ name: 'q', label: 'Tìm kiếm', value: state.search, placeholder: 'MSSV hoặc họ tên', iconLeft: 'search' })}
    ${filters.join('')}
    ${Button.render({ label: 'Đặt lại', variant: 'ghost', action: 'reset' })}
  </div>`;
}

function tabsMarkup() {
  return `<nav class="tabs" role="tablist" aria-label="Công tác sinh viên">
    ${Object.entries(TABS)
      .map(([key, tab]) => `<button type="button" class="tab" role="tab" data-tab="${key}" aria-selected="${key === state.tab}">${escapeHtml(tab.label)}</button>`)
      .join('')}
  </nav>`;
}

function loadTab() {
  const tab = TABS[state.tab];
  return renderAsync(document.getElementById('list-container'), {
    load: async () => {
      const result = await tab.load({ page: state.page, pageSize: state.pageSize, search: state.search, ...state.filters });
      state.rows = result.data;
      return result;
    },
    skeleton: () => Skeleton.table(5, tab.columns.length),
    isEmpty: (result) => result.total === 0,
    empty: () => EmptyState.render({ icon: 'inbox', title: `Chưa có dữ liệu ${tab.label.toLowerCase()}`, description: 'Thử thay đổi bộ lọc hoặc thêm mới.' }),
    render: (result) =>
      Table.render({
        columns: tab.columns,
        rows: result.data,
        rowKey: tab.rowKey,
        actions: tab.actions,
        actionLabel: (row) => `Thao tác cho ${row.mssv ?? row.id}`,
        caption: tab.title,
        footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
      }),
    errorTitle: `Không tải được ${tab.label.toLowerCase()}`,
  });
}

function switchTab(root, key) {
  state.tab = key;
  state.page = 1;
  state.search = '';
  state.filters = {};
  root.querySelector('#tabs-slot').innerHTML = tabsMarkup();
  root.querySelector('#filters-slot').innerHTML = filtersMarkup(TABS[key]);
  root.querySelector('#card-title').textContent = TABS[key].title;
  const create = root.querySelector('[data-action="create"]');
  create.hidden = !TABS[key].create;
  create.querySelector('.btn-label').textContent = TABS[key].createLabel ?? 'Thêm mới';
  loadTab();
}

function bindEvents(root) {
  const debouncedSearch = debounce(() => {
    state.search = root.querySelector('[name="q"]').value.trim();
    state.page = 1;
    loadTab();
  }, 300);

  Input.init(root, {
    onInput: (input) => {
      if (input.name === 'q') debouncedSearch();
    },
  });

  root.addEventListener('change', (event) => {
    if (event.target.matches('select') && filterDefs(TABS[state.tab]).some((f) => f.name === event.target.name)) {
      state.filters[event.target.name] = event.target.value;
      state.page = 1;
      loadTab();
    }
  });

  root.addEventListener('click', (event) => {
    const button = event.target.closest('[data-tab]');
    if (button && button.dataset.tab !== state.tab) switchTab(root, button.dataset.tab);
  });

  Button.init(root, {
    onAction: (action) => {
      if (action === 'create') TABS[state.tab].create?.();
      if (action === 'reset') {
        state.search = '';
        state.filters = {};
        state.page = 1;
        root.querySelector('#filters-slot').innerHTML = filtersMarkup(TABS[state.tab]);
        loadTab();
      }
    },
  });

  Table.init(root, {
    onAction: (action, rowId) => {
      const tab = TABS[state.tab];
      const row = state.rows.find((item) => String(item[tab.rowKey]) === rowId);
      if (row) tab.onAction?.(action, row);
    },
  });

  Pagination.init(root.querySelector('#list-container'), {
    onChange: ({ page, pageSize }) => {
      state.page = page;
      state.pageSize = pageSize;
      loadTab();
    },
  });
}

/**
 * @param {HTMLElement} root
 */
export async function mount(root) {
  root.innerHTML = `${renderPageHeader({
    title: 'Công tác sinh viên',
    description: 'Học phí, học bổng, miễn giảm, điểm rèn luyện, khen thưởng - kỷ luật và phúc khảo điểm.',
    actions: Button.render({ label: 'Thêm mới', variant: 'primary', icon: 'plus', action: 'create' }),
  })}
  <div id="tabs-slot">${tabsMarkup()}</div>
  <section class="card" aria-labelledby="card-title">
    <header class="card-header"><div class="card-header-text"><h2 class="card-title" id="card-title">${escapeHtml(TABS[state.tab].title)}</h2></div></header>
    <div id="filters-slot">${filtersMarkup(TABS[state.tab])}</div>
    <div class="card-body is-flush" id="list-container"></div>
  </section>`;

  bindEvents(root);
  const hdr = root.querySelector('[data-action="create"]');
  hdr.hidden = !TABS[state.tab].create;
  hdr.querySelector('.btn-label').textContent = TABS[state.tab].createLabel ?? 'Thêm mới';

  const [hocKys, students] = await Promise.all([
    api.hocKy.getAll().catch(() => []),
    api.students.list({ pageSize: 100 }).then((r) => r.data).catch(() => []),
  ]);
  refs = { hocKys, students };
  root.querySelector('#filters-slot').innerHTML = filtersMarkup(TABS[state.tab]);
  await loadTab();
}
