/**
 * Biểu đồ cột SVG vẽ thủ công (không dùng thư viện), dùng cho số liệu phân nhóm như sinh viên theo khoa.
 * - render({ items: [{ label, value }], title }) -> HTML string
 * Màu cột lấy từ class .chart-bar (token primary) nên tự đổi theo theme.
 */

import { escapeHtml, nextId } from '../core/dom.js';
import { formatNumber } from '../core/format.js';

const WIDTH = 640;
const HEIGHT = 260;
const PADDING = { top: 16, right: 16, bottom: 32, left: 56 };
const TICK_COUNT = 4;

/**
 * @param {{ items: Array<{ label: string, value: number }>, title: string }} props
 * @returns {string}
 */
function render({ items, title }) {
  const titleId = nextId('bar-chart-title');
  const innerWidth = WIDTH - PADDING.left - PADDING.right;
  const innerHeight = HEIGHT - PADDING.top - PADDING.bottom;
  const maxValue = Math.max(...items.map((item) => item.value), 1);
  const yMax = Math.ceil((maxValue * 1.1) / 10) * 10;
  const baseY = PADDING.top + innerHeight;
  const slot = innerWidth / items.length;
  const barWidth = Math.min(64, slot * 0.6);

  const yAt = (value) => PADDING.top + innerHeight * (1 - value / yMax);

  const gridLines = Array.from({ length: TICK_COUNT + 1 }, (_, step) => {
    const value = (yMax * step) / TICK_COUNT;
    const y = yAt(value);
    return `<line x1="${PADDING.left}" x2="${WIDTH - PADDING.right}" y1="${y}" y2="${y}"></line>
      <text class="chart-axis-label" x="${PADDING.left - 8}" y="${y}" text-anchor="end" dominant-baseline="middle">${formatNumber(value)}</text>`;
  }).join('');

  const bars = items
    .map((item, index) => {
      const centerX = PADDING.left + slot * index + slot / 2;
      const y = yAt(item.value);
      const height = baseY - y;
      return `<rect class="chart-bar" x="${centerX - barWidth / 2}" y="${y}" width="${barWidth}" height="${height}" rx="4">
        <title>${escapeHtml(item.label)}: ${formatNumber(item.value)} sinh viên</title></rect>
      <text class="chart-axis-label" x="${centerX}" y="${y - 6}" text-anchor="middle">${formatNumber(item.value)}</text>
      <text class="chart-axis-label" x="${centerX}" y="${HEIGHT - 8}" text-anchor="middle">${escapeHtml(item.label)}</text>`;
    })
    .join('');

  return `<svg class="chart-svg" viewBox="0 0 ${WIDTH} ${HEIGHT}" role="img" aria-labelledby="${titleId}">
    <title id="${titleId}">${escapeHtml(title)}</title>
    <g class="chart-grid">${gridLines}</g>
    ${bars}
  </svg>`;
}

export const BarChart = { render };
