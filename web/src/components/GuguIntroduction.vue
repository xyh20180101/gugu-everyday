<script setup>
import { ref, render, h } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const router = useRouter()

const stage = ref(null)

const manyFish = () => {
  const count = 1 + Math.floor(10 * Math.pow(Math.random(), 10))
  for (let i = 0; i < count; i++) {
    setTimeout(() => fish(), i * 50)
  }
}

const fish = () => {
  const randomX = 0.8
  const randomY = 0.8
  const randomR = 3.0
  const g = 9.8 * 100

  const rect = stage.value.getBoundingClientRect()
  const iconSize = 36
  const xStart = rect.width * (0.5 + Math.random() * randomX - randomX / 2) - iconSize / 2
  const xEnd = rect.width * (0.5 + Math.random() * randomX - randomX / 2) - iconSize / 2
  const yMax = Math.random() * rect.height * randomY
  const tTotal = 2 * Math.sqrt(2 * yMax / g)
  const rSpeed = randomR * Math.random() - randomR / 2

  const xDiv = document.createElement('div')
  const yDiv = document.createElement('div')
  const rDiv = document.createElement('div')

  xDiv.className = 'x-div'
  xDiv.style.setProperty('--tTotal', `${tTotal}s`)
  xDiv.style.transform = `translate(${xStart}px,32px)`

  yDiv.className = 'y-div'
  yDiv.style.setProperty('--yMax', `-${yMax}px`)
  yDiv.style.setProperty('--tTotal', `${tTotal}s`)

  rDiv.className = 'r-div'
  rDiv.style.setProperty('--rSpeed', rSpeed)

  stage.value.appendChild(xDiv)
  xDiv.appendChild(yDiv)
  yDiv.appendChild(rDiv)

  render(h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', width: '36', height: '36', fill: 'currentColor' }, [
    h('path', { d: 'M9.51477 11.7244C9.5141 10.5939 9.60735 9.32179 9.901 8.13745C10.1959 6.94819 10.6792 5.90237 11.418 5.16351C12.2382 4.34337 13.3915 3.88395 14.6672 3.66471C15.9387 3.44622 17.2673 3.47776 18.3596 3.58455C19.4669 3.6928 20.3133 4.53921 20.4216 5.64653C20.5284 6.73885 20.5599 8.06743 20.3414 9.33891C20.1222 10.6147 19.6628 11.768 18.8426 12.5881C18.103 13.3278 17.0561 13.8116 15.866 14.1069C14.6809 14.4009 13.4082 14.4944 12.2779 14.494C11.2776 14.4937 10.458 15.3055 10.4533 16.3081C10.4453 18.0032 10.2339 19.9684 9.51719 21.4771C9.49171 21.4556 9.45942 21.415 9.44137 21.3409C9.24247 20.5246 8.97818 19.5478 8.66997 18.678C8.51592 18.2433 8.34657 17.8229 8.16341 17.457C7.98547 17.1015 7.77121 16.7495 7.51152 16.4898C7.2518 16.2302 6.89964 16.0158 6.54394 15.8378C6.17781 15.6545 5.75711 15.485 5.32206 15.3308C4.45163 15.0222 3.47417 14.7575 2.65746 14.5583C2.58366 14.5403 2.54312 14.5082 2.52167 14.4827C4.03232 13.7657 6.00138 13.5559 7.69893 13.5495C8.70241 13.5457 9.51537 12.7255 9.51477 11.7244ZM10.3574 4.10285C9.35577 5.10444 8.7769 6.43816 8.44508 7.77647C8.11203 9.1197 8.01405 10.5232 8.01477 11.7253C8.01488 11.9022 7.86784 12.0488 7.69325 12.0495C5.92192 12.0562 3.67688 12.2686 1.86105 13.1359C1.17859 13.4619 0.89395 14.1338 1.03531 14.7708C1.16763 15.3669 1.65025 15.8565 2.30197 16.0155C3.09682 16.2094 4.01898 16.4603 4.82087 16.7446C5.22197 16.8868 5.58059 17.033 5.87253 17.1791C6.17489 17.3305 6.35954 17.4592 6.45095 17.5506C6.54226 17.6419 6.67088 17.8263 6.82207 18.1284C6.96805 18.42 7.1141 18.7783 7.25611 19.179C7.54 19.9802 7.79044 20.9016 7.98401 21.696C8.14285 22.3479 8.63248 22.8307 9.22867 22.9632C9.86557 23.1047 10.5377 22.8203 10.8639 22.1378C11.7306 20.3247 11.9449 18.0842 11.9533 16.3152C11.9541 16.1408 12.1006 15.994 12.2774 15.994C13.4793 15.9945 14.8833 15.8962 16.2272 15.5627C17.5663 15.2305 18.901 14.6511 19.9033 13.6488C21.0225 12.5295 21.5711 11.04 21.8198 9.59295C22.0692 8.14162 22.0286 6.66744 21.9145 5.50058C21.7365 3.68024 20.3259 2.26963 18.5056 2.09167C17.3387 1.97759 15.8645 1.93698 14.4132 2.18638C12.9661 2.43505 11.4766 2.9836 10.3574 4.10285ZM16.0019 5.0001C16.5542 5.0001 17.0019 5.44781 17.0019 6.0001C17.0019 6.55238 16.5542 7.0001 16.0019 7.0001C15.4496 7.0001 15.0019 6.55238 15.0019 6.0001C15.0019 5.44781 15.4496 5.0001 16.0019 5.0001Z' })
  ]), rDiv)

  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      xDiv.style.transform = `translate(${xEnd}px,32px)`
    })
  })

  xDiv.addEventListener('animationend', () => {
    xDiv.remove()
  })
}

