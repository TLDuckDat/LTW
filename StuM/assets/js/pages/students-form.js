/**
 * Form thêm/sửa sinh viên, chi tiết, và hộp thoại chuyển trạng thái học tập cho trang Sinh viên.
 * Dữ liệu và trạng thái đúng với backend (enum TrangThaiSinhVien: DangHoc, BaoLuu, ThoiHoc, DaTotNghiep).
 */

import { api } from '../core/api.js';
import { escapeHtml, nextId } from '../core/dom.js';
import { formatDate } from '../core/format.js';
import { Badge } from '../components/badge.js';
import { Button } from '../components/button.js';
import { Input } from '../components/input.js';
import { Modal } from '../components/modal.js';
import { Select } from '../components/select.js';
import { Toast } from '../components/toast.js';

export const GIOI_TINH_LIST = ['Nam', 'Nữ', 'Khác'];

export const STATUS_META = {
  DangHoc: { label: 'Đang học', variant: 'success' },
  BaoLuu: { label: 'Bảo lưu', variant: 'warning' },
  ThoiHoc: { label: 'Thôi học', variant: 'destructive' },
  DaTotNghiep: { label: 'Đã tốt nghiệp', variant: 'info' },
};

/**
 * @param {string} status
 * @returns {string}
 */
export function statusBadge(status) {
  const meta = STATUS_META[status] ?? { label: status, variant: 'neutral' };
  return Badge.render({ label: meta.label, variant: meta.variant });
}

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

/**
 * Kiểm tra một ô theo tên trường. Trường không có quy tắc được xem là hợp lệ.
 * @param {HTMLInputElement} input
 * @returns {boolean}
 */
function validateInput(input) {
  const value = input.value.trim();
  let message = '';
  if (input.name === 'mssv' && value === '') message = 'Vui lòng nhập MSSV';
  if (input.name === 'hoTen' && value === '') message = 'Vui lòng nhập họ và tên';
  if (input.name === 'ngaySinh' && value === '') message = 'Vui lòng chọn ngày sinh';
  if (input.name === 'email' && value !== '' && !EMAIL_PATTERN.test(value)) message = 'Email không đúng định dạng';

  if (message) {
    Input.setError(input, message);
    return false;
  }
  Input.clearError(input);
  return true;
}

/**
 * @param {object|null} record
 * @param {{ nganhs: object[], lops: object[] }} lists
 * @param {string} formId
 * @returns {string}
 */
function buildFormHtml(record, lists, formId) {
  const isEdit = Boolean(record);
  const nganhOptions = lists.nganhs.map((item) => ({ value: item.id, label: `${item.ma} - ${item.ten}` }));
  const lopOptions = [{ value: '', label: 'Chưa phân lớp' }, ...lists.lops.map((item) => ({ value: item.id, label: `${item.ma} - ${item.ten}` }))];
  const gioiTinhOptions = [{ value: '', label: 'Chưa chọn' }, ...GIOI_TINH_LIST.map((item) => ({ value: item, label: item }))];

  return `<form class="modal-form" id="${formId}" novalidate>
    <div class="form-grid">
      ${Input.render({ name: 'mssv', label: 'Mã số sinh viên (MSSV)', value: record?.mssv ?? '', placeholder: '2025000001', required: true, disabled: isEdit, hint: isEdit ? 'MSSV không đổi sau khi tạo' : undefined })}
      ${Input.render({ name: 'hoTen', label: 'Họ và tên', value: record?.hoTen ?? '', placeholder: 'Nguyễn Văn A', required: true })}
      ${Input.render({ name: 'ngaySinh', type: 'date', label: 'Ngày sinh', value: record?.ngaySinh ?? '', required: true })}
      ${Select.render({ name: 'gioiTinh', label: 'Giới tính', value: record?.gioiTinh ?? '', options: gioiTinhOptions })}
      ${Input.render({ name: 'email', type: 'email', label: 'Email', value: record?.email ?? '', placeholder: 'sv@example.com', iconLeft: 'mail' })}
      ${Input.render({ name: 'soDienThoai', label: 'Số điện thoại', value: record?.soDienThoai ?? '' })}
      ${Input.render({ name: 'cccd', label: 'Căn cước công dân', value: record?.cccd ?? '' })}
      ${Select.render({ name: 'nganhId', label: 'Ngành', value: record?.nganhId ?? '', options: nganhOptions, placeholder: 'Chọn ngành', required: true })}
      ${Select.render({ name: 'lopHanhChinhId', label: 'Lớp hành chính', value: record?.lopHanhChinhId ?? '', options: lopOptions })}
      <div class="field-full">${Input.render({ name: 'diaChi', label: 'Địa chỉ', value: record?.diaChi ?? '' })}</div>
    </div>
    <button type="submit" class="sr-only" tabindex="-1" aria-hidden="true">Lưu</button>
  </form>`;
}

