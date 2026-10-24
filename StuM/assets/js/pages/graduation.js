/**
 * Trang Tốt nghiệp (module Graduation): bảy tab - Điều kiện, Xét tốt nghiệp, Thực tập,
 * Cơ sở thực tập, Đề tài, Hội đồng bảo vệ, Kết quả bảo vệ. Mỗi tab khai báo cột, bộ lọc,
 * thao tác và nguồn dữ liệu; bảng, phân trang và hộp thoại dùng chung.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { debounce, escapeHtml } from '../core/dom.js';
import { formatDate, formatNumber } from '../core/format.js';
import { Badge } from '../components/badge.js';
import { Button } from '../components/button.js';
import { EmptyState } from '../components/empty-state.js';
import { Input } from '../components/input.js';
import { Modal } from '../components/modal.js';
import { Pagination } from '../components/pagination.js';
import { Select } from '../components/select.js';
import { Skeleton } from '../components/skeleton.js';
import { Table } from '../components/table.js';
import { Toast } from '../components/toast.js';
import { renderPageHeader } from '../layout/shell.js';
import { openDetailModal, openFormModal, openRecordDetailModal } from './graduation-forms.js';

const yesNo = (value) => (value ? 'Có' : 'Không');
const KET_LUAN = {
  DuDieuKien: ['success', 'Đủ điều kiện'],
  KhongDuDieuKien: ['destructive', 'Chưa đủ điều kiện'],
  ChuaTheKetLuan: ['neutral', 'Chưa kết luận'],
};
const KET_QUA_XET = {
  ChoXet: ['neutral', 'Chờ xét'],
  DuDieuKien: ['success', 'Đủ điều kiện'],
  KhongDuDieuKien: ['destructive', 'Không đủ điều kiện'],
};
const TRANG_THAI_TT = {
  DangKy: ['info', 'Đang đăng ký'],
  DaHuy: ['destructive', 'Đã hủy'],
  HoanThanh: ['success', 'Hoàn thành'],
};

const badgeOf = (map, key) => {
  const [variant, label] = map[key] ?? ['neutral', key ?? '—'];
  return Badge.render({ label, variant });
};

const state = { tab: 'dieuKien', page: 1, pageSize: 10, search: '', filters: {}, rows: [] };
let refs = { students: [], giangViens: [], coSo: [], hocKys: [], hoiDongs: [], deTais: [] };

const opts = (items, label) => items.map((item) => ({ value: item.id, label: label(item) }));
const giangVienLabel = (gv) => `${gv.ma} - ${gv.hoTen}`;
const sinhVienLabel = (sv) => `${sv.mssv} - ${sv.hoTen}`;

/** Cấu hình từng tab. */
const TABS = {
  dieuKien: {
    label: 'Điều kiện',
    title: 'Điều kiện tốt nghiệp',
    rowKey: 'sinhVienId',
    load: (p) => api.totNghiep.dieuKien(p),
    columns: [
      { key: 'maSinhVien', label: 'MSSV', primary: true },
      { key: 'hoTen', label: 'Họ và tên' },
      { key: 'tenChuongTrinhDaoTao', label: 'Chương trình' },
      { key: 'tinChi', label: 'Tín chỉ (đạt/yêu cầu)', render: (r) => `${r.tinChiTichLuy}/${r.tongTinChiYeuCau}` },
      { key: 'cpa', label: 'CPA (đạt/tối thiểu)', render: (r) => `${formatNumber(r.cpa, 2)}/${formatNumber(r.cpaToiThieu, 2)}` },
      { key: 'ketLuan', label: 'Kết luận', render: (r) => badgeOf(KET_LUAN, r.ketLuan) },
    ],
    actions: [
      { action: 'detail', label: 'Chi tiết điều kiện', icon: 'list-checks' },
      { action: 'xet', label: 'Chạy xét tốt nghiệp', icon: 'award' },
    ],
    onAction: async (action, row) => {
      if (action === 'detail') {
        const d = await api.totNghiep.dieuKienCuaSinhVien(row.sinhVienId);
        const thucTap = d.thucTap ? `${yesNo(d.thucTap.batBuoc)}, ${d.thucTap.soBanGhi} bản ghi, điểm ${d.thucTap.diem ?? '—'}` : '—';
        const khoaLuan = d.khoaLuan ? `${yesNo(d.khoaLuan.batBuoc)}, điểm ${d.khoaLuan.diem ?? '—'} (tối thiểu ${d.khoaLuan.diemToiThieu ?? '—'})` : '—';
        const monThieu = d.monBatBuoc ? (d.monBatBuoc.monConThieu ?? []).map((m) => m.tenMonHoc ?? m.maMonHoc ?? JSON.stringify(m)).join(', ') || 'Không' : '—';
        const dieuKien = (d.danhSachDieuKien ?? []).map((item) => `<li>${escapeHtml(Object.values(item).join(' - '))}</li>`).join('');
        openDetailModal({
          title: `Điều kiện của ${d.maSinhVien}`,
          rows: [
            ['Họ và tên', escapeHtml(d.hoTen)],
            ['Trạng thái sinh viên', escapeHtml(d.trangThaiSinhVien ?? '—')],
            ['Chương trình', escapeHtml(d.tenChuongTrinhDaoTao ?? '—')],
            ['Tín chỉ tích lũy', `${d.tinChiTichLuy}/${d.tongTinChiYeuCau} (còn thiếu ${d.tinChiConThieu})`],
            ['CPA', `${formatNumber(d.cpa, 2)} (tối thiểu ${formatNumber(d.cpaToiThieu, 2)})`],
            ['Thực tập', escapeHtml(thucTap)],
            ['Khóa luận', escapeHtml(khoaLuan)],
            ['Môn bắt buộc', `${d.monBatBuoc?.soMonDaDat ?? 0}/${d.monBatBuoc?.soMonBatBuoc ?? 0} đạt; thiếu: ${escapeHtml(monThieu)}`],
            ['Kết luận', badgeOf(KET_LUAN, d.ketLuan)],
          ],
          extra: dieuKien ? `<p class="form-hint">Chi tiết từng điều kiện:</p><ul>${dieuKien}</ul>` : '',
        });
      }
      if (action === 'xet') confirmXet(row);
    },
  },

  xet: {
    label: 'Xét tốt nghiệp',
    title: 'Kết quả xét tốt nghiệp',
    rowKey: 'sinhVienId',
    filters: [
      { name: 'ketQua', label: 'Kết quả', options: [{ value: '', label: 'Tất cả kết quả' }, ...Object.entries(KET_QUA_XET).map(([v, [, l]]) => ({ value: v, label: l }))] },
      { name: 'daTotNghiep', label: 'Đã tốt nghiệp', options: [{ value: '', label: 'Tất cả' }, { value: 'true', label: 'Đã tốt nghiệp' }, { value: 'false', label: 'Chưa' }] },
    ],
    load: (p) => api.totNghiep.xetList(p),
    columns: [
      { key: 'maSinhVien', label: 'MSSV', primary: true },
      { key: 'hoTen', label: 'Họ và tên' },
      { key: 'ngayXet', label: 'Ngày xét', render: (r) => formatDate(r.ngayXet) },
      { key: 'tongTinChiTichLuy', label: 'TC tích lũy', align: 'right' },
      { key: 'diemTrungBinhTichLuy', label: 'ĐTB tích lũy', align: 'right', render: (r) => formatNumber(r.diemTrungBinhTichLuy, 2) },
      { key: 'xepLoai', label: 'Xếp loại' },
      { key: 'ketQua', label: 'Kết quả', render: (r) => badgeOf(KET_QUA_XET, r.ketQua) },
    ],
    actions: [{ action: 'lichSu', label: 'Chi tiết và lịch sử', icon: 'history' }],
    onAction: async (action, row) => {
      if (action !== 'lichSu') return;
      const d = await api.totNghiep.xetChiTiet(row.sinhVienId);
      const lich = d.lichSu ?? [];
      const table = lich.length
        ? Table.render({
            columns: [
              { key: 'ngayXet', label: 'Ngày xét', render: (r) => formatDate(r.ngayXet) },
              { key: 'ketQua', label: 'Kết quả', render: (r) => badgeOf(KET_QUA_XET, r.ketQua) },
              { key: 'lyDo', label: 'Lý do', render: (r) => escapeHtml(r.lyDo ?? '—') },
            ],
            rows: lich,
            rowKey: 'ngayXet',
            caption: 'Lịch sử xét',
          })
        : '<p class="form-hint">Chưa có lịch sử xét.</p>';
      openDetailModal({
        title: `Xét tốt nghiệp - ${row.maSinhVien}`,
        rows: [
          ['Kết quả hiện tại', badgeOf(KET_QUA_XET, d.hienTai?.ketQua)],
          ['Xếp loại', escapeHtml(d.hienTai?.xepLoai ?? '—')],
          ['Lý do', escapeHtml(d.hienTai?.lyDo ?? '—')],
        ],
        extra: `<div class="mt-md">${table}</div>`,
      });
    },
  },

  thucTap: {
    label: 'Thực tập',
    title: 'Thực tập tốt nghiệp',
    rowKey: 'id',
    filters: () => [
      { name: 'hocKyId', label: 'Học kỳ', options: [{ value: '', label: 'Tất cả học kỳ' }, ...opts(refs.hocKys, (h) => h.ten)] },
      { name: 'coSoThucTapId', label: 'Cơ sở', options: [{ value: '', label: 'Tất cả cơ sở' }, ...opts(refs.coSo, (c) => c.ten)] },
      { name: 'trangThai', label: 'Trạng thái', options: [{ value: '', label: 'Tất cả' }, ...Object.entries(TRANG_THAI_TT).map(([v, [, l]]) => ({ value: v, label: l }))] },
    ],
    load: (p) =>
      api.totNghiep.thucTap.list({
        page: p.page,
        pageSize: p.pageSize,
        search: p.search,
        HocKyId: p.hocKyId,
        CoSoThucTapId: p.coSoThucTapId,
        TrangThai: p.trangThai,
      }),
    columns: [
      { key: 'maSinhVien', label: 'MSSV', primary: true },
      { key: 'hoTenSinhVien', label: 'Họ và tên' },
      { key: 'hocKy', label: 'Học kỳ' },
      { key: 'tenCoSoThucTap', label: 'Cơ sở' },
      { key: 'giangVienHuongDan', label: 'GV hướng dẫn', render: (r) => escapeHtml(r.giangVienHuongDan ?? '—') },
      { key: 'trangThai', label: 'Trạng thái', render: (r) => badgeOf(TRANG_THAI_TT, r.trangThai) },
      { key: 'diem', label: 'Điểm', align: 'right', render: (r) => (r.diem ?? '—') },
    ],
    actions: [
      { action: 'detail', label: 'Chi tiết', icon: 'eye' },
      { action: 'edit', label: 'Đổi cơ sở / GV hướng dẫn', icon: 'pencil' },
      { action: 'ketQua', label: 'Nhập kết quả', icon: 'clipboard-check' },
      { action: 'huy', label: 'Hủy thực tập', icon: 'ban', danger: true },
    ],
    create: () =>
      openFormModal({
        title: 'Đăng ký thực tập',
        size: 'lg',
        fields: [
          { name: 'sinhVienId', label: 'Sinh viên', kind: 'select', numeric: true, required: true, searchable: true, options: opts(refs.students, sinhVienLabel) },
          { name: 'hocKyId', label: 'Học kỳ', kind: 'select', numeric: true, required: true, options: opts(refs.hocKys, (h) => h.ten) },
          { name: 'coSoThucTapId', label: 'Cơ sở thực tập', kind: 'select', numeric: true, required: true, options: opts(refs.coSo, (c) => c.ten) },
          { name: 'giangVienHuongDanId', label: 'GV hướng dẫn', kind: 'select', numeric: true, required: true, options: opts(refs.giangViens, giangVienLabel) },
        ],
        successMessage: 'Đã đăng ký thực tập',
        submitText: 'Đăng ký',
        onSubmit: (payload) => api.totNghiep.thucTap.create(payload),
        onDone: () => loadTab(),
      }),
    onAction: async (action, row) => {
      if (action === 'detail') {
        openRecordDetailModal({ title: `Thực tập của ${row.mssv ?? row.id}`, load: () => api.totNghiep.thucTap.get(row.id) });
      }
      if (action === 'edit') {
        openFormModal({
          title: `Đổi cơ sở và GV hướng dẫn: ${row.maSinhVien}`,
          initial: { coSoThucTapId: row.coSoThucTapId, giangVienHuongDanId: row.giangVienHuongDanId },
          fields: [
            { name: 'coSoThucTapId', label: 'Cơ sở thực tập', kind: 'select', numeric: true, required: true, options: opts(refs.coSo, (c) => c.ten) },
            { name: 'giangVienHuongDanId', label: 'GV hướng dẫn', kind: 'select', numeric: true, required: true, options: opts(refs.giangViens, giangVienLabel) },
          ],
          onSubmit: (payload) => api.totNghiep.thucTap.capNhat(row.id, payload),
          onDone: () => loadTab(),
        });
      }
      if (action === 'ketQua') {
        const ketQua = await api.totNghiep.thucTap.ketQua(row.id).catch(() => null);
        openFormModal({
          title: `Kết quả thực tập: ${row.maSinhVien}`,
          initial: { diem: ketQua?.diem ?? '', nhanXet: ketQua?.nhanXet ?? '' },
          fields: [
            { name: 'diem', label: 'Điểm (0 - 10)', kind: 'number', required: true, min: 0, max: 10, step: 0.25 },
            { name: 'nhanXet', label: 'Nhận xét' },
          ],
          onSubmit: (payload) => api.totNghiep.thucTap.nhapKetQua(row.id, payload),
          onDone: () => loadTab(),
        });
      }
      if (action === 'huy') {
        Modal.confirm({
          title: 'Hủy thực tập',
          message: `Hủy đăng ký thực tập của ${row.maSinhVien}?`,
          confirmText: 'Hủy thực tập',
          danger: true,
          onConfirm: async () => {
            try {
              await api.totNghiep.thucTap.huy(row.id);
              Toast.success('Đã hủy thực tập');
              loadTab();
            } catch (error) {
              Toast.error(error.message);
              throw error;
            }
          },
        }).catch(() => {});
      }
    },
  },

  coSo: {
    label: 'Cơ sở thực tập',
    title: 'Cơ sở thực tập',
    rowKey: 'id',
    load: (p) => api.totNghiep.coSo.list(p),
    columns: [
      { key: 'ma', label: 'Mã', primary: true },
      { key: 'ten', label: 'Tên cơ sở' },
      { key: 'nguoiLienHe', label: 'Người liên hệ', render: (r) => escapeHtml(r.nguoiLienHe ?? '—') },
      { key: 'dangTiepNhan', label: 'Tiếp nhận SV', render: (r) => (r.dangTiepNhan ? Badge.render({ label: 'Đang nhận', variant: 'success' }) : Badge.render({ label: 'Ngừng nhận', variant: 'neutral' })) },
    ],
    actions: [
      { action: 'detail', label: 'Chi tiết', icon: 'eye' },
      { action: 'edit', label: 'Chỉnh sửa', icon: 'pencil' },
      { action: 'toggle', label: 'Đổi tiếp nhận', icon: 'toggle-right' },
      { divider: true },
      { action: 'delete', label: 'Xóa', icon: 'trash-2', danger: true },
    ],
    create: () => coSoForm(null),
    onAction: async (action, row) => {
      if (action === 'detail') {
        openRecordDetailModal({
          title: `Cơ sở ${row.ten}`,
          load: () => api.totNghiep.coSo.get(row.id),
          labels: { ma: 'Mã', ten: 'Tên cơ sở', nguoiLienHe: 'Người liên hệ', soDienThoai: 'Điện thoại', email: 'Email', dangTiepNhan: 'Đang tiếp nhận sinh viên' },
        });
      }
      if (action === 'edit') coSoForm(row);
      if (action === 'toggle') {
        try {
          await api.totNghiep.coSo.trangThai(row.id, !row.dangTiepNhan);
          Toast.success('Đã đổi trạng thái tiếp nhận');
          loadTab();
        } catch (error) {
          Toast.error(error.message);
        }
      }
      if (action === 'delete') removeRecord(row, 'Xóa cơ sở thực tập', () => api.totNghiep.coSo.remove(row.id));
    },
  },

  deTai: {
    label: 'Đề tài khóa luận',
    title: 'Đề tài khóa luận',
    rowKey: 'id',
    load: (p) => api.totNghiep.deTai.list(p),
    columns: [
      { key: 'tenDeTai', label: 'Tên đề tài', primary: true },
      { key: 'maSinhVien', label: 'Sinh viên', render: (r) => escapeHtml(r.maSinhVien ? `${r.maSinhVien} - ${r.hoTenSinhVien}` : 'Chưa đăng ký') },
      { key: 'giangVienHuongDan', label: 'GV hướng dẫn', render: (r) => escapeHtml(r.giangVienHuongDan ?? '—') },
    ],
    actions: [
      { action: 'detail', label: 'Chi tiết', icon: 'eye' },
      { action: 'edit', label: 'Chỉnh sửa tên', icon: 'pencil' },
      { action: 'dangKy', label: 'Sinh viên đăng ký', icon: 'user-plus' },
      { action: 'phanGiangVien', label: 'Phân GV hướng dẫn', icon: 'user-round' },
    ],
    create: () => deTaiForm(null),
    onAction: (action, row) => {
      if (action === 'detail') {
        openRecordDetailModal({
          title: `Đề tài ${row.tenDeTai}`,
          load: () => api.totNghiep.deTai.get(row.id),
          labels: { tenDeTai: 'Tên đề tài', maSinhVien: 'MSSV', hoTenSinhVien: 'Họ và tên sinh viên', giangVienHuongDan: 'Giảng viên hướng dẫn' },
        });
      }
      if (action === 'edit') deTaiForm(row);
      if (action === 'dangKy') {
        openFormModal({
          title: `Sinh viên đăng ký: ${row.tenDeTai}`,
          fields: [{ name: 'sinhVienId', label: 'Sinh viên', kind: 'select', numeric: true, required: true, searchable: true, options: opts(refs.students, sinhVienLabel) }],
          submitText: 'Đăng ký',
          successMessage: 'Đã đăng ký đề tài',
          onSubmit: (payload) => api.totNghiep.deTai.dangKy(row.id, payload.sinhVienId),
          onDone: () => loadTab(),
        });
      }
      if (action === 'phanGiangVien') {
        openFormModal({
          title: `Phân GV hướng dẫn: ${row.tenDeTai}`,
          fields: [{ name: 'giangVienHuongDanId', label: 'Giảng viên', kind: 'select', numeric: true, required: true, options: opts(refs.giangViens, giangVienLabel) }],
          successMessage: 'Đã phân giảng viên hướng dẫn',
          onSubmit: (payload) => api.totNghiep.deTai.phanGiangVien(row.id, payload.giangVienHuongDanId),
          onDone: () => loadTab(),
        });
      }
    },
  },

  hoiDong: {
    label: 'Hội đồng bảo vệ',
    title: 'Hội đồng bảo vệ',
    rowKey: 'id',
    load: (p) => api.totNghiep.hoiDong.list(p),
    columns: [
      { key: 'ten', label: 'Tên hội đồng', primary: true },
      { key: 'ngayBaoVe', label: 'Ngày bảo vệ', render: (r) => formatDate(r.ngayBaoVe) },
      { key: 'tenChuTich', label: 'Chủ tịch', render: (r) => escapeHtml(r.tenChuTich ?? '—') },
      { key: 'tenThuKy', label: 'Thư ký', render: (r) => escapeHtml(r.tenThuKy ?? '—') },
      { key: 'soDeTai', label: 'Số đề tài', align: 'right' },
    ],
    actions: [
      { action: 'detail', label: 'Chi tiết', icon: 'eye' },
      { action: 'edit', label: 'Chỉnh sửa', icon: 'pencil' },
      { action: 'deTai', label: 'Đề tài của hội đồng', icon: 'list' },
    ],
    create: () => hoiDongForm(null),
    onAction: (action, row) => {
      if (action === 'detail') {
        openRecordDetailModal({
          title: `Hội đồng ${row.ten}`,
          load: () => api.totNghiep.hoiDong.get(row.id),
          labels: { ten: 'Tên hội đồng', ngayBaoVe: 'Ngày bảo vệ', soDeTai: 'Số đề tài' },
        });
      }
      if (action === 'edit') hoiDongForm(row);
      if (action === 'deTai') openCouncilTopics(row);
    },
  },

  ketQuaBaoVe: {
    label: 'Kết quả bảo vệ',
    title: 'Kết quả bảo vệ khóa luận',
    rowKey: 'deTaiId',
    filters: () => [
      { name: 'HoiDongBaoVeId', label: 'Hội đồng', options: [{ value: '', label: 'Tất cả hội đồng' }, ...opts(refs.hoiDongs, (h) => h.ten)] },
      { name: 'DaCoKetQua', label: 'Đã có kết quả', options: [{ value: '', label: 'Tất cả' }, { value: 'true', label: 'Đã có' }, { value: 'false', label: 'Chưa có' }] },
    ],
    load: (p) => api.totNghiep.ketQuaBaoVe.list(p),
    columns: [
      { key: 'tenDeTai', label: 'Đề tài', primary: true },
      { key: 'maSinhVien', label: 'MSSV', render: (r) => escapeHtml(`${r.maSinhVien} - ${r.hoTenSinhVien}`) },
      { key: 'tenHoiDong', label: 'Hội đồng', render: (r) => escapeHtml(r.tenHoiDong ?? '—') },
      { key: 'diem', label: 'Điểm', align: 'right', render: (r) => (r.diem ?? '—') },
      { key: 'daCoKetQua', label: 'Kết quả', render: (r) => (r.daCoKetQua ? Badge.render({ label: 'Đã nhập', variant: 'success' }) : Badge.render({ label: 'Chưa nhập', variant: 'neutral' })) },
    ],
    actions: [{ action: 'nhap', label: 'Nhập điểm bảo vệ', icon: 'pencil' }],
    onAction: async (action, row) => {
      if (action !== 'nhap') return;
      const current = await api.totNghiep.ketQuaBaoVe.get(row.deTaiId).catch(() => row);
      openFormModal({
        title: `Điểm bảo vệ: ${row.tenDeTai}`,
        initial: { diem: current.diem ?? '', nhanXetBaoVe: current.nhanXetBaoVe ?? '' },
        fields: [
          { name: 'diem', label: 'Điểm (0 - 10)', kind: 'number', required: true, min: 0, max: 10, step: 0.25 },
          { name: 'nhanXetBaoVe', label: 'Nhận xét' },
        ],
        onSubmit: (payload) => api.totNghiep.ketQuaBaoVe.nhap(row.deTaiId, payload),
        onDone: () => loadTab(),
      });
    },
  },
};

