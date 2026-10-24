/**
 * Sidebar: menu chính, thu gọn (lưu trong localStorage), drawer trên mobile.
 * - render({ activePage, collapsed }) -> HTML string
 * - init(shellEl, { onSettings }) -> gọi một lần sau khi render
 * - initialCollapsed() -> trạng thái thu gọn ban đầu (tablet mặc định thu gọn)
 * - toggleDrawer / openDrawer / closeDrawer(shellEl) -> điều khiển drawer mobile
 */

import { ROUTES } from '../core/router.js';
import { Storage } from '../core/storage.js';
import { escapeHtml } from '../core/dom.js';

const COLLAPSED_KEY = 'sidebar-collapsed';
const TABLET_BREAKPOINT = 1024;

const NAV_ITEMS = [
  { route: 'dashboard', label: 'Dashboard', icon: 'layout-dashboard' },
  { route: 'students', label: 'Sinh viên', icon: 'users' },
  { route: 'courses', label: 'Môn học', icon: 'book-open' },
  { route: 'classes', label: 'Lớp học phần', icon: 'school' },
  { route: 'registration', label: 'Đăng ký học phần', icon: 'calendar-check' },
  { route: 'academic', label: 'Danh mục học vụ', icon: 'library', roles: ['Admin', 'PhongDaoTao'] },
  { route: 'accounts', label: 'Tài khoản', icon: 'user-cog', roles: ['Admin'] },
  { route: 'grades', label: 'Điểm', icon: 'clipboard-list' },
];

function isMobile() {
  return window.matchMedia('(max-width: 767px)').matches;
}

function initialCollapsed() {
  return Storage.get(COLLAPSED_KEY, window.innerWidth < TABLET_BREAKPOINT);
}

/**
 * Cập nhật tooltip, aria và nhãn nút thu gọn theo trạng thái hiện tại.
 * @param {HTMLElement} shellEl
 */
function syncCollapsedState(shellEl) {
  const collapsed = shellEl.classList.contains('is-collapsed');
  shellEl.querySelectorAll('.sidebar .nav-link').forEach((link) => {
    link.title = collapsed ? link.querySelector('.nav-label')?.textContent ?? '' : '';
  });
  const toggle = shellEl.querySelector('[data-action="toggle-collapse"]');
  if (toggle) {
    toggle.setAttribute('aria-expanded', String(!collapsed));
    toggle.setAttribute('aria-label', collapsed ? 'Mở rộng menu' : 'Thu gọn menu');
  }
}

/**
 * @param {{ activePage: string, collapsed: boolean }} props
 * @returns {string}
 */
function render({ activePage, collapsed, vaiTro }) {
  const links = NAV_ITEMS.filter((item) => !item.roles || item.roles.includes(vaiTro)).map((item) => {
    const current = item.route === activePage ? ' aria-current="page"' : '';
    return `<a class="nav-link" href="${ROUTES[item.route]}"${current}><i data-lucide="${item.icon}" aria-hidden="true"></i><span class="nav-label">${escapeHtml(item.label)}</span></a>`;
  }).join('');

  return `<aside class="sidebar" id="app-sidebar" aria-label="Menu chính">
    <div class="sidebar-brand">
      <span class="sidebar-brand-mark" aria-hidden="true">S</span>
      <span class="sidebar-brand-text">StuM</span>
    </div>
    <nav class="sidebar-nav" aria-label="Điều hướng">
      ${links}
      <button type="button" class="nav-link" data-action="settings"><i data-lucide="settings" aria-hidden="true"></i><span class="nav-label">Cài đặt</span></button>
    </nav>
    <div class="sidebar-footer">
      <button type="button" class="btn btn-ghost btn-sm btn-icon-only sidebar-collapse-toggle" data-action="toggle-collapse" aria-controls="app-sidebar" aria-expanded="${String(!collapsed)}" aria-label="${collapsed ? 'Mở rộng menu' : 'Thu gọn menu'}"><i data-lucide="panel-left-close" class="sidebar-collapse-icon btn-icon" aria-hidden="true"></i></button>
    </div>
  </aside>`;
}

/**
 * @param {HTMLElement} shellEl
 */
function openDrawer(shellEl) {
  shellEl.classList.add('is-drawer-open');
  shellEl.querySelector('.topbar-menu-toggle')?.setAttribute('aria-expanded', 'true');
}

/**
 * @param {HTMLElement} shellEl
 */
function closeDrawer(shellEl) {
  shellEl.classList.remove('is-drawer-open');
  shellEl.querySelector('.topbar-menu-toggle')?.setAttribute('aria-expanded', 'false');
}

/**
 * @param {HTMLElement} shellEl
 */
function toggleDrawer(shellEl) {
  if (shellEl.classList.contains('is-drawer-open')) {
    closeDrawer(shellEl);
  } else {
    openDrawer(shellEl);
  }
}

/**
 * @param {HTMLElement} shellEl
 * @param {{ onSettings?: () => void }} [props]
 */
function init(shellEl, { onSettings } = {}) {
  syncCollapsedState(shellEl);

  shellEl.addEventListener('click', (event) => {
    const collapseToggle = event.target.closest('[data-action="toggle-collapse"]');
    if (collapseToggle && shellEl.contains(collapseToggle)) {
      const collapsed = !shellEl.classList.contains('is-collapsed');
      shellEl.classList.toggle('is-collapsed', collapsed);
      Storage.set(COLLAPSED_KEY, collapsed);
      syncCollapsedState(shellEl);
      return;
    }

    if (event.target.closest('.sidebar [data-action="settings"]')) {
      if (isMobile()) closeDrawer(shellEl);
      onSettings?.();
      return;
    }

    if (event.target.closest('.sidebar .nav-link') && isMobile()) {
      closeDrawer(shellEl);
      return;
    }

    if (event.target.closest('.drawer-overlay')) {
      closeDrawer(shellEl);
    }
  });

  document.addEventListener('keydown', (event) => {
    if (event.key === 'Escape' && shellEl.classList.contains('is-drawer-open')) {
      closeDrawer(shellEl);
    }
  });
}

export const Sidebar = {
  render,
  init,
  initialCollapsed,
  openDrawer,
  closeDrawer,
  toggleDrawer,
};