const toLogin = () => {
  const token = localStorage.getItem('token')
  router.push({ name: token ? 'home' : 'login' })
}
</script>

<template>
  <div class="introduction" ref="stage">
    <fluent-text class="text" size="800" weight="bold">GuGu Everyday</fluent-text>
    <fluent-text class="text">{{ $t('publicPage.slogan') }}</fluent-text>
    <fluent-button @click="manyFish" icon-only shape="circular" appearance="transparent" style="margin-top:20px;">
      <svg style="width:36px;height:36px;" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
        <path d="M9.51477 11.7244C9.5141 10.5939 9.60735 9.32179 9.901 8.13745C10.1959 6.94819 10.6792 5.90237 11.418 5.16351C12.2382 4.34337 13.3915 3.88395 14.6672 3.66471C15.9387 3.44622 17.2673 3.47776 18.3596 3.58455C19.4669 3.6928 20.3133 4.53921 20.4216 5.64653C20.5284 6.73885 20.5599 8.06743 20.3414 9.33891C20.1222 10.6147 19.6628 11.768 18.8426 12.5881C18.103 13.3278 17.0561 13.8116 15.866 14.1069C14.6809 14.4009 13.4082 14.4944 12.2779 14.494C11.2776 14.4937 10.458 15.3055 10.4533 16.3081C10.4453 18.0032 10.2339 19.9684 9.51719 21.4771C9.49171 21.4556 9.45942 21.415 9.44137 21.3409C9.24247 20.5246 8.97818 19.5478 8.66997 18.678C8.51592 18.2433 8.34657 17.8229 8.16341 17.457C7.98547 17.1015 7.77121 16.7495 7.51152 16.4898C7.2518 16.2302 6.89964 16.0158 6.54394 15.8378C6.17781 15.6545 5.75711 15.485 5.32206 15.3308C4.45163 15.0222 3.47417 14.7575 2.65746 14.5583C2.58366 14.5403 2.54312 14.5082 2.52167 14.4827C4.03232 13.7657 6.00138 13.5559 7.69893 13.5495C8.70241 13.5457 9.51537 12.7255 9.51477 11.7244ZM10.3574 4.10285C9.35577 5.10444 8.7769 6.43816 8.44508 7.77647C8.11203 9.1197 8.01405 10.5232 8.01477 11.7253C8.01488 11.9022 7.86784 12.0488 7.69325 12.0495C5.92192 12.0562 3.67688 12.2686 1.86105 13.1359C1.17859 13.4619 0.89395 14.1338 1.03531 14.7708C1.16763 15.3669 1.65025 15.8565 2.30197 16.0155C3.09682 16.2094 4.01898 16.4603 4.82087 16.7446C5.22197 16.8868 5.58059 17.033 5.87253 17.1791C6.17489 17.3305 6.35954 17.4592 6.45095 17.5506C6.54226 17.6419 6.67088 17.8263 6.82207 18.1284C6.96805 18.42 7.1141 18.7783 7.25611 19.179C7.54 19.9802 7.79044 20.9016 7.98401 21.696C8.14285 22.3479 8.63248 22.8307 9.22867 22.9632C9.86557 23.1047 10.5377 22.8203 10.8639 22.1378C11.7306 20.3247 11.9449 18.0842 11.9533 16.3152C11.9541 16.1408 12.1006 15.994 12.2774 15.994C13.4793 15.9945 14.8833 15.8962 16.2272 15.5627C17.5663 15.2305 18.901 14.6511 19.9033 13.6488C21.0225 12.5295 21.5711 11.04 21.8198 9.59295C22.0692 8.14162 22.0286 6.66744 21.9145 5.50058C21.7365 3.68024 20.3259 2.26963 18.5056 2.09167C17.3387 1.97759 15.8645 1.93698 14.4132 2.18638C12.9661 2.43505 11.4766 2.9836 10.3574 4.10285ZM16.0019 5.0001C16.5542 5.0001 17.0019 5.44781 17.0019 6.0001C17.0019 6.55238 16.5542 7.0001 16.0019 7.0001C15.4496 7.0001 15.0019 6.55238 15.0019 6.0001C15.0019 5.44781 15.4496 5.0001 16.0019 5.0001Z" />
      </svg>
    </fluent-button>
    <fluent-text class="text" size="500" style="margin-top:20px;">{{ $t('publicPage.subtitle') }}</fluent-text>
    <fluent-button style="margin-top:8px;" appearance="primary" @click="toLogin">{{ $t('publicPage.login') }}</fluent-button>
  </div>
