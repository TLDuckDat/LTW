/**
 * Trang Điểm: nhập và xem điểm theo lớp học phần (GET/PUT /api/diem/lop-hoc-phan/{id}).
 * Điểm được sửa trên bảng rồi lưu một lần; điểm tổng kết và điểm chữ do backend tính.
 * Tính điểm và khóa điểm là hai nghiệp vụ riêng của backend (khóa xong thì không sửa được nữa).
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { escapeHtml } from '../core/dom.js';
import { formatNumber } from '../core/format.js';
import { Badge } from '../components/badge.js';
import { Button } from '../components/button.js';
import { EmptyState } from '../components/empty-state.js';
import { Input } from '../components/input.js';
import { Modal } from '../components/modal.js';
import { Select } from '../components/select.js';
import { Skeleton } from '../components/skeleton.js';
import { Table } from '../components/table.js';
import { Toast } from '../components/toast.js';
import { renderPageHeader } from '../layout/shell.js';

const SCORE_MIN = 0;
const SCORE_MAX = 10;
const SCORE_FIELDS = ['diemQuaTrinh', 'diemGiuaKy', 'diemCuoiKy'];
const SCORE_LABELS = { diemQuaTrinh: 'điểm quá trình', diemGiuaKy: 'điểm giữa kỳ', diemCuoiKy: 'điểm cuối kỳ' };

const state = {
  lopHocPhanId: null,
  rows: [],
  original: new Map(),
  edits: new Map(),
  search: '',
};

/**
 * Điểm đã sửa nhưng chưa lưu, theo dangKyHocPhanId.
 * @returns {Array<{ dangKyHocPhanId: number, diemQuaTrinh: number, diemGiuaKy: number, diemCuoiKy: number }>}
 */
function dirtyRows() {
  return [...state.edits.entries()]
    .filter(([id, values]) => SCORE_FIELDS.some((field) => values[field] !== state.original.get(id)[field]))
    .map(([id, values]) => ({ dangKyHocPhanId: id, ...values }));
}

function updateSaveButton(root) {
  const button = root.querySelector('[data-action="save-grades"]');
  if (button) button.disabled = dirtyRows().length === 0;
}

function scoreInput(row, field) {
  const value = row[field] ?? '';
  const disabled = row.daKhoa ? ' disabled' : '';
  return `<input class="input inline-grade-input" type="number" name="${field}" data-field="${field}" data-row-id="${row.dangKyHocPhanId}" value="${value}" min="${SCORE_MIN}" max="${SCORE_MAX}" step="0.25" aria-label="${SCORE_LABELS[field]} của ${escapeHtml(row.mssv)}"${disabled}>`;
}

const COLUMNS = [
  { key: 'mssv', label: 'MSSV' },
  { key: 'hoTen', label: 'Họ và tên', primary: true },
  { key: 'loaiDangKy', label: 'Loại đăng ký' },
  { key: 'diemQuaTrinh', label: 'Điểm QT', align: 'right', render: (row) => scoreInput(row, 'diemQuaTrinh') },
  { key: 'diemGiuaKy', label: 'Điểm GK', align: 'right', render: (row) => scoreInput(row, 'diemGiuaKy') },
  { key: 'diemCuoiKy', label: 'Điểm CK', align: 'right', render: (row) => scoreInput(row, 'diemCuoiKy') },
  {
    key: 'diemTongKet',
    label: 'Điểm TK',
    align: 'right',
    render: (row) => (row.diemTongKet === null || row.diemTongKet === undefined ? '—' : formatNumber(row.diemTongKet, 2)),
  },
  { key: 'diemChu', label: 'Điểm chữ', render: (row) => escapeHtml(row.diemChu ?? '—') },
  {
    key: 'daKhoa',
    label: 'Trạng thái',
    render: (row) => (row.daKhoa ? Badge.render({ label: 'Đã khóa', variant: 'info' }) : Badge.render({ label: 'Đang nhập', variant: 'neutral' })),
  },
];

function classOptions(classes) {
  return classes.map((item) => ({ value: item.id, label: `${item.ma} - ${item.tenMonHoc}` }));
}

