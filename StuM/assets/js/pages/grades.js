/**
 * Trang Điểm: xem điểm và nhập điểm nhanh (inline edit).
 * Điểm quá trình và cuối kỳ sửa trực tiếp trên bảng; điểm tổng và xếp loại được tính lại sau khi lưu.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { debounce, escapeHtml } from '../core/dom.js';
import { formatNumber } from '../core/format.js';
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

const SCORE_MIN = 0;
const SCORE_MAX = 10;

const RANK_VARIANTS = {
  'Xuất sắc': 'success',
  Giỏi: 'primary',
  Khá: 'info',
  'Trung bình': 'warning',
  Yếu: 'destructive',
};

const ROW_ACTIONS = [{ action: 'view', label: 'Xem chi tiết', icon: 'eye' }];

const state = { search: '', maMon: '', page: 1, pageSize: 10, sort: { key: 'mssv', dir: 'asc' }, rows: [] };
let studentNames = new Map();

/**
 * @param {string} rank
 * @returns {string}
 */
function rankBadge(rank) {
  return Badge.render({ label: rank, variant: RANK_VARIANTS[rank] ?? 'neutral' });
}

/**
 * Ô nhập điểm nội tuyến cho một cột điểm.
 * @param {'diemQT'|'diemCK'} name
 * @param {number} value
 * @param {{ mssv: string, maMon: string }} row
 * @returns {string}
 */
function scoreInput(name, value, row) {
  return `<input class="input inline-grade-input" type="number" name="${name}" data-grade="${name}" value="${escapeHtml(value)}" min="${SCORE_MIN}" max="${SCORE_MAX}" step="0.25" aria-label="${name === 'diemQT' ? 'Điểm quá trình' : 'Điểm cuối kỳ'} của ${escapeHtml(row.mssv)} môn ${escapeHtml(row.maMon)}">`;
}

const COLUMNS = [
  { key: 'mssv', label: 'MSSV', sortable: true },
  { key: 'hoTen', label: 'Họ và tên', primary: true },
  { key: 'maMon', label: 'Mã môn', sortable: true },
  { key: 'diemQT', label: 'Điểm QT', align: 'right', render: (row) => scoreInput('diemQT', row.diemQT, row) },
  { key: 'diemCK', label: 'Điểm CK', align: 'right', render: (row) => scoreInput('diemCK', row.diemCK, row) },
  {
    key: 'diemTK',
    label: 'Điểm TK',
    sortable: true,
    align: 'right',
    render: (row) => `<span data-cell="tk">${formatNumber(row.diemTK, 2)}</span>`,
  },
  { key: 'xepLoai', label: 'Xếp loại', render: (row) => `<span data-cell="xl">${rankBadge(row.xepLoai)}</span>` },
];

function hasActiveFilters() {
  return Boolean(state.search || state.maMon);
}

function syncUrl() {
  router.setQuery({ q: state.search, maMon: state.maMon });
}

/**
 * Điền họ tên từ danh sách sinh viên đã tải. Mã không khớp sẽ hiện "Chưa có trong danh sách".
 * @param {object} result
 */
function enrichRows(result) {
  result.data = result.data.map((row) => ({
    ...row,
    hoTen: studentNames.get(row.mssv) ?? 'Chưa có trong danh sách',
  }));
  return result;
}

function emptyMarkup() {
  if (hasActiveFilters()) {
    return EmptyState.render({
      icon: 'search-x',
      title: 'Không tìm thấy điểm phù hợp',
      description: 'Thử thay đổi từ khóa hoặc môn học.',
      actionText: 'Xóa bộ lọc',
      actionId: 'btn-clear-filters',
      action: 'reset-filters',
    });
  }
  return EmptyState.render({
    icon: 'clipboard-list',
    title: 'Chưa có điểm nào',
    description: 'Điểm sẽ hiển thị ở đây sau khi giảng viên nhập.',
  });
}

function loadTable() {
  return renderAsync(document.getElementById('table-container'), {
    load: async () => {
      const [result, students] = await Promise.all([
        api.grades.list({
          search: state.search,
          filters: { maMon: state.maMon },
          sort: state.sort,
          page: state.page,
          pageSize: state.pageSize,
        }),
        api.students.getAll(),
      ]);
      studentNames = new Map(students.map((student) => [student.mssv, student.hoTen]));
      const enriched = enrichRows(result);
      state.rows = enriched.data;
      return enriched;
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
        actionLabel: (row) => `Thao tác cho ${row.mssv}`,
        caption: 'Bảng điểm',
        footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
      }),
    errorTitle: 'Không tải được bảng điểm',
  });
}

/**
 * Lưu điểm khi người dùng rời ô nhập. Điểm ngoài khoảng 0-10 sẽ bị hoàn lại giá trị cũ.
 * @param {HTMLInputElement} input
 */
