/**
 * Hộp thoại dùng chung cho module Tốt nghiệp và Báo cáo:
 * - openFormModal: form tổng quát theo danh sách trường (text, date, number, select)
 * - openDetailModal: hiển thị chi tiết dạng khóa - giá trị
 */

import { escapeHtml, nextId } from '../core/dom.js';
import { formatDate } from '../core/format.js';
import { Button } from '../components/button.js';
import { Input } from '../components/input.js';
import { Modal } from '../components/modal.js';
import { Select } from '../components/select.js';
import { Toast } from '../components/toast.js';

/**
 * Chuyển giá trị ô form theo kiểu của trường.
 * @param {{ kind: string, numeric?: boolean }} field
 * @param {string} raw
 */
function valueOf(field, raw) {
  const value = String(raw ?? '').trim();
  if (value === '') return null;
  if (field.kind === 'number' || field.numeric) return Number(value);
  return value;
}

/**
 * Giá trị thô của một ô. Ô chọn có tìm kiếm (datalist) chứa nhãn, nên đổi về giá trị theo danh sách lựa chọn.
 */
function rawValueOf(element, field) {
  if (field.kind === 'select' && element.tagName !== 'SELECT') {
    return Select.getValue(element, field.options ?? []);
  }
  return element.value;
}

function fieldMarkup(field, initial) {
  const value = initial[field.name] ?? field.value ?? '';
  if (field.kind === 'select') {
    return Select.render({ name: field.name, label: field.label, options: field.options ?? [], value, placeholder: field.placeholder ?? 'Chọn', required: field.required, searchable: field.searchable });
  }
  const type = field.kind === 'date' ? 'date' : field.kind === 'number' ? 'number' : 'text';
  const shown = field.kind === 'date' && value ? String(value).slice(0, 10) : value;
  return Input.render({ name: field.name, label: field.label, type, value: shown, required: field.required, hint: field.hint, min: field.min, max: field.max, step: field.step });
}

/**
 * Form tổng quát. onSubmit nhận payload đã chuyển kiểu; nếu nó ném lỗi thì giữ form và báo lỗi.
 * @param {{
 *   title: string, fields: Array<object>, initial?: object, submitText?: string,
 *   danger?: boolean, size?: 'md'|'lg', successMessage?: string, onSubmit: (payload: object) => Promise<unknown>|unknown, onDone?: () => void
 * }} options
 */
export function openFormModal({ title, fields, initial = {}, submitText = 'Lưu', danger = false, size = 'md', successMessage = 'Đã lưu', onSubmit, onDone }) {
  const formId = nextId('form');
  const handle = Modal.open({
    title,
    size,
    content: `<form class="modal-form" id="${formId}" novalidate><div class="form-grid">${fields.map((f) => fieldMarkup(f, initial)).join('')}</div></form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: submitText, variant: danger ? 'destructive' : 'primary', action: 'save' }),
    ].join(''),
  });
  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);

  saveButton.addEventListener('click', async () => {
    let valid = true;
    fields.forEach((field) => {
      const input = form.elements.namedItem(field.name);
      const empty = field.required && String(rawValueOf(input, field)).trim() === '';
      if (empty) {
        if (field.kind === 'select') input.focus();
        Input.setError(input, 'Vui lòng nhập hoặc chọn thông tin này');
        valid = false;
      } else {
        Input.clearError(input);
      }
    });
    if (!valid) return;

    const payload = {};
    fields.forEach((field) => {
      payload[field.name] = valueOf(field, rawValueOf(form.elements.namedItem(field.name), field));
    });

    Button.setLoading(saveButton, true);
    try {
      await onSubmit(payload);
      Toast.success(successMessage);
      handle.close();
      onDone?.();
    } catch (error) {
      Button.setLoading(saveButton, false);
      Toast.error(error.message);
    }
  });

  return handle;
}

/**
 * @param {{ title: string, rows: Array<[string, string]>, footerHtml?: string, extra?: string }} options
 *   rows: cặp [nhãn, giá trị đã escape]
 */
export function openDetailModal({ title, rows, extra = '' }) {
  const list = rows.map(([label, value]) => `<dt>${escapeHtml(label)}</dt><dd>${value}</dd>`).join('');
  const handle = Modal.open({
    title,
    size: 'lg',
    content: `<dl class="detail-list">${list}</dl>${extra}`,
    footer: Button.render({ label: 'Đóng', variant: 'primary', action: 'close' }),
  });
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);
  return handle;
}

/** Giá trị hiển thị trong chi tiết: ngày theo dd/MM/yyyy, có/không cho đúng-sai, "—" khi trống. */
function displayValue(value) {
  if (value === null || value === undefined || value === '') return '—';
  if (typeof value === 'boolean') return value ? 'Có' : 'Không';
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}/.test(value)) return formatDate(value);
  return String(value);
}

/**
 * Chi tiết một bản ghi lấy từ máy chủ. Chỉ hiện trường đơn (bỏ mảng và đối tượng lồng nhau).
 * @param {{ title: string, load: () => Promise<object>, labels?: Record<string, string> }} options
 */
export async function openRecordDetailModal({ title, load, labels = {} }) {
  try {
    const record = await load();
    const rows = Object.entries(record)
      .filter(([key, value]) => key !== 'id' && (value === null || typeof value !== 'object'))
      .map(([key, value]) => [labels[key] ?? key, escapeHtml(displayValue(value))]);
    openDetailModal({ title, rows });
  } catch (error) {
    Toast.error(error.message);
  }
}