/* ----- Hộp thoại dùng riêng ----- */

function confirmXet(row) {
  Modal.confirm({
    title: 'Chạy xét tốt nghiệp',
    message: `Xét tốt nghiệp cho ${row.hoTen} (${row.maSinhVien})? Kết quả được ghi vào lịch sử xét.`,
    confirmText: 'Xét',
    onConfirm: async () => {
      try {
        const d = await api.totNghiep.xet(row.sinhVienId);
        Toast.success(`Kết quả: ${KET_QUA_XET[d.ketQua]?.[1] ?? d.ketQua}`);
        loadTab();
      } catch (error) {
        Toast.error(error.message);
        throw error;
      }
    },
  }).catch(() => {});
}

function removeRecord(row, title, run) {
  Modal.confirm({
    title,
    message: `Xóa "${row.ma ?? row.ten ?? row.tenDeTai}"? Backend sẽ từ chối nếu đang được sử dụng.`,
    confirmText: 'Xóa',
    danger: true,
    onConfirm: async () => {
      try {
        await run();
        Toast.success('Đã xóa');
        loadTab();
      } catch (error) {
        Toast.error(error.message);
        throw error;
      }
    },
  }).catch(() => {});
}

function coSoForm(record) {
  openFormModal({
    title: record ? 'Chỉnh sửa cơ sở thực tập' : 'Thêm cơ sở thực tập',
    size: 'lg',
    initial: record ?? {},
    fields: [
      { name: 'ma', label: 'Mã', kind: 'text', required: true },
      { name: 'ten', label: 'Tên cơ sở', kind: 'text', required: true },
      { name: 'diaChi', label: 'Địa chỉ', kind: 'text' },
      { name: 'soDienThoai', label: 'Điện thoại', kind: 'text' },
      { name: 'email', label: 'Email', kind: 'text' },
      { name: 'nguoiLienHe', label: 'Người liên hệ', kind: 'text' },
    ],
    onSubmit: (payload) => (record ? api.totNghiep.coSo.update(record.id, payload) : api.totNghiep.coSo.create(payload)),
    onDone: () => loadTab(),
  });
}

