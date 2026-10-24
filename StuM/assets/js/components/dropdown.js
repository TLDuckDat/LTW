/**
 * Dropdown menu: action menu của bảng, user menu, thông báo.
 *
 * Khi mở, menu được chuyển ra cuối <body> và định vị cố định theo nút mở. Nhờ vậy không bị cắt
 * bởi vùng cuộn của bảng (overflow) hay lớp phủ của hộp thoại. Khi đóng, menu trả về đúng chỗ cũ.
 *
 * - render(props) -> HTML string
 * - init(rootEl, { onAction }) -> đăng ký nhận sự kiện chọn mục menu của các nút nằm trong rootEl
 * - initGlobal() -> mở/đóng, click ra ngoài, ESC, mũi tên lên/xuống, định vị lại khi cuộn (gọi một lần)
 * - closeAll({ restoreFocus }) -> đóng menu đang mở
 */

import { escapeHtml, nextId } from '../core/dom.js';

const MENU_GAP = 4;
const VIEWPORT_PADDING = 8;

let openMenu = null;
let openTrigger = null;
let globalBound = false;

/** Các root đã đăng ký nhận sự kiện: { rootEl, onAction }. */
const owners = [];

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
 *   className?: string,
 *   triggerClass?: string
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
  const menuId = nextId('dropdown');

  return `<div class="dropdown${className ? ` ${escapeHtml(className)}` : ''}">
    <button type="button" class="${triggerClass}" data-dropdown-trigger aria-haspopup="${role === 'menu' ? 'menu' : 'true'}" aria-expanded="false" aria-controls="${menuId}" aria-label="${escapeHtml(label)}">${trigger}</button>
    <div class="${menuClasses}" id="${menuId}" role="${role}" aria-label="${escapeHtml(label)}" hidden${idAttr}>${menuHtml ?? renderItems(items)}</div>
  </div>`;
}

/**
 * Định vị menu theo nút mở (tọa độ khung nhìn). Lật lên trên nếu không đủ chỗ bên dưới.
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
 * Đưa menu về đúng vị trí trong DOM ban đầu (hoặc gỡ bỏ nếu vị trí đó không còn).
 * @param {HTMLElement} menu
 */
function returnHome(menu) {
  const home = menu.__home;
  menu.__home = null;
  if (!home) return;
  if (home.parent.isConnected) {
    const next = home.next && home.next.parentNode === home.parent ? home.next : null;
    home.parent.insertBefore(menu, next);
  } else {
    menu.remove();
  }
}

/**
 * Đóng menu đang mở.
 * @param {{ restoreFocus?: boolean }} [options]
 */
function closeAll({ restoreFocus = false } = {}) {
  if (!openMenu) return;
  const menu = openMenu;
  const trigger = openTrigger;
  openMenu = null;
  openTrigger = null;
  menu.hidden = true;
  menu.style.position = '';
  menu.style.top = '';
  menu.style.left = '';
  menu.style.right = '';
  returnHome(menu);
  trigger.setAttribute('aria-expanded', 'false');
  if (restoreFocus) trigger.focus();
}

/**
 * @param {HTMLElement} trigger
 * @param {HTMLElement} menu
 */
function openFor(trigger, menu) {
  closeAll();
  menu.__home = { parent: menu.parentNode, next: menu.nextSibling };
  document.body.append(menu);
  menu.hidden = false;
  menu.__trigger = trigger;
  trigger.setAttribute('aria-expanded', 'true');
  position(menu, trigger);
  openMenu = menu;
  openTrigger = trigger;
  // preventScroll: focus không được cuộn trang, nếu không sự kiện cuộn sẽ đóng menu ngay khi vừa mở
  menu.querySelector('[role="menuitem"], a, button')?.focus({ preventScroll: true });
}

/** @param {string} controlsId */
function menuFor(trigger) {
  return document.getElementById(trigger.getAttribute('aria-controls'));
}

function handleClick(event) {
  const trigger = event.target.closest('[data-dropdown-trigger]');
  if (trigger) {
    const menu = menuFor(trigger);
    const wasOpen = menu && openMenu === menu;
    closeAll();
    if (menu && !wasOpen) openFor(trigger, menu);
    return;
  }

  const item = event.target.closest('[data-dropdown-item]');
  if (item && item.closest('.dropdown-menu')) {
    const menu = item.closest('.dropdown-menu');
    const owner = menu.__trigger ? owners.find((o) => o.rootEl.contains(menu.__trigger)) : null;
    closeAll({ restoreFocus: true });
    owner?.onAction?.(item.dataset.action, menu.dataset.id, item, event);
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
  // Menu dùng position: fixed nên khi trang cuộn thì định vị lại theo nút mở;
  // chỉ đóng khi nút mở đã cuộn ra khỏi màn hình.
  document.addEventListener(
    'scroll',
    () => {
      if (!openMenu) return;
      const rect = openTrigger.getBoundingClientRect();
      if (rect.bottom < 0 || rect.top > window.innerHeight) {
        closeAll();
      } else {
        position(openMenu, openTrigger);
      }
    },
    true,
  );
}

/**
 * Đăng ký nhận sự kiện chọn mục menu của các nút nằm trong rootEl.
 * @param {HTMLElement} rootEl
 * @param {{ onAction?: (action: string, rowId: string|undefined, item: HTMLElement, event: Event) => void }} [props]
 */
function init(rootEl, { onAction } = {}) {
  owners.push({ rootEl, onAction });
}

export const Dropdown = { render, init, initGlobal, closeAll };
