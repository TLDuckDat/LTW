/**
 * Entry point của StuM cho các trang ứng dụng và trang đăng nhập.
 * Áp dụng theme, bảo vệ route bằng phiên đăng nhập và vai trò, dựng shell rồi nạp module của trang.
 * ui-kit.html không dùng file này vì cần giữ theme riêng cho từng panel.
 */

import { api } from './core/api.js';
import { refreshIcons } from './core/dom.js';
import { homeRouteFor, router } from './core/router.js';
import { theme } from './core/theme.js';
import { Dropdown } from './components/dropdown.js';
import { mountShell } from './layout/shell.js';

const PAGE_TITLES = {
  portal: 'Trang cá nhân',
  dashboard: 'Dashboard',
  students: 'Sinh viên',
  courses: 'Môn học',
  classes: 'Lớp học phần',
  grades: 'Điểm',
  registration: 'Đăng ký học phần',
  accounts: 'Tài khoản',
  academic: 'Danh mục học vụ',
  graduation: 'Tốt nghiệp',
  reports: 'Báo cáo',
  studentAffairs: 'Công tác sinh viên',
};

const STAFF = ['Admin', 'PhongDaoTao'];

/** Trang chỉ dành cho một số vai trò; vai trò khác được chuyển về trang chính của mình. Khớp với backend. */
const PAGE_ROLES = {
  portal: ['SinhVien', 'GiangVien'],
  dashboard: STAFF,
  students: [...STAFF, 'GiangVien'],
  courses: STAFF,
  classes: STAFF,
  grades: [...STAFF, 'GiangVien'],
  registration: [...STAFF, 'SinhVien'],
  accounts: ['Admin'],
  academic: STAFF,
  graduation: STAFF,
  reports: STAFF,
  studentAffairs: STAFF,
};

const PAGE_MODULES = {
  login: () => import('./pages/login.js'),
  portal: () => import('./pages/portal.js'),
  dashboard: () => import('./pages/dashboard.js'),
  students: () => import('./pages/students.js'),
  courses: () => import('./pages/courses.js'),
  classes: () => import('./pages/classes.js'),
  grades: () => import('./pages/grades.js'),
  registration: () => import('./pages/registration.js'),
  accounts: () => import('./pages/accounts.js'),
  academic: () => import('./pages/academic.js'),
  graduation: () => import('./pages/graduation.js'),
  reports: () => import('./pages/reports.js'),
  studentAffairs: () => import('./pages/student-affairs.js'),
};

async function bootstrap() {
  theme.apply(theme.current());
  Dropdown.initGlobal();

  const page = router.current();
  const loadModule = PAGE_MODULES[page];
  if (!loadModule) return;

  const app = document.getElementById('app');
  const session = api.auth.getSession();

  if (page === 'login') {
    if (session) {
      router.navigate(homeRouteFor(session.vaiTro));
      return;
    }
    const loginModule = await loadModule();
    loginModule.mount(app);
    refreshIcons();
    return;
  }

  if (!session) {
    router.navigate('login');
    return;
  }

  if (PAGE_ROLES[page] && !PAGE_ROLES[page].includes(session.vaiTro)) {
    router.navigate(homeRouteFor(session.vaiTro));
    return;
  }

  document.title = `${PAGE_TITLES[page]} - StuM`;
  const content = mountShell(app, { page, title: PAGE_TITLES[page] });
  const pageModule = await loadModule();
  await pageModule.mount(content, { query: router.query() });
  refreshIcons();
}

bootstrap().catch((error) => {
  console.error('[StuM] Không khởi tạo được trang', error);
});
