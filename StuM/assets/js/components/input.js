/**
 * Input component (text, email, number, password).
 * - render(props) -> HTML string gồm label, ô nhập, icon, hint và vùng lỗi
 * - init(rootEl, props) -> gắn event delegation: toggle mật khẩu, onInput, onBlur
 * - setError(inputEl, message) / clearError(inputEl) -> validation inline, không render lại
 */

import { escapeHtml, refreshIcons } from '../core/dom.js';

const TYPES = ['text', 'email', 'number', 'password', 'date'];

let uid = 0;

/**
 * @typedef {Object} InputProps
 * @property {string} [id] tự sinh nếu không truyền
 * @property {string} [name]
 * @property {string} [label]
 * @property {'text'|'email'|'number'|'password'} [type='text']
 * @property {string|number} [value='']
 * @property {string} [placeholder]
 * @property {boolean} [required=false]
 * @property {boolean} [disabled=false]
 * @property {string} [autocomplete]
 * @property {string} [iconLeft] tên icon Lucide
 * @property {string} [iconRight] tên icon Lucide
 * @property {boolean} [togglePassword] hiện nút mắt cho type password (mặc định: bật)
 * @property {string} [hint] gợi ý phía dưới ô nhập
 * @property {string} [error] thông báo lỗi ban đầu
 * @property {number} [min]
 * @property {number} [max]
 * @property {number} [step]
 */

function renderLabel(id, label, required) {
  if (!label) return '';
  const requiredMark = required ? '<span class="form-required" aria-hidden="true">*</span>' : '';
  return `<label class="form-label" for="${id}">${escapeHtml(label)}${requiredMark}</label>`;
}

function renderIcon(name, side) {
  return `<i data-lucide="${escapeHtml(name)}" class="input-icon input-icon-${side}" aria-hidden="true"></i>`;
}

function renderToggle() {
  return `
    <button type="button" class="input-toggle" data-toggle-password aria-label="Hiển thị mật khẩu" aria-pressed="false">
      <i data-lucide="eye" class="icon-show" aria-hidden="true"></i>
      <i data-lucide="eye-off" class="icon-hide" aria-hidden="true"></i>
    </button>`;
}

/**
 * @param {InputProps} props
 * @returns {string}
 */
function render(props = {}) {
  const {
    name = '',
    label = '',
    value = '',
    placeholder = '',
    required = false,
    disabled = false,
    autocomplete,
    iconLeft,
    iconRight,
    hint,
    error,
    min,
    max,
    step,
  } = props;

  const id = escapeHtml(props.id ?? `input-${++uid}`);
  const type = TYPES.includes(props.type) ? props.type : 'text';
  if (!TYPES.includes(props.type ?? 'text')) {
    console.warn(`[Input] type "${props.type}" chưa được hỗ trợ, dùng "text"`);
  }

  const showToggle = type === 'password' && (props.togglePassword ?? true);
  const hasError = Boolean(error);

  const wrapClasses = [
    'input-wrap',
    iconLeft && 'has-icon-left',
    (iconRight || showToggle) && 'has-icon-right',
    showToggle && 'has-toggle',
  ]
    .filter(Boolean)
    .join(' ');

  const describedBy = [hint ? `${id}-hint` : '', `${id}-error`].filter(Boolean).join(' ');

  const inputAttributes = [
    `id="${id}"`,
    `name="${escapeHtml(name || id)}"`,
    `type="${type}"`,
    `class="input"`,
    `value="${escapeHtml(value)}"`,
    placeholder ? `placeholder="${escapeHtml(placeholder)}"` : '',
    required ? 'required' : '',
    disabled ? 'disabled' : '',
    autocomplete ? `autocomplete="${escapeHtml(autocomplete)}"` : '',
    min !== undefined ? `min="${escapeHtml(min)}"` : '',
    max !== undefined ? `max="${escapeHtml(max)}"` : '',
    step !== undefined ? `step="${escapeHtml(step)}"` : '',
    `aria-describedby="${describedBy}"`,
    hasError ? 'aria-invalid="true"' : '',
  ]
    .filter(Boolean)
    .join(' ');

  return `
    <div class="field">
      ${renderLabel(id, label, required)}
      <div class="${wrapClasses}">
        ${iconLeft ? renderIcon(iconLeft, 'left') : ''}
        <input ${inputAttributes}>
        ${iconRight && !showToggle ? renderIcon(iconRight, 'right') : ''}
        ${showToggle ? renderToggle() : ''}
      </div>
      ${hint ? `<p class="form-hint" id="${id}-hint">${escapeHtml(hint)}</p>` : ''}
      <p class="form-error" id="${id}-error" aria-live="polite" ${hasError ? '' : 'hidden'}>
        ${hasError ? `<i data-lucide="circle-alert" aria-hidden="true"></i><span>${escapeHtml(error)}</span>` : ''}
      </p>
    </div>`;
}

/**
 * Gắn event delegation cho mọi input trong rootEl. Gọi một lần sau khi render.
 * @param {HTMLElement} rootEl
 * @param {{
 *   onInput?: (input: HTMLInputElement, event: Event) => void,
 *   onBlur?: (input: HTMLInputElement, event: FocusEvent) => void
 * }} props
 */
function init(rootEl, { onInput, onBlur } = {}) {
  rootEl.addEventListener('click', (event) => {
    const toggle = event.target.closest('[data-toggle-password]');
    if (!toggle || !rootEl.contains(toggle)) return;

    const input = toggle.closest('.input-wrap')?.querySelector('input');
    if (!input) return;

    const isRevealed = toggle.getAttribute('aria-pressed') === 'true';
    input.type = isRevealed ? 'password' : 'text';
    toggle.setAttribute('aria-pressed', String(!isRevealed));
  });

  if (onInput) {
    rootEl.addEventListener('input', (event) => {
      if (event.target.matches('.input')) onInput(event.target, event);
    });
  }

  if (onBlur) {
    rootEl.addEventListener('focusout', (event) => {
      if (event.target.matches('.input')) onBlur(event.target, event);
    });
  }
}

/**
 * Hiển thị lỗi dưới ô nhập và đánh dấu aria-invalid.
 * @param {HTMLInputElement} input
 * @param {string} message
 */
function setError(input, message) {
  const errorEl = input.closest('.field')?.querySelector('.form-error');
  if (!errorEl) return;

  input.setAttribute('aria-invalid', 'true');
  errorEl.hidden = false;
  errorEl.innerHTML = `<i data-lucide="circle-alert" aria-hidden="true"></i><span>${escapeHtml(message)}</span>`;
  refreshIcons();
}

/**
 * Xóa lỗi và bỏ đánh dấu aria-invalid.
 * @param {HTMLInputElement} input
 */
function clearError(input) {
  const errorEl = input.closest('.field')?.querySelector('.form-error');
  if (!errorEl) return;

  input.removeAttribute('aria-invalid');
  errorEl.hidden = true;
  errorEl.textContent = '';
}

export const Input = { render, init, setError, clearError };
