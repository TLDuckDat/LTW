/**
 * Trang Lớp học phần: CRUD theo pattern chuẩn.
 * Đặc thù: chọn môn từ danh sách môn học, hiển thị sĩ số dạng "45/50", kiểm tra sĩ số hợp lệ.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { debounce, nextId } from '../core/dom.js';
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

const HOC_KY_LIST = ['HK1-2026', 'HK2-2025'];
const MA_LOP_PATTERN = /^[A-Z]{2,4}\d{4}-\d{2}$/i;

const COLUMNS = [
  { key: 'maLop', label: 'Mã lớp', sortable: true, primary: true },
  { key: 'maMon', label: 'Mã môn', sortable: true },
  { key: 'giangVien', label: 'Giảng viên' },
  { key: 'hocKy', label: 'Học kỳ' },
  {
    key: 'siSo',
    label: 'Sĩ số',
    align: 'right',
    render: (row) => {
      const full = row.siSo >= row.siSoToiDa;
      const badge = full ? Badge.render({ label: 'Đã đầy', variant: 'warning' }) : '';
      return `<span class="inline-flex items-center justify-end gap-sm text-nowrap">${row.siSo}/${row.siSoToiDa}${badge}</span>`;
    },
  },
  { key: 'lichHoc', label: 'Lịch học' },
  { key: 'phong', label: 'Phòng' },
];

const ROW_ACTIONS = [
  { action: 'edit', label: 'Chỉnh sửa', icon: 'pencil' },
  { divider: true },
  { action: 'delete', label: 'Xóa', icon: 'trash-2', danger: true },
];

const state = { search: '', hocKy: '', maMon: '', page: 1, pageSize: 10, sort: { key: 'maLop', dir: 'asc' }, rows: [] };
let courseOptions = [];

function hasActiveFilters() {
  return Boolean(state.search || state.hocKy || state.maMon);
}

function syncUrl() {
  router.setQuery({ q: state.search, hocKy: state.hocKy, maMon: state.maMon });
}

function emptyMarkup() {
  if (hasActiveFilters()) {
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
      const result = await api.classes.list({
        search: state.search,
        filters: { hocKy: state.hocKy, maMon: state.maMon },
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
    render: (result) =>
      Table.render({
        columns: COLUMNS,
        rows: result.data,
        sort: state.sort,
        actions: ROW_ACTIONS,
        actionLabel: (row) => `Thao tác cho ${row.maLop}`,
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
  if (input.name === 'maLop') {
    if (!value) message = 'Vui lòng nhập mã lớp';
    else if (!MA_LOP_PATTERN.test(value)) message = 'Mã lớp dạng mã môn kèm số nhóm, ví dụ INT1001-01';
  } else if (['giangVien', 'lichHoc', 'phong'].includes(input.name) && !value) {
    message = 'Trường này là bắt buộc';
  } else if (input.name === 'siSoToiDa' && Number(value) < 1) {
    message = 'Sĩ số tối đa phải từ 1 trở lên';
  } else if (input.name === 'siSo' && (value === '' || Number(value) < 0)) {
    message = 'Sĩ số không được âm';
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
  const courseSelectOptions = courseOptions.map((course) => ({
    value: course.maMon,
    label: `${course.maMon} - ${course.tenMon}`,
  }));
  const handle = Modal.open({
    title: isEdit ? 'Chỉnh sửa lớp học phần' : 'Thêm lớp học phần',
    size: 'lg',
    content: `<form class="modal-form" id="${formId}" novalidate>
      <div class="form-grid">
        ${Input.render({ name: 'maLop', label: 'Mã lớp', value: record?.maLop ?? '', placeholder: 'INT1001-01', required: true, hint: 'Mã môn kèm số nhóm' })}
        ${Select.render({ name: 'maMon', label: 'Môn học', value: record?.maMon ?? courseSelectOptions[0]?.value ?? '', options: courseSelectOptions, required: true })}
        ${Input.render({ name: 'giangVien', label: 'Giảng viên', value: record?.giangVien ?? '', placeholder: 'ThS. Nguyễn Văn B', required: true })}
        ${Select.render({ name: 'hocKy', label: 'Học kỳ', value: record?.hocKy ?? HOC_KY_LIST[0], options: HOC_KY_LIST.map((item) => ({ value: item, label: item })) })}
        ${Input.render({ name: 'siSo', type: 'number', label: 'Sĩ số hiện tại', value: record?.siSo ?? 0, min: 0, step: 1, required: true })}
        ${Input.render({ name: 'siSoToiDa', type: 'number', label: 'Sĩ số tối đa', value: record?.siSoToiDa ?? 50, min: 1, step: 1, required: true })}
        ${Input.render({ name: 'lichHoc', label: 'Lịch học', value: record?.lichHoc ?? '', placeholder: 'Thứ 2, 7:00-9:30', required: true })}
        ${Input.render({ name: 'phong', label: 'Phòng', value: record?.phong ?? '', placeholder: 'A101', required: true })}
      </div>
    </form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: isEdit ? 'Lưu thay đổi' : 'Thêm lớp', variant: 'primary', action: 'save' }),
    ].join(''),
  });

  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);

  const validateAll = () => {
    const inputs = Array.from(form.querySelectorAll('input[name]'));
    const results = inputs.map(validateClassInput);
    const siSo = Number(form.elements.namedItem('siSo').value);
    const max = Number(form.elements.namedItem('siSoToiDa').value);
    if (siSo > max) {
      Input.setError(form.elements.namedItem('siSo'), 'Sĩ số không được vượt quá sĩ số tối đa');
      results.push(false);
    }
    return !results.includes(false);
  };

  Input.init(form, {
    onBlur: (input) => validateClassInput(input),
    onInput: (input) => input.getAttribute('aria-invalid') === 'true' && validateClassInput(input),
  });

  saveButton.addEventListener('click', () => {
    if (!validateAll()) return;

    const field = (name) => form.elements.namedItem(name).value.trim();
    const payload = {
      maLop: field('maLop').toUpperCase(),
      maMon: field('maMon'),
      giangVien: field('giangVien'),
      hocKy: field('hocKy'),
      siSo: Number(field('siSo')),
      siSoToiDa: Number(field('siSoToiDa')),
      lichHoc: field('lichHoc'),
      phong: field('phong'),
    };

    Button.setLoading(saveButton, true);
    const request = isEdit ? api.classes.update(record.id, payload) : api.classes.create(payload);
    request
      .then(() => {
        Toast.success(isEdit ? 'Cập nhật lớp học phần thành công' : 'Thêm lớp học phần thành công');
        handle.close();
        loadTable();
      })
      .catch((error) => {
        Button.setLoading(saveButton, false);
        if (error.status === 409) {
          Input.setError(form.elements.namedItem('maLop'), error.message);
        } else {
          Toast.error(error.message);
        }
      });
  });
}

function confirmDelete(id) {
  const record = state.rows.find((row) => row.id === id);
  if (!record) return;

  Modal.confirm({
    title: 'Xóa lớp học phần',
    message: `Bạn có chắc muốn xóa lớp ${record.maLop}? Hành động này không thể hoàn tác.`,
    confirmText: 'Xóa',
    danger: true,
    onConfirm: async () => {
      try {
        await api.classes.remove(id);
        Toast.success(`Đã xóa lớp ${record.maLop}`);
        loadTable();
      } catch (error) {
        Toast.error(error.message);
        throw error;
      }
    },
  }).catch(() => {});
}

function resetFilters(root) {
  state.search = '';
  state.hocKy = '';
  state.maMon = '';
  state.page = 1;
  root.querySelector('[name="q"]').value = '';
  root.querySelector('[name="hocKy"]').value = '';
  root.querySelector('[name="maMon"]').value = '';
  syncUrl();
  loadTable();
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
      if (!['hocKy', 'maMon'].includes(element.name)) return;
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

  Table.init(root, {
    onSort: (key) => {
      const dir = state.sort.key === key && state.sort.dir === 'asc' ? 'desc' : 'asc';
      state.sort = { key, dir };
      state.page = 1;
      loadTable();
    },
    onAction: (action, rowId) => {
      const record = state.rows.find((row) => row.id === rowId);
      if (action === 'edit' && record) openClassForm({ record });
      if (action === 'delete') confirmDelete(rowId);
    },
  });

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
  state.hocKy = query.hocKy ?? '';
  state.maMon = query.maMon ?? '';

  const coursesReady = api.courses.getAll().catch(() => []);

  root.innerHTML = `${renderPageHeader({
    title: 'Lớp học phần',
    description: 'Mở lớp, phân công giảng viên và quản lý sĩ số',
    actions: Button.render({ label: 'Thêm lớp học phần', variant: 'primary', icon: 'plus', id: 'btn-add-class', action: 'add' }),
  })}
  <section class="card" aria-label="Danh sách lớp học phần">
    <div class="filter-bar" role="search">
      ${Input.render({ id: 'filter-search', name: 'q', label: 'Tìm kiếm', value: state.search, placeholder: 'Mã lớp, giảng viên, phòng', iconLeft: 'search' })}
      ${Select.render({ id: 'filter-hocky', name: 'hocKy', label: 'Học kỳ', options: [{ value: '', label: 'Tất cả học kỳ' }, ...HOC_KY_LIST.map((item) => ({ value: item, label: item }))], value: state.hocKy })}
      <div id="filter-course-slot"></div>
      ${Button.render({ label: 'Đặt lại', variant: 'ghost', action: 'reset-filters' })}
    </div>
    <div class="card-body is-flush" id="table-container"></div>
  </section>`;

  bindEvents(root);
  const [courses] = await Promise.all([coursesReady, loadTable()]);

  courseOptions = courses;
  const courseFilterOptions = [
    { value: '', label: 'Tất cả môn' },
    ...courses.map((course) => ({ value: course.maMon, label: `${course.maMon} - ${course.tenMon}` })),
  ];
  root.querySelector('#filter-course-slot').outerHTML = Select.render({
    id: 'filter-course',
    name: 'maMon',
    label: 'Môn học',
    options: courseFilterOptions,
    value: state.maMon,
  });
}