function deTaiForm(record) {
  openFormModal({
    title: record ? 'Chỉnh sửa đề tài' : 'Thêm đề tài',
    initial: record ?? {},
    fields: [{ name: 'tenDeTai', label: 'Tên đề tài', kind: 'text', required: true }],
    onSubmit: (payload) => (record ? api.totNghiep.deTai.update(record.id, payload) : api.totNghiep.deTai.create(payload)),
    onDone: () => loadTab(),
  });
}

function hoiDongForm(record) {
  const gvOptions = opts(refs.giangViens, giangVienLabel);
  openFormModal({
    title: record ? 'Chỉnh sửa hội đồng' : 'Thêm hội đồng bảo vệ',
    size: 'lg',
    initial: record ? { ...record, ngayBaoVe: record.ngayBaoVe, chuTichId: record.chuTichId, thuKyId: record.thuKyId } : {},
    fields: [
      { name: 'ten', label: 'Tên hội đồng', kind: 'text', required: true },
      { name: 'ngayBaoVe', label: 'Ngày bảo vệ', kind: 'date', required: true },
      { name: 'chuTichId', label: 'Chủ tịch', kind: 'select', numeric: true, required: true, options: gvOptions },
      { name: 'thuKyId', label: 'Thư ký', kind: 'select', numeric: true, required: true, options: gvOptions },
    ],
    onSubmit: (payload) => (record ? api.totNghiep.hoiDong.update(record.id, payload) : api.totNghiep.hoiDong.create(payload)),
    onDone: () => loadTab(),
  });
}

