/**
 * Trang Sinh viên: dữ liệu thật từ backend (GET/POST/PUT/DELETE /api/sinh-vien).
 * Lọc, tìm kiếm và phân trang do server xử lý. Các thao tác trạng thái gọi endpoint nghiệp vụ của backend.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { debounce } from '../core/dom.js';
import { router } from '../core/router.js';
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
import { openStatusForm, openStudentDetail, openStudentForm, statusBadge, STATUS_META } from './students-form.js';
import { openTranscript } from './class-roster.js';

const COLUMNS = [
  { key: 'mssv', label: 'MSSV' },
  { key: 'hoTen', label: 'Họ và tên', primary: true },
  { key: 'email', label: 'Email', render: (row) => row.email ?? '—' },
  { key: 'tenNganh', label: 'Ngành', render: (row) => row.tenNganh ?? '—' },
  { key: 'tenLopHanhChinh', label: 'Lớp', render: (row) => row.tenLopHanhChinh ?? '—' },
  { key: 'trangThai', label: 'Trạng thái', render: (row) => statusBadge(row.trangThai) },
];

const ROW_ACTIONS = [
  { action: 'view', label: 'Xem chi tiết', icon: 'eye' },
  { action: 'edit', label: 'Chỉnh sửa', icon: 'pencil' },
  { action: 'transcript', label: 'Bảng điểm', icon: 'file-text' },
  { divider: true },
  { action: 'bao-luu', label: 'Bảo lưu', icon: 'pause' },
  { action: 'thoi-hoc', label: 'Thôi học', icon: 'user-x' },
  { action: 'quay-lai-hoc', label: 'Quay lại học', icon: 'undo-2' },
  { divider: true },
  { action: 'delete', label: 'Xóa', icon: 'trash-2', danger: true },
];

const STATUS_OPTIONS = [
  { value: '', label: 'Tất cả trạng thái' },
  ...Object.entries(STATUS_META).map(([value, meta]) => ({ value, label: meta.label })),
];

const state = { search: '', khoaId: '', trangThai: '', page: 1, pageSize: 10, rows: [] };
let khoaOptions = [];
let formLists = null;

function hasActiveFilters() {
  return Boolean(state.search || state.khoaId || state.trangThai);
}

function syncUrl() {
  router.setQuery({ q: state.search, khoaId: state.khoaId, trangThai: state.trangThai });
}

/** Ngành và lớp hành chính cho form, tải một lần khi mở form lần đầu. */
async function loadFormLists() {
  if (!formLists) {
    const [nganhs, lops] = await Promise.all([api.nganh.getAll(), api.lopHanhChinh.getAll()]);
    formLists = { nganhs, lops };
  }
  return formLists;
}

function emptyMarkup() {
  if (hasActiveFilters()) {
    return EmptyState.render({
      icon: 'search-x',
      title: 'Không tìm thấy sinh viên phù hợp',
      description: 'Thử thay đổi từ khóa hoặc bộ lọc.',
      actionText: 'Xóa bộ lọc',
      actionId: 'btn-clear-filters',
      action: 'reset-filters',
    });
  }
  return EmptyState.render({
    icon: 'users',
    title: 'Chưa có sinh viên nào',
    description: 'Bắt đầu bằng cách thêm sinh viên đầu tiên',
    actionText: 'Thêm sinh viên',
    actionId: 'btn-add-student-empty',
    action: 'add',
  });
}

function loadTable() {
  return renderAsync(document.getElementById('table-container'), {
    load: async () => {
      const result = await api.students.list({
        page: state.page,
        pageSize: state.pageSize,
        search: state.search,
        khoaId: state.khoaId,
        trangThai: state.trangThai,
      });
      state.rows = result.data;
      return result;
    },
    skeleton: () => Skeleton.table(5, 6),
    isEmpty: (result) => result.total === 0,
    empty: emptyMarkup,
    render: (result) =>
      Table.render({
        columns: COLUMNS,
        rows: result.data,
        actions: ROW_ACTIONS,
        actionLabel: (row) => `Thao tác cho ${row.mssv}`,
        caption: 'Danh sách sinh viên',
        footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
      }),
    errorTitle: 'Không tải được danh sách sinh viên',
  });
}

