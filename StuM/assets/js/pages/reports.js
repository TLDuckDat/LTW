/**
 * Trang Báo cáo (chỉ Admin và Phòng đào tạo, chỉ đọc): thống kê sinh viên, kết quả học tập theo học kỳ,
 * công nợ học phí và cảnh báo học vụ. Các số liệu lấy từ /api/bao-cao/* của backend.
 * Mỗi báo cáo là một tab, chỉ tải khi mở; bộ lọc chỉ hiện các ô dùng cho tab đó.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { debounce, escapeHtml } from '../core/dom.js';
import { formatNumber } from '../core/format.js';
import { Badge } from '../components/badge.js';
import { EmptyState } from '../components/empty-state.js';
import { Input } from '../components/input.js';
import { Pagination } from '../components/pagination.js';
import { Select } from '../components/select.js';
import { Skeleton } from '../components/skeleton.js';
import { Table } from '../components/table.js';
import { renderPageHeader } from '../layout/shell.js';

const CANH_BAO = {
  KhongCanhBao: ['success', 'Không cảnh báo'],
  CanhBao: ['destructive', 'Cảnh báo'],
  ChuaDuDuLieu: ['neutral', 'Chưa đủ dữ liệu'],
};

const state = {
  tab: 'thongke',
  hocKyId: null,
  search: '',
  congNoPage: 1,
  canhBaoPage: 1,
};

const kpi = (label, value) => `<div class="kpi-card card"><span class="kpi-label">${escapeHtml(label)}</span><p class="kpi-value">${escapeHtml(value)}</p></div>`;
const kpiGrid = (items) => `<div class="kpi-grid">${items.map(([label, value]) => kpi(label, value)).join('')}</div>`;

/* ----- Thống kê sinh viên (RPT-01) ----- */

function loadThongKe() {
  return renderAsync(document.getElementById('thongke-container'), {
    load: () => api.baoCao.sinhVien(),
    skeleton: () => Skeleton.table(4, 3),
    isEmpty: (data) => data.tongSoSinhVien === 0,
    empty: () => EmptyState.render({ icon: 'users', title: 'Chưa có sinh viên' }),
    render: (data) => `<div class="page-stack">${kpiGrid([['Tổng sinh viên', formatNumber(data.tongSoSinhVien)]])}
      <div class="page-stack">
        ${Table.render({
          columns: [
            { key: 'ma', label: 'Khoa', primary: true, render: (r) => escapeHtml(`${r.ma} - ${r.ten}`) },
            { key: 'soLuong', label: 'Số sinh viên', align: 'right' },
          ],
          rows: data.theoKhoa,
          rowKey: 'ma',
          caption: 'Sinh viên theo khoa',
        })}
        ${Table.render({
          columns: [
            { key: 'ma', label: 'Trạng thái', primary: true },
            { key: 'soLuong', label: 'Số sinh viên', align: 'right' },
          ],
          rows: data.theoTrangThai,
          rowKey: 'ma',
          caption: 'Sinh viên theo trạng thái',
        })}
      </div>
      </div>`,
    errorTitle: 'Không tải được thống kê sinh viên',
  });
}

/* ----- Kết quả học tập theo học kỳ (RPT-02) ----- */

function loadKetQua() {
  if (state.hocKyId === null) return undefined;
  return renderAsync(document.getElementById('ketqua-container'), {
    load: () => api.baoCao.ketQuaHocTap(state.hocKyId),
    skeleton: () => Skeleton.table(4, 4),
    isEmpty: (data) => data.soSinhVien === 0,
    empty: () => EmptyState.render({ icon: 'clipboard-list', title: 'Học kỳ chưa có sinh viên đăng ký' }),
    render: (data) => `<div class="page-stack">${kpiGrid([
      ['Sinh viên đăng ký', formatNumber(data.soSinhVien)],
      ['Có GPA chính thức', formatNumber(data.soSinhVienCoGpa)],
      ['Chưa đủ kết quả', formatNumber(data.soSinhVienChuaDuKetQua)],
      ['GPA trung bình', data.gpaTrungBinh === null ? '—' : formatNumber(data.gpaTrungBinh, 2)],
    ])}
      <div class="page-stack">
        ${Table.render({
          columns: [
            { key: 'ma', label: 'Xếp loại', primary: true },
            { key: 'soLuong', label: 'Số sinh viên', align: 'right' },
          ],
          rows: data.theoXepLoai,
          rowKey: 'ma',
          caption: 'Theo xếp loại',
        })}
        ${Table.render({
          columns: [
            { key: 'ma', label: 'Điểm chữ', primary: true },
            { key: 'soLuong', label: 'Số lượt', align: 'right' },
          ],
          rows: data.theoDiemChu,
          rowKey: 'ma',
          caption: 'Theo điểm chữ',
        })}
        ${Table.render({
          columns: [
            { key: 'maMonHoc', label: 'Môn', primary: true, render: (r) => escapeHtml(`${r.maMonHoc} - ${r.tenMonHoc}`) },
            { key: 'soLuotDangKy', label: 'Đăng ký', align: 'right' },
            { key: 'soLuotCoKetQua', label: 'Có kết quả', align: 'right' },
            { key: 'soLuotDat', label: 'Đạt', align: 'right' },
            { key: 'tyLeDat', label: 'Tỷ lệ đạt', align: 'right', render: (r) => (r.tyLeDat === null ? '—' : `${formatNumber(r.tyLeDat * 100, 1)}%`) },
            { key: 'diemTrungBinh', label: 'ĐTB', align: 'right', render: (r) => (r.diemTrungBinh === null ? '—' : formatNumber(r.diemTrungBinh, 2)) },
          ],
          rows: data.theoMonHoc,
          rowKey: 'monHocId',
          caption: 'Kết quả theo môn',
        })}
      </div>
      </div>`,
    errorTitle: 'Không tải được kết quả học tập',
  });
}