/** Đề tài của một hội đồng: liệt kê và phân thêm đề tài. */
async function openCouncilTopics(council) {
  const handle = Modal.open({
    title: `Đề tài của ${council.ten}`,
    size: 'lg',
    content: `<div class="modal-form"><div data-topics></div>
      <div class="filter-bar">${Select.render({ id: 'council-add', name: 'deTaiId', label: 'Thêm đề tài', options: opts(refs.deTais, (d) => d.tenDeTai), placeholder: 'Chọn đề tài', searchable: true })}</div></div>`,
    footer: [Button.render({ label: 'Đóng', variant: 'outline', action: 'close' }), Button.render({ label: 'Phân vào hội đồng', variant: 'primary', action: 'add' })].join(''),
  });
  const listEl = handle.element.querySelector('[data-topics]');
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);

  const reload = async () => {
    const result = await api.totNghiep.hoiDong.deTai(council.id, { pageSize: 100 });
    listEl.innerHTML = result.data.length
      ? Table.render({
          columns: [
            { key: 'tenDeTai', label: 'Đề tài', primary: true },
            { key: 'maSinhVien', label: 'Sinh viên', render: (r) => escapeHtml(`${r.maSinhVien} - ${r.hoTenSinhVien}`) },
            { key: 'tenGiangVien', label: 'GV hướng dẫn', render: (r) => escapeHtml(r.tenGiangVienHuongDan ?? '—') },
          ],
          rows: result.data,
          rowKey: 'id',
          caption: 'Đề tài trong hội đồng',
        })
      : '<p class="form-hint">Hội đồng chưa có đề tài.</p>';
  };

  handle.element.querySelector('[data-action="add"]').addEventListener('click', async () => {
    const deTaiId = Select.getValue(handle.element.querySelector('[name="deTaiId"]'), opts(refs.deTais, (d) => d.tenDeTai));
    if (deTaiId === '') {
      Toast.warning('Chọn đề tài trước khi phân');
      return;
    }
    try {
      await api.totNghiep.hoiDong.phanDeTai(council.id, Number(deTaiId));
      Toast.success('Đã phân đề tài vào hội đồng');
      reload();
    } catch (error) {
      Toast.error(error.message);
    }
  });

  try {
    await reload();
  } catch (error) {
    listEl.innerHTML = `<p class="form-error">${escapeHtml(error.message)}</p>`;
  }
}

