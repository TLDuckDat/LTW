/**
 * Demo ui-kit: Card, Badge, Table, Pagination, Dropdown, Modal/Confirm, Toast, EmptyState, Skeleton, Chart.
 * Trạng thái (sắp xếp bảng, trang hiện tại) do ui-kit.js giữ cho từng panel.
 */

import { Badge } from '../components/badge.js';
import { Button } from '../components/button.js';
import { Card } from '../components/card.js';
import { Dropdown } from '../components/dropdown.js';
import { EmptyState } from '../components/empty-state.js';
import { LineChart } from '../components/line-chart.js';
import { Pagination } from '../components/pagination.js';
import { Skeleton } from '../components/skeleton.js';
import { Table } from '../components/table.js';

export const DEMO_ROWS = [
  { id: 'r1', mssv: 'SV001', hoTen: 'Nguyễn Văn A', gpa: 3.45, trangThai: 'active' },
  { id: 'r2', mssv: 'SV002', hoTen: 'Trần Thị Bình', gpa: 3.82, trangThai: 'graduated' },
  { id: 'r3', mssv: 'SV003', hoTen: 'Lê Hoàng Cường', gpa: 2.9, trangThai: 'suspended' },
  { id: 'r4', mssv: 'SV004', hoTen: 'Phạm Thị Dung', gpa: 3.15, trangThai: 'active' },
  { id: 'r5', mssv: 'SV005', hoTen: 'Hoàng Minh Đức', gpa: 2.45, trangThai: 'dropped' },
];

const TABLE_COLUMNS = [
  { key: 'mssv', label: 'MSSV', sortable: true },
  { key: 'hoTen', label: 'Họ và tên', sortable: true, primary: true },
  { key: 'gpa', label: 'GPA', sortable: true, align: 'right', render: (row) => row.gpa.toFixed(2) },
  {
    key: 'trangThai',
    label: 'Trạng thái',
    render: (row) => {
      const map = {
        active: ['success', 'Đang học'],
        graduated: ['info', 'Đã tốt nghiệp'],
        suspended: ['warning', 'Bảo lưu'],
        dropped: ['destructive', 'Thôi học'],
      };
      const [variant, label] = map[row.trangThai];
      return Badge.render({ label, variant });
    },
  },
];

const ROW_ACTIONS = [
  { action: 'view', label: 'Xem chi tiết', icon: 'eye' },
  { action: 'edit', label: 'Chỉnh sửa', icon: 'pencil' },
  { divider: true },
  { action: 'delete', label: 'Xóa', icon: 'trash-2', danger: true },
];

const DEMO_MENU = [
  { action: 'refresh', label: 'Làm mới', icon: 'refresh-cw' },
  { action: 'export', label: 'Xuất file', icon: 'download' },
];

/**
 * @param {{ key: string, dir: string }} sort
 * @returns {object[]}
 */
export function sortDemoRows(rows, sort) {
  const direction = sort.dir === 'desc' ? -1 : 1;
  return [...rows].sort((a, b) => {
    const left = a[sort.key];
    const right = b[sort.key];
    if (typeof left === 'number') return (left - right) * direction;
    return String(left).localeCompare(String(right), 'vi') * direction;
  });
}

/**
 * @param {{ rows: object[], sort: { key: string, dir: string } }} tableState
 * @returns {string}
 */
export function tableMarkup(tableState) {
  return Table.render({
    columns: TABLE_COLUMNS,
    rows: sortDemoRows(tableState.rows, tableState.sort),
    sort: tableState.sort,
    actions: ROW_ACTIONS,
    actionLabel: (row) => `Thao tác cho ${row.mssv}`,
    caption: 'Bảng mẫu',
  });
}

/**
 * @param {{ page: number, pageSize: number, total: number }} pageState
 * @returns {string}
 */
export function paginationMarkup(pageState) {
  return Pagination.render(pageState);
}

export function renderCards() {
  return `<div class="ui-kit-fields">
    ${Card.render({
      title: 'Thẻ có action menu',
      subtitle: 'Phụ đề mô tả nội dung',
      body: '<p class="text-body">Nội dung thẻ. Header có tiêu đề, phụ đề và menu thao tác.</p>',
      footer: [
        Button.render({ label: 'Hủy', variant: 'outline', action: 'noop' }),
        Button.render({ label: 'Lưu', variant: 'primary', action: 'demo-save' }),
      ].join(''),
      actions: DEMO_MENU,
    })}
    ${Card.render({ title: 'Thẻ đơn giản', body: '<p class="text-body">Chỉ có phần body.</p>' })}
  </div>`;
}

export function renderBadges() {
  const items = [
    ['success', 'Đang học'],
    ['warning', 'Bảo lưu'],
    ['destructive', 'Thôi học'],
    ['info', 'Đã tốt nghiệp'],
    ['primary', 'Bắt buộc'],
    ['neutral', 'Khác'],
  ];
  return `<div class="ui-kit-row-items">${items.map(([variant, label]) => Badge.render({ label, variant })).join('')}</div>`;
}

export function renderPaginationDemo(pageState) {
  return `<div data-slot="pagination">${paginationMarkup(pageState)}</div>`;
}

export function renderDropdownDemo() {
  return `<div class="ui-kit-row-items">
    ${Dropdown.render({ label: 'Menu mẫu', items: DEMO_MENU })}
    <span class="text-caption text-muted">Menu dùng position fixed nên không bị vùng cuộn của bảng cắt.</span>
  </div>`;
}

export function renderOverlays() {
  return `<div class="ui-kit-row-items">
    ${Button.render({ label: 'Mở form (modal)', variant: 'primary', action: 'open-modal' })}
    ${Button.render({ label: 'Mở xác nhận xóa', variant: 'destructive', action: 'open-confirm' })}
  </div>`;
}

export function renderToastButtons() {
  return `<div class="ui-kit-row-items">
    ${Button.render({ label: 'Success', variant: 'outline', action: 'toast-success' })}
    ${Button.render({ label: 'Warning', variant: 'outline', action: 'toast-warning' })}
    ${Button.render({ label: 'Error', variant: 'outline', action: 'toast-error' })}
    ${Button.render({ label: 'Info', variant: 'outline', action: 'toast-info' })}
  </div>`;
}

export function renderEmpty(prefix) {
  return EmptyState.render({
    icon: 'inbox',
    title: 'Chưa có dữ liệu',
    description: 'Bắt đầu bằng cách thêm mục đầu tiên.',
    actionText: 'Thêm mới',
    actionId: `${prefix}-empty-add`,
    action: 'demo-empty-add',
  });
}

export function renderSkeletons() {
  return `<div class="ui-kit-fields">
    <div>${Skeleton.table(3, 4)}</div>
    <div class="ui-kit-form">${Skeleton.card()}${Skeleton.kpi()}${Skeleton.text(3)}</div>
  </div>`;
}

export function renderChart() {
  return LineChart.render({
    title: 'Demo biểu đồ đường',
    points: [
      { label: 'T1', value: 820 },
      { label: 'T2', value: 905 },
      { label: 'T3', value: 960 },
      { label: 'T4', value: 930 },
      { label: 'T5', value: 1010 },
      { label: 'T6', value: 1080 },
    ],
  });
}
