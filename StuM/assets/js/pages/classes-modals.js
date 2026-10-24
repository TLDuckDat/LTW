/**
 * Hộp thoại của trang Lớp học phần: phân công giảng viên và quản lý lịch học theo phòng.
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

/**
 * @param {{ record: object, giangViens: object[], onDone?: () => void }} options
 */
export function openAssignModal({ record, giangViens, onDone }) {
  const formId = nextId('assign-form');
  const options = [
    { value: '', label: 'Chưa phân công' },
    ...giangViens.map((gv) => ({ value: gv.id, label: `${gv.ma} - ${gv.hoTen}` })),
  ];
  const handle = Modal.open({
    title: `Phân giảng viên cho lớp ${record.ma}`,
    content: `<form class="modal-form" id="${formId}">
      ${Select.render({ name: 'giangVienId', label: 'Giảng viên', options, value: record.giangVienId ?? '' })}
    </form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: 'Lưu phân công', variant: 'primary', action: 'save' }),
    ].join(''),
  });
  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);

  saveButton.addEventListener('click', async () => {
    const value = form.elements.namedItem('giangVienId').value;
    Button.setLoading(saveButton, true);
    try {
      await api.lopHocPhan.phanCongGiangVien(record.id, value === '' ? null : Number(value));
      Toast.success('Đã cập nhật phân công giảng viên');
      handle.close();
      onDone?.();
    } catch (error) {
      Button.setLoading(saveButton, false);
      Toast.error(error.message);
    }
  });
}

/**
 * @param {{ lichHoc: object[], phongs: object[] }} data
 * @returns {string}
 */
function scheduleListMarkup(lichHoc) {
  if (lichHoc.length === 0) return '<p class="form-hint">Lớp chưa có lịch học.</p>';
  const items = lichHoc
    .map((item) => {
      const thu = THU_OPTIONS.find((option) => option.value === item.thu)?.label ?? `Thứ ${item.thu}`;
      const tietKetThuc = item.tietBatDau + item.soTiet - 1;
      return `<li class="todo-item">
        <span class="todo-text">${escapeHtml(thu)}, tiết ${item.tietBatDau}-${tietKetThuc}, phòng ${escapeHtml(item.maPhong ?? '—')}</span>
        ${Button.render({ label: 'Xóa', variant: 'ghost', size: 'sm', action: 'remove-lich', id: `lich-${item.id}` })}
      </li>`;
    })
    .join('');
  return `<ul class="todo-list">${items}</ul>`;
}

/**
 * Quản lý lịch học của lớp: liệt kê, thêm (thứ, tiết, phòng), xóa.
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
      <form class="form-grid" id="${formId}" novalidate>
        ${Select.render({ name: 'thu', label: 'Thứ', options: THU_OPTIONS, value: 2 })}
        ${Input.render({ name: 'tietBatDau', type: 'number', label: 'Tiết bắt đầu', value: 1, min: 1, max: 15, step: 1, required: true })}
        ${Input.render({ name: 'soTiet', type: 'number', label: 'Số tiết', value: 3, min: 1, max: 15, step: 1, required: true })}
        ${Select.render({ name: 'phongHocId', label: 'Phòng học', options: phongOptions, placeholder: 'Chọn phòng', required: true })}
      </form>
    </div>`,
    footer: [
      Button.render({ label: 'Đóng', variant: 'outline', action: 'close' }),
      Button.render({ label: 'Thêm lịch học', variant: 'primary', action: 'add-lich' }),
    ].join(''),
  });

  const listEl = handle.element.querySelector('[data-lich-list]');
  const form = handle.element.querySelector(`#${formId}`);
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);

  const reload = async () => {
    const lichHoc = await api.lopHocPhan.lichHoc(record.id);
    listEl.innerHTML = scheduleListMarkup(lichHoc);
  };

  listEl.addEventListener('click', async (event) => {
    const button = event.target.closest('[data-action="remove-lich"]');
    if (!button) return;
    try {
      await api.lopHocPhan.xoaLichHoc(record.id, Number(button.id.replace('lich-', '')));
      Toast.success('Đã xóa lịch học');
      reload();
      onDone?.();
    } catch (error) {
      Toast.error(error.message);
    }
  });

  handle.element.querySelector('[data-action="add-lich"]').addEventListener('click', async (event) => {
    const field = (name) => form.elements.namedItem(name).value;
    if (field('phongHocId') === '') {
      Toast.warning('Chọn phòng học trước khi thêm lịch');
      return;
    }
    const button = event.currentTarget;
    Button.setLoading(button, true);
    try {
      await api.lopHocPhan.themLichHoc(record.id, {
        thu: Number(field('thu')),
        tietBatDau: Number(field('tietBatDau')),
        soTiet: Number(field('soTiet')),
        phongHocId: Number(field('phongHocId')),
      });
      Toast.success('Đã thêm lịch học');
      await reload();
      onDone?.();
    } catch (error) {
      Toast.error(error.message);
    } finally {
      Button.setLoading(button, false);
    }
  });

  try {
    await reload();
  } catch (error) {
    listEl.innerHTML = `<p class="form-error">${escapeHtml(error.message)}</p>`;
  }
}
