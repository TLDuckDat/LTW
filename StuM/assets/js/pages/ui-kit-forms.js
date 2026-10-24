/**
 * Demo ui-kit: Button, Input, Select.
 * Mỗi hàm nhận prefix để id không trùng giữa panel light và dark.
 */

import { Button } from '../components/button.js';
import { Input } from '../components/input.js';
import { Select } from '../components/select.js';

const VARIANTS = ['primary', 'secondary', 'outline', 'ghost', 'destructive'];
const SIZES = ['sm', 'md', 'lg'];

const FACULTY_OPTIONS = [
  { value: 'cntt', label: 'Công nghệ thông tin' },
  { value: 'kt', label: 'Kinh tế' },
  { value: 'nn', label: 'Ngoại ngữ' },
  { value: 'ck', label: 'Cơ khí' },
];

/** @returns {string} */
export function renderButtons() {
  const rows = VARIANTS.map(
    (variant) => `
    <div class="ui-kit-row">
      <span class="ui-kit-row-label">${variant}</span>
      <div class="ui-kit-row-items">
        ${SIZES.map((size) => Button.render({ label: `Nút ${size}`, variant, size })).join('')}
        ${Button.render({ label: 'Có icon', variant, icon: 'plus' })}
        ${Button.render({ variant, iconOnly: true, icon: 'pencil', ariaLabel: 'Sửa' })}
        ${Button.render({ label: 'Đang lưu', variant, loading: true })}
        ${Button.render({ label: 'Vô hiệu', variant, disabled: true })}
      </div>
    </div>`,
  ).join('');

  const interactive = `
    <div class="ui-kit-row">
      <span class="ui-kit-row-label">Tương tác</span>
      <div class="ui-kit-row-items">
        ${Button.render({ label: 'Lưu (mô phỏng 1.5s)', variant: 'primary', action: 'demo-save' })}
        ${Button.render({ label: 'Nút block', variant: 'outline', block: true })}
      </div>
    </div>`;

  return rows + interactive;
}

/**
 * @param {string} prefix
 * @returns {string}
 */
export function renderInputs(prefix) {
  const id = (name) => `${prefix}-${name}`;
  return `<div class="ui-kit-form">
    <div class="ui-kit-fields">
      ${Input.render({ id: id('fullName'), name: 'fullName', label: 'Họ và tên', placeholder: 'Nguyễn Văn A', required: true, hint: 'Theo giấy tờ tùy thân' })}
      ${Input.render({ id: id('email'), name: 'email', type: 'email', label: 'Email', placeholder: 'ten@stum.edu.vn', iconLeft: 'mail', required: true })}
      ${Input.render({ id: id('gpa'), name: 'gpa', type: 'number', label: 'GPA', value: 3.45, min: 0, max: 4, step: 0.01, hint: 'Thang điểm 4' })}
      ${Input.render({ id: id('search'), name: 'search', label: 'Tìm kiếm', placeholder: 'Nhập MSSV', iconLeft: 'search' })}
      ${Input.render({ id: id('password'), name: 'password', type: 'password', label: 'Mật khẩu', iconLeft: 'lock', autocomplete: 'current-password', required: true })}
      ${Input.render({ id: id('date'), name: 'date', type: 'date', label: 'Ngày sinh', value: '2004-03-12' })}
      ${Input.render({ id: id('mssv'), name: 'mssv', label: 'MSSV', value: 'SV0O1', error: 'MSSV phải gồm 6 chữ số' })}
      ${Input.render({ id: id('disabled'), name: 'classCode', label: 'Mã lớp (vô hiệu)', value: 'CNTT-K20', disabled: true })}
    </div>
    <div class="ui-kit-row-items">
      ${Button.render({ label: 'Kiểm tra form', variant: 'primary', action: 'demo-validate' })}
    </div>
  </div>`;
}

/**
 * @param {string} prefix
 * @returns {string}
 */
export function renderSelects(prefix) {
  const id = (name) => `${prefix}-${name}`;
  return `<div class="ui-kit-fields">
    ${Select.render({ id: id('faculty'), name: 'faculty', label: 'Khoa (dropdown)', options: FACULTY_OPTIONS, value: 'cntt', hint: 'Dropdown chuẩn của trình duyệt, đã style' })}
    ${Select.render({ id: id('faculty-search'), name: 'facultySearch', label: 'Khoa (có tìm kiếm)', options: FACULTY_OPTIONS, value: 'kt', searchable: true, placeholder: 'Gõ để tìm khoa...' })}
    ${Select.render({ id: id('faculty-error'), name: 'facultyError', label: 'Khoa (lỗi)', options: FACULTY_OPTIONS, value: '', placeholder: 'Chọn khoa', required: true, error: 'Vui lòng chọn khoa' })}
    ${Select.render({ id: id('faculty-disabled'), name: 'facultyDisabled', label: 'Khoa (vô hiệu)', options: FACULTY_OPTIONS, value: 'nn', disabled: true })}
  </div>`;
}
