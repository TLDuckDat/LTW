/**
 * Quản lý ba trạng thái của một vùng dữ liệu: loading (skeleton), empty, error (có nút thử lại).
 * Dùng cho mọi trang để không lặp lại logic trạng thái.
 */

import { escapeHtml, refreshIcons } from './dom.js';
import { Button } from '../components/button.js';

/**
 * @param {unknown} data
 * @returns {boolean}
 */
function isEmptyData(data) {
  if (data === null || data === undefined) return true;
  if (Array.isArray(data)) return data.length === 0;
  if (Array.isArray(data.data)) return data.data.length === 0;
  return false;
}

/**
 * @param {string} title
 * @param {string} message
 * @returns {string}
 */
function errorMarkup(title, message) {
  return `<div class="state-panel" role="alert">
    <span class="state-panel-icon"><i data-lucide="circle-alert" aria-hidden="true"></i></span>
    <h3 class="empty-state-title">${escapeHtml(title)}</h3>
    <p class="empty-state-description">${escapeHtml(message)}</p>
    ${Button.render({ label: 'Thử lại', variant: 'outline', icon: 'refresh-cw', action: 'retry' })}
  </div>`;
}

/**
 * Tải dữ liệu rồi hiển thị skeleton -> nội dung / empty / error.
 * @template T
 * @param {HTMLElement} container
 * @param {{
 *   load: () => Promise<T>,
 *   skeleton: () => string,
 *   render: (data: T) => string,
 *   empty: (data: T) => string,
 *   isEmpty?: (data: T) => boolean,
 *   errorTitle?: string,
 *   afterRender?: (container: HTMLElement, data: T) => void
 * }} options
 * @returns {Promise<T|undefined>}
 */
export async function renderAsync(container, options) {
  const {
    load,
    skeleton,
    render,
    empty,
    isEmpty = isEmptyData,
    errorTitle = 'Không tải được dữ liệu',
    afterRender,
  } = options;

  container.setAttribute('aria-busy', 'true');
  container.innerHTML = skeleton();

  try {
    const data = await load();
    container.removeAttribute('aria-busy');
    container.innerHTML = isEmpty(data) ? empty(data) : render(data);
    refreshIcons();
    afterRender?.(container, data);
    return data;
  } catch (error) {
    container.removeAttribute('aria-busy');
    container.innerHTML = errorMarkup(errorTitle, error?.message ?? 'Đã có lỗi xảy ra');
    refreshIcons();
    container.querySelector('[data-action="retry"]')?.addEventListener('click', () => renderAsync(container, options));
    return undefined;
  }
}
