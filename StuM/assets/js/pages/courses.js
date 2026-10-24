/**
 * Trang Môn học: CRUD với backend (GET/POST/PUT/DELETE /api/mon-hoc) và quản lý môn tiên quyết.
 * Backend chặn xóa môn đang được dùng (lớp học phần, chương trình đào tạo), lỗi được hiển thị nguyên văn.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { debounce, escapeHtml, nextId } from '../core/dom.js';
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
import { formatDate } from '../core/format.js';
import { openDetailModal } from './graduation-forms.js';

const COLUMNS = [
  { key: 'ma', label: 'Mã môn', primary: true },
  { key: 'ten', label: 'Tên môn' },
  { key: 'soTinChi', label: 'Số tín chỉ', align: 'right' },
];

const ROW_ACTIONS = [
  { action: 'detail', label: 'Xem chi tiết', icon: 'eye' },
  { action: 'edit', label: 'Chỉnh sửa', icon: 'pencil' },
  { action: 'prerequisites', label: 'Môn tiên quyết', icon: 'git-branch' },
  { divider: true },
  { action: 'delete', label: 'Xóa', icon: 'trash-2', danger: true },
];

const MA_MON_PATTERN = /^[A-Z]{2,4}\d{4}$/i;
const state = { search: '', page: 1, pageSize: 10, rows: [] };

function syncUrl() {
  router.setQuery({ q: state.search });
}

function emptyMarkup() {
  if (state.search) {
    return EmptyState.render({
      icon: 'search-x',
      title: 'Không tìm thấy môn học phù hợp',
      description: 'Thử thay đổi từ khóa tìm kiếm.',
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
      const result = await api.monHoc.list({ page: state.page, pageSize: state.pageSize, search: state.search });
      state.rows = result.data;
      return result;
    },
    skeleton: () => Skeleton.table(5, 3),
    isEmpty: (result) => result.total === 0,
    empty: emptyMarkup,
    render: (result) =>
      Table.render({
        columns: COLUMNS,
        rows: result.data,
        actions: ROW_ACTIONS,
        actionLabel: (row) => `Thao tác cho ${row.ma}`,
        caption: 'Danh sách môn học',
        footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
      }),
    errorTitle: 'Không tải được danh sách môn học',
  });
}

/**
 * @param {HTMLInputElement} input
 * @returns {boolean}
 */