/* ----- Bảng và tab ----- */

/** Bộ lọc của tab; một số tab khai báo dạng hàm vì lựa chọn lấy từ danh mục đã tải. */
function filterDefs(tab) {
  return typeof tab.filters === 'function' ? tab.filters() : (tab.filters ?? []);
}

function filtersMarkup(tab) {
  const filters = filterDefs(tab).map((f) =>
    Select.render({ name: f.name, label: f.label, options: f.options, value: state.filters[f.name] ?? '' }),
  );
  return `<div class="filter-bar" role="search">
    ${Input.render({ name: 'q', label: 'Tìm kiếm', value: state.search, placeholder: 'Mã, họ tên, tên đề tài', iconLeft: 'search' })}
    ${filters.join('')}
    ${Button.render({ label: 'Đặt lại', variant: 'ghost', action: 'reset' })}
  </div>`;
}

function tabsMarkup() {
  return `<nav class="tabs" role="tablist" aria-label="Tốt nghiệp">
    ${Object.entries(TABS)
      .map(([key, tab]) => `<button type="button" class="tab" role="tab" data-tab="${key}" aria-selected="${key === state.tab}">${escapeHtml(tab.label)}</button>`)
      .join('')}
  </nav>`;
}

function loadTab() {
  const tab = TABS[state.tab];
  const rowKey = tab.rowKey ?? 'id';
  return renderAsync(document.getElementById('list-container'), {
    load: async () => {
      const result = await tab.load({ page: state.page, pageSize: state.pageSize, search: state.search, ...state.filters });
      state.rows = result.data;
      return result;
    },
    skeleton: () => Skeleton.table(5, tab.columns.length),
    isEmpty: (result) => result.total === 0,
    empty: () => EmptyState.render({ icon: 'inbox', title: `Chưa có dữ liệu ${tab.label.toLowerCase()}`, description: 'Thử thay đổi bộ lọc hoặc thêm mới.' }),
    render: (result) =>
      Table.render({
        columns: tab.columns,
        rows: result.data,
        rowKey,
        actions: tab.actions,
        actionLabel: (row) => `Thao tác cho ${row[tab.columns[0].key] ?? row[rowKey]}`,
        caption: tab.title,
        footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
      }),
    errorTitle: `Không tải được ${tab.label.toLowerCase()}`,
  });
}

