/**
 * Hộp thoại của trang Đăng ký học phần: tạo/sửa đợt đăng ký và chỉnh sĩ số tối đa của lớp.
 * Backend nhận thời điểm dạng ISO (UTC); form dùng datetime-local theo giờ của trình duyệt.
 */

import { api } from '../core/api.js';
import { escapeHtml, nextId } from '../core/dom.js';
import { Button } from '../components/button.js';
import { Input } from '../components/input.js';
import { Modal } from '../components/modal.js';
import { Select } from '../components/select.js';
import { Toast } from '../components/toast.js';

/**
 * @param {string} iso
 * @returns {string} chuỗi dạng YYYY-MM-DDTHH:mm theo giờ trình duyệt
 */
function toLocalInput(iso) {
  if (!iso) return '';
  const date = new Date(iso);
  const pad = (value) => String(value).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

/**
 * @param {string} local
 * @returns {string} ISO UTC
 */
function toIso(local) {
  return new Date(local).toISOString();
}

/**
 * @param {{ period?: object|null, hocKys: object[], hocKyId: number|null, onSaved?: () => void }} options
 */
export function openPeriodForm({ period = null, hocKys, hocKyId, onSaved }) {
  const formId = nextId('period-form');
  const isEdit = Boolean(period);
  const hocKyOptions = hocKys.map((hk) => ({ value: hk.id, label: `${hk.ten} (${hk.tenNamHoc})` }));

  const handle = Modal.open({
    title: isEdit ? 'Sửa đợt đăng ký' : 'Tạo đợt đăng ký',
    size: 'lg',
    content: `<form class="modal-form" id="${formId}" novalidate>
      <div class="form-grid">
        ${Input.render({ name: 'ten', label: 'Tên đợt', value: period?.ten ?? '', placeholder: 'Đợt 1 - HK1 2025-2026', required: true })}
        ${isEdit ? '' : Select.render({ name: 'hocKyId', label: 'Học kỳ', options: hocKyOptions, value: period?.hocKyId ?? hocKyId ?? '', required: true })}
        ${Input.render({ name: 'ngayBatDau', type: 'datetime-local', label: 'Bắt đầu', value: toLocalInput(period?.ngayBatDau), required: true })}
        ${Input.render({ name: 'ngayKetThuc', type: 'datetime-local', label: 'Kết thúc', value: toLocalInput(period?.ngayKetThuc), required: true })}
      </div>
      ${isEdit ? '<p class="form-hint">Học kỳ của đợt không đổi sau khi tạo.</p>' : ''}
    </form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: isEdit ? 'Lưu thay đổi' : 'Tạo đợt', variant: 'primary', action: 'save' }),
    ].join(''),
  });

  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);

  saveButton.addEventListener('click', async () => {
    const field = (name) => form.elements.namedItem(name).value.trim();
    const inputs = ['ten', 'ngayBatDau', 'ngayKetThuc'].map((name) => form.elements.namedItem(name));
    let valid = true;
    inputs.forEach((input) => {
      if (input.value.trim() === '') {
        Input.setError(input, 'Vui lòng nhập thông tin này');
        valid = false;
      } else {
        Input.clearError(input);
      }
    });
    if (!valid) return;
    if (new Date(field('ngayKetThuc')) <= new Date(field('ngayBatDau'))) {
      Input.setError(form.elements.namedItem('ngayKetThuc'), 'Thời điểm kết thúc phải sau thời điểm bắt đầu');
      return;
    }

    const payload = {
      ten: field('ten'),
      ngayBatDau: toIso(field('ngayBatDau')),
      ngayKetThuc: toIso(field('ngayKetThuc')),
    };
    if (!isEdit) payload.hocKyId = Number(field('hocKyId'));

    Button.setLoading(saveButton, true);
    try {
      if (isEdit) {
        await api.dotDangKy.update(period.id, payload);
        Toast.success('Đã cập nhật đợt đăng ký');
      } else {
        await api.dotDangKy.create(payload);
        Toast.success('Đã tạo đợt đăng ký');
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
 * Chỉnh sĩ số tối đa của một lớp học phần.
 * @param {{ record: { lopHocPhanId: number, maLop: string, daDangKy: number, siSoToiDa: number }, onSaved?: () => void }} options
 */
export function openSiSoForm({ record, onSaved }) {
  const formId = nextId('siso-form');
  const handle = Modal.open({
    title: `Sĩ số lớp ${record.maLop}`,
    content: `<form class="modal-form" id="${formId}" novalidate>
      <p class="modal-message">Đang có ${escapeHtml(record.daDangKy)} sinh viên đăng ký. Sĩ số tối đa không được nhỏ hơn số này.</p>
      ${Input.render({ name: 'siSoToiDa', type: 'number', label: 'Sĩ số tối đa', value: record.siSoToiDa, min: 1, step: 1, required: true })}
    </form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: 'Lưu', variant: 'primary', action: 'save' }),
    ].join(''),
  });

  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);

  saveButton.addEventListener('click', async () => {
    const input = form.elements.namedItem('siSoToiDa');
    const value = Number(input.value);
    if (!Number.isInteger(value) || value < 1) {
      Input.setError(input, 'Sĩ số tối đa là số nguyên từ 1 trở lên');
      return;
    }
    Button.setLoading(saveButton, true);
    try {
      await api.dangKy.capNhatSiSo(record.lopHocPhanId, value);
      Toast.success('Đã cập nhật sĩ số tối đa');
      handle.close();
      onSaved?.();
    } catch (error) {
      Button.setLoading(saveButton, false);
      Toast.error(error.message);
    }
  });
}
