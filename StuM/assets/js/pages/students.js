/**
 * Trang Sinh viên: CRUD mẫu (template cho các trang danh mục khác).
 * Gồm: page header, filter bar (search debounce 300ms, khoa, trạng thái),
 * bảng sortable có phân trang, action menu, form thêm/sửa, xác nhận xóa, empty/skeleton/error.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { debounce } from '../core/dom.js';
import { formatNumber } from '../core/format.js';
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
import { KHOA_LIST, openStudentDetail, openStudentForm, statusBadge, STATUS_META } from './students-form.js';

const KHOA_OPTIONS = [{ value: '', label: 'Tất cả khoa' }, ...KHOA_LIST.map((item) => ({ value: item, label: item }))];
const STATUS_OPTIONS = [
  { value: '', label: 'Tất cả trạng thái' },
  ...Object.entries(STATUS_META).map(([value, meta]) => ({ value, label: meta.label })),
];

const COLUMNS = [
  { key: 'mssv', label: 'MSSV', sortable: true },
  { key: 'hoTen', label: 'Họ và tên', sortable: true, primary: true },
  { key: 'email', label: 'Email' },
  { key: 'khoa', label: 'Khoa' },
  { key: 'lop', label: 'Lớp' },
  { key: 'gpa', label: 'GPA', sortable: true, align: 'right', render: (row) => formatNumber(row.gpa, 2) },
  { key: 'trangThai', label: 'Trạng thái', render: (row) => statusBadge(row.trangThai) },
];

const ROW_ACTIONS = [
  { action: 'view', label: 'Xem chi tiết', icon: 'eye' },
  { action: 'edit', label: 'Chỉnh sửa', icon: 'pencil' },
  { divider: true },
  { action: 'delete', label: 'Xóa', icon: 'trash-2', danger: true },
];

const state = {
  search: '',
  khoa: '',
  trangThai: '',
  page: 1,
  pageSize: 10,
  sort: { key: 'mssv', dir: 'asc' },
  rows: [],
};

function hasActiveFilters() {
  return Boolean(state.search || state.khoa || state.trangThai);
}

function syncUrl() {
  router.setQuery({ q: state.search, khoa: state.khoa, trangThai: state.trangThai });
}

/**
 * @param {import('../core/api.js').ApiError|Error} error
 */
function reportError(error) {
  Toast.error(error.message);
}

function renderTable(result) {
  return Table.render({
    columns: COLUMNS,
    rows: result.data,
    sort: state.sort,
    actions: ROW_ACTIONS,
    actionLabel: (row) => `Thao tác cho ${row.mssv}`,
    caption: 'Danh sách sinh viên',
    footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
  });
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
  const container = document.getElementById('table-container');
  return renderAsync(container, {
    load: async () => {
      const result = await api.students.list({
        search: state.search,
        filters: { khoa: state.khoa, trangThai: state.trangThai },
        sort: state.sort,
        page: state.page,
        pageSize: state.pageSize,
      });
      state.rows = result.data;
      return result;
    },
    skeleton: () => Skeleton.table(5, 7),
    isEmpty: (result) => result.total === 0,
    empty: emptyMarkup,
    render: renderTable,
    errorTitle: 'Không tải được danh sách sinh viên',
  });
}

function confirmDelete(id) {
  const record = state.rows.find((row) => row.id === id);
  if (!record) return;

  Modal.confirm({
    title: 'Xóa sinh viên',
    message: `Bạn có chắc muốn xóa sinh viên ${record.hoTen} (${record.mssv})? Hành động này không thể hoàn tác.`,
    confirmText: 'Xóa',
    danger: true,
    onConfirm: async () => {
      try {
        await api.students.remove(id);
        Toast.success(`Đã xóa sinh viên ${record.mssv}`);
        loadTable();
      } catch (error) {
        reportError(error);
        throw error;
      }
    },
  }).catch(() => {});
}

function handleRowAction(action, rowId) {
  const record = state.rows.find((row) => row.id === rowId);
  if (action === 'delete') confirmDelete(rowId);
  if (!record) return;
  if (action === 'view') openStudentDetail(record);
  if (action === 'edit') openStudentForm({ record, onSaved: loadTable });
}

function resetFilters(root) {
  state.search = '';
  state.khoa = '';
  state.trangThai = '';
  state.page = 1;
  root.querySelector('[name="q"]').value = '';
  root.querySelector('[name="khoa"]').value = '';
  root.querySelector('[name="trangThai"]').value = '';
  syncUrl();
  loadTable();
}

function filtersMarkup() {
  return `<div class="filter-bar" role="search">
    ${Input.render({ id: 'filter-search', name: 'q', label: 'Tìm kiếm', value: state.search, placeholder: 'MSSV, họ tên, email, lớp', iconLeft: 'search' })}
    ${Select.render({ id: 'filter-khoa', name: 'khoa', label: 'Khoa', options: KHOA_OPTIONS, value: state.khoa })}
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
      if (element.name === 'khoa') state.khoa = element.value;
      if (element.name === 'trangThai') state.trangThai = element.value;
      if (!['khoa', 'trangThai'].includes(element.name)) return;
      state.page = 1;
      syncUrl();
      loadTable();
    },
  });

  Button.init(root, {
    onAction: (action) => {
      if (action === 'add') openStudentForm({ onSaved: loadTable });
      if (action === 'reset-filters') resetFilters(root);
    },
  });

  const tableContainer = root.querySelector('#table-container');
  Table.init(root, {
    onSort: (key) => {
      const dir = state.sort.key === key && state.sort.dir === 'asc' ? 'desc' : 'asc';
      state.sort = { key, dir };
      state.page = 1;
      loadTable();
    },
    onAction: (action, rowId) => handleRowAction(action, rowId),
  });

  Pagination.init(tableContainer, {
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
export function mount(root, { query }) {
  state.search = query.q ?? '';
  state.khoa = query.khoa ?? '';
  state.trangThai = query.trangThai ?? '';

  root.innerHTML = `${renderPageHeader({
    title: 'Sinh viên',
    description: 'Quản lý hồ sơ sinh viên toàn trường',
    actions: Button.render({ label: 'Thêm sinh viên', variant: 'primary', icon: 'plus', id: 'btn-add-student', action: 'add' }),
  })}
  <section class="card" aria-label="Danh sách sinh viên">
    ${filtersMarkup()}
    <div class="card-body is-flush" id="table-container"></div>
  </section>`;

  bindEvents(root);
  return loadTable();
}
