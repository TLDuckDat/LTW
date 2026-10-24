/**
 * Lịch học của lớp học phần: liệt kê buổi học, xếp buổi mới, sửa buổi, xóa buổi,
 * và kiểm tra xung đột phòng/giảng viên trước khi lưu (CRS-05, CRS-06).
 */

import { api } from '../core/api.js';
import { escapeHtml, nextId } from '../core/dom.js';
import { Button } from '../components/button.js';
import { Input } from '../components/input.js';
import { Modal } from '../components/modal.js';
import { Select } from '../components/select.js';
import { Toast } from '../components/toast.js';

/** Thứ trong tuần theo quy ước backend: 2 = Thứ 2 ... 8 = Chủ nhật. */
export const THU_OPTIONS = [
  { value: 2, label: 'Thứ 2' },
  { value: 3, label: 'Thứ 3' },
  { value: 4, label: 'Thứ 4' },
  { value: 5, label: 'Thứ 5' },
  { value: 6, label: 'Thứ 6' },
  { value: 7, label: 'Thứ 7' },
  { value: 8, label: 'Chủ nhật' },
];

const thuLabel = (thu) => THU_OPTIONS.find((option) => option.value === thu)?.label ?? `Thứ ${thu}`;

/** Mỗi nút sửa và xóa có id riêng để đọc được mã buổi học (id phải duy nhất trong trang). */
function scheduleListMarkup(lichHoc) {
  if (lichHoc.length === 0) return '<p class="form-hint">Lớp chưa có lịch học.</p>';
  const items = lichHoc
    .map((item) => {
      const tietKetThuc = item.tietBatDau + item.soTiet - 1;
      return `<li class="todo-item">
        <span class="todo-text">${escapeHtml(thuLabel(item.thu))}, tiết ${item.tietBatDau}-${tietKetThuc}, phòng ${escapeHtml(item.maPhong ?? '—')}</span>
        ${Button.render({ label: 'Sửa', variant: 'ghost', size: 'sm', action: 'edit-lich', id: `lich-edit-${item.id}` })}
        ${Button.render({ label: 'Xóa', variant: 'ghost', size: 'sm', action: 'remove-lich', id: `lich-del-${item.id}` })}
      </li>`;
    })
    .join('');
  return `<ul class="todo-list">${items}</ul>`;
}

/**
 * Quản lý lịch học của lớp trong một hộp thoại.
 * @param {{ record: object, phongs: object[], onDone?: () => void }} options
 */
