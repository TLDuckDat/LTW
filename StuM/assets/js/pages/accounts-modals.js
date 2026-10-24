/**
 * Hộp thoại quản trị tài khoản: tạo tài khoản, đổi vai trò, liên kết hồ sơ sinh viên/giảng viên,
 * đặt lại mật khẩu, và đổi mật khẩu của chính mình.
 */

import { api } from '../core/api.js';
import { nextId } from '../core/dom.js';
import { Button } from '../components/button.js';
import { Input } from '../components/input.js';
import { Modal } from '../components/modal.js';
import { Select } from '../components/select.js';
import { Toast } from '../components/toast.js';

export const VAI_TRO = {
  Admin: 'Quản trị viên',
  PhongDaoTao: 'Phòng đào tạo',
  GiangVien: 'Giảng viên',
  SinhVien: 'Sinh viên',
};

const VAI_TRO_OPTIONS = Object.entries(VAI_TRO).map(([value, label]) => ({ value, label }));

/**
 * Hộp thoại đơn giản: một ô chọn (hoặc một ô nhập), nút Hủy và nút Lưu.
 * @returns {{ element: HTMLElement, close: () => void }}
 */
function simpleForm({ title, fieldMarkup, formId, saveText = 'Lưu', danger = false }) {
  const handle = Modal.open({
    title,
    content: `<form class="modal-form" id="${formId}" novalidate>${fieldMarkup}</form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: saveText, variant: danger ? 'destructive' : 'primary', action: 'save' }),
    ].join(''),
  });
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);
  return { ...handle, form: handle.element.querySelector(`#${formId}`), saveButton: handle.element.querySelector('[data-action="save"]') };
}

/**
 * Chạy một hành động lưu: khóa nút khi đang gửi, báo lỗi backend nguyên văn nếu thất bại.
 */
async function submit(saveButton, handle, run, successMessage, onDone) {
  Button.setLoading(saveButton, true);
  try {
    await run();
    Toast.success(successMessage);
    handle.close();
    onDone?.();
  } catch (error) {
    Button.setLoading(saveButton, false);
    Toast.error(error.message);
  }
}

/**
 * @param {{ record: object, onDone?: () => void }} options
 */
export function openRoleModal({ record, onDone }) {
  const formId = nextId('role-form');
  const handle = simpleForm({
    title: `Đổi vai trò của ${record.tenDangNhap}`,
    formId,
    fieldMarkup: Select.render({ name: 'vaiTro', label: 'Vai trò', options: VAI_TRO_OPTIONS, value: record.vaiTro, required: true }),
  });
  handle.saveButton.addEventListener('click', () => {
    const vaiTro = handle.form.elements.namedItem('vaiTro').value;
    if (vaiTro === record.vaiTro) {
      Toast.info('Vai trò không thay đổi');
      return;
    }
    submit(handle.saveButton, handle, () => api.taiKhoan.doiVaiTro(record.id, vaiTro), 'Đã đổi vai trò', onDone);
  });
}

/**
 * Liên kết tài khoản với hồ sơ sinh viên hoặc giảng viên.
 * @param {{ record: object, kind: 'sinhVien'|'giangVien', options: Array<{value: number, label: string}>, onDone?: () => void }} options
 */
export function openLinkModal({ record, kind, options, onDone }) {
  const formId = nextId('link-form');
  const isStudent = kind === 'sinhVien';
  const label = isStudent ? 'Hồ sơ sinh viên' : 'Hồ sơ giảng viên';
  const currentId = isStudent ? record.sinhVienId : record.giangVienId;
  const handle = simpleForm({
    title: `Liên kết ${record.tenDangNhap} với ${label.toLowerCase()}`,
    formId,
    fieldMarkup: `${Select.render({ name: 'hoSo', label, options, value: currentId ?? '', placeholder: 'Chọn hồ sơ', searchable: true, required: true })}
      <p class="form-hint">Hồ sơ đã liên kết với một tài khoản khác sẽ bị từ chối. Backend chỉ hỗ trợ gán, không có thao tác bỏ liên kết.</p>`,
  });
  handle.saveButton.addEventListener('click', () => {
    const value = Select.getValue(handle.form.elements.namedItem('hoSo'), options);
    if (value === '') {
      Input.setError(handle.form.elements.namedItem('hoSo'), 'Vui lòng chọn hồ sơ');
      return;
    }
    const id = Number(value);
    const run = () => (isStudent ? api.taiKhoan.lienKetSinhVien(record.id, id) : api.taiKhoan.lienKetGiangVien(record.id, id));
    submit(handle.saveButton, handle, run, 'Đã liên kết hồ sơ', onDone);
  });
}

/**
 * Đặt lại mật khẩu cho tài khoản khác (Admin).
 * @param {{ record: object, onDone?: () => void }} options
 */
export function openResetPasswordModal({ record, onDone }) {
  const formId = nextId('reset-form');
  const handle = simpleForm({
    title: `Đặt lại mật khẩu cho ${record.tenDangNhap}`,
    formId,
    saveText: 'Đặt lại',
    danger: true,
    fieldMarkup: `${Input.render({ name: 'matKhauMoi', type: 'password', label: 'Mật khẩu mới', required: true, autocomplete: 'new-password' })}
      <p class="form-hint">Mọi phiên đăng nhập cũ của tài khoản này sẽ hết hiệu lực. Việc này cũng gỡ chặn đăng nhập do nhập sai nhiều lần.</p>`,
  });
  handle.saveButton.addEventListener('click', () => {
    const input = handle.form.elements.namedItem('matKhauMoi');
    if (input.value.length < 6) {
      Input.setError(input, 'Mật khẩu tối thiểu 6 ký tự');
      return;
    }
    submit(handle.saveButton, handle, () => api.taiKhoan.datLaiMatKhau(record.id, input.value), 'Đã đặt lại mật khẩu', onDone);
  });
}

/**
 * Tạo tài khoản mới. Vai trò SinhVien/GiangVien có thể gắn hồ sơ tương ứng.
 * @param {{ sinhVienOptions: object[], giangVienOptions: object[], onDone?: () => void }} options
 */
export function openCreateAccountModal({ sinhVienOptions, giangVienOptions, onDone }) {
  const formId = nextId('create-account-form');
  const handle = Modal.open({
    title: 'Tạo tài khoản',
    size: 'lg',
    content: `<form class="modal-form" id="${formId}" novalidate>
      <div class="form-grid">
        ${Input.render({ name: 'tenDangNhap', label: 'Tên đăng nhập', required: true, autocomplete: 'off' })}
        ${Input.render({ name: 'matKhau', type: 'password', label: 'Mật khẩu ban đầu', required: true, autocomplete: 'new-password' })}
        ${Input.render({ name: 'hoTen', label: 'Họ và tên', required: true })}
        ${Select.render({ name: 'vaiTro', label: 'Vai trò', options: VAI_TRO_OPTIONS, value: 'SinhVien', required: true })}
        ${Select.render({ name: 'sinhVienId', label: 'Hồ sơ sinh viên (nếu có)', options: sinhVienOptions, placeholder: 'Không liên kết', searchable: true })}
        ${Select.render({ name: 'giangVienId', label: 'Hồ sơ giảng viên (nếu có)', options: giangVienOptions, placeholder: 'Không liên kết', searchable: true })}
      </div>
    </form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: 'Tạo tài khoản', variant: 'primary', action: 'save' }),
    ].join(''),
  });
  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);

  saveButton.addEventListener('click', () => {
    const field = (name) => form.elements.namedItem(name);
    const required = ['tenDangNhap', 'matKhau', 'hoTen'];
    let valid = true;
    required.forEach((name) => {
      if (field(name).value.trim() === '') {
        Input.setError(field(name), 'Vui lòng nhập thông tin này');
        valid = false;
      }
    });
    if (!valid) return;

    const sinhVienId = Select.getValue(field('sinhVienId'), sinhVienOptions);
    const giangVienId = Select.getValue(field('giangVienId'), giangVienOptions);
    const payload = {
      tenDangNhap: field('tenDangNhap').value.trim(),
      matKhau: field('matKhau').value,
      hoTen: field('hoTen').value.trim(),
      vaiTro: field('vaiTro').value,
      sinhVienId: sinhVienId === '' ? null : Number(sinhVienId),
      giangVienId: giangVienId === '' ? null : Number(giangVienId),
    };
    submit(saveButton, handle, () => api.taiKhoan.create(payload), 'Đã tạo tài khoản', onDone);
  });
}

/**
 * Đổi mật khẩu của chính tài khoản đang đăng nhập.
 */
export function openChangePasswordModal() {
  const formId = nextId('change-password-form');
  const handle = simpleForm({
    title: 'Đổi mật khẩu',
    formId,
    saveText: 'Đổi mật khẩu',
    fieldMarkup: `${Input.render({ name: 'matKhauCu', type: 'password', label: 'Mật khẩu hiện tại', required: true, autocomplete: 'current-password' })}
      ${Input.render({ name: 'matKhauMoi', type: 'password', label: 'Mật khẩu mới', required: true, autocomplete: 'new-password' })}
      <p class="form-hint">Đổi mật khẩu sẽ đăng xuất các thiết bị khác.</p>`,
  });
  handle.saveButton.addEventListener('click', () => {
    const cu = handle.form.elements.namedItem('matKhauCu');
    const moi = handle.form.elements.namedItem('matKhauMoi');
    if (moi.value.length < 6) {
      Input.setError(moi, 'Mật khẩu mới tối thiểu 6 ký tự');
      return;
    }
    submit(handle.saveButton, handle, () => api.phien.doiMatKhau(cu.value, moi.value), 'Đã đổi mật khẩu');
  });
}
