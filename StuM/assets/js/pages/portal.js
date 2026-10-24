/**
 * Trang cá nhân (portal.html) cho sinh viên và giảng viên.
 * Backend tự xác định hồ sơ theo tài khoản đăng nhập nên trang không gửi mã sinh viên/giảng viên.
 * Sinh viên xem hồ sơ, bảng điểm, học vụ, học phí và gửi phúc khảo; giảng viên xem hồ sơ và lớp giảng dạy.
 */

import { api } from '../core/api.js';
import { escapeHtml } from '../core/dom.js';
import { Button } from '../components/button.js';
import { renderPageHeader } from '../layout/shell.js';
import {
  bangDiemTab,
  canhBaoTab,
  diemRenLuyenTab,
  hocBongTab,
  hocPhiTab,
  khenThuongTab,
  lichSuTab,
  lopDayTab,
  openPhucKhaoForm,
  phucKhaoTab,
  profileTab,
} from './portal-tabs.js';

/** Danh sách tab theo vai trò. Mỗi tab nhận (container, ctx). */
function tabsFor(role) {
  if (role === 'GiangVien') {
    return [
      { key: 'ho-so', label: 'Hồ sơ', render: (el) => profileTab(el, role) },
      { key: 'lop-day', label: 'Lớp giảng dạy', render: lopDayTab },
    ];
  }
  return [
    { key: 'ho-so', label: 'Hồ sơ', render: (el) => profileTab(el, role) },
    { key: 'bang-diem', label: 'Bảng điểm', render: bangDiemTab },
    { key: 'canh-bao', label: 'Cảnh báo học vụ', render: canhBaoTab },
    { key: 'hoc-phi', label: 'Học phí', render: hocPhiTab },
    { key: 'hoc-bong', label: 'Học bổng', render: hocBongTab },
    { key: 'ren-luyen', label: 'Điểm rèn luyện', render: diemRenLuyenTab },
    { key: 'khen-thuong', label: 'Khen thưởng, kỷ luật', render: khenThuongTab },
    { key: 'phuc-khao', label: 'Phúc khảo', render: phucKhaoTab },
    { key: 'lich-su', label: 'Lịch sử học tập', render: lichSuTab },
  ];
}

/**
 * Học kỳ của sinh viên lấy từ bảng điểm (backend không cho sinh viên xem danh mục học kỳ).
 * Giảng viên không có bộ lọc học kỳ nên không cần danh sách này.
 */
async function loadHocKys(isStudent) {
  if (!isStudent) return [];
  const bangDiem = await api.cuaToi.bangDiem().catch(() => null);
  return (bangDiem?.danhSachHocKy ?? []).map((hk) => ({ id: hk.hocKyId, ten: hk.tenHocKy, tenNamHoc: hk.tenNamHoc }));
}

/**
 * @param {HTMLElement} root
 */
export async function mount(root) {
  const role = api.auth.getSession()?.vaiTro;
  const isStudent = role === 'SinhVien';
  const ctx = { hocKys: await loadHocKys(isStudent) };
  const tabs = tabsFor(role);

  root.innerHTML = `${renderPageHeader({
    title: 'Trang cá nhân',
    description: isStudent ? 'Hồ sơ, kết quả học tập và học vụ của bạn.' : 'Hồ sơ và các lớp học phần bạn giảng dạy.',
    actions: isStudent ? Button.render({ label: 'Yêu cầu phúc khảo', variant: 'primary', icon: 'plus', action: 'create-review' }) : '',
  })}
    <nav class="tabs" role="tablist" aria-label="Mục trang cá nhân">
      ${tabs.map((tab) => `<button type="button" class="tab" role="tab" data-tab="${escapeHtml(tab.key)}" aria-selected="false">${escapeHtml(tab.label)}</button>`).join('')}
    </nav>
    <section class="card tab-panel" id="tab-panel"></section>`;

  const panel = root.querySelector('#tab-panel');

  /** Vẽ tab trong một container mới để listener của tab cũ không còn sót lại. */
  const select = (key) => {
    const tab = tabs.find((item) => item.key === key) ?? tabs[0];
    root.querySelectorAll('[data-tab]').forEach((button) => {
      button.setAttribute('aria-selected', String(button.dataset.tab === tab.key));
    });
    const container = document.createElement('div');
    panel.replaceChildren(container);
    tab.render(container, ctx);
  };

  root.addEventListener('click', (event) => {
    const tabButton = event.target.closest('[data-tab]');
    if (tabButton) select(tabButton.dataset.tab);
  });
  Button.init(root, {
    onAction: (action) => {
      if (action === 'create-review') openPhucKhaoForm(() => select('phuc-khao'));
    },
  });

  select(tabs[0].key);
}
