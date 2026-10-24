/**
 * Chi tiết lớp học phần (chỉ đọc, GET /api/lop-hoc-phan/{id}) và gộp lớp vào lớp đích (CRS-07).
 */

import { api } from '../core/api.js';
import { escapeHtml, nextId } from '../core/dom.js';
import { Button } from '../components/button.js';
import { Modal } from '../components/modal.js';
import { Select } from '../components/select.js';
import { Toast } from '../components/toast.js';
import { openDetailModal } from './graduation-forms.js';

const STATUS_LABEL = {
  MoDangKy: 'Mở đăng ký',
  DangHoc: 'Đang học',
  DaKetThuc: 'Đã kết thúc',
  DaHuy: 'Đã hủy',
};

/**
 * @param {{ record: { id: number }, hocKys: object[], giangViens: object[] }} options
 */
export async function openClassDetail({ record, hocKys, giangViens }) {
  let lop;
  try {
    lop = await api.lopHocPhan.get(record.id);
  } catch (error) {
    Toast.error(error.message);
    return;
  }

  const hocKy = hocKys.find((item) => item.id === lop.hocKyId);
  const giangVien = giangViens.find((item) => item.id === lop.giangVienId);
  const status = STATUS_LABEL[lop.trangThai] ?? lop.trangThai;
  openDetailModal({
    title: `Lớp ${lop.ma}`,
    rows: [
      ['Mã lớp', escapeHtml(lop.ma)],
      ['Môn học', escapeHtml(`${lop.maMonHoc} - ${lop.tenMonHoc}`)],
      ['Học kỳ', escapeHtml(hocKy ? `${hocKy.ten} (${hocKy.tenNamHoc})` : '—')],
      ['Giảng viên', escapeHtml(giangVien ? `${giangVien.ma} - ${giangVien.hoTen}` : 'Chưa phân công')],
      ['Sĩ số', `${lop.siSoHienTai} / ${lop.siSoToiDa}`],
      ['Trạng thái', escapeHtml(status)],
    ],
  });
}

/**
 * Gộp lớp hiện tại vào một lớp đích cùng môn, cùng học kỳ, cùng đang mở đăng ký.
 * @param {{ record: object, onDone?: () => void }} options
 */
export async function openMergeModal({ record, onDone }) {
  let candidates;
  try {
    const result = await api.lopHocPhan.list({
      hocKyId: record.hocKyId,
      monHocId: record.monHocId,
      trangThai: 'MoDangKy',
      pageSize: 100,
    });
    candidates = result.data.filter((lop) => lop.id !== record.id);
  } catch (error) {
    Toast.error(error.message);
    return;
  }

  const formId = nextId('merge-form');
  const options = candidates.map((lop) => ({ value: lop.id, label: `${lop.ma} (sĩ số ${lop.siSoHienTai}/${lop.siSoToiDa})` }));
  const handle = Modal.open({
    title: `Gộp lớp ${record.ma}`,
    content: `<form class="modal-form" id="${formId}" novalidate>
      <p class="modal-message">Sinh viên trong lớp ${escapeHtml(record.ma)} sẽ được chuyển sang lớp đích. Sinh viên đã có sẵn ở lớp đích không được chuyển trùng. Chỉ gộp được khi cả hai lớp đang mở đăng ký, cùng môn, cùng học kỳ và lớp nguồn chưa có điểm.</p>
      ${options.length
        ? Select.render({ name: 'lopDichId', label: 'Lớp đích', options, placeholder: 'Chọn lớp đích', required: true })
        : '<p class="form-hint">Không có lớp đích nào cùng môn và học kỳ đang mở đăng ký.</p>'}
    </form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: 'Gộp lớp', variant: 'destructive', action: 'save', disabled: options.length === 0 }),
    ].join(''),
  });
  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);

  saveButton.addEventListener('click', async () => {
    const value = form.elements.namedItem('lopDichId').value;
    if (value === '') {
      Toast.warning('Chọn lớp đích trước khi gộp');
      return;
    }
    Button.setLoading(saveButton, true);
    try {
      const result = await api.lopHocPhan.gop(record.id, Number(value));
      Toast.success(`Đã gộp vào lớp ${result.lopDich.ma}: chuyển ${result.soSinhVienChuyen} sinh viên, ${result.soSinhVienTrung} sinh viên đã có sẵn`);
      handle.close();
      onDone?.();
    } catch (error) {
      Button.setLoading(saveButton, false);
      Toast.error(error.message);
    }
  });
}
