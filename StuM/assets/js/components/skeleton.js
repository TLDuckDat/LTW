/**
 * Skeleton loader (shimmer). Hình dạng giống component thật.
 * - table(rows, cols) | card() | text(lines) | kpi()
 * Mỗi skeleton có sr-only text để trình đọc màn hình biết đang tải.
 */

const LOADING_TEXT = '<span class="sr-only">Đang tải dữ liệu</span>';
const LINE_WIDTHS = [92, 78, 86, 64, 90, 72];

/**
 * @param {number} [rows=5]
 * @param {number} [cols=5]
 * @returns {string}
 */
function table(rows = 5, cols = 5) {
  const headerCells = Array.from({ length: cols }, () => '<div class="skeleton skeleton-line"></div>').join('');
  const bodyRows = Array.from({ length: rows }, (_, rowIndex) => {
    const cells = Array.from(
      { length: cols },
      (__, colIndex) =>
        `<div class="skeleton skeleton-line" style="width:${LINE_WIDTHS[(rowIndex + colIndex) % LINE_WIDTHS.length]}%"></div>`,
    ).join('');
    return `<div class="skeleton-row" style="--cols:${cols}">${cells}</div>`;
  }).join('');

  return `<div class="table-wrap" aria-busy="true">
    <div class="skeleton-table" role="status">
      ${LOADING_TEXT}
      <div class="skeleton-row" style="--cols:${cols}">${headerCells}</div>
      ${bodyRows}
    </div>
  </div>`;
}

/** @returns {string} */
function card() {
  return `<div class="card skeleton-card" aria-busy="true" role="status">
    ${LOADING_TEXT}
    <div class="skeleton skeleton-title"></div>
    <div class="skeleton skeleton-line"></div>
    <div class="skeleton skeleton-line" style="width:70%"></div>
    <div class="skeleton skeleton-line" style="width:85%"></div>
  </div>`;
}

/**
 * @param {number} [lines=3]
 * @returns {string}
 */
function text(lines = 3) {
  const items = Array.from(
    { length: lines },
    (_, index) => `<div class="skeleton skeleton-line" style="width:${LINE_WIDTHS[index % LINE_WIDTHS.length]}%"></div>`,
  ).join('');
  return `<div class="skeleton-text" aria-busy="true" role="status">${LOADING_TEXT}${items}</div>`;
}

/** @returns {string} */
function kpi() {
  return `<div class="card kpi-card" aria-busy="true" role="status">
    ${LOADING_TEXT}
    <div class="kpi-header"><div class="skeleton skeleton-line" style="width:45%"></div><div class="skeleton skeleton-circle"></div></div>
    <div class="skeleton" style="height:32px;width:60%"></div>
    <div class="skeleton skeleton-line" style="width:35%"></div>
  </div>`;
}

export const Skeleton = { table, card, text, kpi };
