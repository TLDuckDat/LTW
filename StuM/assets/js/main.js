/**
 * Entry point của StuM.
 * Bước 1: chỉ khởi tạo theme. Router, shell và trang sẽ được nối vào ở các bước sau.
 */

import { Storage } from './core/storage.js';

const THEME_KEY = 'theme';

/**
 * Áp dụng theme lên <html>. Theme được lưu trong localStorage, mặc định là light.
 */
function applySavedTheme() {
  const isDark = Storage.get(THEME_KEY, 'light') === 'dark';
  document.documentElement.classList.toggle('dark', isDark);
}

applySavedTheme();

// Lucide được load qua CDN ở <head>; render lại icon sau khi DOM sẵn sàng.
document.addEventListener('DOMContentLoaded', () => {
  if (window.lucide) {
    window.lucide.createIcons();
  }
});
