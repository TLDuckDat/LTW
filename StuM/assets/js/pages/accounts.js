/**
 * Trang Quản trị tài khoản (chỉ Admin): danh sách tài khoản, tạo, đổi vai trò, liên kết hồ sơ,
 * khóa/mở khóa, đặt lại mật khẩu; và nhật ký hoạt động có bộ lọc. Dữ liệu từ /api/auth/*.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { debounce, escapeHtml } from '../core/dom.js';
import { formatDate } from '../core/format.js';
import { Badge } from '../components/badge.js';
import { Button } from '../components/button.js';
import { EmptyState } from '../components/empty-state.js';
import { Input } from '../components/input.js';
import { Modal } from '../components/modal.js';
import { Pagination } from '../components/pagination.js';
import { Select } from '../components/select.js';
import { Skeleton } from '../components/skeleton.js';
import { Table } from '../components/table.js';
import { Toast } from '../components/toast.js';
import { renderPageHeader } from '../layout/shell.js';
import {
  openCreateAccountModal,
  openLinkModal,
  openResetPasswordModal,
  openRoleModal,
  VAI_TRO,
} from './accounts-modals.js';
import { openRecordDetailModal } from './graduation-forms.js';

const ROW_ACTIONS = [
  { action: 'detail', label: 'Xem chi tiết', icon: 'eye' },
  { action: 'role', label: 'Đổi vai trò', icon: 'shield' },
  { action: 'link-student', label: 'Liên kết sinh viên', icon: 'graduation-cap' },
  { action: 'link-lecturer', label: 'Liên kết giảng viên', icon: 'user-round' },
  { divider: true },
  { action: 'reset', label: 'Đặt lại mật khẩu', icon: 'key-round' },
  { action: 'lock', label: 'Khóa tài khoản', icon: 'lock', danger: true },
  { action: 'unlock', label: 'Mở khóa tài khoản', icon: 'lock-open' },
];

const state = { search: '', page: 1, pageSize: 10, accounts: [], logPage: 1, logPageSize: 10 };
let refs = { students: [], lecturers: [] };

const badgeVaiTro = (vaiTro) => Badge.render({ label: VAI_TRO[vaiTro] ?? vaiTro, variant: vaiTro === 'Admin' ? 'primary' : 'neutral' });

function accountsTable(result) {
  return Table.render({
    columns: [
      { key: 'tenDangNhap', label: 'Tên đăng nhập', primary: true },
      { key: 'hoTen', label: 'Họ và tên' },
      { key: 'vaiTro', label: 'Vai trò', render: (row) => badgeVaiTro(row.vaiTro) },
      {
        key: 'lienKet',
        label: 'Hồ sơ liên kết',
        render: (row) => escapeHtml(row.sinhVienId ? `Sinh viên #${row.sinhVienId}` : row.giangVienId ? `Giảng viên #${row.giangVienId}` : '—'),
      },
      {
        key: 'daKhoa',
        label: 'Trạng thái',
        render: (row) => (row.daKhoa ? Badge.render({ label: 'Đã khóa', variant: 'destructive' }) : Badge.render({ label: 'Hoạt động', variant: 'success' })),
      },
      { key: 'ngayTao', label: 'Ngày tạo', render: (row) => escapeHtml(formatDate(row.ngayTao)) },
    ],
    rows: result.data,
    actions: ROW_ACTIONS,
    actionLabel: (row) => `Thao tác cho ${row.tenDangNhap}`,
    caption: 'Danh sách tài khoản',
    footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
  });
}

function loadAccounts() {
  return renderAsync(document.getElementById('accounts-container'), {
    load: async () => {
      const result = await api.taiKhoan.list({ page: state.page, pageSize: state.pageSize, search: state.search });
      state.accounts = result.data;
      return result;
    },
    skeleton: () => Skeleton.table(5, 6),
    isEmpty: (result) => result.total === 0,
    empty: () => EmptyState.render({ icon: 'users', title: 'Không có tài khoản nào', description: 'Thử thay đổi từ khóa tìm kiếm hoặc tạo tài khoản mới.' }),
    render: accountsTable,
    errorTitle: 'Không tải được danh sách tài khoản',
  });
}

function logsTable(result) {
  return Table.render({
    columns: [
      { key: 'thoiGian', label: 'Thời điểm', primary: true, render: (row) => escapeHtml(new Date(row.thoiGian).toLocaleString('vi-VN')) },
      { key: 'tenDangNhap', label: 'Người thực hiện', render: (row) => escapeHtml(row.tenDangNhap ?? '—') },
      { key: 'hanhDong', label: 'Hành động', render: (row) => escapeHtml(row.hanhDong) },
      { key: 'duongDan', label: 'Đường dẫn', render: (row) => `<span class="text-caption">${escapeHtml(row.phuongThuc)} ${escapeHtml(row.duongDan)}</span>` },
      { key: 'maPhanHoi', label: 'Kết quả', render: (row) => Badge.render({ label: `${row.maPhanHoi}`, variant: row.thanhCong ? 'success' : 'destructive' }) },
      { key: 'diaChiIp', label: 'IP', render: (row) => escapeHtml(row.diaChiIp ?? '—') },
    ],
    rows: result.data,
    rowKey: 'id',
    caption: 'Nhật ký hoạt động',
    footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
  });
}

function logFilters(root) {
  const value = (name) => root.querySelector(`[name="${name}"]`)?.value.trim() ?? '';
  return {
    hanhDong: value('hanhDong') || undefined,
    thanhCong: value('thanhCong') || undefined,
    tuNgay: value('tuNgay') || undefined,
    denNgay: value('denNgay') || undefined,
    search: value('logSearch') || undefined,
  };
}

function loadLogs(root) {
  return renderAsync(root.querySelector('#logs-container'), {
    load: () => api.taiKhoan.nhatKy({ page: state.logPage, pageSize: state.logPageSize, ...logFilters(root) }),
    skeleton: () => Skeleton.table(5, 6),
    isEmpty: (result) => result.total === 0,
    empty: () => EmptyState.render({ icon: 'clipboard-list', title: 'Chưa có nhật ký', description: 'Thử bỏ bộ lọc.' }),
    render: logsTable,
    errorTitle: 'Không tải được nhật ký hoạt động',
  });
}

function handleRowAction(action, rowId) {
  const record = state.accounts.find((row) => String(row.id) === rowId);
  if (!record) return;

  if (action === 'detail') {
    openRecordDetailModal({
      title: `Tài khoản ${record.tenDangNhap}`,
      load: () => api.taiKhoan.get(record.id),
      labels: { tenDangNhap: 'Tên đăng nhập', hoTen: 'Họ và tên', vaiTro: 'Vai trò', daKhoa: 'Bị khóa', ngayTao: 'Ngày tạo', sinhVienId: 'Mã sinh viên liên kết', giangVienId: 'Mã giảng viên liên kết' },
    });
  }
  if (action === 'role') openRoleModal({ record, onDone: loadAccounts });
  if (action === 'link-student') {
    openLinkModal({ record, kind: 'sinhVien', options: refs.students, onDone: loadAccounts });
  }
  if (action === 'link-lecturer') {
    openLinkModal({ record, kind: 'giangVien', options: refs.lecturers, onDone: loadAccounts });
  }
  if (action === 'reset') openResetPasswordModal({ record, onDone: loadAccounts });
  if (action === 'lock' || action === 'unlock') {
    const lock = action === 'lock';
    Modal.confirm({
      title: lock ? 'Khóa tài khoản' : 'Mở khóa tài khoản',
      message: lock
        ? `Khóa ${record.tenDangNhap}? Tài khoản sẽ không đăng nhập được và mọi phiên hiện có bị thu hồi.`
        : `Mở khóa ${record.tenDangNhap}?`,
      confirmText: lock ? 'Khóa' : 'Mở khóa',
      danger: lock,
      onConfirm: async () => {
        try {
          if (lock) await api.taiKhoan.khoa(record.id);
          else await api.taiKhoan.moKhoa(record.id);
          Toast.success(lock ? 'Đã khóa tài khoản' : 'Đã mở khóa tài khoản');
          loadAccounts();
        } catch (error) {
          Toast.error(error.message);
          throw error;
        }
      },
    }).catch(() => {});
  }
}

/**
 * @param {HTMLElement} root
 */
