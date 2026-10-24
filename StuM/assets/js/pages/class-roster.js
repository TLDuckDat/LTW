/**
 * Hộp thoại sinh viên trong lớp học phần (thêm, xóa, chuyển lớp) và bảng điểm đầy đủ của sinh viên.
 */

import { api } from '../core/api.js';
import { escapeHtml, nextId } from '../core/dom.js';
import { formatNumber } from '../core/format.js';
import { Badge } from '../components/badge.js';
import { Button } from '../components/button.js';
import { Modal } from '../components/modal.js';
import { Select } from '../components/select.js';
import { Table } from '../components/table.js';
import { Toast } from '../components/toast.js';
import { openDetailModal } from './graduation-forms.js';

const LOAI = { HocMoi: 'Học mới', HocLai: 'Học lại', HocCaiThien: 'Học cải thiện' };

/**
 * Danh sách sinh viên của lớp, thêm sinh viên, xóa và chuyển lớp.
 * @param {{ record: object, onDone?: () => void }} options
 */
export async function openClassRoster({ record, onDone }) {
  const formId = nextId('roster-add');
  const handle = Modal.open({
    title: `Sinh viên trong lớp ${record.ma}`,
    size: 'lg',
    content: `<div class="modal-form">
      <div data-roster><p class="form-hint">Đang tải...</p></div>
      <form class="filter-bar" id="${formId}" novalidate>
        <div id="roster-student-slot"></div>
        ${Button.render({ label: 'Thêm vào lớp', variant: 'primary', action: 'add' })}
      </form>
    </div>`,
    footer: Button.render({ label: 'Đóng', variant: 'outline', action: 'close' }),
  });
  const listEl = handle.element.querySelector('[data-roster]');
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);

  const [students, classes] = await Promise.all([
    api.students.list({ pageSize: 100 }).then((r) => r.data),
    api.lopHocPhan.getAll(),
  ]);
  const studentOptions = students.map((sv) => ({ value: sv.id, label: `${sv.mssv} - ${sv.hoTen}` }));
  const otherClasses = classes.filter((item) => item.id !== record.id && item.hocKyId === record.hocKyId);
  handle.element.querySelector('#roster-student-slot').outerHTML = Select.render({
    name: 'sinhVienId',
    label: 'Sinh viên',
    options: studentOptions,
    placeholder: 'Chọn sinh viên',
    searchable: true,
  });

  const reload = async () => {
    const result = await api.sinhVienLopHocPhan.list(record.id, { pageSize: 100 });
    listEl.innerHTML = result.data.length
      ? Table.render({
          columns: [
            { key: 'mssv', label: 'MSSV', primary: true },
            { key: 'hoTen', label: 'Họ và tên' },
            { key: 'loaiDangKy', label: 'Loại', render: (row) => Badge.render({ label: LOAI[row.loaiDangKy] ?? row.loaiDangKy, variant: 'neutral' }) },
          ],
          rows: result.data,
          rowKey: 'dangKyHocPhanId',
          actions: [
            { action: 'chuyenLop', label: 'Chuyển lớp', icon: 'arrow-right-left' },
            { divider: true },
            { action: 'xoa', label: 'Xóa khỏi lớp', icon: 'trash-2', danger: true },
          ],
          actionLabel: (row) => `Thao tác cho ${row.mssv}`,
          caption: 'Sinh viên trong lớp',
        })
      : '<p class="form-hint">Lớp chưa có sinh viên.</p>';
    return result.data;
  };

  let rows = [];
  Table.init(listEl, {
    onAction: async (action, rowId) => {
      const row = rows.find((item) => String(item.dangKyHocPhanId) === rowId);
      if (!row) return;
      if (action === 'xoa') {
        try {
          await api.sinhVienLopHocPhan.xoa(record.id, row.dangKyHocPhanId);
          Toast.success('Đã xóa sinh viên khỏi lớp');
          rows = await reload();
          onDone?.();
        } catch (error) {
          Toast.error(error.message);
        }
      }
      if (action === 'chuyenLop') {
        const formIdMove = nextId('move');
        if (otherClasses.length === 0) {
          Toast.warning('Không có lớp khác cùng học kỳ để chuyển');
          return;
        }
        const move = Modal.open({
          title: `Chuyển ${row.mssv} sang lớp khác`,
          content: `<form class="modal-form" id="${formIdMove}">${Select.render({
            name: 'lopHocPhanMoiId',
            label: 'Lớp mới (cùng học kỳ)',
            options: otherClasses.map((item) => ({ value: item.id, label: `${item.ma} - ${item.tenMonHoc}` })),
            placeholder: 'Chọn lớp',
            required: true,
          })}</form>`,
          footer: [
            Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
            Button.render({ label: 'Chuyển lớp', variant: 'primary', action: 'save' }),
          ].join(''),
        });
        const moveForm = move.element.querySelector(`#${formIdMove}`);
        move.element.querySelector('[data-action="cancel"]').addEventListener('click', move.close);
        move.element.querySelector('[data-action="save"]').addEventListener('click', async () => {
          const value = moveForm.elements.namedItem('lopHocPhanMoiId').value;
          if (value === '') return;
          try {
            await api.sinhVienLopHocPhan.chuyenLop(record.id, row.dangKyHocPhanId, Number(value));
            Toast.success('Đã chuyển lớp');
            move.close();
            rows = await reload();
            onDone?.();
          } catch (error) {
            Toast.error(error.message);
          }
        });
      }
    },
  });

  handle.element.querySelector('[data-action="add"]').addEventListener('click', async () => {
    const select = handle.element.querySelector('[name="sinhVienId"]');
    const value = Select.getValue(select, studentOptions);
    if (value === '') {
      Toast.warning('Chọn sinh viên trước khi thêm');
      return;
    }
    try {
      await api.sinhVienLopHocPhan.them(record.id, Number(value));
      Toast.success('Đã thêm sinh viên vào lớp');
      rows = await reload();
      onDone?.();
    } catch (error) {
      Toast.error(error.message);
    }
  });

  try {
    rows = await reload();
  } catch (error) {
    listEl.innerHTML = `<p class="form-error">${escapeHtml(error.message)}</p>`;
  }
}