/**
 * Bảng điểm đang hiển thị, theo ô tìm kiếm. Ô điểm lấy giá trị đang sửa (chưa lưu) nếu có.
 * @param {object[]} rows
 * @returns {string}
 */
function tableMarkup(rows) {
  const search = state.search.toLowerCase();
  const withEdits = rows.map((row) => ({ ...row, ...state.edits.get(row.dangKyHocPhanId) }));
  const visible = withEdits.filter((row) => !search || row.mssv.toLowerCase().includes(search) || row.hoTen.toLowerCase().includes(search));
  if (visible.length === 0) {
    return EmptyState.render({
      icon: 'search-x',
      title: 'Không có sinh viên phù hợp',
      description: 'Thử tìm theo MSSV hoặc họ tên khác.',
    });
  }
  return Table.render({ columns: COLUMNS, rows: visible, rowKey: 'dangKyHocPhanId', caption: 'Bảng điểm lớp học phần' });
}

/**
 * Tải bảng điểm của lớp đang chọn và đổ vào vùng bảng.
 * @param {HTMLElement} root
 */
function loadGrades(root) {
  const container = root.querySelector('#grades-container');
  if (state.lopHocPhanId === null) {
    container.innerHTML = EmptyState.render({ icon: 'clipboard-list', title: 'Chọn một lớp học phần', description: 'Bảng điểm sẽ hiển thị sau khi bạn chọn lớp.' });
    updateSaveButton(root);
    return;
  }

  renderAsync(container, {
    load: async () => {
      const bang = await api.diem.bangDiem(state.lopHocPhanId);
      state.rows = bang.danhSach;
      state.original = new Map(bang.danhSach.map((row) => [row.dangKyHocPhanId, scoresOf(row)]));
      state.edits = new Map(bang.danhSach.map((row) => [row.dangKyHocPhanId, scoresOf(row)]));
      return bang;
    },
    skeleton: () => Skeleton.table(5, 8),
    isEmpty: (bang) => bang.danhSach.length === 0,
    empty: () => EmptyState.render({ icon: 'users', title: 'Lớp chưa có sinh viên đăng ký', description: 'Điểm sẽ hiển thị khi sinh viên đăng ký lớp.' }),
    render: (bang) => tableMarkup(bang.danhSach),
    afterRender: () => updateSaveButton(root),
    errorTitle: 'Không tải được bảng điểm',
  });
}

function scoresOf(row) {
  return {
    diemQuaTrinh: row.diemQuaTrinh ?? null,
    diemGiuaKy: row.diemGiuaKy ?? null,
    diemCuoiKy: row.diemCuoiKy ?? null,
  };
}

/**
 * Kiểm tra toàn bộ điểm sắp lưu: đủ ba cột và trong khoảng 0-10.
 * @param {Array<object>} rows
 * @returns {string|null} thông báo lỗi đầu tiên, hoặc null nếu hợp lệ
 */
function validateRows(rows) {
  for (const row of rows) {
    const student = state.rows.find((item) => item.dangKyHocPhanId === row.dangKyHocPhanId);
    for (const field of SCORE_FIELDS) {
      const value = row[field];
      if (value === null || value === '' || Number.isNaN(value)) {
        return `Thiếu ${SCORE_LABELS[field]} của ${student?.mssv ?? 'sinh viên'}`;
      }
      if (value < SCORE_MIN || value > SCORE_MAX) {
        return `${SCORE_LABELS[field]} của ${student?.mssv ?? 'sinh viên'} phải trong khoảng ${SCORE_MIN} - ${SCORE_MAX}`;
      }
    }
  }
  return null;
}

async function saveGrades(root, button) {
  const rows = dirtyRows();
  const error = validateRows(rows);
  if (error) {
    Toast.warning(error);
    return;
  }

  Button.setLoading(button, true);
  try {
    const result = await api.diem.nhapDiem(state.lopHocPhanId, rows);
    Toast.success(`Đã lưu điểm: ${result.soThemMoi} mới, ${result.soCapNhat} cập nhật`);
    loadGrades(root);
  } catch (error) {
    Toast.error(error.message);
  } finally {
    Button.setLoading(button, false);
    updateSaveButton(root);
  }
}

