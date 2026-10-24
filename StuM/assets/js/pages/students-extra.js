/**
 * Hộp thoại bổ sung cho trang Sinh viên: đổi lớp hành chính, đổi trạng thái học tập (PUT),
 * lịch sử học tập, cảnh báo học vụ của một sinh viên và danh sách sinh viên bị cảnh báo trong học kỳ.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { escapeHtml, nextId } from '../core/dom.js';
import { formatDate, formatNumber } from '../core/format.js';
import { Badge } from '../components/badge.js';
import { Button } from '../components/button.js';
import { EmptyState } from '../components/empty-state.js';
import { Modal } from '../components/modal.js';
import { Pagination } from '../components/pagination.js';
import { Select } from '../components/select.js';
import { Skeleton } from '../components/skeleton.js';
import { Table } from '../components/table.js';
import { Toast } from '../components/toast.js';
import { Input } from '../components/input.js';
import { STATUS_META, statusBadge } from './students-form.js';

const CANH_BAO = {
  CanhBao: { label: 'Cảnh báo học vụ', variant: 'destructive' },
  KhongCanhBao: { label: 'Không cảnh báo', variant: 'success' },
  ChuaDuDuLieu: { label: 'Chưa đủ dữ liệu', variant: 'neutral' },
};

const dash = (value) => (value === null || value === undefined || value === '' ? '—' : escapeHtml(value));
const decimal = (value) => escapeHtml(formatNumber(value, 2) || '—');

/**
 * Hộp thoại có form và nút lưu. onSubmit(form) trả về promise; lỗi được hiển thị và hộp thoại giữ nguyên.
 */
function openFormDialog({ title, content, confirmText = 'Lưu', danger = false, successMessage, onSubmit, onDone }) {
  const handle = Modal.open({
    title,
    content,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: confirmText, variant: danger ? 'destructive' : 'primary', action: 'save' }),
    ].join(''),
  });
  const form = handle.element.querySelector('form');
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);

  saveButton.addEventListener('click', async () => {
    Button.setLoading(saveButton, true);
    try {
      await onSubmit(form);
      Toast.success(successMessage);
      handle.close();
      onDone?.();
    } catch (error) {
      Button.setLoading(saveButton, false);
      Toast.error(error.message);
    }
  });
}

/** Đổi lớp hành chính. Chọn "Gỡ khỏi lớp" để gửi lopHanhChinhId = null. */
export function openChangeClassModal({ record, lops, onDone }) {
  const options = [
    { value: '', label: 'Gỡ khỏi lớp hành chính' },
    ...lops.map((lop) => ({ value: lop.id, label: `${lop.ma} - ${lop.ten}` })),
  ];
  openFormDialog({
    title: `Đổi lớp hành chính · ${record.mssv}`,
    content: `<form class="modal-form" id="${nextId('class-form')}" novalidate>
      <p class="modal-message">Sinh viên <strong>${escapeHtml(record.hoTen)}</strong>, lớp hiện tại: ${dash(record.tenLopHanhChinh)}.</p>
      ${Select.render({ name: 'lopHanhChinhId', label: 'Lớp hành chính mới', options, value: record.lopHanhChinhId ?? '' })}
    </form>`,
    successMessage: 'Đã cập nhật lớp hành chính',
    onSubmit: (form) => {
      const value = form.elements.namedItem('lopHanhChinhId').value;
      return api.students.chuyenLop(record.id, value === '' ? null : Number(value));
    },
    onDone,
  });
}

/** Đổi trạng thái học tập bằng PUT /trang-thai (dùng khi cần chọn trực tiếp trạng thái đích). */
export function openChangeStatusModal({ record, onDone }) {
  const options = Object.entries(STATUS_META).map(([value, meta]) => ({ value, label: meta.label }));
  openFormDialog({
    title: `Đổi trạng thái học tập · ${record.mssv}`,
    content: `<form class="modal-form" id="${nextId('status-form')}" novalidate>
      ${Select.render({ name: 'trangThai', label: 'Trạng thái mới', options, value: record.trangThai })}
      ${Input.render({ name: 'lyDo', label: 'Lý do', placeholder: 'Tùy chọn' })}
      ${Input.render({ name: 'soQuyetDinh', label: 'Số quyết định', placeholder: 'Tùy chọn' })}
    </form>`,
    successMessage: 'Đã cập nhật trạng thái học tập',
    onSubmit: (form) => {
      const field = (name) => form.elements.namedItem(name).value.trim() || null;
      return api.students.trangThai(record.id, {
        trangThai: form.elements.namedItem('trangThai').value,
        lyDo: field('lyDo'),
        soQuyetDinh: field('soQuyetDinh'),
      });
    },
    onDone,
  });
}

/** Lịch sử thay đổi trạng thái học tập của một sinh viên. */
export async function openStudentHistory(record) {
  let rows;
  try {
    rows = await api.students.lichSuHocTap(record.id);
  } catch (error) {
    Toast.error(error.message);
    return;
  }

  const content = rows.length
    ? Table.render({
        caption: 'Lịch sử học tập',
        rows,
        rowKey: 'id',
        columns: [
          { key: 'ngayHieuLuc', label: 'Ngày hiệu lực', primary: true, render: (r) => dash(formatDate(r.ngayHieuLuc)) },
          { key: 'trangThaiCu', label: 'Từ', render: (r) => statusBadge(r.trangThaiCu) },
          { key: 'trangThaiMoi', label: 'Sang', render: (r) => statusBadge(r.trangThaiMoi) },
          { key: 'soQuyetDinh', label: 'Số quyết định', render: (r) => dash(r.soQuyetDinh) },
          { key: 'lyDo', label: 'Lý do', render: (r) => dash(r.lyDo) },
        ],
      })
    : '<p class="form-hint">Sinh viên chưa có thay đổi trạng thái học tập.</p>';
  const handle = Modal.open({
    title: `Lịch sử học tập · ${record.mssv}`,
    size: 'lg',
    content,
    footer: Button.render({ label: 'Đóng', variant: 'primary', action: 'close' }),
  });
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);
}

