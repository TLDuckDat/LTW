/**
 * Trang Đăng ký học phần (REG-02...REG-07), dữ liệu thật từ backend.
 * - Nhân viên (Admin, PhongDaoTao): chọn sinh viên để đăng ký thay, quản lý đợt đăng ký và sĩ số lớp.
 * - Sinh viên: chỉ thao tác trên hồ sơ của mình (backend kiểm tra theo tài khoản).
 * Điều kiện đăng ký do backend tính; giao diện hiển thị đúng lý do khi lớp chưa đủ điều kiện.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { debounce, escapeHtml } from '../core/dom.js';
import { formatDate } from '../core/format.js';
import { session } from '../core/http.js';
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
import { openPeriodForm, openSiSoForm } from './registration-modals.js';

const THU = { 2: 'Thứ 2', 3: 'Thứ 3', 4: 'Thứ 4', 5: 'Thứ 5', 6: 'Thứ 6', 7: 'Thứ 7', 8: 'Chủ nhật' };

const DOT_STATUS = {
  ChuaMo: { label: 'Chưa mở', variant: 'neutral' },
  DangMo: { label: 'Đang mở', variant: 'success' },
  DaDong: { label: 'Đã đóng', variant: 'destructive' },
};
const LOAI_DANG_KY = {
  HocMoi: { label: 'Học mới', variant: 'primary' },
  HocLai: { label: 'Học lại', variant: 'warning' },
  HocCaiThien: { label: 'Học cải thiện', variant: 'info' },
};
const LOP_STATUS = {
  MoDangKy: { label: 'Mở đăng ký', variant: 'info' },
  DangHoc: { label: 'Đang học', variant: 'success' },
  DaKetThuc: { label: 'Đã kết thúc', variant: 'neutral' },
  DaHuy: { label: 'Đã hủy', variant: 'destructive' },
};

const PERIOD_ACTIONS = [
  { action: 'open', label: 'Mở đợt', icon: 'door-open' },
  { action: 'close', label: 'Đóng đợt', icon: 'door-closed' },
  { action: 'edit', label: 'Chỉnh sửa', icon: 'pencil' },
  { divider: true },
  { action: 'delete', label: 'Xóa', icon: 'trash-2', danger: true },
];

const state = {
  isStaff: false,
  hocKyId: null,
  hocKys: [],
  sinhVienId: null,
  studentOptions: [],
  monHocOptions: [],
  monHocId: '',
  search: '',
  chiDuDieuKien: false,
  periods: [],
  periodPage: 1,
  periodPageSize: 10,
  lopMoById: new Map(),
  siSo: [],
};

/**
 * @param {string} iso
 * @returns {string} dd/MM/yyyy HH:mm theo giờ trình duyệt
 */