function bindEvents(root) {
  const debouncedSearch = debounce(() => {
    state.search = root.querySelector('[name="q"]').value.trim();
    state.page = 1;
    loadAccounts();
  }, 300);

  Input.init(root, {
    onInput: (input) => {
      if (input.name === 'q') debouncedSearch();
      if (input.name === 'logSearch' || input.name === 'hanhDong') debouncedLogs();
    },
  });

  const debouncedLogs = debounce(() => {
    state.logPage = 1;
    loadLogs(root);
  }, 300);

  root.addEventListener('change', (event) => {
    if (['thanhCong', 'tuNgay', 'denNgay'].includes(event.target.name)) {
      state.logPage = 1;
      loadLogs(root);
    }
  });

  Button.init(root, {
    onAction: (action) => {
      if (action === 'create') {
        openCreateAccountModal({
          sinhVienOptions: refs.students,
          giangVienOptions: refs.lecturers,
          onDone: loadAccounts,
        });
      }
      if (action === 'reset-logs') {
        root.querySelectorAll('[data-log-filter]').forEach((input) => {
          input.value = '';
        });
        state.logPage = 1;
        loadLogs(root);
      }
    },
  });

  Table.init(root, { onAction: (action, rowId) => handleRowAction(action, rowId) });

  Pagination.init(root.querySelector('#accounts-container'), {
    onChange: ({ page, pageSize }) => {
      state.page = page;
      state.pageSize = pageSize;
      loadAccounts();
    },
  });
  Pagination.init(root.querySelector('#logs-container'), {
    onChange: ({ page, pageSize }) => {
      state.logPage = page;
      state.logPageSize = pageSize;
      loadLogs(root);
    },
  });
}

