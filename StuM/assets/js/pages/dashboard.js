/**
 * Trang Dashboard: 4 KPI, biểu đồ SVG thủ công, việc cần làm (checkbox), sinh viên mới nhất.
 * Mỗi khu vực có loading (skeleton), empty và error riêng, không làm hỏng các khu vực khác.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { escapeHtml } from '../core/dom.js';
import { formatNumber } from '../core/format.js';
import { Card } from '../components/card.js';
import { EmptyState } from '../components/empty-state.js';
import { LineChart } from '../components/line-chart.js';
import { Skeleton } from '../components/skeleton.js';
import { Table } from '../components/table.js';
import { Toast } from '../components/toast.js';
import { renderPageHeader } from '../layout/shell.js';
import { statusBadge } from './students-form.js';

const KPI_META = [
  { key: 'totalStudents', label: 'Tổng sinh viên', icon: 'users', format: (value) => formatNumber(value) },
  { key: 'activeStudents', label: 'Sinh viên đang học', icon: 'graduation-cap', format: (value) => formatNumber(value) },
  { key: 'graduationRate', label: 'Tỷ lệ tốt nghiệp', icon: 'award', format: (value) => `${formatNumber(value * 100, 1)}%` },
  { key: 'averageGpa', label: 'GPA trung bình', icon: 'gauge', format: (value) => formatNumber(value, 2) },
];

const LATEST_COLUMNS = [
  { key: 'mssv', label: 'MSSV' },
  { key: 'hoTen', label: 'Họ và tên', primary: true },
  { key: 'khoa', label: 'Khoa' },
  { key: 'gpa', label: 'GPA', align: 'right', render: (row) => formatNumber(row.gpa, 2) },
  { key: 'trangThai', label: 'Trạng thái', render: (row) => statusBadge(row.trangThai) },
];

/**
 * Tỷ lệ thay đổi so với kỳ trước, tính theo phần trăm.
 * @param {{ value: number, previous: number }} stat
 * @returns {number}
 */
function changeOf(stat) {
  return ((stat.value - stat.previous) / stat.previous) * 100;
}

function deltaMarkup(change) {
  const up = change >= 0;
  const sign = up ? '+' : '−';
  return `<span class="kpi-delta ${up ? 'is-up' : 'is-down'}"><i data-lucide="${up ? 'trending-up' : 'trending-down'}" aria-hidden="true"></i><span>${sign}${formatNumber(Math.abs(change), 1)}%</span><span class="kpi-delta-note">so với kỳ trước</span></span>`;
}

function renderKpis(data) {
  return KPI_META.map((meta) => {
    const stat = data[meta.key];
    return `<article class="card kpi-card">
      <div class="kpi-header">
        <span class="kpi-label">${escapeHtml(meta.label)}</span>
        <span class="kpi-icon" aria-hidden="true"><i data-lucide="${meta.icon}"></i></span>
      </div>
      <p class="kpi-value">${escapeHtml(meta.format(stat.value))}</p>
      ${deltaMarkup(changeOf(stat))}
    </article>`;
  }).join('');
}

function renderTodos(todos) {
  const items = todos
    .map(
      (todo) => `<li class="todo-item${todo.done ? ' is-done' : ''}">
        <input type="checkbox" id="todo-${escapeHtml(todo.id)}" data-todo="${escapeHtml(todo.id)}"${todo.done ? ' checked' : ''}>
        <label class="todo-text" for="todo-${escapeHtml(todo.id)}">${escapeHtml(todo.text)}</label>
      </li>`,
    )
    .join('');
  return `<ul class="todo-list">${items}</ul>`;
}

function bindTodoEvents(root) {
  root.addEventListener('change', async (event) => {
    if (!event.target.matches('[data-todo]')) return;
    const input = event.target;
    const item = input.closest('.todo-item');
    const done = input.checked;
    item.classList.toggle('is-done', done);

    try {
      await api.setTodoDone(input.dataset.todo, done);
    } catch (error) {
      input.checked = !done;
      item.classList.toggle('is-done', !done);
      Toast.error(error.message);
    }
  });
}

/**
 * @param {HTMLElement} root
 */
export function mount(root) {
  root.innerHTML = `${renderPageHeader({
    title: 'Dashboard',
    description: 'Tổng quan đào tạo học kỳ HK1-2026',
  })}
  <div class="page-stack">
    <section aria-label="Chỉ số chính"><div class="kpi-grid" id="kpi-container"></div></section>
    <div class="dashboard-grid">
      <div class="dashboard-wide">${Card.render({
        title: 'Sinh viên đăng ký theo tháng',
        subtitle: 'Năm học 2026',
        titleTag: 'h2',
        body: '<div id="chart-container"></div>',
      })}</div>
      ${Card.render({
        title: 'Việc cần làm',
        subtitle: 'Công việc của bạn trong tuần này',
        titleTag: 'h2',
        body: '<div id="todo-container"></div>',
      })}
      <div class="dashboard-wide">${Card.render({
        title: 'Sinh viên mới nhất',
        subtitle: '5 hồ sơ được cập nhật gần đây',
        titleTag: 'h2',
        flush: true,
        body: '<div id="students-container"></div>',
        footer: '<a class="btn btn-outline btn-sm" href="students.html">Xem tất cả sinh viên</a>',
      })}</div>
    </div>
  </div>`;

  const dashboardReady = api.getDashboard();

  renderAsync(root.querySelector('#kpi-container'), {
    load: () => dashboardReady,
    skeleton: () => Array.from({ length: KPI_META.length }, () => Skeleton.kpi()).join(''),
    isEmpty: () => false,
    render: renderKpis,
    empty: () => '',
    errorTitle: 'Không tải được chỉ số',
  });

  renderAsync(root.querySelector('#chart-container'), {
    load: () => dashboardReady,
    skeleton: () => '<div class="skeleton" style="height:260px" role="status" aria-busy="true"><span class="sr-only">Đang tải biểu đồ</span></div>',
    isEmpty: (data) => data.enrollmentByMonth.length === 0,
    empty: () => EmptyState.render({ icon: 'chart-line', title: 'Chưa có dữ liệu đăng ký' }),
    render: (data) => `${LineChart.render({ points: data.enrollmentByMonth, title: 'Số sinh viên đăng ký theo tháng' })}
      <div class="chart-legend mt-sm"><span class="chart-legend-swatch" aria-hidden="true"></span>Sinh viên đăng ký</div>`,
    errorTitle: 'Không tải được biểu đồ',
  });

  renderAsync(root.querySelector('#todo-container'), {
    load: () => api.getTodos(),
    skeleton: () => Skeleton.text(4),
    isEmpty: (todos) => todos.length === 0,
    empty: () => EmptyState.render({ icon: 'circle-check', title: 'Không còn việc cần làm' }),
    render: renderTodos,
    errorTitle: 'Không tải được danh sách việc cần làm',
  });

  renderAsync(root.querySelector('#students-container'), {
    load: () => api.students.list({ page: 1, pageSize: 5, sort: { key: 'mssv', dir: 'desc' } }),
    skeleton: () => Skeleton.table(5, 5),
    isEmpty: (result) => result.total === 0,
    empty: () => EmptyState.render({ icon: 'users', title: 'Chưa có sinh viên nào' }),
    render: (result) => Table.render({ columns: LATEST_COLUMNS, rows: result.data, caption: 'Sinh viên mới nhất' }),
    errorTitle: 'Không tải được danh sách sinh viên',
  });

  bindTodoEvents(root);
}
