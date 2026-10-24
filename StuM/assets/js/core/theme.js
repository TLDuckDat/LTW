/**
 * Theme sáng/tối. Lựa chọn được lưu trong localStorage (key "stum:theme").
 * Khi đổi theme, phát sự kiện window "stum:theme" để các thành phần khác (topbar) cập nhật.
 */

import { Storage } from './storage.js';

const THEME_KEY = 'theme';

export const theme = {
  /** @returns {'light'|'dark'} */
  current() {
    return Storage.get(THEME_KEY, 'light') === 'dark' ? 'dark' : 'light';
  },

  /** @param {'light'|'dark'} mode */
  apply(mode) {
    document.documentElement.classList.toggle('dark', mode === 'dark');
    Storage.set(THEME_KEY, mode);
    window.dispatchEvent(new CustomEvent('stum:theme', { detail: mode }));
  },

  /** @returns {'light'|'dark'} theme sau khi đổi */
  toggle() {
    const next = theme.current() === 'dark' ? 'light' : 'dark';
    theme.apply(next);
    return next;
  },
};