function switchTab(root, key) {
  state.tab = key;
  state.page = 1;
  state.search = '';
  state.filters = {};
  root.querySelector('#tabs-slot').innerHTML = tabsMarkup();
  root.querySelector('#filters-slot').innerHTML = filtersMarkup(TABS[key]);
  root.querySelector('#card-title').textContent = TABS[key].title;
  root.querySelector('[data-action="create"]').hidden = !TABS[key].create;
  loadTab();
}

function bindEvents(root) {
  const debouncedSearch = debounce(() => {
    state.search = root.querySelector('[name="q"]').value.trim();
    state.page = 1;
    loadTab();
  }, 300);

  Input.init(root, {
    onInput: (input) => {
      if (input.name === 'q') debouncedSearch();
    },
  });

  root.addEventListener('change', (event) => {
    const tab = TABS[state.tab];
    if (event.target.matches('select') && filterDefs(tab).some((f) => f.name === event.target.name)) {
      state.filters[event.target.name] = event.target.value;
      state.page = 1;
      loadTab();
    }
  });

  root.addEventListener('click', (event) => {
    const button = event.target.closest('[data-tab]');
    if (button && button.dataset.tab !== state.tab) switchTab(root, button.dataset.tab);
  });

  Button.init(root, {
    onAction: (action) => {
      if (action === 'create') TABS[state.tab].create?.();
      if (action === 'reset') {
        state.search = '';
        state.filters = {};
        state.page = 1;
        root.querySelector('#filters-slot').innerHTML = filtersMarkup(TABS[state.tab]);
        loadTab();
      }
    },
  });

  Table.init(root, {
    onAction: (action, rowId) => {
      const tab = TABS[state.tab];
      const row = state.rows.find((item) => String(item[tab.rowKey ?? 'id']) === rowId);
      if (row) tab.onAction?.(action, row);
    },
  });

  Pagination.init(root.querySelector('#list-container'), {
    onChange: ({ page, pageSize }) => {
      state.page = page;
      state.pageSize = pageSize;
      loadTab();
    },
  });
}

