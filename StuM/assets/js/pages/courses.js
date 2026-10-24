/**
 * Trang Môn học: CRUD theo đúng pattern của trang Sinh viên.
 * Không cho xóa môn đang có lớp học phần.
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

const KHOA_LIST = ['CNTT', 'Kinh tế', 'Ngoại ngữ', 'Cơ khí'];
const LOAI_MON_LIST = ['Bắt buộc', 'Tự chọn'];
const MA_MON_PATTERN = /^[A-Z]{2,4}\d{4}$/i;

const COLUMNS = [
  { key: 'maMon', label: 'Mã môn', sortable: true },
  { key: 'tenMon', label: 'Tên môn', sortable: true, primary: true },
  { key: 'soTinChi', label: 'Số tín chỉ', sortable: true, align: 'right' },
  { key: 'khoa', label: 'Khoa' },
  {
    key: 'loaiMon',
    label: 'Loại môn',
    render: (row) => Badge.render({ label: row.loaiMon, variant: row.loaiMon === 'Bắt buộc' ? 'primary' : 'neutral' }),
  },
];

const ROW_ACTIONS = [
  { action: 'edit', label: 'Chỉnh sửa', icon: 'pencil' },
  { divider: true },
  { action: 'delete', label: 'Xóa', icon: 'trash-2', danger: true },
];

const state = { search: '', khoa: '', loaiMon: '', page: 1, pageSize: 10, sort: { key: 'maMon', dir: 'asc' }, rows: [] };

const toOptions = (list) => list.map((item) => ({ value: item, label: item }));

function hasActiveFilters() {
  return Boolean(state.search || state.khoa || state.loaiMon);
}

function syncUrl() {
  router.setQuery({ q: state.search, khoa: state.khoa, loaiMon: state.loaiMon });
}

function emptyMarkup() {
  if (hasActiveFilters()) {
    return EmptyState.render({
      icon: 'search-x',
      title: 'Không tìm thấy môn học phù hợp',
      description: 'Thử thay đổi từ khóa hoặc bộ lọc.',
      actionText: 'Xóa bộ lọc',
      actionId: 'btn-clear-filters',
      action: 'reset-filters',
    });
  }
  return EmptyState.render({
    icon: 'book-open',
    title: 'Chưa có môn học nào',
    description: 'Bắt đầu bằng cách thêm môn học đầu tiên',
    actionText: 'Thêm môn học',
    actionId: 'btn-add-course-empty',
    action: 'add',
  });
}

function loadTable() {
  return renderAsync(document.getElementById('table-container'), {
    load: async () => {
      const result = await api.courses.list({
        search: state.search,
        filters: { khoa: state.khoa, loaiMon: state.loaiMon },
        sort: state.sort,
        page: state.page,
        pageSize: state.pageSize,
      });
      state.rows = result.data;
      return result;
    },
    skeleton: () => Skeleton.table(5, 5),
    isEmpty: (result) => result.total === 0,
    empty: emptyMarkup,
    render: (result) =>
      Table.render({
        columns: COLUMNS,
        rows: result.data,
        sort: state.sort,
        actions: ROW_ACTIONS,
        actionLabel: (row) => `Thao tác cho ${row.maMon}`,
        caption: 'Danh sách môn học',
        footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
      }),
    errorTitle: 'Không tải được danh sách môn học',
  });
}

/**
 * @param {{ record?: object|null }} options
 */
