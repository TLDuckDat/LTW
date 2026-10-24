/**
 * Hộp thoại của trang Lớp học phần: phân công giảng viên. Lịch học, chi tiết và gộp lớp nằm ở file riêng.
 */

import { api } from '../core/api.js';
import { nextId } from '../core/dom.js';
import { Button } from '../components/button.js';
import { Modal } from '../components/modal.js';
import { Select } from '../components/select.js';
import { Toast } from '../components/toast.js';

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