/**
 * @param {FormData|HTMLFormElement} form
 * @returns {object} payload khớp CreateSinhVienRequest / UpdateSinhVienRequest
 */
function readPayload(form, isEdit) {
  const text = (name) => form.elements.namedItem(name).value.trim();
  const optionalText = (name) => text(name) || null;
  const optionalId = (name) => (text(name) ? Number(text(name)) : null);

  const payload = {
    hoTen: text('hoTen'),
    ngaySinh: text('ngaySinh'),
    gioiTinh: optionalText('gioiTinh'),
    email: optionalText('email'),
    soDienThoai: optionalText('soDienThoai'),
    cccd: optionalText('cccd'),
    diaChi: optionalText('diaChi'),
    nganhId: Number(text('nganhId')),
    lopHanhChinhId: optionalId('lopHanhChinhId'),
  };
  if (!isEdit) payload.mssv = text('mssv');
  return payload;
}

/**
 * Mở form thêm (record = null) hoặc sửa sinh viên.
 * @param {{ record?: object|null, lists: { nganhs: object[], lops: object[] }, onSaved?: () => void }} options
 */
export function openStudentForm({ record = null, lists, onSaved }) {
  const formId = nextId('student-form');
  const isEdit = Boolean(record);
  const footer = [
    Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
    Button.render({ label: isEdit ? 'Lưu thay đổi' : 'Thêm sinh viên', variant: 'primary', action: 'save' }),
  ].join('');

  const handle = Modal.open({
    title: isEdit ? 'Chỉnh sửa sinh viên' : 'Thêm sinh viên',
    content: buildFormHtml(record, lists, formId),
    footer,
    size: 'lg',
  });

  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);

  Input.init(form, {
    onBlur: (input) => validateInput(input),
    onInput: (input) => {
      if (input.getAttribute('aria-invalid') === 'true') validateInput(input);
    },
  });

  saveButton.addEventListener('click', () => form.requestSubmit());

  form.addEventListener('submit', async (event) => {
    event.preventDefault();
    const inputs = Array.from(form.querySelectorAll('input[name]')).filter((input) => !input.disabled);
    const results = inputs.map(validateInput);
    const nganhSelect = form.elements.namedItem('nganhId');
    const nganhOk = nganhSelect.value !== '';
    if (!nganhOk) Input.setError(nganhSelect, 'Vui lòng chọn ngành');
    const firstInvalid = inputs[results.indexOf(false)];
    if (firstInvalid || !nganhOk) {
      (firstInvalid ?? nganhSelect).focus();
      return;
    }

    const payload = readPayload(form, isEdit);
    Button.setLoading(saveButton, true);
    try {
      if (isEdit) {
        await api.students.update(record.id, payload);
        Toast.success('Cập nhật sinh viên thành công');
      } else {
        await api.students.create(payload);
        Toast.success('Thêm sinh viên thành công');
      }
      handle.close();
      onSaved?.();
    } catch (error) {
      Button.setLoading(saveButton, false);
      Toast.error(error.message);
    }
  });
}

/**
 * Chi tiết sinh viên (chỉ đọc), lấy đầy đủ từ GET /api/sinh-vien/{id}.
 * @param {number} id
 */