function openCourseForm({ record = null }) {
  const formId = nextId('course-form');
  const isEdit = Boolean(record);
  const handle = Modal.open({
    title: isEdit ? 'Chỉnh sửa môn học' : 'Thêm môn học',
    content: `<form class="modal-form" id="${formId}" novalidate>
      <div class="form-grid">
        ${Input.render({ name: 'maMon', label: 'Mã môn', value: record?.maMon ?? '', placeholder: 'INT1001', required: true, hint: '2-4 chữ cái và 4 chữ số' })}
        ${Input.render({ name: 'tenMon', label: 'Tên môn', value: record?.tenMon ?? '', placeholder: 'Lập trình Web', required: true })}
        ${Input.render({ name: 'soTinChi', type: 'number', label: 'Số tín chỉ', value: record?.soTinChi ?? 3, min: 1, max: 6, step: 1, required: true })}
        ${Select.render({ name: 'khoa', label: 'Khoa', value: record?.khoa ?? KHOA_LIST[0], options: toOptions(KHOA_LIST) })}
        ${Select.render({ name: 'loaiMon', label: 'Loại môn', value: record?.loaiMon ?? LOAI_MON_LIST[0], options: toOptions(LOAI_MON_LIST) })}
      </div>
    </form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: isEdit ? 'Lưu thay đổi' : 'Thêm môn học', variant: 'primary', action: 'save' }),
    ].join(''),
  });

  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);
  Input.init(form, {
    onBlur: (input) => validateCourseInput(input),
    onInput: (input) => input.getAttribute('aria-invalid') === 'true' && validateCourseInput(input),
  });

  saveButton.addEventListener('click', () => {
    const inputs = Array.from(form.querySelectorAll('input[name]'));
    const results = inputs.map(validateCourseInput);
    if (results.includes(false)) return;

    const field = (name) => form.elements.namedItem(name).value.trim();
    const payload = {
      maMon: field('maMon').toUpperCase(),
      tenMon: field('tenMon'),
      soTinChi: Number(field('soTinChi')),
      khoa: field('khoa'),
      loaiMon: field('loaiMon'),
    };

    Button.setLoading(saveButton, true);
    const request = isEdit ? api.courses.update(record.id, payload) : api.courses.create(payload);
    request
      .then(() => {
        Toast.success(isEdit ? 'Cập nhật môn học thành công' : 'Thêm môn học thành công');
        handle.close();
        loadTable();
      })
      .catch((error) => {
        Button.setLoading(saveButton, false);
        if (error.status === 409) {
          Input.setError(form.elements.namedItem('maMon'), error.message);
        } else {
          Toast.error(error.message);
        }
      });
  });
}

/**
 * @param {HTMLInputElement} input
 * @returns {boolean}
 */
function validateCourseInput(input) {
  const value = input.value.trim();
  let message = '';
  if (input.name === 'maMon') {
    if (!value) message = 'Vui lòng nhập mã môn';
    else if (!MA_MON_PATTERN.test(value)) message = 'Mã môn dạng 2-4 chữ cái và 4 chữ số, ví dụ INT1001';
  } else if (input.name === 'tenMon') {
    if (!value) message = 'Vui lòng nhập tên môn';
  } else if (input.name === 'soTinChi') {
    const credits = Number(value);
    if (!Number.isInteger(credits) || credits < 1 || credits > 6) message = 'Số tín chỉ là số nguyên từ 1 đến 6';
  }

  if (message) {
    Input.setError(input, message);
    return false;
  }
  Input.clearError(input);
  return true;
}

/**
 * Không xóa môn đang có lớp học phần. Nếu không còn lớp nào thì mới hỏi xác nhận.
 * @param {string} id
 */
async function confirmDelete(id) {
  const record = state.rows.find((row) => row.id === id);
  if (!record) return;

  try {
    const classes = await api.classes.getAll();
    const usedBy = classes.filter((item) => item.maMon === record.maMon).length;
    if (usedBy > 0) {
      Toast.warning(`Không thể xóa: môn ${record.maMon} còn ${usedBy} lớp học phần`);
      return;
    }
  } catch (error) {
    Toast.error(error.message);
    return;
  }

  Modal.confirm({
    title: 'Xóa môn học',
    message: `Bạn có chắc muốn xóa môn ${record.tenMon} (${record.maMon})? Hành động này không thể hoàn tác.`,
    confirmText: 'Xóa',
    danger: true,
    onConfirm: async () => {
      try {
        await api.courses.remove(id);
        Toast.success(`Đã xóa môn ${record.maMon}`);
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
  state.khoa = '';
  state.loaiMon = '';
  state.page = 1;
  root.querySelector('[name="q"]').value = '';
  root.querySelector('[name="khoa"]').value = '';
  root.querySelector('[name="loaiMon"]').value = '';
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
      if (!['khoa', 'loaiMon'].includes(element.name)) return;
      state[element.name] = element.value;
      state.page = 1;
      syncUrl();
      loadTable();
    },
  });

  Button.init(root, {
    onAction: (action) => {
      if (action === 'add') openCourseForm({});
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
      if (action === 'edit' && record) openCourseForm({ record });
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
export function mount(root, { query }) {
  state.search = query.q ?? '';
  state.khoa = query.khoa ?? '';
  state.loaiMon = query.loaiMon ?? '';

  const khoaOptions = [{ value: '', label: 'Tất cả khoa' }, ...toOptions(KHOA_LIST)];
  const loaiOptions = [{ value: '', label: 'Tất cả loại' }, ...toOptions(LOAI_MON_LIST)];

  root.innerHTML = `${renderPageHeader({
    title: 'Môn học',
    description: 'Danh mục môn học và số tín chỉ',
    actions: Button.render({ label: 'Thêm môn học', variant: 'primary', icon: 'plus', id: 'btn-add-course', action: 'add' }),
  })}
  <section class="card" aria-label="Danh sách môn học">
    <div class="filter-bar" role="search">
      ${Input.render({ id: 'filter-search', name: 'q', label: 'Tìm kiếm', value: state.search, placeholder: 'Mã môn hoặc tên môn', iconLeft: 'search' })}
      ${Select.render({ id: 'filter-khoa', name: 'khoa', label: 'Khoa', options: khoaOptions, value: state.khoa })}
      ${Select.render({ id: 'filter-loai', name: 'loaiMon', label: 'Loại môn', options: loaiOptions, value: state.loaiMon })}
      ${Button.render({ label: 'Đặt lại', variant: 'ghost', action: 'reset-filters' })}
    </div>
    <div class="card-body is-flush" id="table-container"></div>
  </section>`;

  bindEvents(root);
  return loadTable();
}
