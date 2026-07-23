<script setup>
import { onMounted, provide, ref, h, inject, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { auth } from '@/api'
import { useRouter, useRoute } from 'vue-router'
import GuguMenuItem from '@/components/GuguMenuItem.vue'
import GuguLangSwitch from '@/components/GuguLangSwitch.vue'
import { NScrollbar } from 'naive-ui'

const { t, locale } = useI18n()
const router = useRouter()
const route = useRoute()

const userInfo = ref({})
provide('userInfo', userInfo)

onMounted(async () => {
  try {
    userInfo.value = await auth.me()
  } catch { }
})

const themeName = inject('themeName')
const switchTheme = () => {
  themeName.value = themeName.value === 'webDarkTheme' ? 'webLightTheme' : 'webDarkTheme'
}

const drawer = ref(null)
const help = () => {
  drawer.value.show()
}

const logout = () => {
  localStorage.removeItem('token')
  router.push({ name: 'login' })
}

const lastIndex = ref(0)
const activeId = ref(route.name)
const transitionName = ref('slide-left')

const tabDatas = computed(() => [
  {
    key: 'home',
    name: t('nav.home'),
    svg: h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', slot: 'start', width: '20', height: '20' }, [
      h('path', { d: 'M10.5495 2.53189C11.3874 1.82531 12.6126 1.82531 13.4505 2.5319L20.2005 8.224C20.7074 8.65152 21 9.2809 21 9.94406L21 19.2539C21 20.2204 20.2165 21.0039 19.25 21.0039H15.75C14.7835 21.0039 14 20.2204 14 19.2539L14 14.2468C14 14.1088 13.8881 13.9968 13.75 13.9968H10.25C10.1119 13.9968 9.99999 14.1088 9.99999 14.2468L9.99999 19.2539C9.99999 20.2204 9.2165 21.0039 8.25 21.0039H4.75C3.7835 21.0039 3 20.2204 3 19.2539V9.94406C3 9.2809 3.29255 8.65152 3.79952 8.224L10.5495 2.53189ZM12.4835 3.6786C12.2042 3.44307 11.7958 3.44307 11.5165 3.6786L4.76651 9.37071C4.59752 9.51321 4.5 9.72301 4.5 9.94406L4.5 19.2539C4.5 19.392 4.61193 19.5039 4.75 19.5039H8.25C8.38807 19.5039 8.49999 19.392 8.49999 19.2539L8.49999 14.2468C8.49999 13.2803 9.2835 12.4968 10.25 12.4968H13.75C14.7165 12.4968 15.5 13.2803 15.5 14.2468L15.5 19.2539C15.5 19.392 15.6119 19.5039 15.75 19.5039H19.25C19.3881 19.5039 19.5 19.392 19.5 19.2539L19.5 9.94406C19.5 9.72301 19.4025 9.51321 19.2335 9.37071L12.4835 3.6786Z' })
    ])
  },
  {
    key: 'projecttype',
    name: t('nav.projectType'),
    svg: h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', slot: 'start', width: '20', height: '20' }, [
      h('path', { d: 'M19.7498 2C20.9925 2 21.9998 3.00736 21.9998 4.25V9.71196C21.9998 10.5738 21.6575 11.4003 21.0482 12.0098L12.5472 20.5129C11.2777 21.7798 9.22195 21.7807 7.95079 20.5143L3.48909 16.0592C2.21862 14.7913 2.21699 12.7334 3.48531 11.4632L11.985 2.95334C12.5946 2.34297 13.4218 2 14.2845 2H19.7498ZM19.7498 3.5H14.2845C13.82 3.5 13.3745 3.68467 13.0463 4.01333L4.53412 12.5358C3.86389 13.2207 3.86898 14.3191 4.54884 14.9977L9.01006 19.4522C9.69493 20.1345 10.8033 20.134 11.487 19.4518L19.9874 10.9492C20.3155 10.6211 20.4998 10.176 20.4998 9.71196V4.25C20.4998 3.83579 20.164 3.5 19.7498 3.5ZM16.9998 5.50218C17.8282 5.50218 18.4998 6.17374 18.4998 7.00216C18.4998 7.83057 17.8282 8.50213 16.9998 8.50213C16.1714 8.50213 15.4998 7.83057 15.4998 7.00216C15.4998 6.17374 16.1714 5.50218 16.9998 5.50218Z' })
    ])
  },
  {
    key: 'project',
    name: t('nav.project'),
    svg: h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', slot: 'start', width: '20', height: '20' }, [
      h('path', { d: 'M9.51477 11.7244C9.5141 10.5939 9.60735 9.32179 9.901 8.13745C10.1959 6.94819 10.6792 5.90237 11.418 5.16351C12.2382 4.34337 13.3915 3.88395 14.6672 3.66471C15.9387 3.44622 17.2673 3.47776 18.3596 3.58455C19.4669 3.6928 20.3133 4.53921 20.4216 5.64653C20.5284 6.73885 20.5599 8.06743 20.3414 9.33891C20.1222 10.6147 19.6628 11.768 18.8426 12.5881C18.103 13.3278 17.0561 13.8116 15.866 14.1069C14.6809 14.4009 13.4082 14.4944 12.2779 14.494C11.2776 14.4937 10.458 15.3055 10.4533 16.3081C10.4453 18.0032 10.2339 19.9684 9.51719 21.4771C9.49171 21.4556 9.45942 21.415 9.44137 21.3409C9.24247 20.5246 8.97818 19.5478 8.66997 18.678C8.51592 18.2433 8.34657 17.8229 8.16341 17.457C7.98547 17.1015 7.77121 16.7495 7.51152 16.4898C7.2518 16.2302 6.89964 16.0158 6.54394 15.8378C6.17781 15.6545 5.75711 15.485 5.32206 15.3308C4.45163 15.0222 3.47417 14.7575 2.65746 14.5583C2.58366 14.5403 2.54312 14.5082 2.52167 14.4827C4.03232 13.7657 6.00138 13.5559 7.69893 13.5495C8.70241 13.5457 9.51537 12.7255 9.51477 11.7244ZM10.3574 4.10285C9.35577 5.10444 8.7769 6.43816 8.44508 7.77647C8.11203 9.1197 8.01405 10.5232 8.01477 11.7253C8.01488 11.9022 7.86784 12.0488 7.69325 12.0495C5.92192 12.0562 3.67688 12.2686 1.86105 13.1359C1.17859 13.4619 0.89395 14.1338 1.03531 14.7708C1.16763 15.3669 1.65025 15.8565 2.30197 16.0155C3.09682 16.2094 4.01898 16.4603 4.82087 16.7446C5.22197 16.8868 5.58059 17.033 5.87253 17.1791C6.17489 17.3305 6.35954 17.4592 6.45095 17.5506C6.54226 17.6419 6.67088 17.8263 6.82207 18.1284C6.96805 18.42 7.1141 18.7783 7.25611 19.179C7.54 19.9802 7.79044 20.9016 7.98401 21.696C8.14285 22.3479 8.63248 22.8307 9.22867 22.9632C9.86557 23.1047 10.5377 22.8203 10.8639 22.1378C11.7306 20.3247 11.9449 18.0842 11.9533 16.3152C11.9541 16.1408 12.1006 15.994 12.2774 15.994C13.4793 15.9945 14.8833 15.8962 16.2272 15.5627C17.5663 15.2305 18.901 14.6511 19.9033 13.6488C21.0225 12.5295 21.5711 11.04 21.8198 9.59295C22.0692 8.14162 22.0286 6.66744 21.9145 5.50058C21.7365 3.68024 20.3259 2.26963 18.5056 2.09167C17.3387 1.97759 15.8645 1.93698 14.4132 2.18638C12.9661 2.43505 11.4766 2.9836 10.3574 4.10285ZM16.0019 5.0001C16.5542 5.0001 17.0019 5.44781 17.0019 6.0001C17.0019 6.55238 16.5542 7.0001 16.0019 7.0001C15.4496 7.0001 15.0019 6.55238 15.0019 6.0001C15.0019 5.44781 15.4496 5.0001 16.0019 5.0001Z' })
    ])
  },
  {
    key: 'settings',
    name: t('nav.settings'),
    svg: h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', slot: 'start', width: '20', height: '20' }, [
      h('path', { d: 'M12.0122 2.25C12.7462 2.25846 13.4773 2.34326 14.1937 2.50304C14.5064 2.57279 14.7403 2.83351 14.7758 3.15196L14.946 4.67881C15.0231 5.37986 15.615 5.91084 16.3206 5.91158C16.5103 5.91188 16.6979 5.87238 16.8732 5.79483L18.2738 5.17956C18.5651 5.05159 18.9055 5.12136 19.1229 5.35362C20.1351 6.43464 20.8889 7.73115 21.3277 9.14558C21.4223 9.45058 21.3134 9.78203 21.0564 9.9715L19.8149 10.8866C19.4607 11.1468 19.2516 11.56 19.2516 11.9995C19.2516 12.4389 19.4607 12.8521 19.8157 13.1129L21.0582 14.0283C21.3153 14.2177 21.4243 14.5492 21.3297 14.8543C20.8911 16.2685 20.1377 17.5649 19.1261 18.6461C18.9089 18.8783 18.5688 18.9483 18.2775 18.8206L16.8712 18.2045C16.4688 18.0284 16.0068 18.0542 15.6265 18.274C15.2463 18.4937 14.9933 18.8812 14.945 19.3177L14.7759 20.8444C14.741 21.1592 14.5122 21.4182 14.204 21.4915C12.7556 21.8361 11.2465 21.8361 9.79803 21.4915C9.48991 21.4182 9.26105 21.1592 9.22618 20.8444L9.05736 19.32C9.00777 18.8843 8.75434 18.498 8.37442 18.279C7.99451 18.06 7.5332 18.0343 7.1322 18.2094L5.72557 18.8256C5.43422 18.9533 5.09403 18.8833 4.87678 18.6509C3.86462 17.5685 3.11119 16.2705 2.6732 14.8548C2.57886 14.5499 2.68786 14.2186 2.94485 14.0293L4.18818 13.1133C4.54232 12.8531 4.75147 12.4399 4.75147 12.0005C4.75147 11.561 4.54232 11.1478 4.18771 10.8873L2.94516 9.97285C2.6878 9.78345 2.5787 9.45178 2.67337 9.14658C3.11212 7.73215 3.86594 6.43564 4.87813 5.35462C5.09559 5.12236 5.43594 5.05259 5.72724 5.18056L7.12762 5.79572C7.53056 5.97256 7.9938 5.94585 8.37577 5.72269C8.75609 5.50209 9.00929 5.11422 9.05817 4.67764L9.22824 3.15196C9.26376 2.83335 9.49786 2.57254 9.8108 2.50294C10.5281 2.34342 11.26 2.25865 12.0122 2.25ZM12.0124 3.7499C11.5583 3.75524 11.1056 3.79443 10.6578 3.86702L10.5489 4.84418C10.4471 5.75368 9.92003 6.56102 9.13042 7.01903C8.33597 7.48317 7.36736 7.53903 6.52458 7.16917L5.62629 6.77456C5.05436 7.46873 4.59914 8.25135 4.27852 9.09168L5.07632 9.67879C5.81513 10.2216 6.25147 11.0837 6.25147 12.0005C6.25147 12.9172 5.81513 13.7793 5.0771 14.3215L4.27805 14.9102C4.59839 15.752 5.05368 16.5361 5.626 17.2316L6.53113 16.8351C7.36923 16.4692 8.33124 16.5227 9.12353 16.9794C9.91581 17.4361 10.4443 18.2417 10.548 19.1526L10.657 20.1365C11.5466 20.2878 12.4555 20.2878 13.3451 20.1365L13.4541 19.1527C13.5549 18.2421 14.0828 17.4337 14.876 16.9753C15.6692 16.5168 16.6332 16.463 17.4728 16.8305L18.3772 17.2267C18.949 16.5323 19.4041 15.7495 19.7247 14.909L18.9267 14.3211C18.1879 13.7783 17.7516 12.9162 17.7516 11.9995C17.7516 11.0827 18.1879 10.2206 18.9258 9.67847L19.7227 9.09109C19.4021 8.25061 18.9468 7.46784 18.3748 6.77356L17.4783 7.16737C17.113 7.32901 16.7178 7.4122 16.3187 7.41158C14.849 7.41004 13.6155 6.30355 13.4551 4.84383L13.3462 3.8667C12.9007 3.7942 12.4526 3.75512 12.0124 3.7499ZM11.9997 8.24995C14.0708 8.24995 15.7497 9.92888 15.7497 12C15.7497 14.071 14.0708 15.75 11.9997 15.75C9.92863 15.75 8.2497 14.071 8.2497 12C8.2497 9.92888 9.92863 8.24995 11.9997 8.24995ZM11.9997 9.74995C10.7571 9.74995 9.7497 10.7573 9.7497 12C9.7497 13.2426 10.7571 14.25 11.9997 14.25C13.2423 14.25 14.2497 13.2426 14.2497 12C14.2497 10.7573 13.2423 9.74995 11.9997 9.74995Z' })
    ])
  },
])