/* ----- Công nợ học phí (RPT-03) ----- */

/** Công nợ cần cả phần tổng hợp lẫn danh sách; tải một lần và vẽ cả hai. */
async function renderCongNo() {
  const container = document.getElementById('congno-container');
  container.setAttribute('aria-busy', 'true');
  container.innerHTML = Skeleton.table(5, 5);
  try {
    const data = await api.baoCao.congNo(state.hocKyId, { page: state.congNoPage, pageSize: 10, search: state.search });
    const list = data.danhSachConNo ?? { items: [], total: 0, page: 1, pageSize: 10 };
    container.removeAttribute('aria-busy');
    container.innerHTML = `<div class="page-stack">${kpiGrid([
      ['Khoản học phí', formatNumber(data.soKhoanHocPhi)],
      ['Sinh viên còn nợ', formatNumber(data.soSinhVienConNo)],
      ['Chưa tính học phí', formatNumber(data.soSinhVienChuaTinhHocPhi)],
      ['Tổng phải thu', formatNumber(data.tongPhaiThu)],
      ['Đã thu', formatNumber(data.daThu)],
      ['Còn nợ', formatNumber(data.conNo)],
    ])}
      ${list.items.length === 0
        ? EmptyState.render({ icon: 'search-x', title: 'Không có sinh viên còn nợ', description: 'Thử bỏ bộ lọc tìm kiếm.' })
        : Table.render({
            columns: [
              { key: 'mssv', label: 'MSSV', primary: true },
              { key: 'hoTen', label: 'Họ và tên' },
              { key: 'soTien', label: 'Phải nộp', align: 'right', render: (r) => formatNumber(r.soTien) },
              { key: 'soTienDaNop', label: 'Đã nộp', align: 'right', render: (r) => formatNumber(r.soTienDaNop) },
              { key: 'soConNo', label: 'Còn nợ', align: 'right', render: (r) => formatNumber(r.soConNo) },
            ],
            rows: list.items,
            rowKey: 'hocPhiId',
            caption: 'Sinh viên còn nợ học phí',
            footer: Pagination.render({ page: list.page, pageSize: list.pageSize, total: list.totalCount ?? list.total }),
          })}
    </div>`;
  } catch (error) {
    container.removeAttribute('aria-busy');
    container.innerHTML = `<p class="form-error">${escapeHtml(error.message)}</p>`;
  }
}

/* ----- Cảnh báo học vụ (RPT-04) ----- */

function loadCanhBao() {
  if (state.hocKyId === null) return undefined;
  return renderAsync(document.getElementById('canhbao-container'), {
    load: () => api.baoCao.canhBao(state.hocKyId, { page: state.canhBaoPage, pageSize: 10, search: state.search }),
    skeleton: () => Skeleton.table(5, 6),
    isEmpty: (result) => result.total === 0,
    empty: () => EmptyState.render({ icon: 'shield-check', title: 'Không có sinh viên cảnh báo', description: 'Học kỳ này chưa có sinh viên bị cảnh báo học vụ.' }),
    render: (result) =>
      Table.render({
        columns: [
          { key: 'mssv', label: 'MSSV', primary: true },
          { key: 'hoTen', label: 'Họ và tên' },
          { key: 'gpa', label: 'GPA', align: 'right', render: (r) => formatNumber(r.gpa, 2) },
          { key: 'cpa', label: 'CPA', align: 'right', render: (r) => formatNumber(r.cpa, 2) },
          { key: 'soTinChiKhongDat', label: 'TC không đạt', align: 'right' },
          {
            key: 'trangThaiCanhBao',
            label: 'Trạng thái',
            render: (r) => {
              const [variant, label] = CANH_BAO[r.trangThaiCanhBao] ?? ['neutral', r.trangThaiCanhBao];
              return Badge.render({ label, variant });
            },
          },
          { key: 'danhSachLyDo', label: 'Lý do', render: (r) => `<span class="text-caption">${escapeHtml((r.danhSachLyDo ?? []).join('; ') || '—')}</span>` },
        ],
        rows: result.data,
        rowKey: 'sinhVienId',
        caption: 'Sinh viên cảnh báo học vụ',
        footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
      }),
    errorTitle: 'Không tải được cảnh báo học vụ',
  });
}

