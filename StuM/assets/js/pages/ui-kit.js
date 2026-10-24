/**
 * Trang ui-kit.html: render Button và Input vào từng panel theme.
 * Mỗi panel có data-ui-panel; panel có class .dark dùng bộ token tối.
 */

import { refreshIcons } from '../core/dom.js';
import { Button } from '../components/button.js';
import { Input } from '../components/input.js';

const VARIANTS = ['primary', 'secondary', 'outline', 'ghost', 'destructive'];
const SIZES = ['sm', 'md', 'lg'];
const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const DEMO_LOADING_MS = 1500;

const RULES = {
  fullName: { check: (value) => value.length > 0, message: 'Vui lòng nhập họ và tên' },
  email: { check: (value) => EMAIL_PATTERN.test(value), message: 'Email không đúng định dạng' },
};

function renderButtonDemo() {
  const rows = VARIANTS.map((variant) => `
    <div class="ui-kit-row">
      <span class="ui-kit-row-label">${variant}</span>
      <div class="ui-kit-row-items">
        ${SIZES.map((size) => Button.render({ label: `Nút ${size}`, variant, size })).join('')}
        ${Button.render({ label: 'Có icon', variant, icon: 'plus' })}
        ${Button.render({ variant, iconOnly: true, icon: 'pencil', ariaLabel: 'Sửa' })}
        ${Button.render({ label: 'Đang lưu', variant, loading: true })}
        ${Button.render({ label: 'Vô hiệu', variant, disabled: true })}
      </div>
    </div>`).join('');

  const interactive = `
    <div class="ui-kit-row">
      <span class="ui-kit-row-label">Tương tác</span>
      <div class="ui-kit-row-items">
        ${Button.render({ label: 'Lưu (mô phỏng 1.5s)', variant: 'primary', action: 'demo-save' })}
      </div>
    </div>`;

  return rows + interactive;
}

function renderInputDemo() {
  return `
    <div class="ui-kit-form" data-demo-form>
      <div class="ui-kit-fields">
        ${Input.render({ name: 'fullName', label: 'Họ và tên', placeholder: 'Nguyễn Văn A', required: true, hint: 'Theo giấy tờ tùy thân' })}
        ${Input.render({ name: 'email', type: 'email', label: 'Email', placeholder: 'ten@stum.edu.vn', iconLeft: 'mail', required: true })}
        ${Input.render({ name: 'gpa', type: 'number', label: 'GPA', value: 3.45, min: 0, max: 4, step: 0.01, hint: 'Thang điểm 4' })}
        ${Input.render({ name: 'search', label: 'Tìm kiếm', placeholder: 'Nhập MSSV', iconLeft: 'search' })}
        ${Input.render({ name: 'password', type: 'password', label: 'Mật khẩu', iconLeft: 'lock', autocomplete: 'current-password', required: true })}
        ${Input.render({ name: 'mssv', label: 'MSSV', value: 'SV0O1', error: 'MSSV phải gồm 6 chữ số' })}
        ${Input.render({ name: 'classCode', label: 'Mã lớp (vô hiệu)', value: 'CNTT-K20', disabled: true })}
      </div>
      <div class="ui-kit-row-items">
        ${Button.render({ label: 'Kiểm tra form', variant: 'primary', action: 'demo-validate' })}
      </div>
    </div>`;
}

/**
 * Kiểm tra một ô theo RULES. Hiện hoặc xóa lỗi inline.
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
  // Không dùng every() trực tiếp để mọi ô đều được kiểm tra và hiện lỗi cùng lúc
  return inputs.map(validateField).every(Boolean);
}

/**
 * @param {HTMLElement} root
 * @param {string} action
 * @param {HTMLButtonElement} button
 */
function handleAction(root, action, button) {
  if (action === 'demo-save') {
    Button.setLoading(button, true);
    setTimeout(() => Button.setLoading(button, false), DEMO_LOADING_MS);
  }
  if (action === 'demo-validate') {
    validateForm(root);
  }
}

/**
 * @param {HTMLElement} root
 */
function mountPanel(root) {
  root.querySelector('[data-demo="button"]').innerHTML = renderButtonDemo();
  root.querySelector('[data-demo="input"]').innerHTML = renderInputDemo();
  refreshIcons();

  Button.init(root, { onAction: (action, button) => handleAction(root, action, button) });
  Input.init(root, {
    onInput: (input) => {
      // Ô đang lỗi: kiểm tra lại ngay khi người dùng gõ để lỗi biến mất sớm
      if (input.getAttribute('aria-invalid') === 'true') validateField(input);
    },
    onBlur: (input) => validateField(input),
  });
}

document.querySelectorAll('[data-ui-panel]').forEach(mountPanel);