</template>

<style scoped>
.introduction {
  height: 100%;
  width: 100%;
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 8px;
  align-items: center;
  justify-content: center;
  z-index: 2;
}

.text {
  text-align: center;
}

::v-deep(.x-div) {
  position: absolute;
  left: 0;
  bottom: -8px;
  transition: transform var(--tTotal) linear;
  z-index: 0;
}

::v-deep(.y-div) {
  animation: move var(--tTotal) forwards;
}

::v-deep(.r-div) {
  transform-origin: calc(50% + 6px) calc(50% - 6px);
  animation: spin var(--tTotal) linear infinite;
  color: var(--colorNeutralForeground2);
}

@keyframes move {
  0% { transform: translateY(0); animation-timing-function: cubic-bezier(0, 0, 0.5, 1); }
  50% { transform: translateY(var(--yMax)); animation-timing-function: cubic-bezier(0.5, 0, 1, 1); }
  100% { transform: translateY(0); }
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  50% { transform: rotate(calc(360deg * var(--rSpeed))); }
  100% { transform: rotate(calc(720deg * var(--rSpeed))); }
}

.scale-enter-active {
  transition: opacity var(--durationGentle) var(--curveDecelerateMid), transform var(--durationGentle) var(--curveDecelerateMid);
  transform-origin: center;
}
.scale-leave-active {
  transition: opacity var(--durationGentle) var(--curveAccelerateMid), transform var(--durationGentle) var(--curveAccelerateMid);
  transform-origin: center;
}
.scale-enter-from, .scale-leave-to { opacity: 0; transform: scale(0.85); }
.scale-enter-to, .scale-leave-from { opacity: 1; transform: scale(1); }
</style>
