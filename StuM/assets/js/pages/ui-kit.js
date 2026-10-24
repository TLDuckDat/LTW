/**
 * Trang ui-kit.html: showcase toàn bộ component ở hai panel light và dark.
 * Modal và toast được gắn vào body nên theo theme của trang, không theo panel.
 */

import { refreshIcons } from '../core/dom.js';
import { Button } from '../components/button.js';
import { Dropdown } from '../components/dropdown.js';
import { Input } from '../components/input.js';
import { Modal } from '../components/modal.js';
import { Pagination } from '../components/pagination.js';
import { Table } from '../components/table.js';
import { Toast } from '../components/toast.js';
import * as display from './ui-kit-display.js';
import * as forms from './ui-kit-forms.js';

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const DEMO_LOADING_MS = 1500;
const DEMO_TOTAL = 142;

const RULES = {
  fullName: { check: (value) => value.length > 0, message: 'Vui lòng nhập họ và tên' },
  email: { check: (value) => EMAIL_PATTERN.test(value), message: 'Email không đúng định dạng' },
};

const wait = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

const SECTIONS = [
  { key: 'button', title: 'Button', render: () => forms.renderButtons() },
  { key: 'input', title: 'Input', render: (prefix) => forms.renderInputs(prefix) },
  { key: 'select', title: 'Select', render: (prefix) => forms.renderSelects(prefix) },
  { key: 'card', title: 'Card', render: () => display.renderCards() },
  { key: 'badge', title: 'Badge', render: () => display.renderBadges() },
  {
    key: 'table',
    title: 'Table (sắp xếp, menu thao tác, card view dưới 768px)',
    render: (_, state) => `<div data-slot="table">${display.tableMarkup(state.table)}</div>`,
  },
  { key: 'pagination', title: 'Pagination', render: (_, state) => display.renderPaginationDemo(state.pagination) },
  { key: 'dropdown', title: 'Dropdown', render: () => display.renderDropdownDemo() },
  { key: 'overlay', title: 'Modal và Confirm Dialog', render: () => display.renderOverlays() },
  { key: 'toast', title: 'Toast', render: () => display.renderToastButtons() },
  { key: 'empty', title: 'Empty State', render: (prefix) => display.renderEmpty(prefix) },
  { key: 'skeleton', title: 'Skeleton', render: () => display.renderSkeletons() },
  { key: 'chart', title: 'Biểu đồ SVG', render: () => display.renderChart() },
];

function createPanelState() {
  return {
    table: { rows: display.DEMO_ROWS, sort: { key: 'mssv', dir: 'asc' } },
    pagination: { page: 1, pageSize: 10, total: DEMO_TOTAL },
  };
}

/**
 * @param {HTMLInputElement} input
 * @returns {boolean}
 */
function validateField(input) {
  const rule = RULES[input.name];
  if (!rule) return true;

  const isValid = rule.check(input.value.trim());
  if (isValid) {
    Input.clearError(input);
  } else {
    Input.setError(input, rule.message);
  }
  return isValid;
}

/**
 * @param {HTMLElement} root
 * @returns {boolean}
 */
function validateForm(root) {
  const inputs = Array.from(root.querySelectorAll('input[name]'));
  // Dùng map trước every() để mọi ô đều được kiểm tra và hiện lỗi cùng lúc
  return inputs.map(validateField).every(Boolean);
}

