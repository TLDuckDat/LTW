/**
 * Pagination: "Hiển thị 1-10 của 1,420 kết quả", điều hướng trang, chọn 10/25/50/100 dòng.
 * - render({ page, pageSize, total, sizes }) -> HTML string
 * - init(rootEl, { onChange({ page, pageSize }) }) -> gọi một lần trên root cố định
 */

import { escapeHtml } from '../core/dom.js';
import { formatNumber } from '../core/format.js';

const DEFAULT_SIZES = [10, 25, 50, 100];

/**
 * @param {{ page: number, pageSize: number, total: number, sizes?: number[] }} props
 * @returns {string}
 */
function render({ page, pageSize, total, sizes = DEFAULT_SIZES }) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const current = Math.min(Math.max(page, 1), totalPages);
  const from = total === 0 ? 0 : (current - 1) * pageSize + 1;
  const to = Math.min(total, current * pageSize);
  const info = total === 0 ? 'Không có kết quả' : `Hiển thị ${from}-${to} của ${formatNumber(total)} kết quả`;

  const options = sizes
    .map((size) => `<option value="${size}"${size === pageSize ? ' selected' : ''}>${size}</option>`)
    .join('');

  return `<nav class="pagination" aria-label="Phân trang" data-pagination data-page="${current}" data-page-size="${pageSize}" data-total-pages="${totalPages}">
    <p class="pagination-info" aria-live="polite">${escapeHtml(info)}</p>
    <div class="pagination-controls">
      <button type="button" class="btn btn-outline btn-sm btn-icon-only" data-page-nav="prev" aria-label="Trang trước"${current <= 1 ? ' disabled' : ''}><i data-lucide="chevron-left" class="btn-icon" aria-hidden="true"></i></button>
      <span class="pagination-page">Trang ${current} / ${totalPages}</span>
      <button type="button" class="btn btn-outline btn-sm btn-icon-only" data-page-nav="next" aria-label="Trang sau"${current >= totalPages ? ' disabled' : ''}><i data-lucide="chevron-right" class="btn-icon" aria-hidden="true"></i></button>
    </div>
    <div class="pagination-size">
      <span>Số dòng</span>
      <div class="select-wrap">
        <select class="input input-sm select" data-page-size aria-label="Số dòng mỗi trang">${options}</select>
        <i data-lucide="chevron-down" class="select-chevron" aria-hidden="true"></i>
      </div>
    </div>
  </nav>`;
}

/**
 * @param {HTMLElement} rootEl
 * @param {{ onChange?: (state: { page: number, pageSize: number }) => void }} [props]
 */
function init(rootEl, { onChange } = {}) {
  rootEl.addEventListener('click', (event) => {
    const button = event.target.closest('[data-page-nav]');
    if (!button || button.disabled) return;
    const nav = button.closest('[data-pagination]');
    const page = Number(nav.dataset.page);
    const pageSize = Number(nav.dataset.pageSize);
    const next = button.dataset.pageNav === 'prev' ? page - 1 : page + 1;
    onChange?.({ page: next, pageSize });
  });

  rootEl.addEventListener('change', (event) => {
    if (!event.target.matches('[data-page-size]')) return;
    onChange?.({ page: 1, pageSize: Number(event.target.value) });
  });
}

export const Pagination = { render, init };
