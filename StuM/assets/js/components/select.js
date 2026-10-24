/**
 * Select: dropdown native, hoặc searchable (input + datalist) khi searchable = true.
 * - render(props) -> HTML string gồm label, control, hint và vùng lỗi (dùng chung với Input.setError)
 * - getValue(element, options) -> giá trị đang chọn (với searchable: khớp theo nhãn)
 * - init(rootEl, { onChange }) -> gọi một lần trên root cố định
 */

import { escapeHtml, nextId } from '../core/dom.js';

/**
 * @typedef {{ value: string|number, label: string }} SelectOption
 */

/**
 * @param {SelectOption[]} options
 * @param {string|number} value
 * @param {string} placeholder
 * @returns {string}
 */
function renderOptions(options, value, placeholder) {
  const placeholderOption = placeholder
    ? `<option value="" disabled${value === '' ? ' selected' : ''}>${escapeHtml(placeholder)}</option>`
    : '';
  const items = options
    .map(
      (option) =>
        `<option value="${escapeHtml(option.value)}"${String(option.value) === String(value) ? ' selected' : ''}>${escapeHtml(option.label)}</option>`,
    )
    .join('');
  return placeholderOption + items;
}

/**
 * @param {{
 *   id?: string, name?: string, label?: string, options?: SelectOption[], value?: string|number,
 *   required?: boolean, disabled?: boolean, searchable?: boolean, placeholder?: string,
 *   hint?: string, error?: string
 * }} props
 * @returns {string}
 */
function render({
  id,
  name,
  label = '',
  options = [],
  value = '',
  required = false,
  disabled = false,
  searchable = false,
  placeholder = '',
  hint,
  error,
}) {
  const fieldId = escapeHtml(id ?? nextId('select'));
  const fieldName = escapeHtml(name || fieldId);
  const describedBy = [hint ? `${fieldId}-hint` : '', `${fieldId}-error`].filter(Boolean).join(' ');
  const requiredAttr = required ? ' required' : '';
  const disabledAttr = disabled ? ' disabled' : '';
  const invalidAttr = error ? ' aria-invalid="true"' : '';

  let control;
  if (searchable) {
    const current = options.find((option) => String(option.value) === String(value))?.label ?? '';
    const datalistOptions = options.map((option) => `<option value="${escapeHtml(option.label)}"></option>`).join('');
    control = `<div class="input-wrap">
        <input class="input" id="${fieldId}" name="${fieldName}" type="text" list="${fieldId}-list" value="${escapeHtml(current)}" placeholder="${escapeHtml(placeholder)}" autocomplete="off" aria-describedby="${describedBy}"${requiredAttr}${disabledAttr}${invalidAttr}>
      </div>
      <datalist id="${fieldId}-list">${datalistOptions}</datalist>`;
  } else {
    control = `<div class="select-wrap">
        <select class="input select" id="${fieldId}" name="${fieldName}" aria-describedby="${describedBy}"${requiredAttr}${disabledAttr}${invalidAttr}>${renderOptions(options, value, placeholder)}</select>
        <i data-lucide="chevron-down" class="select-chevron" aria-hidden="true"></i>
      </div>`;
  }

  const labelHtml = label
    ? `<label class="form-label" for="${fieldId}">${escapeHtml(label)}${required ? '<span class="form-required" aria-hidden="true">*</span>' : ''}</label>`
    : '';
  const hintHtml = hint ? `<p class="form-hint" id="${fieldId}-hint">${escapeHtml(hint)}</p>` : '';
  const errorHtml = error
    ? `<p class="form-error" id="${fieldId}-error" aria-live="polite"><i data-lucide="circle-alert" aria-hidden="true"></i><span>${escapeHtml(error)}</span></p>`
    : `<p class="form-error" id="${fieldId}-error" aria-live="polite" hidden></p>`;

  return `<div class="field">${labelHtml}${control}${hintHtml}${errorHtml}</div>`;
}

/**
 * @param {HTMLSelectElement|HTMLInputElement} element
 * @param {SelectOption[]} [options]
 * @returns {string}
 */
function getValue(element, options = []) {
  if (element.tagName === 'SELECT') return element.value;
  return options.find((option) => option.label === element.value)?.value ?? '';
}

/**
 * @param {HTMLElement} rootEl
 * @param {{ onChange?: (element: HTMLElement, event: Event) => void }} [props]
 */
function init(rootEl, { onChange } = {}) {
  rootEl.addEventListener('change', (event) => {
    if (event.target.matches('select.select, input[list]')) {
      onChange?.(event.target, event);
    }
  });
}

export const Select = { render, init, getValue };
