/**
 * Empty state: icon mờ 64px, tiêu đề, mô tả và nút CTA.
 * - render({ icon, title, description, actionText, actionId, action }) -> HTML string
 *   CTA dùng Button; actionId là id của nút, action là giá trị data-action (mặc định = actionId).
 */

import { escapeHtml } from '../core/dom.js';
import { Button } from './button.js';

/**
 * @param {{
 *   icon?: string,
 *   title: string,
 *   description?: string,
 *   actionText?: string,
 *   actionId?: string,
 *   action?: string
 * }} props
 * @returns {string}
 */
function render({ icon = 'inbox', title, description = '', actionText, actionId, action }) {
  const cta = actionText
    ? Button.render({
        label: actionText,
        variant: 'primary',
        icon: 'plus',
        id: actionId,
        action: action ?? actionId,
      })
    : '';
  const descriptionHtml = description
    ? `<p class="empty-state-description">${escapeHtml(description)}</p>`
    : '';

  return `<div class="empty-state" role="status">
    <div class="empty-state-icon"><i data-lucide="${escapeHtml(icon)}" aria-hidden="true"></i></div>
    <h3 class="empty-state-title">${escapeHtml(title)}</h3>
    ${descriptionHtml}
    ${cta}
  </div>`;
}

export const EmptyState = { render };