export async function openScheduleModal({ record, phongs, onDone }) {
  const formId = nextId('schedule-form');
  const phongOptions = phongs.map((phong) => ({ value: phong.id, label: `${phong.ma} (sức chứa ${phong.sucChua})` }));
  const handle = Modal.open({
    title: `Lịch học lớp ${record.ma}`,
    size: 'lg',
    content: `<div class="modal-form">
      <div data-lich-list><p class="form-hint">Đang tải lịch học...</p></div>
      <p class="form-hint" data-lich-mode>Thêm buổi học mới</p>
      <form class="form-grid" id="${formId}" novalidate>
        ${Select.render({ name: 'thu', label: 'Thứ', options: THU_OPTIONS, value: 2 })}
        ${Input.render({ name: 'tietBatDau', type: 'number', label: 'Tiết bắt đầu', value: 1, min: 1, max: 12, step: 1, required: true })}
        ${Input.render({ name: 'soTiet', type: 'number', label: 'Số tiết', value: 3, min: 1, max: 12, step: 1, required: true })}
        ${Select.render({ name: 'phongHocId', label: 'Phòng học', options: phongOptions, placeholder: 'Chọn phòng', required: true })}
      </form>
    </div>`,
    footer: [
      Button.render({ label: 'Đóng', variant: 'outline', action: 'close' }),
      Button.render({ label: 'Buổi học mới', variant: 'outline', action: 'new-lich' }),
      Button.render({ label: 'Kiểm tra xung đột', variant: 'outline', action: 'check-lich' }),
      Button.render({ label: 'Thêm lịch học', variant: 'primary', action: 'save-lich' }),
    ].join(''),
  });

  const listEl = handle.element.querySelector('[data-lich-list]');
  const modeEl = handle.element.querySelector('[data-lich-mode]');
  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save-lich"]');
  const checkButton = handle.element.querySelector('[data-action="check-lich"]');
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);

  let lichHocCache = [];
  let editingId = null;

  const field = (name) => form.elements.namedItem(name).value;

  const readPayload = () => ({
    thu: Number(field('thu')),
    tietBatDau: Number(field('tietBatDau')),
    soTiet: Number(field('soTiet')),
    phongHocId: Number(field('phongHocId')),
  });

  const hasRoom = () => {
    if (field('phongHocId') !== '') return true;
    Toast.warning('Chọn phòng học trước');
    return false;
  };

  const setMode = () => {
    saveButton.querySelector('.btn-label').textContent = editingId ? 'Lưu buổi học' : 'Thêm lịch học';
    modeEl.textContent = editingId ? 'Đang sửa một buổi học' : 'Thêm buổi học mới';
  };

  const resetForm = () => {
    editingId = null;
    form.elements.namedItem('thu').value = '2';
    form.elements.namedItem('tietBatDau').value = '1';
    form.elements.namedItem('soTiet').value = '3';
    form.elements.namedItem('phongHocId').value = '';
    setMode();
  };

  const startEdit = (item) => {
    editingId = item.id;
    form.elements.namedItem('thu').value = String(item.thu);
    form.elements.namedItem('tietBatDau').value = String(item.tietBatDau);
    form.elements.namedItem('soTiet').value = String(item.soTiet);
    form.elements.namedItem('phongHocId').value = String(item.phongHocId ?? '');
    setMode();
  };

  const reload = async () => {
    lichHocCache = await api.lopHocPhan.lichHoc(record.id);
    listEl.innerHTML = scheduleListMarkup(lichHocCache);
  };

  listEl.addEventListener('click', async (event) => {
    const edit = event.target.closest('[data-action="edit-lich"]');
    if (edit) {
      const item = lichHocCache.find((row) => row.id === Number(edit.id.replace('lich-edit-', '')));
      if (item) startEdit(item);
      return;
    }

    const remove = event.target.closest('[data-action="remove-lich"]');
    if (!remove) return;
    const lichHocId = Number(remove.id.replace('lich-del-', ''));
    try {
      await api.lopHocPhan.xoaLichHoc(record.id, lichHocId);
      Toast.success('Đã xóa lịch học');
      if (editingId === lichHocId) resetForm();
      await reload();
      onDone?.();
    } catch (error) {
      Toast.error(error.message);
    }
  });

  handle.element.querySelector('[data-action="new-lich"]').addEventListener('click', resetForm);

  checkButton.addEventListener('click', async () => {
    if (!hasRoom()) return;
    Button.setLoading(checkButton, true);
    try {
      const result = await api.lopHocPhan.kiemTraXungDot(record.id, readPayload(), editingId ?? undefined);
      if (result.coXungDot) {
        Toast.warning(result.xungDot.map((item) => item.thongBao).join('. '));
      } else {
        Toast.success('Không có xung đột phòng hay giảng viên');
      }
    } catch (error) {
      Toast.error(error.message);
    } finally {
      Button.setLoading(checkButton, false);
    }
  });

  saveButton.addEventListener('click', async () => {
    if (!hasRoom()) return;
    Button.setLoading(saveButton, true);
    try {
      if (editingId) {
        await api.lopHocPhan.suaLichHoc(record.id, editingId, readPayload());
        Toast.success('Đã cập nhật buổi học');
      } else {
        await api.lopHocPhan.themLichHoc(record.id, readPayload());
        Toast.success('Đã thêm lịch học');
      }
      resetForm();
      await reload();
      onDone?.();
    } catch (error) {
      Toast.error(error.message);
    } finally {
      Button.setLoading(saveButton, false);
    }
  });

  try {
    await reload();
  } catch (error) {
    listEl.innerHTML = `<p class="form-error">${escapeHtml(error.message)}</p>`;
  }
}
