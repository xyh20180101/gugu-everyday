import './assets/main.css'

import { createApp, ref, watch, provide } from 'vue'
import App from './App.vue'
import router from './router'
import { i18n } from './i18n'

import { setTheme } from '@fluentui/web-components'
import { webDarkTheme, webLightTheme } from '@fluentui/tokens'

const themeName = ref(localStorage.getItem('themeName'))

if (!themeName.value || themeName.value === 'null') {
    themeName.value = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'webDarkTheme' : 'webLightTheme'
    localStorage.setItem('themeName', themeName.value)
}
setTheme(themeName.value === 'webDarkTheme' ? webDarkTheme : webLightTheme)
watch(themeName, (val) => {
    setTheme(val === 'webDarkTheme' ? webDarkTheme : webLightTheme)
    localStorage.setItem('themeName', val)
})
//相同值不会触发
window.addEventListener('storage', (e) => {
    if (e.key === 'themeName')
        themeName.value = e.newValue
})

import '@fluentui/web-components/avatar.js';
import '@fluentui/web-components/button.js';
import '@fluentui/web-components/dialog.js';
import '@fluentui/web-components/dialog-body.js';
import '@fluentui/web-components/divider.js';
import '@fluentui/web-components/drawer.js';
import '@fluentui/web-components/drawer-body.js';
import '@fluentui/web-components/dropdown.js';
import '@fluentui/web-components/field.js';
import '@fluentui/web-components/listbox.js';
import '@fluentui/web-components/menu.js';
import '@fluentui/web-components/message-bar.js';
import '@fluentui/web-components/option.js';
import '@fluentui/web-components/spinner.js';
import '@fluentui/web-components/switch.js';
import '@fluentui/web-components/tab.js';
import '@fluentui/web-components/tablist.js';
import '@fluentui/web-components/text.js';
import '@fluentui/web-components/textarea.js';
import '@fluentui/web-components/text-input.js';
import '@fluentui/web-components/tooltip.js';

const app = createApp(App)

app.use(router)
app.use(i18n)
app.provide('themeName', themeName)

app.mount('#app')