/**
 * @param {HTMLElement} root
 */
export async function mount(root) {
  root.innerHTML = `${renderPageHeader({
    title: 'Quản trị tài khoản',
    description: 'Cấp tài khoản, phân vai trò, liên kết hồ sơ và xem nhật ký hoạt động của hệ thống.',
    actions: Button.render({ label: 'Tạo tài khoản', variant: 'primary', icon: 'plus', action: 'create' }),
  })}
  <section class="card" aria-label="Danh sách tài khoản">
    <div class="filter-bar" role="search">
      ${Input.render({ id: 'acc-search', name: 'q', label: 'Tìm tài khoản', placeholder: 'Tên đăng nhập, họ tên', iconLeft: 'search' })}
    </div>
    <div class="card-body is-flush" id="accounts-container"></div>
  </section>
  <section class="card" aria-label="Nhật ký hoạt động" style="margin-top: 24px;">
    <header class="card-header"><div class="card-header-text"><h2 class="card-title">Nhật ký hoạt động</h2><p class="card-subtitle">Đăng nhập, đăng xuất và mọi thao tác ghi của người dùng</p></div></header>
    <div class="filter-bar" role="search">
      ${Input.render({ name: 'logSearch', label: 'Tìm kiếm', placeholder: 'Người dùng, đường dẫn', iconLeft: 'search', }).replace('name="logSearch"', 'name="logSearch" data-log-filter')}
      ${Input.render({ name: 'hanhDong', label: 'Hành động', placeholder: 'Ví dụ: HocPhi.' }).replace('name="hanhDong"', 'name="hanhDong" data-log-filter')}
      ${Select.render({ name: 'thanhCong', label: 'Kết quả', value: '', options: [{ value: '', label: 'Tất cả' }, { value: 'true', label: 'Thành công' }, { value: 'false', label: 'Thất bại' }] }).replace('<select', '<select data-log-filter')}
      ${Input.render({ name: 'tuNgay', type: 'date', label: 'Từ ngày' }).replace('name="tuNgay"', 'name="tuNgay" data-log-filter')}
      ${Input.render({ name: 'denNgay', type: 'date', label: 'Đến ngày' }).replace('name="denNgay"', 'name="denNgay" data-log-filter')}
      ${Button.render({ label: 'Đặt lại', variant: 'ghost', action: 'reset-logs' })}
    </div>
    <div class="card-body is-flush" id="logs-container"></div>
  </section>`;

  bindEvents(root);

  const [students, lecturers] = await Promise.all([
    api.students.list({ pageSize: 100 }).then((r) => r.data).catch(() => []),
    api.giangVien.getAll().catch(() => []),
  ]);
  refs = {
    students: students.map((sv) => ({ value: sv.id, label: `${sv.mssv} - ${sv.hoTen}` })),
    lecturers: lecturers.map((gv) => ({ value: gv.id, label: `${gv.ma} - ${gv.hoTen}` })),
  };

  await Promise.all([loadAccounts(), loadLogs(root)]);
}
