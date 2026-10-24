/**
 * Button component.
 * - render(props) -> HTML string
 * - init(rootEl, props) -> gắn event delegation lên container cha
 * - setLoading(buttonEl, isLoading) -> bật/tắt trạng thái loading trên nút đã render
 */

import { el, escapeHtml } from '../core/dom.js';

const VARIANTS = ['primary', 'secondary', 'outline', 'ghost', 'destructive'];
const SIZES = ['sm', 'md', 'lg'];

/**
 * @typedef {Object} ButtonProps
 * @property {string} [label]
 * @property {'primary'|'secondary'|'outline'|'ghost'|'destructive'} [variant='primary']
 * @property {'sm'|'md'|'lg'} [size='md']
 * @property {'button'|'submit'|'reset'} [type='button']
 * @property {string} [icon] tên icon Lucide, vd: 'plus'
 * @property {'left'|'right'} [iconPosition='left']
 * @property {boolean} [iconOnly=false] chỉ hiện icon (bắt buộc có ariaLabel)
 * @property {boolean} [loading=false]
 * @property {boolean} [disabled=false]
 * @property {boolean} [block=false] chiếm full chiều ngang
 * @property {string} [ariaLabel]
 * @property {string} [action] giá trị data-action dùng trong init()
 * @property {string} [id]
 */

/**
 * @param {string} value
 * @param {string[]} allowed
 * @param {string} fallback
 * @returns {string}
 */
function pick(value, allowed, fallback) {
  if (allowed.includes(value)) return value;
  console.warn(`[Button] Giá trị "${value}" không hợp lệ, dùng "${fallback}"`);
  return fallback;
}

/**
 * @param {ButtonProps} props
 * @returns {string}
 */
function render(props = {}) {
  const {
    label = '',
    type = 'button',
    icon = null,
    iconPosition = 'left',
    iconOnly = false,
    loading = false,
    disabled = false,
    block = false,
    ariaLabel,
    action,
    id,
  } = props;

  const variant = pick(props.variant ?? 'primary', VARIANTS, 'primary');
  const size = pick(props.size ?? 'md', SIZES, 'md');

  if (iconOnly && !ariaLabel) {
    console.warn('[Button] iconOnly cần ariaLabel để đọc được bằng trình đọc màn hình');
  }

  const classes = [
    'btn',
    `btn-${variant}`,
    `btn-${size}`,
    iconOnly && 'btn-icon-only',
    block && 'btn-block',
    loading && 'btn-loading',
  ]
    .filter(Boolean)
    .join(' ');

  const iconHtml = icon
    ? `<i data-lucide="${escapeHtml(icon)}" class="btn-icon" aria-hidden="true"></i>`
    : '';
  const spinnerHtml = loading ? '<span class="btn-spinner" aria-hidden="true"></span>' : '';
  const leading = loading ? spinnerHtml : iconPosition === 'left' ? iconHtml : '';
  const trailing = !loading && iconPosition === 'right' ? iconHtml : '';

  const content = iconOnly
    ? leading || iconHtml
    : `${leading}<span class="btn-label">${escapeHtml(label)}</span>${trailing}`;

  const attributes = [
    `type="${escapeHtml(type)}"`,
    `class="${classes}"`,
    id ? `id="${escapeHtml(id)}"` : '',
    action ? `data-action="${escapeHtml(action)}"` : '',
    ariaLabel ? `aria-label="${escapeHtml(ariaLabel)}"` : '',
    loading ? 'aria-busy="true"' : '',
    loading || disabled ? 'disabled' : '',
  ]
    .filter(Boolean)
    .join(' ');

  return `<button ${attributes}>${content}</button>`;
}

/**
 * Gắn event delegation cho mọi nút trong rootEl. Gọi một lần sau khi render.
 * @param {HTMLElement} rootEl
 * @param {{ onAction?: (action: string, button: HTMLButtonElement, event: Event) => void }} props
 */
function init(rootEl, { onAction } = {}) {
  rootEl.addEventListener('click', (event) => {
    const button = event.target.closest('.btn');
    if (!button || !rootEl.contains(button) || button.disabled) return;
    if (onAction && button.dataset.action) {
      onAction(button.dataset.action, button, event);
    }
  });
}

/**
 * Bật/tắt loading trên nút đã có trong DOM mà không cần render lại.
 * @param {HTMLButtonElement} button
 * @param {boolean} isLoading
 */
function setLoading(button, isLoading) {
  button.classList.toggle('btn-loading', isLoading);
  button.disabled = isLoading;
  button.setAttribute('aria-busy', String(isLoading));

  const spinner = button.querySelector('.btn-spinner');
  if (isLoading && !spinner) {
    button.prepend(el('span', { class: 'btn-spinner', 'aria-hidden': 'true' }));
  } else if (!isLoading && spinner) {
    spinner.remove();
  }
}

export const Button = { render, init, setLoading };