function confirmDelete(record) {
  Modal.confirm({
    title: 'Xóa sinh viên',
    message: `Bạn có chắc muốn xóa sinh viên ${record.hoTen} (${record.mssv})? Hành động này không thể hoàn tác.`,
    confirmText: 'Xóa',
    danger: true,
    onConfirm: async () => {
      try {
        await api.students.remove(record.id);
        Toast.success(`Đã xóa sinh viên ${record.mssv}`);
        loadTable();
      } catch (error) {
        Toast.error(error.message);
        throw error;
      }
    },
  }).catch(() => {});
}

async function openForm(record = null) {
  try {
    const lists = await loadFormLists();
    openStudentForm({ record, lists, onSaved: loadTable });
  } catch (error) {
    Toast.error(error.message);
  }
}

function handleRowAction(action, rowId) {
  const record = state.rows.find((row) => String(row.id) === String(rowId));
  if (!record) return;
  if (action === 'view') openStudentDetail(record.id);
  if (action === 'transcript') openTranscript(record.id);
  if (action === 'edit') openForm(record);
  if (action === 'delete') confirmDelete(record);
  if (['bao-luu', 'thoi-hoc', 'quay-lai-hoc'].includes(action)) {
    openStatusForm({ record, action, onDone: loadTable });
  }
}

function resetFilters(root) {
  state.search = '';
  state.khoaId = '';
  state.trangThai = '';
  state.page = 1;
  root.querySelector('[name="q"]').value = '';
  root.querySelector('[name="khoaId"]').value = '';
  root.querySelector('[name="trangThai"]').value = '';
  syncUrl();
  loadTable();
}

function filtersMarkup() {
  const khoa = [{ value: '', label: 'Tất cả khoa' }, ...khoaOptions];
  return `<div class="filter-bar" role="search">
    ${Input.render({ id: 'filter-search', name: 'q', label: 'Tìm kiếm', value: state.search, placeholder: 'MSSV, họ tên, email', iconLeft: 'search' })}
    ${Select.render({ id: 'filter-khoa', name: 'khoaId', label: 'Khoa', options: khoa, value: state.khoaId })}
    ${Select.render({ id: 'filter-status', name: 'trangThai', label: 'Trạng thái', options: STATUS_OPTIONS, value: state.trangThai })}
    ${Button.render({ label: 'Đặt lại', variant: 'ghost', action: 'reset-filters' })}
  </div>`;
}

/**
 * @param {HTMLElement} root
 */
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
      if (!['khoaId', 'trangThai'].includes(element.name)) return;
      state[element.name] = element.value;
      state.page = 1;
      syncUrl();
      loadTable();
    },
  });

  Button.init(root, {
    onAction: (action) => {
      if (action === 'add') openForm();
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
  state.search = query.q ?? '';
  state.khoaId = query.khoaId ?? '';
  state.trangThai = query.trangThai ?? '';
  formLists = null;

  const khoaReady = api.khoa.getAll().catch(() => []);
  root.innerHTML = `${renderPageHeader({
    title: 'Sinh viên',
    description: 'Hồ sơ sinh viên toàn trường, lấy trực tiếp từ cơ sở dữ liệu',
    actions: Button.render({ label: 'Thêm sinh viên', variant: 'primary', icon: 'plus', id: 'btn-add-student', action: 'add' }),
  })}
  <section class="card" aria-label="Danh sách sinh viên">
    <div id="filters-slot"></div>
    <div class="card-body is-flush" id="table-container"></div>
  </section>`;

  bindEvents(root);
  const [khoas] = await Promise.all([khoaReady, loadTable()]);
  khoaOptions = khoas.map((item) => ({ value: item.id, label: `${item.ma} - ${item.ten}` }));
  root.querySelector('#filters-slot').outerHTML = filtersMarkup();
}