const onTabChange = (event) => {
  const tabId = event.detail.id
  activeId.value = tabId
  const newIndex = tabDatas.value.findIndex(item => item.key === tabId)
  transitionName.value = newIndex > lastIndex.value ? 'slide-left' : 'slide-right'
  lastIndex.value = newIndex
  router.push({ name: tabId })
}
</script>

<template>
  <div style="height: 100%;display: flex; flex-direction: column;">
    <div class="header">
      <div class="header-item">
        <fluent-menu class="hidden-up" style="--menu-max-height: auto;">
          <fluent-button icon-only appearance="transparent" :title="$t('nav.menu')" slot="trigger">
            <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 -960 960 960" width="24px">
              <path
                d="M160-240q-17 0-28.5-11.5T120-280q0-17 11.5-28.5T160-320h640q17 0 28.5 11.5T840-280q0 17-11.5 28.5T800-240H160Zm0-200q-17 0-28.5-11.5T120-480q0-17 11.5-28.5T160-520h640q17 0 28.5 11.5T840-480q0 17-11.5 28.5T800-440H160Zm0-200q-17 0-28.5-11.5T120-680q0-17 11.5-28.5T160-720h640q17 0 28.5 11.5T840-680q0 17-11.5 28.5T800-640H160Z" />
            </svg>
          </fluent-button>
          <fluent-listbox>
            <gugu-menu-item v-for="tabData in tabDatas" :selected="tabData.key == activeId"
              @click="activeId = tabData.key">
              <template #icon>
                <component :is="tabData.svg" />
              </template>
              {{ tabData.name }}
            </gugu-menu-item>
          </fluent-listbox>
        </fluent-menu>
      </div>
      <div class="header-item"></div>
      <div class="header-item">
        <fluent-text>{{ userInfo.userName }}</fluent-text>
        <fluent-button appearance="transparent" icon-only shape="circular" :title="$t('nav.help')" @click="help">
          <svg width="24" height="24" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
            <path
              d="M12 2C17.523 2 22 6.478 22 12C22 17.522 17.523 22 12 22C6.477 22 2 17.522 2 12C2 6.478 6.477 2 12 2ZM12 3.667C7.405 3.667 3.667 7.405 3.667 12C3.667 16.595 7.405 20.333 12 20.333C16.595 20.333 20.333 16.595 20.333 12C20.333 7.405 16.595 3.667 12 3.667ZM12 15.5C12.5523 15.5 13 15.9477 13 16.5C13 17.0523 12.5523 17.5 12 17.5C11.4477 17.5 11 17.0523 11 16.5C11 15.9477 11.4477 15.5 12 15.5ZM12 6.75C13.5188 6.75 14.75 7.98122 14.75 9.5C14.75 10.5108 14.4525 11.074 13.6989 11.8586L13.5303 12.0303C12.9084 12.6522 12.75 12.9163 12.75 13.5C12.75 13.9142 12.4142 14.25 12 14.25C11.5858 14.25 11.25 13.9142 11.25 13.5C11.25 12.4892 11.5475 11.926 12.3011 11.1414L12.4697 10.9697C13.0916 10.3478 13.25 10.0837 13.25 9.5C13.25 8.80964 12.6904 8.25 12 8.25C11.3528 8.25 10.8205 8.74187 10.7565 9.37219L10.75 9.5C10.75 9.91421 10.4142 10.25 10 10.25C9.58579 10.25 9.25 9.91421 9.25 9.5C9.25 7.98122 10.4812 6.75 12 6.75Z" />
          </svg>
        </fluent-button>
        <GuguLangSwitch />
        <fluent-button appearance="transparent" icon-only shape="circular" :title="$t('theme.switch')"
          @click="switchTheme">
          <svg width="24" height="24" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
            <path
              d="M12 22C17.5228 22 22 17.5228 22 12C22 6.47715 17.5228 2 12 2C6.47715 2 2 6.47715 2 12C2 17.5228 6.47715 22 12 22ZM12 20.5V3.5C16.6944 3.5 20.5 7.30558 20.5 12C20.5 16.6944 16.6944 20.5 12 20.5Z" />
          </svg>
        </fluent-button>
        <fluent-button appearance="transparent" icon-only shape="circular" :title="$t('nav.logout')" @click="logout">
          <svg width="24" height="24" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
            <path
              d="M6.25 3C4.45507 3 3 4.45507 3 6.25V17.75C3 19.5449 4.45507 21 6.25 21H15.25C15.6642 21 16 20.6642 16 20.25C16 19.8358 15.6642 19.5 15.25 19.5H6.25C5.2835 19.5 4.5 18.7165 4.5 17.75V6.25C4.5 5.2835 5.2835 4.5 6.25 4.5H15.25C15.6642 4.5 16 4.16421 16 3.75C16 3.33579 15.6642 3 15.25 3H6.25ZM17.5303 7.21967C17.2374 6.92678 16.7626 6.92678 16.4697 7.21967C16.1768 7.51256 16.1768 7.98744 16.4697 8.28033L19.4393 11.25H8.75C8.33579 11.25 8 11.5858 8 12C8 12.4142 8.33579 12.75 8.75 12.75H19.4393L16.4697 15.7197C16.1768 16.0126 16.1768 16.4874 16.4697 16.7803C16.7626 17.0732 17.2374 17.0732 17.5303 16.7803L21.7803 12.5303C22.0732 12.2374 22.0732 11.7626 21.7803 11.4697L17.5303 7.21967Z" />
          </svg>
        </fluent-button>
      </div>
    </div>
    <fluent-divider></fluent-divider>
    <div class="main">
      <fluent-tablist class="hidden-down" appearance="neutral" :activeid="activeId" :key="locale" @change="onTabChange">
        <fluent-tab v-for="tabData in tabDatas" :id="tabData.key">
          <component :is="tabData.svg" />
          <div class="tab-title">{{ tabData.name }}</div>
        </fluent-tab>
      </fluent-tablist>
      <router-view class="tab-router-view" v-slot="{ Component }">
        <keep-alive>
          <transition :name="transitionName" mode="out-in">
            <component :is="Component" :key="$route.fullPath" />
          </transition>
        </keep-alive>
      </router-view>
      <fluent-drawer ref="drawer" position="start" size="medium" type="modal"
        style="--dialog-backdrop: var(--colorBackgroundOverlay); --drawer-width: 50%; position: absolute;">
        <fluent-drawer-body>
          <h2 slot="title">{{ $t('help.title') }}</h2>
          <fluent-button slot="close" appearance="transparent" icon-only aria-label="close">
            <svg fill="currentColor" aria-hidden="true" width="20" height="20" viewBox="0 0 20 20"
              xmlns="http://www.w3.org/2000/svg">
              <path
                d="m4.09 4.22.06-.07a.5.5 0 0 1 .63-.06l.07.06L10 9.29l5.15-5.14a.5.5 0 0 1 .63-.06l.07.06c.18.17.2.44.06.63l-.06.07L10.71 10l5.14 5.15c.18.17.2.44.06.63l-.06.07a.5.5 0 0 1-.63.06l-.07-.06L10 10.71l-5.15 5.14a.5.5 0 0 1-.63.06l-.07-.06a.5.5 0 0 1-.06-.63l.06-.07L9.29 10 4.15 4.85a.5.5 0 0 1-.06-.63l.06-.07-.06.07Z"
                fill="currentColor" />
            </svg>
          </fluent-button>
          <n-scrollbar style="padding-top: 8px; box-sizing: border-box;">
            <div style="display: flex; flex-direction: column; gap: 8px; margin-right: 16px;">
              <fluent-text weight="bold">{{ $t('help.s1Title') }}</fluent-text>
              <fluent-text>{{ $t('help.s1ProgressDesc') }}</fluent-text>
              <fluent-text>{{ $t('help.s1StepDesc') }}</fluent-text>
              <fluent-text>{{ $t('help.s1Total') }}</fluent-text>
              <br>
              <fluent-text weight="bold">{{ $t('help.s2Title') }}</fluent-text>
              <fluent-text>{{ $t('help.s2Type') }}</fluent-text>
              <fluent-text>{{ $t('help.s2Date') }}</fluent-text>
              <fluent-text>{{ $t('help.s2Public') }}</fluent-text>
              <br>
              <fluent-text weight="bold">{{ $t('help.s3Title') }}</fluent-text>
              <fluent-text>{{ $t('help.s3Progress') }}</fluent-text>
              <fluent-text>{{ $t('help.s3Archive') }}</fluent-text>
              <br>
              <fluent-text weight="bold">{{ $t('help.s4Title') }}</fluent-text>
              <fluent-text>{{ $t('help.s4Bulletin') }}</fluent-text>
              <fluent-text>{{ $t('help.s4ShowPage') }}</fluent-text>
              <fluent-text>{{ $t('help.s4Reminder') }}</fluent-text>
              <fluent-text>{{ $t('help.s4Mask') }}</fluent-text>
              <br>
              <fluent-text weight="bold">{{ $t('help.s5Title') }}</fluent-text>
              <fluent-text>{{ $t('help.s5Gantt') }}</fluent-text>
              <fluent-text>{{ $t('help.s5ReminderLog') }}</fluent-text>

            </div>
          </n-scrollbar>
        </fluent-drawer-body>
      </fluent-drawer>
    </div>
  </div>
