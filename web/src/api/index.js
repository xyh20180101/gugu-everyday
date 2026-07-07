import { reactive } from 'vue'
import router from '../router'
import { i18n } from '../i18n'

const t = i18n.global.t

export const messageStore = reactive({
  messages: []
})

export function addMessage(text, intent = 'info', duration = 3000) {
  const id = Date.now() + Math.random()
  messageStore.messages.push({ id, text, intent })

  setTimeout(() => {
    const index = messageStore.messages.findIndex(m => m.id === id)
    if (index !== -1) messageStore.messages.splice(index, 1)
  }, duration)
}

const BASE = import.meta.env.VITE_API_BASE

export async function requestDownload(url, fileName) {
  const token = localStorage.getItem('token')
  try {
    const res = await fetch(url, {
      headers: { Authorization: token ? `Bearer ${token}` : '' }
    })
    if (res.ok) {
      const blob = await res.blob()
      const downloadUrl = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = downloadUrl
      a.download = fileName
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(downloadUrl)
    } else if (res.status === 401) {
      addMessage(t('login.sessionExpired'), 'error')
      router.push({ name: 'login' })
    } else {
      addMessage(t('login.serverError'), 'error')
    }
  } catch {
    addMessage(t('login.networkError'), 'error')
  }
}

export async function request(url, options = {}) {
  const token = localStorage.getItem('token')
  options.headers = { ...options.headers, Authorization: token ? `Bearer ${token}` : '', 'Content-Type': 'application/json' }

  try {
    const res = await fetch(url, options)
    if (res.ok) {
      const text = await res.text()
      return text ? JSON.parse(text) : null
    }
    else if (res.status === 401) {
      addMessage(t('login.sessionExpired'), 'error')
      router.push({ name: 'login' })
      throw new Error()
    }
    else {
      let message = t('login.serverError')
      try {
        const body = await res.json()
        message = body?.error?.message || message
      } catch {}
      throw new Error(message)
    }
  } catch (err) {
    if (err instanceof TypeError && err.message.includes('Failed to fetch')) {
      addMessage(t('login.networkError'), 'error')
    }
    else if(err.message)
      addMessage(err.message, 'error')
    throw err
  }
}

export const auth = {
  register: (data) => request(`${BASE}/api/auth/register`, { method: 'POST', body: JSON.stringify(data) }),
  loginChallenge: (email) => request(`${BASE}/api/auth/login-challenge?email=${encodeURIComponent(email)}`),
  login: (data) => request(`${BASE}/api/auth/login`, { method: 'POST', body: JSON.stringify(data) }),
  me: () => request(`${BASE}/api/auth/me`),
  userIdHash: () => request(`${BASE}/api/auth/userIdHash`),
  updatePasswordChallenge: () => request(`${BASE}/api/auth/update-password-challenge`),
  updatePassword: (data) => request(`${BASE}/api/auth/update-password`, { method: 'POST', body: JSON.stringify(data) }),
  resetPasswordSendEmail: (data) => request(`${BASE}/api/auth/reset-password-send-email`, { method: 'POST', body: JSON.stringify(data) }),
  resetPasswordValid: (data) => request(`${BASE}/api/auth/reset-password-valid`, { method: 'POST', body: JSON.stringify(data) }),
  resetPassword: (data) => request(`${BASE}/api/auth/reset-password`, { method: 'POST', body: JSON.stringify(data) }),
  activate: (data) => request(`${BASE}/api/auth/activate`, { method: 'POST', body: JSON.stringify(data) }),
}

export const projectType = {
  getList: (params) => request(`${BASE}/api/projecttype?${new URLSearchParams(params)}`),
  create: (data) => request(`${BASE}/api/projecttype`, { method: 'POST', body: JSON.stringify(data) }),
  update: (id, data) => request(`${BASE}/api/projecttype/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  delete: (id) => request(`${BASE}/api/projecttype/${id}`, { method: 'DELETE' }),
}

export const project = {
  getList: (params) => request(`${BASE}/api/project?${new URLSearchParams(params)}`),
  create: (data) => request(`${BASE}/api/project`, { method: 'POST', body: JSON.stringify(data) }),
  update: (id, data) => request(`${BASE}/api/project/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  delete: (id) => request(`${BASE}/api/project/${id}`, { method: 'DELETE' }),
  setIsArchived: (id, data) => request(`${BASE}/api/project/${id}/set-is-archived`, { method: 'PATCH', body: JSON.stringify(data) }),
  getProgressList: (id, params) => request(`${BASE}/api/project/${id}/progresses?${new URLSearchParams(params)}`),
  createProgress: (id, data) => request(`${BASE}/api/project/${id}/progress`, { method: 'POST', body: JSON.stringify(data) }),
  deleteProgress: (id, id2) => request(`${BASE}/api/project/${id}/progress/${id2}`, { method: 'DELETE' }),
  publicGetList: (params) => request(`${BASE}/api/project/public?${new URLSearchParams(params)}`),
}

export const userInfo = {
  update: (id, data) => request(`${BASE}/api/userinfo/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  exportJson: () => requestDownload(`${BASE}/api/userinfo/export-json`, 'gugu-export.json'),
  exportCsv: () => requestDownload(`${BASE}/api/userinfo/export-csv`, 'gugu-export.zip'),
}

export const home = {
  waterfall: (params) => request(`${BASE}/api/home/waterfall?${new URLSearchParams(params)}`),
}

export const reminder = {
  getSummary: (params) => request(`${BASE}/api/reminder/summary?${new URLSearchParams(params)}`),
  create: (data) => request(`${BASE}/api/reminder`, { method: 'POST', body: JSON.stringify(data) }),
}