function formatDateTime(iso) {
  if (!iso) return '—';
  const date = new Date(iso);
  const pad = (value) => String(value).padStart(2, '0');
  return `${formatDate(iso)} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

/**
 * @param {Array<{ thu: number, tietBatDau: number, soTiet: number, maPhong?: string }>} lich
 * @returns {string}
 */
function formatLich(lich) {
  if (!lich || lich.length === 0) return '—';
  return lich
    .map((item) => `${THU[item.thu] ?? `Thứ ${item.thu}`}, tiết ${item.tietBatDau}-${item.tietBatDau + item.soTiet - 1}${item.maPhong ? ` (${escapeHtml(item.maPhong)})` : ''}`)
    .join('; ');
}

function badge(map, key) {
  const meta = map[key] ?? { label: key ?? '—', variant: 'neutral' };
  return Badge.render({ label: meta.label, variant: meta.variant });
}

function needsStudent(container) {
  container.innerHTML = EmptyState.render({ icon: 'user-round', title: 'Chọn sinh viên', description: 'Chọn một sinh viên ở phía trên để xem và đăng ký học phần.' });
}

/* ----- Đợt đăng ký (nhân viên) ----- */

function loadPeriods() {
  if (!state.isStaff) return Promise.resolve();
  return renderAsync(document.getElementById('periods-container'), {
    load: async () => {
      const result = await api.dotDangKy.list({
        hocKyId: state.hocKyId,
        page: state.periodPage,
        pageSize: state.periodPageSize,
      });
      state.periods = result.data;
      return result;
    },
    skeleton: () => Skeleton.table(3, 4),
    isEmpty: (result) => result.total === 0,
    empty: () => EmptyState.render({ icon: 'calendar-plus', title: 'Chưa có đợt đăng ký', description: 'Tạo đợt để sinh viên được phép đăng ký học phần.', actionText: 'Tạo đợt đăng ký', actionId: 'btn-create-period-empty', action: 'create-period' }),
    render: (result) =>
      Table.render({
        columns: [
          { key: 'ten', label: 'Đợt', primary: true },
          { key: 'thoiGian', label: 'Thời gian', render: (row) => `${formatDateTime(row.ngayBatDau)} → ${formatDateTime(row.ngayKetThuc)}` },
          { key: 'trangThai', label: 'Trạng thái', render: (row) => badge(DOT_STATUS, row.trangThai) },
        ],
        rows: result.data,
        actions: PERIOD_ACTIONS,
        actionLabel: (row) => `Thao tác cho ${row.ten}`,
        caption: 'Danh sách đợt đăng ký',
        footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
      }),
    errorTitle: 'Không tải được đợt đăng ký',
  });
}

/* ----- Lớp mở đăng ký ----- */

function lopMoColumns() {
  return [
    { key: 'ma', label: 'Lớp', primary: true },
    { key: 'monHoc', label: 'Môn học', render: (row) => `${escapeHtml(row.maMonHoc)} - ${escapeHtml(row.tenMonHoc)} (${row.soTinChi} TC)` },
    { key: 'tenGiangVien', label: 'Giảng viên', render: (row) => escapeHtml(row.tenGiangVien ?? '—') },
    { key: 'lichHoc', label: 'Lịch học', render: (row) => formatLich(row.lichHoc) },
    { key: 'siSo', label: 'Sĩ số', align: 'right', render: (row) => `${row.siSoHienTai}/${row.siSoToiDa}` },
    { key: 'loaiDangKy', label: 'Loại', render: (row) => badge(LOAI_DANG_KY, row.loaiDangKy) },
    {
      key: 'dieuKien',
      label: 'Điều kiện',
      render: (row) => {
        if (row.daDangKy) return Badge.render({ label: 'Đã đăng ký', variant: 'success' });
        if (row.duDieuKien) return Badge.render({ label: 'Đủ điều kiện', variant: 'info' });
        return `<span class="text-caption text-muted">${escapeHtml((row.lyDoKhongDuDieuKien ?? []).join(' '))}</span>`;
      },
    },
    {
      key: 'dangKy',
      label: '',
      render: (row) =>
        Button.render({
          label: row.daDangKy ? 'Đã đăng ký' : 'Đăng ký',
          variant: 'primary',
          size: 'sm',
          action: 'register',
          id: `reg-${row.id}`,
          disabled: row.daDangKy || !row.duDieuKien,
        }),
    },
  ];
}

function loadLopMo() {
  const container = document.getElementById('lop-container');
  if (state.sinhVienId === null) return needsStudent(container);

  return renderAsync(container, {
    load: async () => {
      const result = await api.dangKy.lopMo({
        sinhVienId: state.sinhVienId,
        hocKyId: state.hocKyId,
        monHocId: state.monHocId,
        search: state.search,
      });
      state.lopMoById = new Map(result.data.map((row) => [String(row.id), row]));
      return state.chiDuDieuKien ? result.data.filter((row) => row.duDieuKien) : result.data;
    },
    skeleton: () => Skeleton.table(5, 8),
    isEmpty: (rows) => rows.length === 0,
    empty: () => EmptyState.render({ icon: 'search-x', title: 'Không có lớp mở đăng ký', description: 'Thử bỏ bộ lọc hoặc chọn học kỳ khác.' }),
    render: (rows) => Table.render({ columns: lopMoColumns(), rows, caption: 'Lớp học phần mở đăng ký' }),
    errorTitle: 'Không tải được lớp mở đăng ký',
  });
}

/* ----- Học phần đã đăng ký và thời khóa biểu ----- */

function loadDaDangKy() {
  const container = document.getElementById('da-container');
  if (state.sinhVienId === null) return needsStudent(container);

  return renderAsync(container, {
    load: () => api.dangKy.daDangKy(state.sinhVienId, state.hocKyId),
    skeleton: () => Skeleton.table(3, 6),
    isEmpty: (data) => data.hocPhan.length === 0,
    empty: () => EmptyState.render({ icon: 'clipboard-list', title: 'Chưa đăng ký học phần nào', description: 'Chọn lớp ở mục bên trên để đăng ký.' }),
    render: (data) =>
      `<p class="form-hint mb-sm">Tổng số tín chỉ: <strong>${data.tongSoTinChi}</strong></p>${Table.render({
        columns: [
          { key: 'maLop', label: 'Lớp', primary: true },
          { key: 'monHoc', label: 'Môn học', render: (row) => `${escapeHtml(row.maMonHoc)} - ${escapeHtml(row.tenMonHoc)}` },
          { key: 'soTinChi', label: 'TC', align: 'right' },
          { key: 'tenGiangVien', label: 'Giảng viên', render: (row) => escapeHtml(row.tenGiangVien ?? '—') },
          { key: 'loaiDangKy', label: 'Loại', render: (row) => badge(LOAI_DANG_KY, row.loaiDangKy) },
          { key: 'trangThaiLop', label: 'Lớp', render: (row) => badge(LOP_STATUS, row.trangThaiLop) },
          {
            key: 'huy',
            label: '',
            render: (row) =>
              Button.render({
                label: 'Hủy đăng ký',
                variant: 'outline',
                size: 'sm',
                action: 'cancel-reg',
                id: `huy-${row.dangKyId}`,
                disabled: row.trangThaiLop !== 'MoDangKy',
              }),
          },
        ],
        rows: data.hocPhan,
        caption: 'Học phần đã đăng ký',
      })}`,
    errorTitle: 'Không tải được học phần đã đăng ký',
  });
}

function loadTkb() {
  const container = document.getElementById('tkb-container');
  if (state.sinhVienId === null) return needsStudent(container);

  return renderAsync(container, {
    load: () => api.dangKy.thoiKhoaBieu(state.sinhVienId, state.hocKyId),
    skeleton: () => Skeleton.table(3, 5),
    isEmpty: (data) => data.buoi.length === 0,
    empty: () => EmptyState.render({ icon: 'calendar', title: 'Chưa có lịch học', description: 'Thời khóa biểu hiện các lớp đã đăng ký trong học kỳ.' }),
    render: (data) => {
      const rows = [...data.buoi].sort((a, b) => a.thu - b.thu || a.tietBatDau - b.tietBatDau);
      return Table.render({
        columns: [
          { key: 'thu', label: 'Thứ', primary: true, render: (row) => THU[row.thu] ?? `Thứ ${row.thu}` },
          { key: 'tiet', label: 'Tiết', render: (row) => `${row.tietBatDau}-${row.tietBatDau + row.soTiet - 1}` },
          { key: 'maPhong', label: 'Phòng', render: (row) => escapeHtml(row.maPhong ?? '—') },
          { key: 'maLop', label: 'Lớp', render: (row) => escapeHtml(row.maLop) },
          { key: 'tenMonHoc', label: 'Môn học', render: (row) => escapeHtml(row.tenMonHoc) },
          { key: 'tenGiangVien', label: 'Giảng viên', render: (row) => escapeHtml(row.tenGiangVien ?? '—') },
        ],
        rows,
        rowKey: 'lopHocPhanId',
        caption: 'Thời khóa biểu',
      });
    },
    errorTitle: 'Không tải được thời khóa biểu',
  });
}

/* ----- Sĩ số (nhân viên) ----- */

function loadSiSo() {
  if (!state.isStaff) return Promise.resolve();
  return renderAsync(document.getElementById('siso-container'), {
    load: async () => {
      const result = await api.dangKy.siSo({ hocKyId: state.hocKyId, search: state.search });
      state.siSo = result.data;
      return result.data;
    },
    skeleton: () => Skeleton.table(5, 6),
    isEmpty: (rows) => rows.length === 0,
    empty: () => EmptyState.render({ icon: 'users', title: 'Chưa có lớp nào trong học kỳ' }),
    render: (rows) =>
      Table.render({
        columns: [
          { key: 'maLop', label: 'Lớp', primary: true },
          { key: 'monHoc', label: 'Môn học', render: (row) => `${escapeHtml(row.maMonHoc)} - ${escapeHtml(row.tenMonHoc)}` },
          { key: 'trangThaiLop', label: 'Trạng thái', render: (row) => badge(LOP_STATUS, row.trangThaiLop) },
          { key: 'siSo', label: 'Đăng ký / tối đa', align: 'right', render: (row) => `${row.daDangKy}/${row.siSoToiDa}` },
          { key: 'conCho', label: 'Còn chỗ', align: 'right' },
          { key: 'tinhTrang', label: 'Tình trạng', render: (row) => escapeHtml(row.tinhTrang ?? '—') },
        ],
        rows,
        rowKey: 'lopHocPhanId',
        actions: [{ action: 'edit-siso', label: 'Đổi sĩ số tối đa', icon: 'users' }],
        actionLabel: (row) => `Thao tác cho ${row.maLop}`,
        caption: 'Theo dõi sĩ số',
      }),
    errorTitle: 'Không tải được sĩ số lớp',
  });
}

function refreshAll() {
  return Promise.all([loadPeriods(), loadLopMo(), loadDaDangKy(), loadTkb(), loadSiSo()]);
}

/* ----- Hành động ----- */

async function registerClass(button) {
  const lopId = button.id.replace('reg-', '');
  const lop = state.lopMoById.get(lopId);
  const payload = { sinhVienId: state.sinhVienId, lopHocPhanId: Number(lopId) };

  const submit = async (xacNhanHocCaiThien) => {
    await api.dangKy.dangKy({ ...payload, xacNhanHocCaiThien });
    Toast.success(`Đã đăng ký lớp ${lop?.ma ?? ''}`.trim());
    refreshAll();
  };

  if (lop?.loaiDangKy === 'HocCaiThien') {
    Modal.confirm({
      title: 'Đăng ký học cải thiện',
      message: `Môn ${lop.maMonHoc} đã đạt. Bạn chắc chắn muốn đăng ký học cải thiện để có điểm mới?`,
      confirmText: 'Đăng ký',
      onConfirm: () => submit(true),
    }).catch(() => {});
    return;
  }

  Button.setLoading(button, true);
  try {
    await submit(false);
  } catch (error) {
    Button.setLoading(button, false);
    Toast.error(error.message);
  }
}

function cancelRegistration(dangKyId) {
  Modal.confirm({
    title: 'Hủy đăng ký học phần',
    message: 'Bạn có chắc muốn hủy đăng ký lớp này? Lượt đăng ký sẽ bị hủy, không thể hoàn tác.',
    confirmText: 'Hủy đăng ký',
    danger: true,
    onConfirm: async () => {
      try {
        await api.dangKy.huy(dangKyId);
        Toast.success('Đã hủy đăng ký học phần');
        refreshAll();
      } catch (error) {
        Toast.error(error.message);
        throw error;
      }
    },
  }).catch(() => {});
}

function deletePeriod(period) {
  Modal.confirm({
    title: 'Xóa đợt đăng ký',
    message: `Xóa đợt "${period.ten}"? Backend chỉ cho xóa khi đợt chưa mở hoặc đã đóng.`,
    confirmText: 'Xóa',
    danger: true,
    onConfirm: async () => {
      try {
        await api.dotDangKy.remove(period.id);
        Toast.success('Đã xóa đợt đăng ký');
        loadPeriods();
      } catch (error) {
        Toast.error(error.message);
        throw error;
      }
    },
  }).catch(() => {});
}

async function periodAction(action, period) {
  try {
    if (action === 'open') await api.dotDangKy.mo(period.id);
    if (action === 'close') await api.dotDangKy.dong(period.id);
    Toast.success(action === 'open' ? 'Đã mở đợt đăng ký' : 'Đã đóng đợt đăng ký');
    refreshAll();
  } catch (error) {
    Toast.error(error.message);
  }
}

function handleTableAction(action, rowId) {
  if (['open', 'close', 'edit', 'delete'].includes(action)) {
    const period = state.periods.find((item) => String(item.id) === rowId);
    if (!period) return;
    if (action === 'edit') {
      openPeriodForm({ period, hocKys: state.hocKys, hocKyId: state.hocKyId, onSaved: loadPeriods });
    } else if (action === 'delete') {
      deletePeriod(period);
    } else {
      periodAction(action, period);
    }
    return;
  }
  if (action === 'edit-siso') {
    const record = state.siSo.find((item) => String(item.lopHocPhanId) === rowId);
    if (record) openSiSoForm({ record, onSaved: loadSiSo });
  }
}

/* ----- Bố cục và sự kiện ----- */

function filtersMarkup() {
  const hocKyOptions = state.hocKys.map((hk) => ({ value: hk.id, label: `${hk.ten} (${hk.tenNamHoc})` }));
  const monOptions = [{ value: '', label: 'Tất cả môn' }, ...state.monHocOptions];
  return `<div class="filter-bar" role="search">
    ${state.isStaff ? Select.render({ id: 'reg-student', name: 'sinhVienId', label: 'Sinh viên', options: state.studentOptions, placeholder: 'Gõ MSSV hoặc họ tên để tìm', searchable: true, value: '' }) : ''}
    ${Select.render({ id: 'reg-hocky', name: 'hocKyId', label: 'Học kỳ', options: hocKyOptions, value: state.hocKyId })}
    ${Select.render({ id: 'reg-mon', name: 'monHocId', label: 'Môn học', options: monOptions, value: state.monHocId })}
    ${Input.render({ id: 'reg-search', name: 'q', label: 'Tìm lớp', placeholder: 'Mã lớp, giảng viên', iconLeft: 'search', value: state.search })}
    <label class="checkbox-row"><input type="checkbox" name="chiDuDieuKien"${state.chiDuDieuKien ? ' checked' : ''}><span>Chỉ lớp đủ điều kiện</span></label>
  </div>`;
}

function sectionMarkup(id, title, subtitle, body, staffOnly = false) {
  return `<section class="card"${staffOnly ? ' data-staff-only' : ''} aria-labelledby="${id}-title">
    <header class="card-header">
      <div class="card-header-text">
        <h2 class="card-title" id="${id}-title">${escapeHtml(title)}</h2>
        ${subtitle ? `<p class="card-subtitle">${escapeHtml(subtitle)}</p>` : ''}
      </div>
    </header>
    <div class="card-body is-flush" id="${id}-container">${body}</div>
  </section>`;
}

function bindEvents(root) {
  const debouncedSearch = debounce(() => {
    state.search = root.querySelector('[name="q"]').value.trim();
    loadLopMo();
    if (state.isStaff) loadSiSo();
  }, 300);

  Input.init(root, {
    onInput: (input) => {
      if (input.name === 'q') debouncedSearch();
    },
  });

  Select.init(root, {
    onChange: (element) => {
      if (element.name === 'sinhVienId') {
        state.sinhVienId = Number(Select.getValue(element, state.studentOptions)) || null;
        refreshAll();
      }
      if (element.name === 'hocKyId') {
        state.hocKyId = Number(element.value);
        refreshAll();
      }
      if (element.name === 'monHocId') {
        state.monHocId = element.value;
        loadLopMo();
      }
    },
  });

  root.addEventListener('change', (event) => {
    if (event.target.matches('[name="chiDuDieuKien"]')) {
      state.chiDuDieuKien = event.target.checked;
      loadLopMo();
    }
  });

  Button.init(root, {
    onAction: (action, button) => {
      if (action === 'register') registerClass(button);
      if (action === 'cancel-reg') cancelRegistration(Number(button.id.replace('huy-', '')));
      if (action === 'create-period') {
        openPeriodForm({ hocKys: state.hocKys, hocKyId: state.hocKyId, onSaved: loadPeriods });
      }
    },
  });

  Table.init(root, { onAction: (action, rowId) => handleTableAction(action, rowId) });

  Pagination.init(root.querySelector('#periods-container'), {
    onChange: ({ page, pageSize }) => {
      state.periodPage = page;
      state.periodPageSize = pageSize;
      loadPeriods();
    },
  });
}

/**
 * @param {HTMLElement} root
 */
export async function mount(root) {
  state.isStaff = ['Admin', 'PhongDaoTao'].includes(session.get()?.vaiTro);

  root.innerHTML = `${renderPageHeader({
    title: 'Đăng ký học phần',
    description: state.isStaff
      ? 'Chọn sinh viên để đăng ký học phần thay, quản lý đợt đăng ký và sĩ số lớp.'
      : 'Đăng ký các lớp đang mở trong học kỳ và xem thời khóa biểu của bạn.',
  })}
  <div id="filters-slot"></div>
  <div class="page-stack">
    ${state.isStaff ? sectionMarkup('periods', 'Đợt đăng ký', 'Đợt đang mở cho phép sinh viên đăng ký học phần', '', true) : ''}
    ${sectionMarkup('lop', 'Lớp học phần mở đăng ký', 'Điều kiện đăng ký do hệ thống kiểm tra (tiên quyết, sĩ số, trùng lịch, đợt đăng ký)', '')}
    ${sectionMarkup('da', 'Học phần đã đăng ký', 'Trong học kỳ đã chọn', '')}
    ${sectionMarkup('tkb', 'Thời khóa biểu', 'Lịch học của các học phần đã đăng ký', '')}
    ${state.isStaff ? sectionMarkup('siso', 'Theo dõi sĩ số', 'Sĩ số các lớp trong học kỳ', '', true) : ''}
  </div>`;

  if (!state.isStaff) {
    root.querySelectorAll('[data-staff-only]').forEach((node) => node.remove());
  }

  const [hocKys, monHocs, students] = await Promise.all([
    api.hocKy.getAll(),
    api.monHoc.getAll(),
    state.isStaff ? api.students.list({ pageSize: 100 }) : Promise.resolve(null),
  ]);
  state.hocKys = hocKys;
  state.hocKyId = hocKys[0]?.id ?? null;
  state.monHocOptions = monHocs.map((mon) => ({ value: mon.id, label: `${mon.ma} - ${mon.ten}` }));
  state.studentOptions = students
    ? students.data.map((sv) => ({ value: sv.id, label: `${sv.mssv} - ${sv.hoTen}` }))
    : [];

  if (state.isStaff) {
    // Nhân viên chưa chọn sinh viên thì chưa có lớp để đăng ký
    state.sinhVienId = null;
  } else {
    const me = await api.students.me();
    state.sinhVienId = me.id;
  }

  root.querySelector('#filters-slot').outerHTML = `<div id="filters-slot">${filtersMarkup()}</div>`;
  bindEvents(root);
  await refreshAll();
}
