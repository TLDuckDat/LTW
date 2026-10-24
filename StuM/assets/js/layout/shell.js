/**
 * Shell: render layout tổng (sidebar + topbar + vùng nội dung #page-content).
 * - mountShell(container, { page, title }) -> trả về phần tử #page-content
 * - renderPageHeader({ title, description, actions }) -> HTML tiêu đề trang
 * - openSettings() -> hộp thoại cài đặt (theme, đăng xuất)
 */

import { api } from '../core/api.js';
import { nextId, refreshIcons, escapeHtml } from '../core/dom.js';
import { ROUTES, router } from '../core/router.js';
import { theme } from '../core/theme.js';
import { Button } from '../components/button.js';
import { Modal } from '../components/modal.js';
import { Sidebar } from './sidebar.js';
import { Topbar } from './topbar.js';

/**
 * @param {{ title: string, description?: string, actions?: string }} props
 * @returns {string}
 */
export function renderPageHeader({ title, description = '', actions = '' }) {
  const descriptionHtml = description
    ? `<p class="page-header-description">${escapeHtml(description)}</p>`
    : '';
  const actionsHtml = actions ? `<div class="page-header-actions">${actions}</div>` : '';
  return `<div class="page-header">
    <div class="page-header-text">
      <h1 class="h1">${escapeHtml(title)}</h1>
      ${descriptionHtml}
    </div>
    ${actionsHtml}
  </div>`;
}

export function openSettings() {
  const checkboxId = nextId('setting-dark');
  const checked = theme.current() === 'dark' ? ' checked' : '';
  const content = `<div class="modal-form">
    <label class="checkbox-row" for="${checkboxId}"><input type="checkbox" id="${checkboxId}"${checked}><span>Giao diện tối</span></label>
    <p class="form-hint">Lựa chọn được lưu trên trình duyệt này.</p>
  </div>`;
  const footer = [
    Button.render({ label: 'Đăng xuất', variant: 'outline', icon: 'log-out', action: 'logout' }),
    Button.render({ label: 'Đóng', variant: 'primary', action: 'close' }),
  ].join('');

  const handle = Modal.open({ title: 'Cài đặt', content, footer });
  handle.element
    .querySelector(`#${checkboxId}`)
    .addEventListener('change', (event) => theme.apply(event.target.checked ? 'dark' : 'light'));
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);
  handle.element.querySelector('[data-action="logout"]').addEventListener('click', async () => {
    await api.auth.logout();
    router.navigate('login');
  });
}

/**
 * @param {HTMLElement} container
 * @param {{ page: string, title: string }} meta
 * @returns {HTMLElement} phần tử #page-content để trang vẽ nội dung
 */
export function mountShell(container, { page, title }) {
  const collapsed = Sidebar.initialCollapsed();
  const breadcrumb = [{ label: 'StuM', href: ROUTES.dashboard }, { label: title }];

  container.innerHTML = `
    <a class="skip-link" href="#page-content">Bỏ qua và đi tới nội dung chính</a>
    <div class="app-shell${collapsed ? ' is-collapsed' : ''}" data-shell>
      ${Sidebar.render({ activePage: page, collapsed })}
      <div class="drawer-overlay" data-action="close-drawer"></div>
      <div class="app-main">
        ${Topbar.render({ breadcrumb, user: api.auth.getSession() })}
        <main class="app-content" id="page-content" tabindex="-1"></main>
      </div>
    </div>`;

  const shell = container.querySelector('[data-shell]');
  Sidebar.init(shell, { onSettings: openSettings });
  Topbar.init(shell.querySelector('.topbar'), {
    onToggleDrawer: () => Sidebar.toggleDrawer(shell),
    onSettings: openSettings,
  });
  refreshIcons();

  return shell.querySelector('#page-content');
}
