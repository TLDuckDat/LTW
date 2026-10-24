/**
 * Toast: góc trên phải, tự đóng sau 4s, tạm dừng khi hover/focus, tối đa 4 thông báo.
 * API: Toast.success(msg), Toast.error(msg), Toast.warning(msg), Toast.info(msg)
 */

import { el, refreshIcons } from '../core/dom.js';

const DURATION_MS = 4000;
const MAX_VISIBLE = 4;
const LEAVE_MS = 200;

const ICONS = {
  success: 'circle-check',
  warning: 'triangle-alert',
  error: 'circle-x',
  info: 'info',
};

/** @type {WeakMap<HTMLElement, { remaining: number, startedAt: number, timerId: number|null, paused: boolean }>} */
const timers = new WeakMap();

function getContainer() {
  let container = document.querySelector('.toast-container');
  if (!container) {
    container = el('div', { class: 'toast-container', 'aria-live': 'polite' });
    document.body.append(container);
  }
  return container;
}

/**
 * @param {HTMLElement} toast
 */
function dismiss(toast) {
  const state = timers.get(toast);
  if (state && state.timerId !== null) clearTimeout(state.timerId);
  if (toast.classList.contains('is-leaving')) return;
  toast.classList.add('is-leaving');
  setTimeout(() => toast.remove(), LEAVE_MS);
}

/**
 * @param {HTMLElement} toast
 */
function resume(toast) {
  const state = timers.get(toast);
  if (!state || !state.paused) return;
  state.paused = false;
  state.startedAt = Date.now();
  state.timerId = window.setTimeout(() => dismiss(toast), Math.max(state.remaining, 0));
}

/**
 * @param {HTMLElement} toast
 */
function pause(toast) {
  const state = timers.get(toast);
  if (!state || state.paused) return;
  state.paused = true;
  clearTimeout(state.timerId);
  state.remaining -= Date.now() - state.startedAt;
}

/**
 * @param {'success'|'warning'|'error'|'info'} type
 * @param {string} message
 * @param {{ duration?: number }} [options]
 * @returns {HTMLElement}
 */
function show(type, message, { duration = DURATION_MS } = {}) {
  const container = getContainer();
  const visible = Array.from(container.children).filter((node) => !node.classList.contains('is-leaving'));
  if (visible.length >= MAX_VISIBLE) dismiss(visible[0]);

  const toast = el(
    'div',
    { class: `toast toast-${type}`, role: type === 'error' ? 'alert' : 'status' },
    [
      el('i', { 'data-lucide': ICONS[type], class: 'toast-icon', 'aria-hidden': 'true' }),
      el('div', { class: 'toast-message' }, message),
      el(
        'button',
        {
          type: 'button',
          class: 'btn btn-ghost btn-sm btn-icon-only toast-close',
          'aria-label': 'Đóng thông báo',
          onclick: () => dismiss(toast),
        },
        [el('i', { 'data-lucide': 'x', class: 'btn-icon', 'aria-hidden': 'true' })],
      ),
    ],
  );

  container.append(toast);
  refreshIcons();

  timers.set(toast, { remaining: duration, startedAt: Date.now(), timerId: null, paused: true });
  resume(toast);

  toast.addEventListener('mouseenter', () => pause(toast));
  toast.addEventListener('mouseleave', () => resume(toast));
  toast.addEventListener('focusin', () => pause(toast));
  toast.addEventListener('focusout', () => resume(toast));

  return toast;
}

export const Toast = {
  show,
  success: (message, options) => show('success', message, options),
  warning: (message, options) => show('warning', message, options),
  error: (message, options) => show('error', message, options),
  info: (message, options) => show('info', message, options),
};
