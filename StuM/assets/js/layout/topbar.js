/**
 * Topbar: breadcrumb, tìm kiếm sinh viên, đổi theme, thông báo (từ backend), menu tài khoản.
 * - render({ breadcrumb, user }) -> HTML string
 * - init(topbarEl, { onToggleDrawer, onSettings }) -> gọi một lần sau khi render
 */

import { api } from '../core/api.js';
import { escapeHtml, refreshIcons } from '../core/dom.js';
import { formatDate } from '../core/format.js';
import { renderAsync } from '../core/async-view.js';
import { router } from '../core/router.js';
import { theme } from '../core/theme.js';
import { Dropdown } from '../components/dropdown.js';
import { EmptyState } from '../components/empty-state.js';
import { Skeleton } from '../components/skeleton.js';

/**
 * @param {Array<{ label: string, href?: string }>} items
 * @returns {string}
 */
function renderBreadcrumb(items) {
  const parts = items
    .map((item, index) => {
      const isLast = index === items.length - 1;
      if (isLast) return `<li aria-current="page">${escapeHtml(item.label)}</li>`;
      return `<li><a href="${escapeHtml(item.href)}">${escapeHtml(item.label)}</a></li>`;
    })
    .join('');
  return `<nav class="topbar-breadcrumb" aria-label="Vị trí hiện tại"><ol class="breadcrumb">${parts}</ol></nav>`;
}

/**
 * @param {'light'|'dark'} mode
 * @returns {string}
 */
function renderThemeButton(mode) {
  const isDark = mode === 'dark';
  return `<button type="button" class="btn btn-ghost btn-sm btn-icon-only" data-action="toggle-theme" aria-pressed="${String(isDark)}" aria-label="Chế độ tối"><i data-lucide="${isDark ? 'sun' : 'moon'}" class="btn-icon" aria-hidden="true"></i></button>`;
}

/**
 * @param {string} name
 * @returns {string}
 */
function initialsOf(name) {
  return name
    .split(' ')
    .filter(Boolean)
    .slice(-2)
    .map((word) => word[0])
    .join('')
    .toUpperCase();
}

/** Vai trò được mở trang Sinh viên (khớp với PAGE_ROLES trong main.js); chỉ các vai trò này thấy ô tìm kiếm. */
const STUDENT_PAGE_ROLES = ['Admin', 'PhongDaoTao', 'GiangVien'];

/**
 * @param {{ vaiTro?: string } | null} user
 * @returns {string}
 */
function renderSearch(user) {
  if (!STUDENT_PAGE_ROLES.includes(user?.vaiTro)) return '';
  return `<form class="topbar-search" role="search" data-global-search>
      <label class="sr-only" for="global-search">Tìm sinh viên theo MSSV hoặc họ tên</label>
      <div class="input-wrap has-icon-left">
        <i data-lucide="search" class="input-icon input-icon-left" aria-hidden="true"></i>
        <input class="input" id="global-search" type="search" name="globalSearch" placeholder="Tìm sinh viên theo MSSV, họ tên..." autocomplete="off">
      </div>
    </form>`;
}

/**
 * @param {{ breadcrumb: Array<{ label: string, href?: string }>, user?: { name?: string, vaiTro?: string } | null }} props
 * @returns {string}
 */
