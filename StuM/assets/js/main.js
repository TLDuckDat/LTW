/**
 * Entry point của StuM cho các trang ứng dụng và trang đăng nhập.
 * Áp dụng theme, bảo vệ route bằng phiên đăng nhập, dựng shell rồi nạp module của trang.
 * ui-kit.html không dùng file này vì cần giữ theme riêng cho từng panel.
 */

import { api } from './core/api.js';
import { refreshIcons } from './core/dom.js';
import { router } from './core/router.js';
import { theme } from './core/theme.js';
import { Dropdown } from './components/dropdown.js';
import { mountShell } from './layout/shell.js';

const PAGE_TITLES = {
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
};

/** Trang chỉ dành cho một số vai trò; vai trò khác bị chuyển về Dashboard. */
const PAGE_ROLES = {
  accounts: ['Admin'],
  academic: ['Admin', 'PhongDaoTao'],
  graduation: ['Admin', 'PhongDaoTao'],
  reports: ['Admin', 'PhongDaoTao'],
};

const PAGE_MODULES = {
  login: () => import('./pages/login.js'),
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
      router.navigate('dashboard');
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
    router.navigate('dashboard');
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
