/**
 * Form thêm/sửa sinh viên, xem chi tiết và hằng số dùng chung cho trang Sinh viên.
 */

import { api } from '../core/api.js';
import { escapeHtml, nextId } from '../core/dom.js';
import { formatDate, formatNumber } from '../core/format.js';
import { Badge } from '../components/badge.js';
import { Button } from '../components/button.js';
import { Input } from '../components/input.js';
import { Modal } from '../components/modal.js';
import { Select } from '../components/select.js';
import { Toast } from '../components/toast.js';

export const KHOA_LIST = ['CNTT', 'Kinh tế', 'Ngoại ngữ', 'Cơ khí'];

export const STATUS_META = {
  active: { label: 'Đang học', variant: 'success' },
  graduated: { label: 'Đã tốt nghiệp', variant: 'info' },
  suspended: { label: 'Bảo lưu', variant: 'warning' },
  dropped: { label: 'Thôi học', variant: 'destructive' },
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
const MSSV_PATTERN = /^SV\d{3,}$/i;

/** Quy tắc kiểm tra theo tên field. Field không có quy tắc được xem là hợp lệ. */
const RULES = {
  mssv: { required: true, check: (value) => MSSV_PATTERN.test(value), message: 'MSSV có dạng SV kèm ít nhất 3 chữ số, ví dụ SV011' },
  hoTen: { required: true, message: 'Vui lòng nhập họ và tên' },
  email: { required: true, check: (value) => EMAIL_PATTERN.test(value), message: 'Email không đúng định dạng' },
  ngaySinh: { required: true, check: (value) => new Date(value) <= new Date(), message: 'Ngày sinh không hợp lệ' },
  lop: { required: true, message: 'Vui lòng nhập lớp' },
  gpa: {
    required: true,
    check: (value) => Number(value) >= 0 && Number(value) <= 4,
    message: 'GPA phải nằm trong khoảng 0 - 4',
  },
};

/**
 * @param {HTMLInputElement} input
 * @returns {boolean}
 */
function validateInput(input) {
  const rule = RULES[input.name];
  if (!rule) return true;

  const value = input.value.trim();
  let message = '';
  if (rule.required && value === '') {
    message = rule.message ?? 'Trường này là bắt buộc';
  } else if (value !== '' && rule.check && !rule.check(value)) {
    message = rule.message;
  }

  if (message) {
    Input.setError(input, message);
    return false;
  }
  Input.clearError(input);
  return true;
}

function buildFormHtml(record, formId) {
  return `<form class="modal-form" id="${formId}" novalidate>
    <div class="form-grid">
      ${Input.render({ name: 'mssv', label: 'Mã sinh viên (MSSV)', value: record?.mssv ?? '', placeholder: 'SV011', required: true, hint: 'Định dạng SV + số, ví dụ SV011' })}
      ${Input.render({ name: 'hoTen', label: 'Họ và tên', value: record?.hoTen ?? '', placeholder: 'Nguyễn Văn A', required: true })}
      ${Input.render({ name: 'email', type: 'email', label: 'Email', value: record?.email ?? '', placeholder: 'sv011@sv.stum.edu.vn', required: true, iconLeft: 'mail' })}
      ${Input.render({ name: 'ngaySinh', type: 'date', label: 'Ngày sinh', value: record?.ngaySinh ?? '', required: true })}
      ${Select.render({ name: 'gioiTinh', label: 'Giới tính', value: record?.gioiTinh ?? 'Nam', options: ['Nam', 'Nữ', 'Khác'].map((item) => ({ value: item, label: item })) })}
      ${Select.render({ name: 'khoa', label: 'Khoa', value: record?.khoa ?? KHOA_LIST[0], options: KHOA_LIST.map((item) => ({ value: item, label: item })) })}
      ${Input.render({ name: 'lop', label: 'Lớp', value: record?.lop ?? '', placeholder: 'CNTT-K20', required: true })}
      ${Input.render({ name: 'gpa', type: 'number', label: 'GPA (thang 4)', value: record?.gpa ?? '', min: 0, max: 4, step: 0.01, required: true })}
      ${Select.render({ name: 'trangThai', label: 'Trạng thái', value: record?.trangThai ?? 'active', options: Object.entries(STATUS_META).map(([valueKey, meta]) => ({ value: valueKey, label: meta.label })) })}
    </div>
    <button type="submit" class="sr-only" tabindex="-1" aria-hidden="true">Lưu</button>
  </form>`;
}

/**
 * Mở form thêm (record = null) hoặc sửa sinh viên.
 * @param {{ record?: object|null, onSaved?: () => void }} options
 */
export function openStudentForm({ record = null, onSaved } = {}) {
  const formId = nextId('student-form');
  const isEdit = Boolean(record);
  const footer = [
    Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
    Button.render({ label: isEdit ? 'Lưu thay đổi' : 'Thêm sinh viên', variant: 'primary', action: 'save' }),
  ].join('');

  const handle = Modal.open({
    title: isEdit ? 'Chỉnh sửa sinh viên' : 'Thêm sinh viên',
    content: buildFormHtml(record, formId),
    footer,
    size: 'lg',
  });

  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);

  Input.init(form, {
    onBlur: (input) => {
      if (input.name in RULES) validateInput(input);
    },
    onInput: (input) => {
      if (input.getAttribute('aria-invalid') === 'true') validateInput(input);
    },
  });

  saveButton.addEventListener('click', () => form.requestSubmit());

  form.addEventListener('submit', async (event) => {
    event.preventDefault();
    const inputs = Array.from(form.querySelectorAll('input[name]'));
    const results = inputs.map(validateInput);
    const firstInvalid = inputs[results.indexOf(false)];
    if (firstInvalid) {
      firstInvalid.focus();
      return;
    }

    const field = (name) => form.elements.namedItem(name).value.trim();
    const payload = {
      mssv: field('mssv').toUpperCase(),
      hoTen: field('hoTen'),
      email: field('email'),
      ngaySinh: field('ngaySinh'),
      gioiTinh: field('gioiTinh'),
      khoa: field('khoa'),
      lop: field('lop'),
      gpa: Number(field('gpa')),
      trangThai: field('trangThai'),
    };

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
      if (error.status === 409) {
        Input.setError(form.elements.namedItem('mssv'), error.message);
      } else {
        Toast.error(error.message);
      }
    }
  });
}

/**
 * Hộp thoại xem chi tiết sinh viên (chỉ đọc).
 * @param {object} record
 */
export function openStudentDetail(record) {
  const content = `<dl class="detail-list">
    <dt>MSSV</dt><dd>${escapeHtml(record.mssv)}</dd>
    <dt>Họ và tên</dt><dd>${escapeHtml(record.hoTen)}</dd>
    <dt>Email</dt><dd>${escapeHtml(record.email)}</dd>
    <dt>Ngày sinh</dt><dd>${escapeHtml(formatDate(record.ngaySinh))}</dd>
    <dt>Giới tính</dt><dd>${escapeHtml(record.gioiTinh)}</dd>
    <dt>Khoa</dt><dd>${escapeHtml(record.khoa)}</dd>
    <dt>Lớp</dt><dd>${escapeHtml(record.lop)}</dd>
    <dt>GPA</dt><dd>${escapeHtml(formatNumber(record.gpa, 2))}</dd>
    <dt>Trạng thái</dt><dd>${statusBadge(record.trangThai)}</dd>
  </dl>`;
  const footer = Button.render({ label: 'Đóng', variant: 'primary', action: 'close' });
  const handle = Modal.open({ title: 'Chi tiết sinh viên', content, footer });
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);
}