function confirmLock(root) {
  Modal.confirm({
    title: 'Khóa điểm lớp học phần',
    message: 'Sau khi khóa, điểm của lớp không thể sửa nữa. Hãy chắc chắn đã tính điểm và nhập đủ.',
    confirmText: 'Khóa điểm',
    danger: true,
    onConfirm: async () => {
      try {
        const result = await api.diem.khoaDiem(state.lopHocPhanId);
        Toast.success(`Đã khóa điểm ${result.soMoiKhoa} sinh viên`);
        loadGrades(root);
      } catch (error) {
        Toast.error(error.message);
        throw error;
      }
    },
  }).catch(() => {});
}

async function calculate(root) {
  try {
    const result = await api.diem.tinhDiem(state.lopHocPhanId);
    Toast.success(`Đã tính điểm: ${result.soDaTinh} bản ghi, còn thiếu điểm ${result.soThieuDiem}`);
    loadGrades(root);
  } catch (error) {
    Toast.error(error.message);
  }
}

/**
 * Cập nhật giá trị đang sửa khi người dùng gõ vào ô điểm.
 * @param {HTMLInputElement} input
 * @param {HTMLElement} root
 */
function onScoreInput(input, root) {
  const id = Number(input.dataset.rowId);
  const field = input.dataset.field;
  const values = state.edits.get(id);
  if (!values) return;
  values[field] = input.value === '' ? null : Number(input.value);
  input.removeAttribute('aria-invalid');
  if (values[field] !== null && (values[field] < SCORE_MIN || values[field] > SCORE_MAX)) {
    input.setAttribute('aria-invalid', 'true');
  }
  updateSaveButton(root);
}

function bindEvents(root) {
  Select.init(root, {
    onChange: (element) => {
      if (element.name !== 'lopHocPhanId') return;
      state.lopHocPhanId = element.value === '' ? null : Number(element.value);
      loadGrades(root);
    },
  });

  root.addEventListener('input', (event) => {
    if (event.target.matches('.inline-grade-input')) onScoreInput(event.target, root);
    if (event.target.matches('[name="q"]')) {
      state.search = event.target.value.trim();
      const container = root.querySelector('#grades-container');
      if (state.rows.length > 0) container.innerHTML = tableMarkup(state.rows);
    }
  });

  Button.init(root, {
    onAction: (action, button) => {
      if (action === 'save-grades') saveGrades(root, button);
      if (action === 'calculate') calculate(root);
      if (action === 'lock') confirmLock(root);
    },
  });
}

/**
 * @param {HTMLElement} root
 * @param {{ query: Record<string, string> }} context
 */
export async function mount(root, { query }) {
  state.lopHocPhanId = query.lopHocPhanId ? Number(query.lopHocPhanId) : null;
  state.search = '';

  root.innerHTML = `${renderPageHeader({
    title: 'Điểm',
    description: 'Chọn lớp học phần, nhập điểm quá trình, giữa kỳ và cuối kỳ (thang 0 - 10). Điểm tổng kết do hệ thống tính.',
  })}
  <section class="card" aria-label="Bảng điểm">
    <div class="filter-bar" role="search">
      <div id="class-select-slot"></div>
      ${Input.render({ name: 'q', label: 'Tìm sinh viên', placeholder: 'MSSV hoặc họ tên', iconLeft: 'search' })}
    </div>
    <div class="filter-bar">
      ${Button.render({ label: 'Tính điểm', variant: 'outline', icon: 'calculator', action: 'calculate' })}
      ${Button.render({ label: 'Khóa điểm', variant: 'destructive', icon: 'lock', action: 'lock' })}
      ${Button.render({ label: 'Lưu thay đổi', variant: 'primary', icon: 'save', action: 'save-grades', disabled: true })}
    </div>
    <div class="card-body is-flush" id="grades-container"></div>
  </section>`;

  bindEvents(root);

  const classes = await api.lopHocPhan.getAll().catch((error) => {
    Toast.error(error.message);
    return [];
  });
  root.querySelector('#class-select-slot').outerHTML = Select.render({
    name: 'lopHocPhanId',
    label: 'Lớp học phần',
    options: classOptions(classes),
    value: state.lopHocPhanId ?? '',
    placeholder: 'Chọn lớp học phần',
  });
  loadGrades(root);
}