async function saveScore(input) {
  const row = input.closest('tr');
  const record = state.rows.find((item) => item.id === row.dataset.rowId);
  if (!record) return;

  const qtInput = row.querySelector('[data-grade="diemQT"]');
  const ckInput = row.querySelector('[data-grade="diemCK"]');
  const inRange = (element) => element.value !== '' && Number(element.value) >= SCORE_MIN && Number(element.value) <= SCORE_MAX;

  if (!inRange(input)) {
    Toast.warning(`Điểm phải nằm trong khoảng ${SCORE_MIN} - ${SCORE_MAX}`);
    input.value = record[input.dataset.grade];
    return;
  }

  const diemQT = Number(qtInput.value);
  const diemCK = Number(ckInput.value);
  if (diemQT === record.diemQT && diemCK === record.diemCK) return;

  row.classList.add('grade-cell-saving');
  qtInput.disabled = true;
  ckInput.disabled = true;
  try {
    const updated = await api.updateGrade(record.id, { diemQT, diemCK });
    Object.assign(record, updated);
    row.querySelector('[data-cell="tk"]').textContent = formatNumber(updated.diemTK, 2);
    row.querySelector('[data-cell="xl"]').innerHTML = rankBadge(updated.xepLoai);
    Toast.success(`Đã lưu điểm ${record.mssv} - ${record.maMon}`);
  } catch (error) {
    qtInput.value = record.diemQT;
    ckInput.value = record.diemCK;
    Toast.error(error.message);
  } finally {
    row.classList.remove('grade-cell-saving');
    qtInput.disabled = false;
    ckInput.disabled = false;
  }
}

/**
 * @param {string} action
 * @param {string} rowId
 */
function handleRowAction(action, rowId) {
  const record = state.rows.find((row) => row.id === rowId);
  if (action !== 'view' || !record) return;

  const content = `<dl class="detail-list">
    <dt>MSSV</dt><dd>${escapeHtml(record.mssv)}</dd>
    <dt>Họ và tên</dt><dd>${escapeHtml(record.hoTen)}</dd>
    <dt>Mã môn</dt><dd>${escapeHtml(record.maMon)}</dd>
    <dt>Điểm quá trình</dt><dd>${formatNumber(record.diemQT, 2)}</dd>
    <dt>Điểm cuối kỳ</dt><dd>${formatNumber(record.diemCK, 2)}</dd>
    <dt>Điểm tổng kết</dt><dd>${formatNumber(record.diemTK, 2)}</dd>
    <dt>Xếp loại</dt><dd>${rankBadge(record.xepLoai)}</dd>
  </dl>
  <p class="form-hint mt-md">Điểm tổng kết = 50% điểm quá trình + 50% điểm cuối kỳ.</p>`;
  const handle = Modal.open({
    title: 'Chi tiết điểm',
    content,
    footer: Button.render({ label: 'Đóng', variant: 'primary', action: 'close' }),
  });
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);
}

function resetFilters(root) {
  state.search = '';
  state.maMon = '';
  state.page = 1;
  root.querySelector('[name="q"]').value = '';
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
      if (element.name === 'maMon') {
        state.maMon = element.value;
        state.page = 1;
        syncUrl();
        loadTable();
      }
    },
  });

  Button.init(root, {
    onAction: (action) => {
      if (action === 'reset-filters') resetFilters(root);
    },
  });

  root.addEventListener('change', (event) => {
    if (event.target.matches('[data-grade]')) saveScore(event.target);
  });

  Table.init(root, {
    onSort: (key) => {
      const dir = state.sort.key === key && state.sort.dir === 'asc' ? 'desc' : 'asc';
      state.sort = { key, dir };
      state.page = 1;
      loadTable();
    },
    onAction: (action, rowId) => handleRowAction(action, rowId),
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
  state.maMon = query.maMon ?? '';
  const coursesReady = api.courses.getAll().catch(() => []);

  root.innerHTML = `${renderPageHeader({
    title: 'Điểm',
    description: 'Nhập điểm nhanh: sửa trực tiếp điểm quá trình và cuối kỳ trên bảng (thang 0 - 10)',
  })}
  <section class="card" aria-label="Bảng điểm">
    <div class="filter-bar" role="search">
      ${Input.render({ id: 'filter-search', name: 'q', label: 'Tìm kiếm', value: state.search, placeholder: 'MSSV hoặc mã môn', iconLeft: 'search' })}
      <div id="filter-course-slot"></div>
      ${Button.render({ label: 'Đặt lại', variant: 'ghost', action: 'reset-filters' })}
    </div>
    <div class="card-body is-flush" id="table-container"></div>
  </section>`;

  bindEvents(root);
  const [courses] = await Promise.all([coursesReady, loadTable()]);

  const courseOptions = [
    { value: '', label: 'Tất cả môn' },
    ...courses.map((course) => ({ value: course.maMon, label: `${course.maMon} - ${course.tenMon}` })),
  ];
  root.querySelector('#filter-course-slot').outerHTML = Select.render({
    id: 'filter-course',
    name: 'maMon',
    label: 'Môn học',
    options: courseOptions,
    value: state.maMon,
  });
}