function openFormModal() {
  const handle = Modal.open({
    title: 'Thêm mục mới',
    content: `<form class="modal-form" novalidate>${Input.render({
      name: 'itemName',
      label: 'Tên mục',
      placeholder: 'Nhập tên mục',
      required: true,
      hint: 'Tối đa 100 ký tự',
    })}</form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: 'Lưu', variant: 'primary', action: 'save' }),
    ].join(''),
  });
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);
  handle.element.querySelector('[data-action="save"]').addEventListener('click', () => {
    Toast.success('Đã lưu mục mới (mô phỏng)');
    handle.close();
  });
}

/**
 * @param {{ mssv: string, hoTen: string }} row
 */
function confirmDelete(row) {
  Modal.confirm({
    title: 'Xóa sinh viên',
    message: `Bạn có chắc muốn xóa ${row.hoTen} (${row.mssv})? Hành động này không thể hoàn tác.`,
    confirmText: 'Xóa',
    danger: true,
    onConfirm: async () => {
      await wait(600);
      Toast.success(`Đã xóa ${row.mssv} (mô phỏng)`);
    },
  }).catch(() => {});
}

/**
 * @param {HTMLElement} root
 * @param {string} action
 * @param {HTMLButtonElement} button
 */
function handleButton(root, action, button) {
  switch (action) {
    case 'demo-save':
      Button.setLoading(button, true);
      setTimeout(() => Button.setLoading(button, false), DEMO_LOADING_MS);
      break;
    case 'demo-validate':
      validateForm(root);
      break;
    case 'toast-success':
      Toast.success('Lưu thành công (mô phỏng)');
      break;
    case 'toast-warning':
      Toast.warning('Còn 2 ngày để nhập điểm');
      break;
    case 'toast-error':
      Toast.error('Không kết nối được máy chủ');
      break;
    case 'toast-info':
      Toast.info('Có 3 thông báo mới');
      break;
    case 'open-modal':
      openFormModal();
      break;
    case 'open-confirm':
      confirmDelete({ mssv: 'SV005', hoTen: 'Hoàng Minh Đức' });
      break;
    case 'demo-empty-add':
      Toast.info('Mở form thêm mới (mô phỏng)');
      break;
    default:
      break;
  }
}

/**
 * Xử lý menu: dòng của bảng có rowId, menu độc lập thì không có.
 * @param {string} action
 * @param {string|undefined} rowId
 * @param {ReturnType<typeof createPanelState>} state
 */
function handleMenu(action, rowId, state) {
  if (!rowId) {
    Toast.info(`Đã chọn "${action}" (mô phỏng)`);
    return;
  }
  const row = state.table.rows.find((item) => item.id === rowId);
  if (!row) return;
  if (action === 'delete') {
    confirmDelete(row);
    return;
  }
  Toast.info(`${action === 'view' ? 'Xem' : 'Sửa'} ${row.mssv} (mô phỏng)`);
}

/**
 * @param {HTMLElement} root
 */
function mountPanel(root) {
  const prefix = root.dataset.uiPanel;
  const state = createPanelState();
  const sectionsEl = root.querySelector('[data-ui-sections]');

  sectionsEl.innerHTML = SECTIONS.map((section) => {
    const titleId = `${prefix}-${section.key}-title`;
    return `<section class="ui-kit-section" aria-labelledby="${titleId}">
      <h3 class="h3" id="${titleId}">${section.title}</h3>
      ${section.render(prefix, state)}
    </section>`;
  }).join('');
  refreshIcons();

  const tableSlot = root.querySelector('[data-slot="table"]');
  const paginationSlot = root.querySelector('[data-slot="pagination"]');

  Button.init(root, { onAction: (action, button) => handleButton(root, action, button) });

  // Table.init cũng gắn menu thao tác (Dropdown) của toàn panel
  Table.init(root, {
    onSort: (key) => {
      const dir = state.table.sort.key === key && state.table.sort.dir === 'asc' ? 'desc' : 'asc';
      state.table.sort = { key, dir };
      tableSlot.innerHTML = display.tableMarkup(state.table);
      refreshIcons();
    },
    onAction: (action, rowId) => handleMenu(action, rowId, state),
  });

  Pagination.init(paginationSlot, {
    onChange: ({ page, pageSize }) => {
      state.pagination = { ...state.pagination, page, pageSize };
      paginationSlot.innerHTML = display.paginationMarkup(state.pagination);
      refreshIcons();
    },
  });

  Input.init(root, {
    onInput: (input) => {
      if (input.getAttribute('aria-invalid') === 'true') validateField(input);
    },
    onBlur: (input) => validateField(input),
  });
}

Dropdown.initGlobal();
document.querySelectorAll('[data-ui-panel]').forEach(mountPanel);