function validateCourseInput(input) {
  const value = input.value.trim();
  let message = '';
  if (input.name === 'ma') {
    if (!value) message = 'Vui lòng nhập mã môn';
    else if (!MA_MON_PATTERN.test(value)) message = 'Mã môn dạng 2-4 chữ cái và 4 chữ số, ví dụ INT1001';
  } else if (input.name === 'ten' && !value) {
    message = 'Vui lòng nhập tên môn';
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
 * @param {{ record?: object|null }} options
 */
function openCourseForm({ record = null }) {
  const formId = nextId('course-form');
  const isEdit = Boolean(record);
  const handle = Modal.open({
    title: isEdit ? 'Chỉnh sửa môn học' : 'Thêm môn học',
    content: `<form class="modal-form" id="${formId}" novalidate>
      <div class="form-grid">
        ${Input.render({ name: 'ma', label: 'Mã môn', value: record?.ma ?? '', placeholder: 'INT1001', required: true, hint: '2-4 chữ cái và 4 chữ số' })}
        ${Input.render({ name: 'ten', label: 'Tên môn', value: record?.ten ?? '', placeholder: 'Lập trình Web', required: true })}
        ${Input.render({ name: 'soTinChi', type: 'number', label: 'Số tín chỉ', value: record?.soTinChi ?? 3, min: 1, max: 6, step: 1, required: true })}
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
    if (inputs.map(validateCourseInput).includes(false)) return;

    const field = (name) => form.elements.namedItem(name).value.trim();
    const payload = { ma: field('ma').toUpperCase(), ten: field('ten'), soTinChi: Number(field('soTinChi')) };

    Button.setLoading(saveButton, true);
    const request = isEdit ? api.monHoc.update(record.id, payload) : api.monHoc.create(payload);
    request
      .then(() => {
        Toast.success(isEdit ? 'Cập nhật môn học thành công' : 'Thêm môn học thành công');
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
 * Quản lý môn tiên quyết: liệt kê, thêm, bỏ.
 * @param {object} record
 */
async function openPrerequisites(record) {
  const formId = nextId('prereq-form');
  const handle = Modal.open({
    title: `Môn tiên quyết của ${record.ma}`,
    size: 'lg',
    content: `<div class="modal-form">
      <div data-prereq-list></div>
      <form class="filter-bar" id="${formId}" novalidate>
        <div class="field" data-prereq-select-slot></div>
        ${Button.render({ label: 'Thêm', variant: 'primary', action: 'add-prereq' })}
      </form>
    </div>`,
    footer: Button.render({ label: 'Đóng', variant: 'outline', action: 'close' }),
  });
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);

  const listEl = handle.element.querySelector('[data-prereq-list]');
  const selectSlot = handle.element.querySelector('[data-prereq-select-slot]');

  const reload = async () => {
    const [prereqs, all] = await Promise.all([api.monHoc.tienQuyet(record.id), api.monHoc.getAll()]);
    listEl.innerHTML = prereqs.length
      ? `<ul class="todo-list">${prereqs
          .map(
            (item) => `<li class="todo-item">
              <span class="todo-text">${escapeHtml(item.ma)} - ${escapeHtml(item.ten)}</span>
              ${Button.render({ label: 'Bỏ', variant: 'ghost', size: 'sm', action: 'remove-prereq', id: `prereq-${item.id}` })}
            </li>`,
          )
          .join('')}</ul>`
      : '<p class="form-hint">Môn này chưa có môn tiên quyết.</p>';

    const taken = new Set(prereqs.map((item) => item.id));
    const options = all
      .filter((item) => item.id !== record.id && !taken.has(item.id))
      .map((item) => ({ value: item.id, label: `${item.ma} - ${item.ten}` }));
    selectSlot.innerHTML = Select.render({ name: 'tienQuyetId', label: 'Chọn môn tiên quyết', options, placeholder: 'Chọn môn', value: '' });
  };

  listEl.addEventListener('click', async (event) => {
    const button = event.target.closest('[data-action="remove-prereq"]');
    if (!button) return;
    const tienQuyetId = Number(button.id.replace('prereq-', ''));
    try {
      await api.monHoc.boTienQuyet(record.id, tienQuyetId);
      Toast.success('Đã bỏ môn tiên quyết');
      reload();
    } catch (error) {
      Toast.error(error.message);
    }
  });

  handle.element.querySelector('[data-action="add-prereq"]').addEventListener('click', async () => {
    const select = handle.element.querySelector('[name="tienQuyetId"]');
    if (!select || select.value === '') {
      Toast.warning('Chọn một môn tiên quyết trước khi thêm');
      return;
    }
    try {
      await api.monHoc.themTienQuyet(record.id, Number(select.value));
      Toast.success('Đã thêm môn tiên quyết');
      reload();
    } catch (error) {
      Toast.error(error.message);
    }
  });

  try {
    await reload();
  } catch (error) {
    listEl.innerHTML = `<p class="form-error">${escapeHtml(error.message)}</p>`;
  }
}

function confirmDelete(record) {
  Modal.confirm({
    title: 'Xóa môn học',
    message: `Bạn có chắc muốn xóa môn ${record.ten} (${record.ma})? Backend sẽ từ chối nếu môn đang được sử dụng.`,
    confirmText: 'Xóa',
    danger: true,
    onConfirm: async () => {
      try {
        await api.monHoc.remove(record.id);
        Toast.success(`Đã xóa môn ${record.ma}`);
        loadTable();
      } catch (error) {
        Toast.error(error.message);
        throw error;
      }
    },
  }).catch(() => {});
}

/** Chi tiết một môn học, lấy từ GET /api/mon-hoc/{id}. */
async function openCourseDetail(record) {
  try {
    const mon = await api.monHoc.get(record.id);
    openDetailModal({
      title: `Môn ${mon.ma}`,
      rows: [
        ['Mã môn', escapeHtml(mon.ma)],
        ['Tên môn', escapeHtml(mon.ten)],
        ['Số tín chỉ', escapeHtml(mon.soTinChi)],
        ['Ngày tạo', escapeHtml(formatDate(mon.createdAt))],
        ['Cập nhật gần nhất', escapeHtml(mon.updatedAt ? formatDate(mon.updatedAt) : '—')],
      ],
    });
  } catch (error) {
    Toast.error(error.message);
  }
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

  Button.init(root, {
    onAction: (action) => {
      if (action === 'add') openCourseForm({});
      if (action === 'reset-filters') {
        state.search = '';
        state.page = 1;
        root.querySelector('[name="q"]').value = '';
        syncUrl();
        loadTable();
      }
    },
  });

  Table.init(root, {
    onAction: (action, rowId) => {
      const record = state.rows.find((row) => String(row.id) === String(rowId));
      if (!record) return;
      if (action === 'detail') openCourseDetail(record);
      if (action === 'edit') openCourseForm({ record });
      if (action === 'prerequisites') openPrerequisites(record);
      if (action === 'delete') confirmDelete(record);
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
  root.innerHTML = `${renderPageHeader({
    title: 'Môn học',
    description: 'Danh mục môn học, số tín chỉ và môn tiên quyết',
    actions: Button.render({ label: 'Thêm môn học', variant: 'primary', icon: 'plus', id: 'btn-add-course', action: 'add' }),
  })}
  <section class="card" aria-label="Danh sách môn học">
    <div class="filter-bar" role="search">
      ${Input.render({ id: 'filter-search', name: 'q', label: 'Tìm kiếm', value: state.search, placeholder: 'Mã môn hoặc tên môn', iconLeft: 'search' })}
      ${Button.render({ label: 'Đặt lại', variant: 'ghost', action: 'reset-filters' })}
    </div>
    <div class="card-body is-flush" id="table-container"></div>
  </section>`;

  bindEvents(root);
  return loadTable();
}
