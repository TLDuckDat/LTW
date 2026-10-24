/**
 * Dropdown menu: action menu của bảng, user menu, thông báo.
 * Menu dùng position: fixed để không bị cắt bởi vùng cuộn của bảng.
 * - render(props) -> HTML string
 * - init(rootEl, { onAction }) -> bắt click mục menu trong rootEl (gọi một lần cho mỗi root)
 * - initGlobal() -> mở/đóng, click ra ngoài, ESC, mũi tên lên/xuống (gọi một lần trong main)
 * - closeAll() -> đóng menu đang mở
 */

import { escapeHtml } from '../core/dom.js';

const MENU_GAP = 4;
const VIEWPORT_PADDING = 8;

let openMenu = null;
let openTrigger = null;
let globalBound = false;

/**
 * @typedef {Object} DropdownItem
 * @property {string} [action]
 * @property {string} [label]
 * @property {string} [icon]
 * @property {boolean} [danger]
 * @property {boolean} [divider]
 */

/**
 * @param {DropdownItem[]} items
 * @returns {string}
 */
function renderItems(items) {
  return items
    .map((item) => {
      if (item.divider) return '<div class="dropdown-divider" role="separator"></div>';
      const icon = item.icon ? `<i data-lucide="${escapeHtml(item.icon)}" aria-hidden="true"></i>` : '';
      const danger = item.danger ? ' is-danger' : '';
      return `<button type="button" role="menuitem" class="dropdown-item${danger}" data-dropdown-item data-action="${escapeHtml(item.action)}">${icon}<span>${escapeHtml(item.label)}</span></button>`;
    })
    .join('');
}

/**
 * @param {{
 *   label: string,
 *   icon?: string,
 *   triggerHtml?: string,
 *   items?: DropdownItem[],
 *   menuHtml?: string,
 *   align?: 'right'|'left',
 *   wide?: boolean,
 *   rowId?: string|number,
 *   className?: string
 * }} props
 * @returns {string}
 */
function render({
  label,
  icon = 'ellipsis-vertical',
  triggerHtml,
  items = [],
  menuHtml,
  align = 'right',
  wide = false,
  rowId,
  className = '',
  triggerClass: triggerClassName,
}) {
  const trigger = triggerHtml ?? `<i data-lucide="${escapeHtml(icon)}" aria-hidden="true"></i>`;
  const triggerClass =
    triggerClassName ?? (triggerHtml ? 'btn btn-ghost btn-sm' : 'btn btn-ghost btn-sm btn-icon-only');
  const menuClasses = ['dropdown-menu', align === 'left' ? 'align-left' : '', wide ? 'is-wide' : '']
    .filter(Boolean)
    .join(' ');
  const role = menuHtml ? 'region' : 'menu';
  const idAttr = rowId !== undefined ? ` data-id="${escapeHtml(rowId)}"` : '';

  return `<div class="dropdown${className ? ` ${escapeHtml(className)}` : ''}">
    <button type="button" class="${triggerClass}" data-dropdown-trigger aria-haspopup="${role === 'menu' ? 'menu' : 'true'}" aria-expanded="false" aria-label="${escapeHtml(label)}">${trigger}</button>
    <div class="${menuClasses}" role="${role}" aria-label="${escapeHtml(label)}" hidden${idAttr}>${menuHtml ?? renderItems(items)}</div>
  </div>`;
}

/**
 * Đặt vị trí menu cố định so với nút mở. Lật lên trên nếu không đủ chỗ bên dưới.
 * @param {HTMLElement} menu
 * @param {HTMLElement} trigger
 */
function position(menu, trigger) {
  const rect = trigger.getBoundingClientRect();
  menu.style.position = 'fixed';

  if (menu.classList.contains('align-left')) {
    menu.style.left = `${Math.max(rect.left, VIEWPORT_PADDING)}px`;
    menu.style.right = 'auto';
  } else {
    menu.style.left = 'auto';
    menu.style.right = `${Math.max(window.innerWidth - rect.right, VIEWPORT_PADDING)}px`;
  }

  menu.style.top = `${rect.bottom + MENU_GAP}px`;
  const menuHeight = menu.getBoundingClientRect().height;
  const overflowsBelow = rect.bottom + MENU_GAP + menuHeight > window.innerHeight - VIEWPORT_PADDING;
  if (overflowsBelow && rect.top > menuHeight + MENU_GAP) {
    menu.style.top = `${rect.top - menuHeight - MENU_GAP}px`;
  }
}

/**
 * Đóng menu đang mở.
 * @param {{ restoreFocus?: boolean }} [options]
 */
function closeAll({ restoreFocus = false } = {}) {
  if (!openMenu) return;
  openMenu.hidden = true;
  openTrigger.setAttribute('aria-expanded', 'false');
  if (restoreFocus) openTrigger.focus();
  openMenu = null;
  openTrigger = null;
}

/**
 * @param {HTMLElement} trigger
 * @param {HTMLElement} menu
 */
function openFor(trigger, menu) {
  closeAll();
  menu.hidden = false;
  trigger.setAttribute('aria-expanded', 'true');
  position(menu, trigger);
  openMenu = menu;
  openTrigger = trigger;
  menu.querySelector('[role="menuitem"], a, button')?.focus();
}

function handleClick(event) {
  const trigger = event.target.closest('[data-dropdown-trigger]');
  if (trigger) {
    const menu = trigger.closest('.dropdown')?.querySelector(':scope > .dropdown-menu');
    const wasOpen = menu && openMenu === menu;
    closeAll();
    if (menu && !wasOpen) openFor(trigger, menu);
    return;
  }
  if (openMenu && !event.target.closest('.dropdown-menu')) {
    closeAll();
  }
}

function handleKeydown(event) {
  if (!openMenu) return;

  if (event.key === 'Escape') {
    event.preventDefault();
    closeAll({ restoreFocus: true });
    return;
  }

  if (event.key === 'Tab') {
    closeAll();
    return;
  }

  if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
    const items = Array.from(openMenu.querySelectorAll('[role="menuitem"]'));
    if (items.length === 0) return;
    event.preventDefault();
    const index = items.indexOf(document.activeElement);
    const step = event.key === 'ArrowDown' ? 1 : -1;
    const next = (index + step + items.length) % items.length;
    items[next].focus();
  }
}

/**
 * Gắn xử lý toàn cục. Gọi đúng một lần khi khởi động ứng dụng.
 */
function initGlobal() {
  if (globalBound) return;
  globalBound = true;
  document.addEventListener('click', handleClick);
  document.addEventListener('keydown', handleKeydown);
  window.addEventListener('resize', () => closeAll());
  document.addEventListener('scroll', () => closeAll(), true);
}

/**
 * Bắt click trên mục menu trong rootEl. Gọi một lần cho mỗi root cố định.
 * @param {HTMLElement} rootEl
 * @param {{ onAction?: (action: string, rowId: string|undefined, item: HTMLElement, event: Event) => void }} [props]
 */
function init(rootEl, { onAction } = {}) {
  rootEl.addEventListener('click', (event) => {
    const item = event.target.closest('[data-dropdown-item]');
    if (!item || !rootEl.contains(item)) return;

    const rowId = item.closest('.dropdown-menu')?.dataset.id;
    closeAll({ restoreFocus: true });
    onAction?.(item.dataset.action, rowId, item, event);
  });
}

export const Dropdown = { render, init, initGlobal, closeAll };
