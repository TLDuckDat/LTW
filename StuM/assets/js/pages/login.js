/**
 * Trang đăng nhập (index.html): layout 2 cột trên desktop, chỉ hiện form trên mobile.
 * Đăng nhập qua POST /api/auth/login của backend; phiên lưu trong localStorage (access + refresh token).
 */

import { api, ApiError } from '../core/api.js';
import { refreshIcons } from '../core/dom.js';
import { router } from '../core/router.js';
import { Storage } from '../core/storage.js';
import { Button } from '../components/button.js';
import { Input } from '../components/input.js';

const REMEMBER_KEY = 'remembered-user';

const RULES = {
  tenDangNhap: { check: (value) => value.length > 0, message: 'Vui lòng nhập tên đăng nhập' },
  matKhau: { check: (value) => value.length > 0, message: 'Vui lòng nhập mật khẩu' },
};

/**
 * @param {HTMLInputElement} input
 * @returns {boolean}
 */
function validateInput(input) {
  const rule = RULES[input.name];
  const value = input.value.trim();
  if (!rule || rule.check(value)) {
    Input.clearError(input);
    return true;
  }
  Input.setError(input, rule.message);
  return false;
}

function brandMarkup() {
  return `<aside class="auth-brand" aria-label="Giới thiệu StuM">
    <div class="auth-brand-head sidebar-brand">
      <span class="auth-brand-mark" aria-hidden="true">S</span>
      <span>StuM</span>
    </div>
    <div class="page-stack">
      <h2 class="auth-brand-title">Quản lý sinh viên và đào tạo</h2>
      <p class="auth-brand-text">Một nơi duy nhất để theo dõi sinh viên, môn học, lớp học phần và kết quả học tập.</p>
      <ul class="auth-brand-features">
        <li><i data-lucide="circle-check" aria-hidden="true"></i>Hồ sơ sinh viên và trạng thái học tập</li>
        <li><i data-lucide="circle-check" aria-hidden="true"></i>Lớp học phần, sĩ số và lịch học</li>
        <li><i data-lucide="circle-check" aria-hidden="true"></i>Nhập điểm theo lớp và xếp loại tự động</li>
      </ul>
    </div>
    <svg class="auth-brand-illustration" viewBox="0 0 420 260" role="img" aria-label="Minh họa sinh viên, sách và mũ tốt nghiệp">
      <rect x="40" y="200" width="340" height="14" rx="7" fill="currentColor" opacity="0.25"/>
      <rect x="86" y="150" width="64" height="50" rx="6" fill="currentColor" opacity="0.45"/>
      <rect x="96" y="134" width="48" height="16" rx="4" fill="currentColor" opacity="0.7"/>
      <rect x="276" y="158" width="70" height="42" rx="6" fill="currentColor" opacity="0.35"/>
      <polygon points="210,52 336,104 210,156 84,104" fill="currentColor" opacity="0.9"/>
      <path d="M142 128 V164 Q210 192 278 164 V128 L210 156 Z" fill="currentColor" opacity="0.55"/>
      <line x1="336" y1="104" x2="336" y2="160" stroke="currentColor" stroke-width="4" opacity="0.8"/>
      <circle cx="336" cy="168" r="8" fill="currentColor" opacity="0.8"/>
    </svg>
  </aside>`;
}

/**
 * @param {string} rememberedUser
 * @returns {string}
 */
function formMarkup(rememberedUser) {
  return `<main class="auth-form-wrap" id="main-content">
    <form class="auth-form" id="login-form" novalidate>
      <div class="auth-form-header">
        <h1 class="auth-form-title">Đăng nhập</h1>
        <p class="auth-form-subtitle">Dùng tài khoản được cấp trong hệ thống</p>
      </div>
      <div id="login-alert" class="auth-alert" role="alert" hidden></div>
      ${Input.render({ name: 'tenDangNhap', label: 'Tên đăng nhập', value: rememberedUser, placeholder: 'admin', iconLeft: 'user', autocomplete: 'username', required: true })}
      ${Input.render({ name: 'matKhau', type: 'password', label: 'Mật khẩu', placeholder: 'Nhập mật khẩu', iconLeft: 'lock', autocomplete: 'current-password', required: true })}
      <div class="auth-form-footer">
        <label class="checkbox-row">
          <input type="checkbox" name="remember"${rememberedUser ? ' checked' : ''}>
          <span>Ghi nhớ tên đăng nhập</span>
        </label>
        ${Button.render({ label: 'Đăng nhập', type: 'submit', variant: 'primary', size: 'lg', block: true })}
        <p class="auth-demo-hint">Tài khoản quản trị mặc định khi seed dữ liệu: <code>admin</code> / <code>Admin@123</code></p>
      </div>
    </form>
  </main>`;
}

function showAlert(form, message) {
  const alert = form.querySelector('#login-alert');
  alert.innerHTML = '<i data-lucide="circle-alert" aria-hidden="true"></i><span></span>';
  alert.querySelector('span').textContent = message;
  alert.hidden = false;
  refreshIcons();
}

function hideAlert(form) {
  form.querySelector('#login-alert').hidden = true;
}

/**
 * @param {HTMLFormElement} form
 */
async function handleSubmit(form) {
  hideAlert(form);
  const username = form.elements.namedItem('tenDangNhap');
  const password = form.elements.namedItem('matKhau');
  const results = [validateInput(username), validateInput(password)];
  if (results.includes(false)) {
    (results[0] ? password : username).focus();
    return;
  }

  const submitButton = form.querySelector('[type="submit"]');
  const remember = form.elements.namedItem('remember').checked;
  Button.setLoading(submitButton, true);

  try {
    await api.auth.login(username.value.trim(), password.value);
    if (remember) {
      Storage.set(REMEMBER_KEY, username.value.trim());
    } else {
      Storage.remove(REMEMBER_KEY);
    }
    router.navigate('dashboard');
  } catch (error) {
    // Thông báo từ backend đã là tiếng Việt (sai mật khẩu, bị khóa tạm thời, không kết nối được máy chủ...)
    Button.setLoading(submitButton, false);
    showAlert(form, error instanceof ApiError ? error.message : 'Đã có lỗi xảy ra, vui lòng thử lại.');
    password.value = '';
    password.focus();
  }
}

/**
 * @param {HTMLElement} root
 */
export function mount(root) {
  const rememberedUser = Storage.get(REMEMBER_KEY, '');
  root.innerHTML = `<div class="auth-layout">${brandMarkup()}${formMarkup(rememberedUser)}</div>`;
  refreshIcons();

  const form = root.querySelector('#login-form');
  Input.init(form, {
    onBlur: (input) => validateInput(input),
    onInput: (input) => {
      if (input.getAttribute('aria-invalid') === 'true') validateInput(input);
    },
  });

  form.addEventListener('submit', (event) => {
    event.preventDefault();
    handleSubmit(form);
  });
}
