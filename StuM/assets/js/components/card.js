/**
 * Card: header (tiêu đề, phụ đề, action menu), body, footer.
 * - render(props) -> HTML string
 * Lưu ý: body và footer là HTML đã được tạo/escape bởi bên gọi (không nhận dữ liệu thô).
 */

import { escapeHtml } from '../core/dom.js';
import { Dropdown } from './dropdown.js';

/**
 * @param {{
 *   title?: string,
 *   subtitle?: string,
 *   body?: string,
 *   footer?: string,
 *   actions?: Array<{ action: string, label: string, icon?: string, danger?: boolean, divider?: boolean }>,
 *   flush?: boolean,
 *   titleTag?: string,
 *   ariaLabel?: string
 * }} props
 * @returns {string}
 */
function render({
  title = '',
  subtitle = '',
  body = '',
  footer = '',
  actions = [],
  flush = false,
  titleTag = 'h3',
  ariaLabel = '',
}) {
  const hasHeader = Boolean(title || subtitle || actions.length);
  const headerHtml = hasHeader
    ? `<header class="card-header">
        <div class="card-header-text">
          ${title ? `<${titleTag} class="card-title">${escapeHtml(title)}</${titleTag}>` : ''}
          ${subtitle ? `<p class="card-subtitle">${escapeHtml(subtitle)}</p>` : ''}
        </div>
        ${actions.length ? Dropdown.render({ label: 'Thao tác của thẻ', items: actions }) : ''}
      </header>`
    : '';
  const footerHtml = footer ? `<footer class="card-footer">${footer}</footer>` : '';
  const labelAttr = ariaLabel ? ` aria-label="${escapeHtml(ariaLabel)}"` : '';

  return `<section class="card"${labelAttr}>
    ${headerHtml}
    <div class="card-body${flush ? ' is-flush' : ''}">${body}</div>
    ${footerHtml}
  </section>`;
}

export const Card = { render };