/**
 * @param {HTMLElement} root
 */
export async function mount(root) {
  root.innerHTML = `${renderPageHeader({
    title: 'Tốt nghiệp',
    description: 'Điều kiện, xét tốt nghiệp, thực tập, đề tài khóa luận, hội đồng và kết quả bảo vệ.',
    actions: Button.render({ label: 'Thêm mới', variant: 'primary', icon: 'plus', action: 'create' }),
  })}
  <div id="tabs-slot">${tabsMarkup()}</div>
  <section class="card" aria-labelledby="card-title">
    <header class="card-header"><div class="card-header-text"><h2 class="card-title" id="card-title">${escapeHtml(TABS[state.tab].title)}</h2></div></header>
    <div id="filters-slot">${filtersMarkup(TABS[state.tab])}</div>
    <div class="card-body is-flush" id="list-container"></div>
  </section>`;

  bindEvents(root);
  root.querySelector('[data-action="create"]').hidden = !TABS[state.tab].create;
  const [students, giangViens, coSo, hocKys, hoiDongs, deTais] = await Promise.all([
    api.students.list({ pageSize: 100 }).then((r) => r.data).catch(() => []),
    api.giangVien.getAll().catch(() => []),
    api.totNghiep.coSo.list({ pageSize: 100 }).then((r) => r.data).catch(() => []),
    api.hocKy.getAll().catch(() => []),
    api.totNghiep.hoiDong.list({ pageSize: 100 }).then((r) => r.data).catch(() => []),
    api.totNghiep.deTai.list({ pageSize: 100 }).then((r) => r.data).catch(() => []),
  ]);
  refs = { students, giangViens, coSo, hocKys, hoiDongs, deTais };
  root.querySelector('#filters-slot').innerHTML = filtersMarkup(TABS[state.tab]);
  await loadTab();
}