/**
 * Bảng điểm đầy đủ của sinh viên: CPA, tín chỉ, và điểm từng môn theo học kỳ.
 * @param {number} sinhVienId
 */
export async function openTranscript(sinhVienId) {
  let data;
  try {
    data = await api.bangDiemSinhVien(sinhVienId);
  } catch (error) {
    Toast.error(error.message);
    return;
  }
  const hocKy = (data.danhSachHocKy ?? [])
    .map(
      (hk) => `<h3 class="h3 mt-md">${escapeHtml(hk.tenHocKy)} (${escapeHtml(hk.tenNamHoc)})</h3>
        <p class="form-hint">GPA ${hk.gpaTamTinh ? '(tạm tính) ' : ''}${formatNumber(hk.gpa, 2)} · CPA ${formatNumber(hk.cpa, 2)} · ${hk.tongTinChiTinhGpa} TC tính GPA</p>
        ${Table.render({
          columns: [
            { key: 'maMonHoc', label: 'Môn', primary: true, render: (r) => escapeHtml(`${r.maMonHoc} - ${r.tenMonHoc}`) },
            { key: 'soTinChi', label: 'TC', align: 'right' },
            { key: 'diemTongKet', label: 'Điểm', align: 'right', render: (r) => (r.diemTongKet ?? '—') },
            { key: 'diemChu', label: 'Điểm chữ', render: (r) => escapeHtml(r.diemChu ?? '—') },
            { key: 'diemHe4', label: 'Hệ 4', align: 'right', render: (r) => (r.diemHe4 ?? '—') },
          ],
          rows: hk.danhSachMonHoc ?? [],
          rowKey: 'dangKyHocPhanId',
          caption: `Điểm học kỳ ${hk.tenHocKy}`,
        })}`,
    )
    .join('');

  openDetailModal({
    title: `Bảng điểm - ${data.mssv} ${data.hoTen}`,
    rows: [
      ['CPA', formatNumber(data.cpa, 2)],
      ['Tín chỉ tích lũy đạt', String(data.tongTinChiTichLuyDat)],
      ['Tín chỉ tính CPA', String(data.tongTinChiTinhCpa)],
    ],
    extra: hocKy || '<p class="form-hint">Chưa có kết quả học tập.</p>',
  });
}
