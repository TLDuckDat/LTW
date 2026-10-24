/**
 * Trang Dashboard: số liệu thật từ backend.
 * - KPI và biểu đồ sinh viên theo khoa: GET /api/bao-cao/tong-quan (một lần gọi)
 * - Sinh viên mới cập nhật: GET /api/sinh-vien
 * - Thông báo: GET /api/thong-bao
 * Mỗi khu vực có loading, empty và error riêng.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { escapeHtml } from '../core/dom.js';
import { formatDate, formatNumber } from '../core/format.js';
import { BarChart } from '../components/bar-chart.js';
import { Card } from '../components/card.js';
import { EmptyState } from '../components/empty-state.js';
import { Skeleton } from '../components/skeleton.js';
import { Table } from '../components/table.js';
import { renderPageHeader } from '../layout/shell.js';
import { statusBadge } from './students-form.js';

const LATEST_COLUMNS = [
  { key: 'mssv', label: 'MSSV' },
  { key: 'hoTen', label: 'Họ và tên', primary: true },
  { key: 'tenNganh', label: 'Ngành', render: (row) => escapeHtml(row.tenNganh ?? '—') },
  { key: 'trangThai', label: 'Trạng thái', render: (row) => statusBadge(row.trangThai) },
];

/**
 * @param {object} data TongQuanDto
 * @returns {string}
 */
function renderKpis(data) {
  const ratioDangHoc = data.tongSoSinhVien === 0 ? 0 : (data.soSinhVienDangHoc / data.tongSoSinhVien) * 100;
  const kpis = [
    { label: 'Tổng sinh viên', icon: 'users', value: formatNumber(data.tongSoSinhVien), note: 'Toàn trường' },
    {
      label: 'Sinh viên đang học',
      icon: 'graduation-cap',
      value: formatNumber(data.soSinhVienDangHoc),
      note: `${formatNumber(ratioDangHoc, 1)}% tổng số`,
    },
    {
      label: 'Tỷ lệ tốt nghiệp',
      icon: 'award',
      value: `${formatNumber(data.tyLeTotNghiep * 100, 1)}%`,
      note: 'Trên tổng số sinh viên',
    },
    {
      label: 'GPA trung bình',
      icon: 'gauge',
      value: data.gpaTrungBinh === null ? '—' : formatNumber(data.gpaTrungBinh, 2),
      note: data.hocKyMoc ? `${data.hocKyMoc.ten} - ${data.hocKyMoc.tenNamHoc}` : 'Chưa có học kỳ',
    },
  ];

  return kpis
    .map(
      (kpi) => `<article class="card kpi-card">
        <div class="kpi-header">
          <span class="kpi-label">${escapeHtml(kpi.label)}</span>
          <span class="kpi-icon" aria-hidden="true"><i data-lucide="${kpi.icon}"></i></span>
        </div>
        <p class="kpi-value">${escapeHtml(kpi.value)}</p>
        <span class="kpi-delta-note">${escapeHtml(kpi.note)}</span>
      </article>`,
    )
    .join('');
}

/**
 * @param {HTMLElement} root
 * @param {Promise<object>} tongQuanReady
 */
function renderSections(root, tongQuanReady) {
  renderAsync(root.querySelector('#kpi-container'), {
    load: () => tongQuanReady,
    skeleton: () => Array.from({ length: 4 }, () => Skeleton.kpi()).join(''),
    isEmpty: () => false,
    render: renderKpis,
    empty: () => '',
    errorTitle: 'Không tải được chỉ số',
  });

  renderAsync(root.querySelector('#chart-container'), {
    load: () => tongQuanReady,
    skeleton: () => '<div class="skeleton" style="height:260px" role="status" aria-busy="true"><span class="sr-only">Đang tải biểu đồ</span></div>',
    isEmpty: (data) => data.theoKhoa.every((item) => item.soLuong === 0),
    empty: () => EmptyState.render({ icon: 'chart-column', title: 'Chưa có sinh viên theo khoa', description: 'Biểu đồ sẽ hiện khi có sinh viên thuộc một ngành.' }),
    render: (data) => BarChart.render({ items: data.theoKhoa.map((item) => ({ label: item.ma, value: item.soLuong })), title: 'Sinh viên theo khoa' }),
    errorTitle: 'Không tải được biểu đồ',
  });

  renderAsync(root.querySelector('#notice-container'), {
    load: () => api.thongBao.list({ pageSize: 5 }),
    skeleton: () => Skeleton.text(4),
    isEmpty: (result) => result.data.length === 0,
    empty: () => EmptyState.render({ icon: 'bell', title: 'Chưa có thông báo' }),
    render: (result) =>
      `<ul class="todo-list">${result.data
        .map(
          (item) => `<li class="todo-item">
            <span class="todo-text">${escapeHtml(item.tieuDe)}</span>
            <span class="dropdown-notice-meta">${escapeHtml(formatDate(item.ngayDang))}</span>
          </li>`,
        )
        .join('')}</ul>`,
    errorTitle: 'Không tải được thông báo',
  });

  renderAsync(root.querySelector('#students-container'), {
    load: () => api.students.list({ page: 1, pageSize: 5 }),
    skeleton: () => Skeleton.table(5, 4),
    isEmpty: (result) => result.total === 0,
    empty: () => EmptyState.render({ icon: 'users', title: 'Chưa có sinh viên nào' }),
    render: (result) => Table.render({ columns: LATEST_COLUMNS, rows: result.data, caption: 'Sinh viên mới nhất' }),
    errorTitle: 'Không tải được danh sách sinh viên',
  });
}

/**
 * @param {HTMLElement} root
 */
export function mount(root) {
  root.innerHTML = `${renderPageHeader({
    title: 'Dashboard',
    description: 'Tổng quan sinh viên và đào tạo, số liệu lấy từ cơ sở dữ liệu',
  })}
  <div class="page-stack">
    <section aria-label="Chỉ số chính"><div class="kpi-grid" id="kpi-container"></div></section>
    <div class="dashboard-grid">
      <div class="dashboard-wide">${Card.render({
        title: 'Sinh viên theo khoa',
        subtitle: 'Số sinh viên thuộc mỗi khoa',
        titleTag: 'h2',
        body: '<div id="chart-container"></div>',
      })}</div>
      ${Card.render({
        title: 'Thông báo mới nhất',
        subtitle: 'Thông báo đăng gần đây',
        titleTag: 'h2',
        body: '<div id="notice-container"></div>',
      })}
      <div class="dashboard-wide">${Card.render({
        title: 'Sinh viên mới cập nhật',
        subtitle: '5 hồ sơ sinh viên gần đây',
        titleTag: 'h2',
        flush: true,
        body: '<div id="students-container"></div>',
        footer: '<a class="btn btn-outline btn-sm" href="students.html">Xem tất cả sinh viên</a>',
      })}</div>
    </div>
  </div>`;

  renderSections(root, api.baoCao.tongQuan());
}