/** Bảng chọn học kỳ dùng chung cho hai hộp thoại cảnh báo. */
function hocKyOptions(hocKys) {
  return hocKys.map((hk) => ({ value: hk.id, label: hk.tenNamHoc ? `${hk.ten} (${hk.tenNamHoc})` : hk.ten }));
}

function canhBaoMarkup(data) {
  const meta = CANH_BAO[data.trangThaiCanhBao] ?? { label: data.trangThaiCanhBao, variant: 'neutral' };
  return `<dl class="detail-list">
    <dt>Học kỳ</dt><dd>${dash(data.tenHocKy)}</dd>
    <dt>GPA / CPA</dt><dd>${decimal(data.gpa)} / ${decimal(data.cpa)}</dd>
    <dt>Tín chỉ đăng ký</dt><dd>${dash(data.tongTinChiDangKy)}</dd>
    <dt>Tín chỉ không đạt</dt><dd>${dash(data.soTinChiKhongDat)}</dd>
    <dt>Đánh giá</dt><dd>${Badge.render({ label: meta.label, variant: meta.variant })}</dd>
    <dt>Lý do</dt><dd>${dash((data.danhSachLyDo ?? []).join('; '))}</dd>
  </dl>`;
}

/** Cảnh báo học vụ của một sinh viên, chọn học kỳ để xem. */
export async function openStudentWarning(record) {
  let hocKys;
  try {
    hocKys = await api.hocKy.getAll();
  } catch (error) {
    Toast.error(error.message);
    return;
  }

  const handle = Modal.open({
    title: `Cảnh báo học vụ · ${record.mssv}`,
    size: 'lg',
    content: `<div class="modal-form">
      <div class="filter-bar">${Select.render({ name: 'hocKyId', label: 'Học kỳ', options: hocKyOptions(hocKys), value: hocKys[0]?.id ?? '' })}</div>
      <div data-warning-body></div>
    </div>`,
    footer: Button.render({ label: 'Đóng', variant: 'primary', action: 'close' }),
  });
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);
  const body = handle.element.querySelector('[data-warning-body]');

  const load = (hocKyId) =>
    renderAsync(body, {
      load: () => api.diem.canhBaoSinhVien(record.id, hocKyId),
      skeleton: () => Skeleton.text(5),
      isEmpty: (data) => !data,
      empty: () => EmptyState.render({ icon: 'shield-check', title: 'Không có dữ liệu cảnh báo' }),
      render: canhBaoMarkup,
      errorTitle: 'Không tải được cảnh báo',
    });

  handle.element.querySelector('select[name="hocKyId"]').addEventListener('change', (event) => load(Number(event.target.value)));
  if (hocKys[0]) load(hocKys[0].id);
  else body.innerHTML = EmptyState.render({ icon: 'calendar', title: 'Chưa có học kỳ nào' });
}

/** Danh sách sinh viên bị cảnh báo học vụ trong một học kỳ, có phân trang. */
export async function openWarningList() {
  let hocKys;
  try {
    hocKys = await api.hocKy.getAll();
  } catch (error) {
    Toast.error(error.message);
    return;
  }

  let hocKyId = hocKys[0]?.id;
  let page = 1;
  let pageSize = 10;
  const handle = Modal.open({
    title: 'Sinh viên bị cảnh báo học vụ',
    size: 'lg',
    content: `<div class="modal-form">
      <div class="filter-bar">${Select.render({ name: 'hocKyId', label: 'Học kỳ', options: hocKyOptions(hocKys), value: hocKyId ?? '' })}</div>
      <div data-warning-body></div>
    </div>`,
    footer: Button.render({ label: 'Đóng', variant: 'primary', action: 'close' }),
  });
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);
  const body = handle.element.querySelector('[data-warning-body]');

  const draw = () =>
    renderAsync(body, {
      load: () => api.diem.canhBaoDanhSach(hocKyId, { page, pageSize }),
      skeleton: () => Skeleton.table(5, 4),
      isEmpty: (result) => result.total === 0,
      empty: () => EmptyState.render({ icon: 'shield-check', title: 'Không có sinh viên bị cảnh báo', description: 'Trong học kỳ này chưa có sinh viên nào bị cảnh báo học vụ.' }),
      render: (result) =>
        Table.render({
          caption: 'Sinh viên bị cảnh báo học vụ',
          rows: result.data,
          rowKey: 'sinhVienId',
          columns: [
            { key: 'mssv', label: 'MSSV', primary: true },
            { key: 'hoTen', label: 'Họ và tên' },
            { key: 'gpa', label: 'GPA', align: 'right', render: (r) => decimal(r.gpa) },
            { key: 'soTinChiKhongDat', label: 'TC không đạt', align: 'right' },
            { key: 'danhSachLyDo', label: 'Lý do', render: (r) => dash((r.danhSachLyDo ?? []).join('; ')) },
          ],
          footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
        }),
      errorTitle: 'Không tải được danh sách cảnh báo',
    });

  Pagination.init(body, {
    onChange: (next) => {
      page = next.page;
      pageSize = next.pageSize;
      draw();
    },
  });
  handle.element.querySelector('select[name="hocKyId"]').addEventListener('change', (event) => {
    hocKyId = Number(event.target.value);
    page = 1;
    draw();
  });
  if (hocKyId) draw();
  else body.innerHTML = EmptyState.render({ icon: 'calendar', title: 'Chưa có học kỳ nào' });
}