export async function openStudentDetail(id) {
  let record;
  try {
    record = await api.students.get(id);
  } catch (error) {
    Toast.error(error.message);
    return;
  }

  const content = `<dl class="detail-list">
    <dt>MSSV</dt><dd>${escapeHtml(record.mssv)}</dd>
    <dt>Họ và tên</dt><dd>${escapeHtml(record.hoTen)}</dd>
    <dt>Ngày sinh</dt><dd>${escapeHtml(formatDate(record.ngaySinh))}</dd>
    <dt>Giới tính</dt><dd>${escapeHtml(record.gioiTinh ?? '—')}</dd>
    <dt>Email</dt><dd>${escapeHtml(record.email ?? '—')}</dd>
    <dt>Số điện thoại</dt><dd>${escapeHtml(record.soDienThoai ?? '—')}</dd>
    <dt>CCCD</dt><dd>${escapeHtml(record.cccd ?? '—')}</dd>
    <dt>Địa chỉ</dt><dd>${escapeHtml(record.diaChi ?? '—')}</dd>
    <dt>Khoa</dt><dd>${escapeHtml(record.tenKhoa ?? '—')}</dd>
    <dt>Ngành</dt><dd>${escapeHtml(record.tenNganh ?? '—')}</dd>
    <dt>Lớp hành chính</dt><dd>${escapeHtml(record.tenLop ?? record.maLop ?? '—')}</dd>
    <dt>Cố vấn học tập</dt><dd>${escapeHtml(record.tenCoVan ?? '—')}</dd>
    <dt>Trạng thái</dt><dd>${statusBadge(record.trangThai)}</dd>
  </dl>`;
  const footer = Button.render({ label: 'Đóng', variant: 'primary', action: 'close' });
  const handle = Modal.open({ title: 'Chi tiết sinh viên', content, footer });
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);
}

const STATUS_ACTIONS = {
  'bao-luu': { title: 'Bảo lưu sinh viên', confirmText: 'Bảo lưu', lyDoRequired: true },
  'thoi-hoc': { title: 'Cho sinh viên thôi học', confirmText: 'Thôi học', lyDoRequired: true, danger: true },
  'quay-lai-hoc': { title: 'Cho sinh viên quay lại học', confirmText: 'Quay lại học', lyDoRequired: false },
};

/**
 * Hộp thoại nhập lý do và số quyết định cho các thao tác trạng thái học tập.
 * @param {{ record: object, action: 'bao-luu'|'thoi-hoc'|'quay-lai-hoc', onDone?: () => void }} options
 */
export function openStatusForm({ record, action, onDone }) {
  const meta = STATUS_ACTIONS[action];
  const formId = nextId('status-form');
  const isResume = action === 'quay-lai-hoc';
  const content = `<form class="modal-form" id="${formId}" novalidate>
    <p class="modal-message">Sinh viên <strong>${escapeHtml(record.hoTen)}</strong> (${escapeHtml(record.mssv)})</p>
    ${isResume
      ? Input.render({ name: 'ghiChu', label: 'Ghi chú', placeholder: 'Tùy chọn' })
      : `${Input.render({ name: 'lyDo', label: 'Lý do', placeholder: 'Nhập lý do', required: true })}
         ${Input.render({ name: 'soQuyetDinh', label: 'Số quyết định', placeholder: 'Tùy chọn' })}`}
  </form>`;
  const footer = [
    Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
    Button.render({ label: meta.confirmText, variant: meta.danger ? 'destructive' : 'primary', action: 'confirm' }),
  ].join('');

  const handle = Modal.open({ title: meta.title, content, footer });
  const form = handle.element.querySelector(`#${formId}`);
  const confirmButton = handle.element.querySelector('[data-action="confirm"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);

  confirmButton.addEventListener('click', async () => {
    const lyDo = form.elements.namedItem('lyDo');
    if (meta.lyDoRequired && lyDo.value.trim() === '') {
      Input.setError(lyDo, 'Vui lòng nhập lý do');
      lyDo.focus();
      return;
    }

    const body = isResume
      ? { ghiChu: form.elements.namedItem('ghiChu').value.trim() || null }
      : {
          lyDo: lyDo.value.trim(),
          soQuyetDinh: form.elements.namedItem('soQuyetDinh').value.trim() || null,
        };

    Button.setLoading(confirmButton, true);
    try {
      await api.students.changeStatus(record.id, action, body);
      Toast.success(`${meta.confirmText} thành công`);
      handle.close();
      onDone?.();
    } catch (error) {
      Button.setLoading(confirmButton, false);
      Toast.error(error.message);
    }
  });
}
