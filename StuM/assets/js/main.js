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
};

const PAGE_MODULES = {
  login: () => import('./pages/login.js'),
  dashboard: () => import('./pages/dashboard.js'),
  students: () => import('./pages/students.js'),
  courses: () => import('./pages/courses.js'),
  classes: () => import('./pages/classes.js'),
  grades: () => import('./pages/grades.js'),
};

async function bootstrap() {
  theme.apply(theme.current());
  Dropdown.initGlobal();

  const page = router.current();
  const loadModule = PAGE_MODULES[page];
  if (!loadModule) return;

  const app = document.getElementById('app');
  const session = api.getSession();

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

  document.title = `${PAGE_TITLES[page]} - StuM`;
  const content = mountShell(app, { page, title: PAGE_TITLES[page] });
  const pageModule = await loadModule();
  await pageModule.mount(content, { query: router.query() });
  refreshIcons();
}

bootstrap().catch((error) => {
  console.error('[StuM] Không khởi tạo được trang', error);
});