</template>

<style scoped>
.header {
  width: 100%;
  height: 48px;
  padding: 8px;
  box-sizing: border-box;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-item {
  display: flex;
  align-items: center;
  gap: 8px;
}

.main {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  flex: 1;
  overflow: hidden;
  background-color: var(--colorNeutralBackground1);
  gap: 8px;
  box-sizing: border-box;
}

.hidden-down {
  display: block;
}

.hidden-up {
  display: none;
}

@media (max-width: 480px) {
  .hidden-down {
    display: none;
  }

  .hidden-up {
    display: block;
  }

  .tab-router-view {
    margin: 16px;
  }
}

fluent-tab {
  margin: 0 8px;
}

.tab-title {
  min-width: max-content;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.slide-left-enter-active {
  transition: all 0.15s cubic-bezier(0, 1, 1, 1);
}

.slide-left-leave-active {
  transition: all 0.15s cubic-bezier(1, 0, 1, 0);
}

.slide-left-enter-from {
  transform: translateX(100%);
  opacity: 0;
}

.slide-left-enter-to {
  transform: translateX(0);
  opacity: 1;
}

.slide-left-leave-from {
  transform: translateX(0);
  opacity: 1;
}

.slide-left-leave-to {
  transform: translateX(-100%);
  opacity: 0;
}

.slide-right-enter-active {
  transition: all 0.15s cubic-bezier(0, 1, 1, 1);
}

.slide-right-leave-active {
  transition: all 0.15s cubic-bezier(1, 0, 1, 0);
}

.slide-right-enter-from {
  transform: translateX(-100%);
  opacity: 0;
}

.slide-right-enter-to {
  transform: translateX(0);
  opacity: 1;
}

.slide-right-leave-from {
  transform: translateX(0);
  opacity: 1;
}

.slide-right-leave-to {
  transform: translateX(100%);
  opacity: 0;
}

.tab-router-view {
  width: 100%;
  max-width: 1200px;
  max-height: 100%;
  padding: 0 16px;
  box-sizing: border-box;
}

fluent-drawer-body::part(content) {
  overflow: hidden;
}
</style>
