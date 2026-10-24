/**
 * Biểu đồ đường SVG vẽ thủ công (không dùng thư viện).
 * - render({ points: [{ label, value }], title }) -> HTML string
 * Màu lấy từ class CSS (.chart-line, .chart-area...) nên tự đổi theo theme.
 */

import { escapeHtml, nextId } from '../core/dom.js';
import { formatNumber } from '../core/format.js';

const WIDTH = 640;
const HEIGHT = 260;
const PADDING = { top: 16, right: 16, bottom: 32, left: 56 };
const TICK_COUNT = 4;

/**
 * @param {{ points: Array<{ label: string, value: number }>, title: string }} props
 * @returns {string}
 */
function render({ points, title }) {
  const titleId = nextId('chart-title');
  const innerWidth = WIDTH - PADDING.left - PADDING.right;
  const innerHeight = HEIGHT - PADDING.top - PADDING.bottom;
  const maxValue = Math.max(...points.map((point) => point.value), 1);
  const yMax = Math.ceil((maxValue * 1.1) / 100) * 100;
  const baseY = PADDING.top + innerHeight;

  const xAt = (index) => PADDING.left + (points.length === 1 ? innerWidth / 2 : (index * innerWidth) / (points.length - 1));
  const yAt = (value) => PADDING.top + innerHeight * (1 - value / yMax);

  const gridLines = Array.from({ length: TICK_COUNT + 1 }, (_, step) => {
    const value = (yMax * step) / TICK_COUNT;
    const y = yAt(value);
    return `<line x1="${PADDING.left}" x2="${WIDTH - PADDING.right}" y1="${y}" y2="${y}"></line>
      <text class="chart-axis-label" x="${PADDING.left - 8}" y="${y}" text-anchor="end" dominant-baseline="middle">${formatNumber(value)}</text>`;
  }).join('');

  const coordinates = points.map((point, index) => [xAt(index), yAt(point.value)]);
  const linePath = coordinates.map(([x, y], index) => `${index === 0 ? 'M' : 'L'}${x},${y}`).join(' ');
  const [firstX] = coordinates[0];
  const [lastX] = coordinates[coordinates.length - 1];
  const areaPath = `${linePath} L${lastX},${baseY} L${firstX},${baseY} Z`;

  const labels = points
    .map((point, index) => `<text class="chart-axis-label" x="${xAt(index)}" y="${HEIGHT - 8}" text-anchor="middle">${escapeHtml(point.label)}</text>`)
    .join('');

  const markers = points
    .map(
      (point, index) =>
        `<circle class="chart-point" cx="${coordinates[index][0]}" cy="${coordinates[index][1]}" r="4"><title>${escapeHtml(point.label)}: ${formatNumber(point.value)} sinh viên</title></circle>`,
    )
    .join('');

  return `<svg class="chart-svg" viewBox="0 0 ${WIDTH} ${HEIGHT}" role="img" aria-labelledby="${titleId}">
    <title id="${titleId}">${escapeHtml(title)}</title>
    <g class="chart-grid">${gridLines}</g>
    <path class="chart-area" d="${areaPath}"></path>
    <path class="chart-line" d="${linePath}"></path>
    ${markers}
    ${labels}
  </svg>`;
}

export const LineChart = { render };