function render({ breadcrumb, user }) {
  const name = user?.name ?? user?.tenDangNhap ?? 'Người dùng';
  const notifications = Dropdown.render({
    label: 'Thông báo',
    icon: 'bell',
    triggerHtml: '<i data-lucide="bell" class="btn-icon" aria-hidden="true"></i><span class="notification-dot" aria-hidden="true" hidden></span>',
    triggerClass: 'btn btn-ghost btn-sm btn-icon-only',
    menuHtml: `<div class="dropdown-header">Thông báo mới nhất</div><div data-notifications>${Skeleton.text(3)}</div>`,
    wide: true,
  });
  const userMenu = Dropdown.render({
    label: `Tài khoản: ${name}`,
    triggerHtml: `<span class="avatar" aria-hidden="true">${escapeHtml(initialsOf(name))}</span><span class="text-truncate">${escapeHtml(name)}</span>`,
    triggerClass: 'btn btn-ghost btn-sm topbar-user-trigger',
    items: [
      { action: 'settings', label: 'Cài đặt', icon: 'settings' },
      { divider: true },
      { action: 'logout', label: 'Đăng xuất', icon: 'log-out', danger: true },
    ],
  });

  return `<header class="topbar">
    <button type="button" class="btn btn-ghost btn-sm btn-icon-only topbar-menu-toggle" data-action="toggle-drawer" aria-controls="app-sidebar" aria-expanded="false" aria-label="Mở menu"><i data-lucide="menu" class="btn-icon" aria-hidden="true"></i></button>
    ${renderBreadcrumb(breadcrumb)}
    ${renderSearch(user)}
    <div class="topbar-actions">
      ${renderThemeButton(theme.current())}
      ${notifications}
      ${userMenu}
    </div>
  </header>`;
}

/**
 * @param {HTMLElement} topbarEl
 * @param {'light'|'dark'} mode
 */
function syncThemeButton(topbarEl, mode) {
  const button = topbarEl.querySelector('[data-action="toggle-theme"]');
  if (!button) return;
  button.setAttribute('aria-pressed', String(mode === 'dark'));
  button.innerHTML = `<i data-lucide="${mode === 'dark' ? 'sun' : 'moon'}" class="btn-icon" aria-hidden="true"></i>`;
  refreshIcons();
}

/**
 * Thông báo lấy từ backend (module Reports, ThongBao). Mục mới nhất hiển thị trước.
 * @param {HTMLElement} topbarEl
 */
function loadNotifications(topbarEl) {
  const list = topbarEl.querySelector('[data-notifications]');
  const dot = topbarEl.querySelector('.notification-dot');
  if (!list) return;

  renderAsync(list, {
    load: () => api.thongBao.list({ pageSize: 5 }),
    skeleton: () => Skeleton.text(3),
    empty: () => EmptyState.render({ icon: 'bell', title: 'Không có thông báo' }),
    render: (result) =>
      result.data
        .map(
          (item) => `<div class="dropdown-notice" role="note"><span>${escapeHtml(item.tieuDe)}</span><span class="dropdown-notice-meta">${escapeHtml(formatDate(item.ngayDang))}</span></div>`,
        )
        .join(''),
    afterRender: (_, result) => {
      if (dot) dot.hidden = result.data.length === 0;
    },
    errorTitle: 'Không tải được thông báo',
  });
}

/**
 * @param {string} action
 * @param {() => void} onSettings
 */
async function handleUserAction(action, onSettings) {
  if (action === 'settings') {
    onSettings();
  } else if (action === 'logout') {
    await api.auth.logout();
    router.navigate('login');
  }
}

/**
 * @param {HTMLElement} topbarEl
 * @param {{ onToggleDrawer: () => void, onSettings: () => void }} handlers
 */
function init(topbarEl, { onToggleDrawer, onSettings }) {
  topbarEl.addEventListener('click', (event) => {
    if (event.target.closest('[data-action="toggle-drawer"]')) {
      onToggleDrawer();
      return;
    }
    if (event.target.closest('[data-action="toggle-theme"]')) {
      theme.toggle();
    }
  });

  window.addEventListener('stum:theme', (event) => syncThemeButton(topbarEl, event.detail));

  topbarEl.querySelector('[data-global-search]')?.addEventListener('submit', (event) => {
    event.preventDefault();
    const query = new FormData(event.currentTarget).get('globalSearch').trim();
    router.navigate('students', query ? { q: query } : {});
  });

  Dropdown.init(topbarEl, {
    onAction: (action) => handleUserAction(action, onSettings),
  });

  loadNotifications(topbarEl);
}

export const Topbar = { render, init };
