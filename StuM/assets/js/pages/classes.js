/**
 * Trang Lớp học phần: CRUD và các nghiệp vụ của backend (phân giảng viên, lịch học, bắt đầu/kết thúc/hủy lớp).
 * Danh mục học kỳ, môn học, giảng viên, phòng được tải một lần để hiển thị tên thay vì mã.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { debounce, escapeHtml, nextId } from '../core/dom.js';
import { router } from '../core/router.js';
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
import { openAssignModal, openScheduleModal } from './classes-modals.js';

export const CLASS_STATUS_META = {
  MoDangKy: { label: 'Mở đăng ký', variant: 'info' },
  DangHoc: { label: 'Đang học', variant: 'success' },
  DaKetThuc: { label: 'Đã kết thúc', variant: 'neutral' },
  DaHuy: { label: 'Đã hủy', variant: 'destructive' },
};

const ROW_ACTIONS = [
  { action: 'edit', label: 'Chỉnh sửa', icon: 'pencil' },
  { action: 'assign', label: 'Phân giảng viên', icon: 'user-round' },
  { action: 'schedule', label: 'Lịch học', icon: 'calendar' },
  { divider: true },
  { action: 'start', label: 'Bắt đầu học', icon: 'play' },
  { action: 'end', label: 'Kết thúc lớp', icon: 'flag' },
  { action: 'cancel', label: 'Hủy lớp', icon: 'ban', danger: true },
  { divider: true },
  { action: 'delete', label: 'Xóa', icon: 'trash-2', danger: true },
];

const MA_LOP_PATTERN = /^[A-Z]{2,4}\d{4}-\d{2}$/i;
const state = { search: '', hocKyId: '', monHocId: '', trangThai: '', page: 1, pageSize: 10, rows: [] };
let refs = { hocKys: [], monHocs: [], giangViens: [], phongs: [] };
let hocKyLabel = new Map();
let giangVienLabel = new Map();

function statusBadge(status) {
  const meta = CLASS_STATUS_META[status] ?? { label: status, variant: 'neutral' };
  return Badge.render({ label: meta.label, variant: meta.variant });
}

const COLUMNS = [
  { key: 'ma', label: 'Mã lớp', primary: true },
  { key: 'maMonHoc', label: 'Môn học', render: (row) => `${escapeHtml(row.maMonHoc)} - ${escapeHtml(row.tenMonHoc)}` },
  { key: 'hocKyId', label: 'Học kỳ', render: (row) => escapeHtml(hocKyLabel.get(row.hocKyId) ?? '—') },
  {
    key: 'giangVienId',
    label: 'Giảng viên',
    render: (row) => escapeHtml(giangVienLabel.get(row.giangVienId) ?? 'Chưa phân công'),
  },
  {
    key: 'siSoHienTai',
    label: 'Sĩ số',
    align: 'right',
    render: (row) => {
      const full = row.siSoHienTai >= row.siSoToiDa;
      const badge = full ? Badge.render({ label: 'Đã đầy', variant: 'warning' }) : '';
      return `<span class="inline-flex items-center justify-end gap-sm text-nowrap">${row.siSoHienTai}/${row.siSoToiDa}${badge}</span>`;
    },
  },
  { key: 'trangThai', label: 'Trạng thái', render: (row) => statusBadge(row.trangThai) },
];

function syncUrl() {
  router.setQuery({ q: state.search, hocKyId: state.hocKyId, monHocId: state.monHocId, trangThai: state.trangThai });
}

function emptyMarkup() {
  if (state.search || state.hocKyId || state.monHocId || state.trangThai) {
    return EmptyState.render({
      icon: 'search-x',
      title: 'Không tìm thấy lớp học phần phù hợp',
      description: 'Thử thay đổi từ khóa hoặc bộ lọc.',
      actionText: 'Xóa bộ lọc',
      actionId: 'btn-clear-filters',
      action: 'reset-filters',
    });
  }
  return EmptyState.render({
    icon: 'school',
    title: 'Chưa có lớp học phần nào',
    description: 'Mở lớp đầu tiên cho một học kỳ',
    actionText: 'Thêm lớp học phần',
    actionId: 'btn-add-class-empty',
    action: 'add',
  });
}

function loadTable() {
  return renderAsync(document.getElementById('table-container'), {
    load: async () => {
      const result = await api.lopHocPhan.list({
        page: state.page,
        pageSize: state.pageSize,
        search: state.search,
        hocKyId: state.hocKyId,
        monHocId: state.monHocId,
        trangThai: state.trangThai,
      });
      state.rows = result.data;
      return result;
    },
    skeleton: () => Skeleton.table(5, 7),
    isEmpty: (result) => result.total === 0,
    empty: emptyMarkup,
    render: (result) =>
      Table.render({
        columns: COLUMNS,
        rows: result.data,
        actions: ROW_ACTIONS,
        actionLabel: (row) => `Thao tác cho ${row.ma}`,
        caption: 'Danh sách lớp học phần',
        footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
      }),
    errorTitle: 'Không tải được danh sách lớp học phần',
  });
}

/**
 * @param {HTMLInputElement|HTMLSelectElement} input
 * @returns {boolean}
 */
