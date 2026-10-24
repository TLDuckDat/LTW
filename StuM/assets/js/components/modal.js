/**
 * Modal dùng chung: form modal và confirm dialog.
 * - open({ title, content, footer, size, dismissible, onClose }) -> { element, close }
 * - confirm({ title, message, confirmText, cancelText, danger, onConfirm }) -> Promise<boolean>
 * Đóng bằng nút X, phím ESC, click nền. Focus được giữ trong dialog (focus trap).
 */

import { escapeHtml, nextId, refreshIcons } from '../core/dom.js';
import { Button } from './button.js';

const FOCUSABLE_SELECTOR = [
  'a[href]',
  'button:not([disabled])',
  'input:not([disabled]):not([type="hidden"])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
].join(', ');
const LEAVE_MS = 150;

/** Các modal đang mở, phần tử cuối là modal trên cùng. */
const stack = [];

/**
 * @param {HTMLElement} container
 * @returns {HTMLElement[]}
 */
function getFocusable(container) {
  return Array.from(container.querySelectorAll(FOCUSABLE_SELECTOR)).filter(
    (node) => node.getClientRects().length > 0,
  );
}

/**
 * @param {KeyboardEvent} event
 */
function handleKeydown(event) {
  const top = stack[stack.length - 1];
  if (!top) return;

  if (event.key === 'Escape' && top.dismissible) {
    event.preventDefault();
    top.close();
    return;
  }

  if (event.key === 'Tab') {
    const items = getFocusable(top.element);
    if (items.length === 0) return;
    const first = items[0];
    const last = items[items.length - 1];

    if (!top.element.contains(document.activeElement)) {
      event.preventDefault();
      first.focus();
    } else if (event.shiftKey && document.activeElement === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault();
      first.focus();
    }
  }
}

document.addEventListener('keydown', handleKeydown);

/**
 * @param {HTMLElement} target
 * @param {string|Node} value
 */
function mountContent(target, value) {
  if (value instanceof Node) {
    target.append(value);
  } else if (value) {
    target.innerHTML = value;
  }
}

/**
 * @param {{ title: string, content: string|Node, footer: string|Node, size: string, dismissible: boolean }} options
 * @returns {HTMLElement}
 */
function createBackdrop({ title, content, footer, size, dismissible }) {
  const titleId = nextId('modal-title');
  const backdrop = document.createElement('div');
  backdrop.className = 'modal-backdrop';
  backdrop.innerHTML = `
    <div class="modal${size === 'lg' ? ' modal-lg' : ''}" role="dialog" aria-modal="true" aria-labelledby="${titleId}" tabindex="-1">
      <div class="modal-header">
        <h2 class="modal-title" id="${titleId}">${escapeHtml(title)}</h2>
        ${dismissible ? '<button type="button" class="btn btn-ghost btn-sm btn-icon-only" data-modal-close aria-label="Đóng"><i data-lucide="x" class="btn-icon" aria-hidden="true"></i></button>' : ''}
      </div>
      <div class="modal-body"></div>
      ${footer ? '<div class="modal-footer"></div>' : ''}
    </div>`;

  mountContent(backdrop.querySelector('.modal-body'), content);
  if (footer) mountContent(backdrop.querySelector('.modal-footer'), footer);
  return backdrop;
}

/**
 * Mở modal.
 * @param {{
 *   title?: string,
 *   content?: string|Node,
 *   footer?: string|Node,
 *   size?: 'md'|'lg',
 *   dismissible?: boolean,
 *   onClose?: () => void
 * }} [options]
 * @returns {{ element: HTMLElement, close: () => void }}
 */
function open({ title = '', content = '', footer = '', size = 'md', dismissible = true, onClose } = {}) {
  const previousFocus = document.activeElement;
  const backdrop = createBackdrop({ title, content, footer, size, dismissible });
  const dialog = backdrop.querySelector('.modal');
  let closed = false;

  const entry = { element: dialog, dismissible, close: null };

  function close() {
    if (closed) return;
    closed = true;
    const index = stack.indexOf(entry);
    if (index !== -1) stack.splice(index, 1);
    if (stack.length === 0) document.body.classList.remove('modal-open');

    backdrop.classList.remove('is-open');
    setTimeout(() => backdrop.remove(), LEAVE_MS);

    if (previousFocus instanceof HTMLElement && previousFocus.isConnected) {
      previousFocus.focus();
    }
    onClose?.();
  }

  entry.close = close;
  stack.push(entry);
  document.body.classList.add('modal-open');
  document.body.append(backdrop);
  refreshIcons();

  backdrop.addEventListener('click', (event) => {
    if (event.target === backdrop && dismissible) close();
  });
  backdrop.querySelector('[data-modal-close]')?.addEventListener('click', close);

  requestAnimationFrame(() => {
    backdrop.classList.add('is-open');
    const firstField = getFocusable(dialog).find((node) => !node.hasAttribute('data-modal-close'));
    (firstField ?? dialog).focus();
  });

  return { element: dialog, close };
}

/**
 * Hộp thoại xác nhận cho thao tác xóa, khóa, hủy.
 * Nếu onConfirm ném lỗi, modal giữ nguyên, tắt loading và promise bị reject để bên gọi xử lý.
 * @param {{
 *   title: string,
 *   message: string,
 *   confirmText?: string,
 *   cancelText?: string,
 *   danger?: boolean,
 *   onConfirm?: () => Promise<unknown>|unknown
 * }} options
 * @returns {Promise<boolean>}
 */
function confirm({
  title,
  message,
  confirmText = 'Xác nhận',
  cancelText = 'Hủy',
  danger = false,
  onConfirm,
}) {
  return new Promise((resolve, reject) => {
    const footer = [
      Button.render({ label: cancelText, variant: 'outline', action: 'cancel' }),
      Button.render({ label: confirmText, variant: danger ? 'destructive' : 'primary', action: 'confirm' }),
    ].join('');
    let settled = false;

    const handle = open({
      title,
      content: `<p class="modal-message">${escapeHtml(message)}</p>`,
      footer,
      onClose: () => {
        if (!settled) {
          settled = true;
          resolve(false);
        }
      },
    });

    const confirmButton = handle.element.querySelector('[data-action="confirm"]');
    const cancelButton = handle.element.querySelector('[data-action="cancel"]');

    cancelButton.addEventListener('click', () => handle.close());
    confirmButton.addEventListener('click', async () => {
      Button.setLoading(confirmButton, true);
      cancelButton.disabled = true;
      try {
        await onConfirm?.();
        settled = true;
        resolve(true);
        handle.close();
      } catch (error) {
        Button.setLoading(confirmButton, false);
        cancelButton.disabled = false;
        reject(error);
      }
    });
  });
}

export const Modal = { open, confirm };