/* ----- Tab ----- */

/** filters: tên các ô lọc dùng cho tab; tab không có ô lọc nào thì ẩn cả thanh lọc. */
const TABS = [
  { key: 'thongke', label: 'Thống kê sinh viên', subtitle: 'Toàn trường theo khoa và trạng thái', load: loadThongKe, filters: [] },
  { key: 'ketqua', label: 'Kết quả học tập', subtitle: 'Học kỳ đã chọn', load: loadKetQua, filters: ['hocKyId'] },
  { key: 'congno', label: 'Công nợ học phí', subtitle: 'Học kỳ đã chọn', load: renderCongNo, filters: ['hocKyId', 'q'] },
  { key: 'canhbao', label: 'Cảnh báo học vụ', subtitle: 'Học kỳ đã chọn', load: loadCanhBao, filters: ['hocKyId', 'q'] },
];

const activeTab = () => TABS.find((tab) => tab.key === state.tab);

/** Chỉ tải báo cáo của tab đang mở; tab khác tải lại khi được mở nên luôn mới. */
function refreshActiveTab() {
  return activeTab().load();
}

function syncFilters(root) {
  const filters = activeTab().filters;
  const bar = root.querySelector('#report-filters');
  bar.hidden = filters.length === 0;
  bar.querySelectorAll('[name]').forEach((control) => {
    const field = control.closest('.field');
    if (field) field.hidden = !filters.includes(control.name);
  });
}

function switchTab(root, key) {
  state.tab = key;
  root.querySelectorAll('[data-tab]').forEach((tab) => tab.setAttribute('aria-selected', String(tab.dataset.tab === key)));
  TABS.forEach((tab) => {
    root.querySelector(`#tab-${tab.key}`).hidden = tab.key !== key;
  });
  syncFilters(root);
  refreshActiveTab();
}

function tabsMarkup() {
  return `<nav class="tabs" role="tablist" aria-label="Báo cáo">
    ${TABS.map((tab) => `<button type="button" class="tab" role="tab" data-tab="${tab.key}" aria-controls="tab-${tab.key}" aria-selected="${tab.key === state.tab}">${escapeHtml(tab.label)}</button>`).join('')}
  </nav>`;
}

function section(tab) {
  return `<section class="card" id="tab-${tab.key}" role="tabpanel" aria-labelledby="${tab.key}-title"${tab.key === state.tab ? '' : ' hidden'}>
    <header class="card-header"><div class="card-header-text"><h2 class="card-title" id="${tab.key}-title">${escapeHtml(tab.label)}</h2><p class="card-subtitle">${escapeHtml(tab.subtitle)}</p></div></header>
    <div class="card-body" id="${tab.key}-container"></div>
  </section>`;
}

function bindEvents(root) {
  const debouncedSearch = debounce(() => {
    state.search = root.querySelector('[name="q"]').value.trim();
    state.congNoPage = 1;
    state.canhBaoPage = 1;
    refreshActiveTab();
  }, 300);

  root.querySelector('.tabs').addEventListener('click', (event) => {
    const tab = event.target.closest('[data-tab]');
    if (tab && tab.dataset.tab !== state.tab) switchTab(root, tab.dataset.tab);
  });

  Input.init(root, {
    onInput: (input) => {
      if (input.name === 'q') debouncedSearch();
    },
  });

  Select.init(root, {
    onChange: (element) => {
      if (element.name !== 'hocKyId') return;
      state.hocKyId = Number(element.value);
      state.congNoPage = 1;
      state.canhBaoPage = 1;
      refreshActiveTab();
    },
  });

  Pagination.init(root.querySelector('#congno-container'), {
    onChange: ({ page }) => {
      state.congNoPage = page;
      renderCongNo();
    },
  });
  Pagination.init(root.querySelector('#canhbao-container'), {
    onChange: ({ page }) => {
      state.canhBaoPage = page;
      loadCanhBao();
    },
  });
}

/**
 * @param {HTMLElement} root
 */
export async function mount(root) {
  root.innerHTML = `${renderPageHeader({
    title: 'Báo cáo',
    description: 'Thống kê sinh viên, kết quả học tập, công nợ học phí và cảnh báo học vụ theo học kỳ.',
  })}
  ${tabsMarkup()}
  <div class="page-stack">
    <div class="filter-bar" role="search" id="report-filters"></div>
    ${TABS.map(section).join('')}
  </div>`;

  const hocKys = await api.hocKy.getAll().catch(() => []);
  state.hocKyId = hocKys[0]?.id ?? null;
  root.querySelector('#report-filters').innerHTML = `${Select.render({
    id: 'report-hocky',
    name: 'hocKyId',
    label: 'Học kỳ',
    options: hocKys.map((hk) => ({ value: hk.id, label: `${hk.ten} (${hk.tenNamHoc})` })),
    value: state.hocKyId ?? '',
  })}${Input.render({ name: 'q', label: 'Tìm sinh viên', placeholder: 'MSSV hoặc họ tên', iconLeft: 'search' })}`;

  syncFilters(root);
  bindEvents(root);
  await refreshActiveTab();
}