function validateClassInput(input) {
  const value = input.value.trim();
  let message = '';
  if (input.name === 'ma') {
    if (!value) message = 'Vui lòng nhập mã lớp';
    else if (!MA_LOP_PATTERN.test(value)) message = 'Mã lớp dạng mã môn kèm số nhóm, ví dụ INT1001-01';
  } else if (input.name === 'siSoToiDa' && Number(value) < 1) {
    message = 'Sĩ số tối đa phải từ 1 trở lên';
  }

  if (message) {
    Input.setError(input, message);
    return false;
  }
  Input.clearError(input);
  return true;
}

/**
 * @param {{ record?: object|null }} options
 */
function openClassForm({ record = null }) {
  const formId = nextId('class-form');
  const isEdit = Boolean(record);
  const hocKyOptions = refs.hocKys.map((hk) => ({ value: hk.id, label: `${hk.ten} (${hk.tenNamHoc})` }));
  const monOptions = refs.monHocs.map((mon) => ({ value: mon.id, label: `${mon.ma} - ${mon.ten}` }));

  const handle = Modal.open({
    title: isEdit ? 'Chỉnh sửa lớp học phần' : 'Thêm lớp học phần',
    size: 'lg',
    content: `<form class="modal-form" id="${formId}" novalidate>
      <div class="form-grid">
        ${Input.render({ name: 'ma', label: 'Mã lớp', value: record?.ma ?? '', placeholder: 'INT1001-01', required: true, hint: 'Mã môn kèm số nhóm' })}
        ${Input.render({ name: 'siSoToiDa', type: 'number', label: 'Sĩ số tối đa', value: record?.siSoToiDa ?? 50, min: 1, step: 1, required: true })}
        ${isEdit ? '' : Select.render({ name: 'hocKyId', label: 'Học kỳ', options: hocKyOptions, value: '', placeholder: 'Chọn học kỳ', required: true })}
        ${isEdit ? '' : Select.render({ name: 'monHocId', label: 'Môn học', options: monOptions, value: '', placeholder: 'Chọn môn học', required: true })}
      </div>
      ${isEdit ? '<p class="form-hint">Học kỳ và môn học của lớp không đổi sau khi tạo.</p>' : ''}
    </form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: isEdit ? 'Lưu thay đổi' : 'Thêm lớp', variant: 'primary', action: 'save' }),
    ].join(''),
  });

  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);
  Input.init(form, {
    onBlur: (input) => validateClassInput(input),
    onInput: (input) => input.getAttribute('aria-invalid') === 'true' && validateClassInput(input),
  });

  saveButton.addEventListener('click', () => {
    const inputs = Array.from(form.querySelectorAll('input[name]'));
    const results = inputs.map(validateClassInput);
    const hocKySelect = form.elements.namedItem('hocKyId');
    const monSelect = form.elements.namedItem('monHocId');
    if (!isEdit && hocKySelect.value === '') Input.setError(hocKySelect, 'Vui lòng chọn học kỳ');
    if (!isEdit && monSelect.value === '') Input.setError(monSelect, 'Vui lòng chọn môn học');
    if (results.includes(false) || (!isEdit && (hocKySelect.value === '' || monSelect.value === ''))) return;

    const field = (name) => form.elements.namedItem(name).value.trim();
    const payload = isEdit
      ? { ma: field('ma').toUpperCase(), siSoToiDa: Number(field('siSoToiDa')) }
      : {
          ma: field('ma').toUpperCase(),
          hocKyId: Number(hocKySelect.value),
          monHocId: Number(monSelect.value),
          siSoToiDa: Number(field('siSoToiDa')),
        };

    Button.setLoading(saveButton, true);
    const request = isEdit ? api.lopHocPhan.update(record.id, payload) : api.lopHocPhan.create(payload);
    request
      .then(() => {
        Toast.success(isEdit ? 'Cập nhật lớp học phần thành công' : 'Thêm lớp học phần thành công');
        handle.close();
        loadTable();
      })
      .catch((error) => {
        Button.setLoading(saveButton, false);
        Toast.error(error.message);
      });
  });
}

/**
 * Gọi một thao tác nghiệp vụ (bắt đầu, kết thúc, hủy lớp) rồi tải lại bảng.
 * @param {() => Promise<unknown>} run
 * @param {string} successMessage
 */
async function runAction(run, successMessage) {
  try {
    await run();
    Toast.success(successMessage);
    loadTable();
  } catch (error) {
    Toast.error(error.message);
  }
}

function confirmAction({ title, message, confirmText, danger, run, successMessage }) {
  Modal.confirm({
    title,
    message,
    confirmText,
    danger,
    onConfirm: () => runAction(run, successMessage),
  }).catch(() => {});
}

function confirmDelete(record) {
  confirmAction({
    title: 'Xóa lớp học phần',
    message: `Bạn có chắc muốn xóa lớp ${record.ma}? Backend sẽ từ chối nếu lớp đã có sinh viên đăng ký.`,
    confirmText: 'Xóa',
    danger: true,
    run: () => api.lopHocPhan.remove(record.id),
    successMessage: `Đã xóa lớp ${record.ma}`,
  });
}

function handleRowAction(action, rowId) {
  const record = state.rows.find((row) => String(row.id) === String(rowId));
  if (!record) return;

  if (action === 'edit') openClassForm({ record });
  if (action === 'assign') openAssignModal({ record, giangViens: refs.giangViens, onDone: loadTable });
  if (action === 'schedule') openScheduleModal({ record, phongs: refs.phongs, onDone: loadTable });
  if (action === 'start') runAction(() => api.lopHocPhan.batDauHoc(record.id), `Lớp ${record.ma} đã bắt đầu học`);
  if (action === 'end') runAction(() => api.lopHocPhan.ketThuc(record.id), `Lớp ${record.ma} đã kết thúc`);
  if (action === 'cancel') {
    confirmAction({
      title: 'Hủy lớp học phần',
      message: `Hủy lớp ${record.ma}? Các đăng ký của sinh viên trong lớp sẽ bị ảnh hưởng.`,
      confirmText: 'Hủy lớp',
      danger: true,
      run: () => api.lopHocPhan.huy(record.id),
      successMessage: `Đã hủy lớp ${record.ma}`,
    });
  }
  if (action === 'delete') confirmDelete(record);
}

function resetFilters(root) {
  Object.assign(state, { search: '', hocKyId: '', monHocId: '', trangThai: '', page: 1 });
  root.querySelector('[name="q"]').value = '';
  ['hocKyId', 'monHocId', 'trangThai'].forEach((name) => {
    root.querySelector(`[name="${name}"]`).value = '';
  });
  syncUrl();
  loadTable();
}

function filtersMarkup() {
  const hocKyOptions = [{ value: '', label: 'Tất cả học kỳ' }, ...refs.hocKys.map((hk) => ({ value: hk.id, label: `${hk.ten} (${hk.tenNamHoc})` }))];
  const monOptions = [{ value: '', label: 'Tất cả môn' }, ...refs.monHocs.map((mon) => ({ value: mon.id, label: `${mon.ma} - ${mon.ten}` }))];
  const statusOptions = [{ value: '', label: 'Tất cả trạng thái' }, ...Object.entries(CLASS_STATUS_META).map(([value, meta]) => ({ value, label: meta.label }))];
  return `<div class="filter-bar" role="search">
    ${Input.render({ id: 'filter-search', name: 'q', label: 'Tìm kiếm', value: state.search, placeholder: 'Mã lớp, phòng', iconLeft: 'search' })}
    ${Select.render({ id: 'filter-hocky', name: 'hocKyId', label: 'Học kỳ', options: hocKyOptions, value: state.hocKyId })}
    ${Select.render({ id: 'filter-mon', name: 'monHocId', label: 'Môn học', options: monOptions, value: state.monHocId })}
    ${Select.render({ id: 'filter-status', name: 'trangThai', label: 'Trạng thái', options: statusOptions, value: state.trangThai })}
    ${Button.render({ label: 'Đặt lại', variant: 'ghost', action: 'reset-filters' })}
  </div>`;
}

function bindEvents(root) {
  const debouncedSearch = debounce(() => {
    state.search = root.querySelector('[name="q"]').value.trim();
    state.page = 1;
    syncUrl();
    loadTable();
  }, 300);

  Input.init(root, {
    onInput: (input) => {
      if (input.name === 'q') debouncedSearch();
    },
  });

  Select.init(root, {
    onChange: (element) => {
      if (!['hocKyId', 'monHocId', 'trangThai'].includes(element.name)) return;
      state[element.name] = element.value;
      state.page = 1;
      syncUrl();
      loadTable();
    },
  });

  Button.init(root, {
    onAction: (action) => {
      if (action === 'add') openClassForm({});
      if (action === 'reset-filters') resetFilters(root);
    },
  });

  Table.init(root, { onAction: (action, rowId) => handleRowAction(action, rowId) });

  Pagination.init(root.querySelector('#table-container'), {
    onChange: ({ page, pageSize }) => {
      state.page = page;
      state.pageSize = pageSize;
      loadTable();
    },
  });
}

/**
 * @param {HTMLElement} root
 * @param {{ query: Record<string, string> }} context
 */
export async function mount(root, { query }) {
  Object.assign(state, {
    search: query.q ?? '',
    hocKyId: query.hocKyId ?? '',
    monHocId: query.monHocId ?? '',
    trangThai: query.trangThai ?? '',
  });

  root.innerHTML = `${renderPageHeader({
    title: 'Lớp học phần',
    description: 'Mở lớp, phân giảng viên, xếp lịch học và quản lý sĩ số',
    actions: Button.render({ label: 'Thêm lớp học phần', variant: 'primary', icon: 'plus', id: 'btn-add-class', action: 'add' }),
  })}
  <section class="card" aria-label="Danh sách lớp học phần">
    <div id="filters-slot"></div>
    <div class="card-body is-flush" id="table-container"></div>
  </section>`;

  bindEvents(root);
  const [hocKys, monHocs, giangViens, phongs] = await Promise.all([
    api.hocKy.getAll(),
    api.monHoc.getAll(),
    api.giangVien.getAll(),
    api.phongHoc.getAll(),
  ]).catch((error) => {
    Toast.error(error.message);
    return [[], [], [], []];
  });

  refs = { hocKys, monHocs, giangViens, phongs };
  hocKyLabel = new Map(hocKys.map((hk) => [hk.id, `${hk.ten} (${hk.tenNamHoc})`]));
  giangVienLabel = new Map(giangViens.map((gv) => [gv.id, `${gv.ma} - ${gv.hoTen}`]));
  root.querySelector('#filters-slot').outerHTML = filtersMarkup();
  return loadTable();
}
