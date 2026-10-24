/**
 * Table: sortable header, action dropdown 3 chấm, responsive card view (CSS xử lý < 768px).
 * - render({ columns, rows, rowKey, sort, actions, actionLabel, footer }) -> HTML string
 * - init(rootEl, { onSort, onAction }) -> gọi một lần trên root cố định của trang
 *
 * columns: [{ key, label, sortable?, align?: 'left'|'right', primary?, render?(row) => html }]
 *   - render(row) trả về HTML đã escape (dùng escapeHtml cho dữ liệu người dùng)
 * sort: { key, dir: 'asc'|'desc' } | null
 */

import { escapeHtml } from '../core/dom.js';
import { Dropdown } from './dropdown.js';

const SORT_ICONS = { none: 'arrow-up-down', asc: 'arrow-up', desc: 'arrow-down' };
const ARIA_SORT = { none: 'none', asc: 'ascending', desc: 'descending' };

/**
 * @param {{ key: string }} column
 * @param {{ key: string, dir: string }|null} sort
 * @returns {'none'|'asc'|'desc'}
 */
function sortState(column, sort) {
  if (!sort || sort.key !== column.key) return 'none';
  return sort.dir === 'desc' ? 'desc' : 'asc';
}

function renderHeader(columns, sort, hasActions) {
  const cells = columns.map((column) => {
    const numeric = column.align === 'right' ? ' class="is-numeric"' : '';
    if (!column.sortable) {
      return `<th scope="col"${numeric}>${escapeHtml(column.label)}</th>`;
    }
    const state = sortState(column, sort);
    return `<th scope="col"${numeric} aria-sort="${ARIA_SORT[state]}"><button type="button" class="th-sort" data-sort="${escapeHtml(column.key)}">${escapeHtml(column.label)}<i data-lucide="${SORT_ICONS[state]}" aria-hidden="true"></i></button></th>`;
  });

  if (hasActions) {
    cells.push('<th scope="col" class="col-actions"><span class="sr-only">Thao tác</span></th>');
  }
  return `<thead><tr>${cells.join('')}</tr></thead>`;
}

function renderCell(column, row) {
  const content = column.render ? column.render(row) : escapeHtml(row[column.key] ?? '');
  const classes = [column.primary ? 'table-cell-primary' : '', column.align === 'right' ? 'is-numeric' : '']
    .filter(Boolean)
    .join(' ');
  const classAttr = classes ? ` class="${classes}"` : '';
  return `<td data-label="${escapeHtml(column.label)}"${classAttr}>${content}</td>`;
}

function renderRow(row, { columns, rowKey, actions, actionLabel }) {
  const id = row[rowKey];
  const cells = columns.map((column) => renderCell(column, row)).join('');
  const actionCell =
    actions.length > 0
      ? `<td class="col-actions" data-label="Thao tác">${Dropdown.render({
          label: actionLabel ? actionLabel(row) : 'Thao tác',
          items: actions,
          rowId: id,
        })}</td>`
      : '';
  return `<tr data-row-id="${escapeHtml(id)}">${cells}${actionCell}</tr>`;
}

/**
 * @param {{
 *   columns: Array<{ key: string, label: string, sortable?: boolean, align?: string, primary?: boolean, render?: (row: object) => string }>,
 *   rows: object[],
 *   rowKey?: string,
 *   sort?: { key: string, dir: string } | null,
 *   actions?: Array<{ action: string, label: string, icon?: string, danger?: boolean, divider?: boolean }>,
 *   actionLabel?: (row: object) => string,
 *   caption?: string,
 *   footer?: string
 * }} props
 * @returns {string}
 */
function render({
  columns,
  rows,
  rowKey = 'id',
  sort = null,
  actions = [],
  actionLabel,
  caption,
  footer = '',
}) {
  const captionHtml = caption ? `<caption class="sr-only">${escapeHtml(caption)}</caption>` : '';
  const bodyHtml = rows.map((row) => renderRow(row, { columns, rowKey, actions, actionLabel })).join('');

  return `<div class="table-wrap">
    <table class="table">${captionHtml}${renderHeader(columns, sort, actions.length > 0)}<tbody>${bodyHtml}</tbody></table>
    ${footer}
  </div>`;
}

/**
 * @param {HTMLElement} rootEl
 * @param {{
 *   onSort?: (key: string) => void,
 *   onAction?: (action: string, rowId: string|undefined, item: HTMLElement, event: Event) => void
 * }} [props]
 */
function init(rootEl, { onSort, onAction } = {}) {
  rootEl.addEventListener('click', (event) => {
    const sortButton = event.target.closest('[data-sort]');
    if (sortButton && rootEl.contains(sortButton)) {
      onSort?.(sortButton.dataset.sort);
    }
  });
  Dropdown.init(rootEl, { onAction });
}

export const Table = { render, init };
