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

import {
    FluentDesignSystem,
    ButtonDefinition,
    CompoundButtonDefinition,
    DialogBodyDefinition,
    DialogDefinition,
    DividerDefinition,
    DrawerDefinition,
    DrawerBodyDefinition,
    DropdownDefinition,
    DropdownOptionDefinition,
    FieldDefinition,
    LabelDefinition,
    LinkDefinition,
    ListboxDefinition,
    MenuButtonDefinition,
    MenuDefinition,
    MenuListDefinition,
    MenuItemDefinition,
    MessageBarDefinition,
    SpinnerDefinition,
    SwitchDefinition,
    TabDefinition,
    TablistDefinition,
    TextAreaDefinition,
    TextDefinition,
    TextInputDefinition,
    TreeDefinition,
    TreeItemDefinition
} from '@fluentui/web-components'
ButtonDefinition.define(FluentDesignSystem.registry)
CompoundButtonDefinition.define(FluentDesignSystem.registry)
DialogBodyDefinition.define(FluentDesignSystem.registry)
DialogDefinition.define(FluentDesignSystem.registry)
DividerDefinition.define(FluentDesignSystem.registry)
DrawerDefinition.define(FluentDesignSystem.registry)
DrawerBodyDefinition.define(FluentDesignSystem.registry)
DropdownDefinition.define(FluentDesignSystem.registry)
DropdownOptionDefinition.define(FluentDesignSystem.registry)
FieldDefinition.define(FluentDesignSystem.registry)
LabelDefinition.define(FluentDesignSystem.registry)
LinkDefinition.define(FluentDesignSystem.registry)
ListboxDefinition.define(FluentDesignSystem.registry)
MenuButtonDefinition.define(FluentDesignSystem.registry)
MenuDefinition.define(FluentDesignSystem.registry)
MenuListDefinition.define(FluentDesignSystem.registry)
MenuItemDefinition.define(FluentDesignSystem.registry)
MessageBarDefinition.define(FluentDesignSystem.registry)
SpinnerDefinition.define(FluentDesignSystem.registry)
SwitchDefinition.define(FluentDesignSystem.registry)
TabDefinition.define(FluentDesignSystem.registry)
TablistDefinition.define(FluentDesignSystem.registry)
TextAreaDefinition.define(FluentDesignSystem.registry)
TextDefinition.define(FluentDesignSystem.registry)
TextInputDefinition.define(FluentDesignSystem.registry)
TreeDefinition.define(FluentDesignSystem.registry)
TreeItemDefinition.define(FluentDesignSystem.registry)

const app = createApp(App)

app.use(router)
app.use(i18n)
app.provide('themeName', themeName)

app.mount('#app')