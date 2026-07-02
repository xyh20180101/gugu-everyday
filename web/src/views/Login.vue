<script setup>
import { ref, inject } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { auth, addMessage } from '@/api'
import { solvePow } from '@/public'
import { NIcon } from 'naive-ui'
import GuguLangSwitch from '@/components/GuguLangSwitch.vue'

const { t } = useI18n()

const router = useRouter()

const themeName = inject('themeName')
const switchTheme = () => {
  themeName.value = themeName.value === 'webDarkTheme' ? 'webLightTheme' : 'webDarkTheme'
}

const loginModel = ref({
  email: '',
  password: ''
})

const loginLoading = ref(false)
const onLogin = async () => {
  loginLoading.value = true
  try {
    const loginChallengeResponse = await auth.loginChallenge(loginModel.value.email)
    const { counter, hash } = await solvePow(loginChallengeResponse.challenge, loginChallengeResponse.nonce)
    const loginResponse = await auth.login({
      email: loginModel.value.email,
      password: loginModel.value.password,
      nonce: loginChallengeResponse.nonce,
      counter: counter.toString()
    })
    localStorage.setItem('token', loginResponse.token)
    router.push({ name: 'home' })
    addMessage(t('login.loginSuccess'), 'success', 2000)
  }
  finally {
    loginLoading.value = false
  }
}

const currentCard = ref('login')

const registerModel = ref({
  email: '',
  password: '',
  confirmPassword: ''
})

const confirmPasswordErrorText = ref('')
const checkConfirmPassword = () => {
  const success = registerModel.value.confirmPassword === registerModel.value.password
  if (success) {
    confirmPasswordErrorText.value = ''
  } else {
    confirmPasswordErrorText.value = t('login.passwordMismatch')
  }
  return success
}

const registerLoading = ref(false)
const registerSuccess = ref(false)
const onRegister = async () => {
  try {
    registerLoading.value = true
    if (!checkConfirmPassword())
      return
    const challengeResponse = await auth.loginChallenge(registerModel.value.email)
    const { counter } = await solvePow(challengeResponse.challenge, challengeResponse.nonce)
    await auth.register({
      email: registerModel.value.email,
      password: registerModel.value.password,
      nonce: challengeResponse.nonce,
      counter: counter.toString()
    })
    registerSuccess.value = true
  }
  finally {
    registerLoading.value = false
  }
}

const onOpenResetPassword = () => {
  resetPasswordSendEmailLoading.value = false
  resetPasswordSendEmailSuccess.value = false
  currentCard.value = 'resetPassword'
}
const resetPasswordSendEmailModel = ref({
  email: ''
})
const resetPasswordSendEmailLoading = ref(false)
const resetPasswordSendEmailSuccess = ref(false)
const onResetPasswordSendEmail = async () => {
  try {
    resetPasswordSendEmailLoading.value = true
    const loginChallengeResponse = await auth.loginChallenge(resetPasswordSendEmailModel.value.email)
    const { counter, hash } = await solvePow(loginChallengeResponse.challenge, loginChallengeResponse.nonce)
    await auth.resetPasswordSendEmail({
      email: resetPasswordSendEmailModel.value.email,
      nonce: loginChallengeResponse.nonce,
      counter: counter.toString()
    })
    resetPasswordSendEmailSuccess.value = true
    addMessage(t('login.resetEmailSent'), 'success', 10000)
  }
  finally {
    resetPasswordSendEmailLoading.value = false
  }
}
</script>

<template>
  <div class="main">
    <div style="display:flex;justify-content:end;padding:8px;gap:4px;">
      <GuguLangSwitch />
      <fluent-button appearance="transparent" icon-only shape="circular" :title="$t('publicPage.switchTheme')"
        @click="switchTheme">
        <svg width="24" height="24" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
          <path
            d="M12 22C17.5228 22 22 17.5228 22 12C22 6.47715 17.5228 2 12 2C6.47715 2 2 6.47715 2 12C2 17.5228 6.47715 22 12 22ZM12 20.5V3.5C16.6944 3.5 20.5 7.30558 20.5 12C20.5 16.6944 16.6944 20.5 12 20.5Z" />
        </svg>
      </fluent-button>
    </div>
    <div style="display: flex; flex-direction: column;align-items: center;justify-content: center;height: 100%; padding: 16px;">
      <transition name="scale" mode="out-in" appear>
        <form v-if="currentCard === 'login'" class="card" @submit.prevent="onLogin" @keydown.enter.prevent>
          <label class="title">{{ $t('login.title') }}</label>
          <fluent-field label-position="above">
            <label class="hide-required" slot="label">{{ $t('login.email') }}
              <n-icon style="align-self: center; cursor: pointer; margin-left: 2px;" size="16" id="tips">
                <svg width="24" height="24" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                  <path
                    d="M12 1.999c5.524 0 10.002 4.478 10.002 10.002 0 5.523-4.478 10.001-10.002 10.001-5.524 0-10.002-4.478-10.002-10.001C1.998 6.477 6.476 1.999 12 1.999Zm0 1.5a8.502 8.502 0 1 0 0 17.003A8.502 8.502 0 0 0 12 3.5Zm-.004 7a.75.75 0 0 1 .744.648l.007.102.003 5.502a.75.75 0 0 1-1.493.102l-.007-.101-.003-5.502a.75.75 0 0 1 .75-.75ZM12 7.003a.999.999 0 1 1 0 1.997.999.999 0 0 1 0-1.997Z" />
                </svg>
              </n-icon>
              <fluent-tooltip anchor="tips" positioning="above">
                {{ $t('login.tips') }}
              </fluent-tooltip>
            </label>
            <fluent-text-input slot="input" :required="true" type="email" spellcheck="false" autocomplete="username"
              v-model="loginModel.email"></fluent-text-input>
            <fluent-text slot="message" flag="value-missing" class="field-error">
              {{ $t('common.cannotBeEmpty') }}
            </fluent-text>
            <fluent-text slot="message" flag="type-mismatch" class="field-error">
              {{ $t('login.emailFormatError') }}
            </fluent-text>
          </fluent-field>
          <fluent-field label-position="above">
            <label class="hide-required" slot="label">{{ $t('login.password') }}</label>
            <fluent-text-input slot="input" :required="true" type="password" autocomplete="current-password"
              v-model="loginModel.password"></fluent-text-input>
            <fluent-text slot="message" flag="value-missing" class="field-error">
              {{ $t('common.cannotBeEmpty') }}
            </fluent-text>
          </fluent-field>
          <fluent-button style="width: fit-content; align-self: flex-end;margin-top: -8px;" appearance="transparent"
            size="small" @click="onOpenResetPassword">{{ $t('login.forgotPassword') }}</fluent-button>
          <fluent-button appearance="primary" type="submit" :disabled="loginLoading">
            <div v-show="!loginLoading">{{ $t('login.loginBtn') }}</div>
            <fluent-spinner v-show="loginLoading" size="tiny"></fluent-spinner>
          </fluent-button>
          <fluent-button @click="currentCard = 'register'">{{ $t('login.register') }}</fluent-button>
          <fluent-button @click="router.push({ name: 'publicshowpage' })">{{ $t('login.backToHome') }}</fluent-button>
        </form>
        <div v-else-if="currentCard === 'register'" class="card">
          <template v-if="registerSuccess">
            <fluent-text style="display:flex;align-items:center;gap:8px;">
              <svg width="24" height="24" viewBox="0 0 24 24" style="color:var(--colorPaletteGreenForeground1)"
                fill="currentcolor" xmlns="http://www.w3.org/2000/svg">
                <path
                  d="M12 2C17.5228 2 22 6.47715 22 12C22 17.5228 17.5228 22 12 22C6.47715 22 2 17.5228 2 12C2 6.47715 6.47715 2 12 2ZM15.2197 8.96967L10.75 13.4393L8.78033 11.4697C8.48744 11.1768 8.01256 11.1768 7.71967 11.4697C7.42678 11.7626 7.42678 12.2374 7.71967 12.5303L10.2197 15.0303C10.5126 15.3232 10.9874 15.3232 11.2803 15.0303L16.2803 10.0303C16.5732 9.73744 16.5732 9.26256 16.2803 8.96967C15.9874 8.67678 15.5126 8.67678 15.2197 8.96967Z" />
              </svg>
              {{ $t('login.verifyEmailSent') }}
            </fluent-text>
            <fluent-button @click="currentCard = 'login'">{{ $t('login.backToLogin') }}</fluent-button>
          </template>
          <form v-else style="width:100%;display:flex;flex-direction:column;align-items:center;gap:16px;"
            @submit.prevent="onRegister" @keydown.enter.prevent>
            <label class="title">{{ $t('login.registerTitle') }}</label>
            <fluent-field label-position="above">
              <label class="hide-required" slot="label">{{ $t('login.email') }}</label>
              <fluent-text-input slot="input" :required="true" type="email" spellcheck="false" autocomplete="off"
                v-model="registerModel.email"></fluent-text-input>
              <fluent-text slot="message" flag="value-missing" class="field-error">
                {{ $t('common.cannotBeEmpty') }}
              </fluent-text>
              <fluent-text slot="message" flag="type-mismatch" class="field-error">
                {{ $t('login.emailFormatError') }}
              </fluent-text>
            </fluent-field>
            <fluent-field label-position="above">
              <label class="hide-required" slot="label">{{ $t('login.password') }}</label>
              <fluent-text-input slot="input" :required="true" pattern="^(?=.{6,20}$)[\x21-\x7E]+$" type="password"
                autocomplete="off" v-model="registerModel.password"></fluent-text-input>
              <fluent-text slot="message" flag="value-missing" class="field-error">
                {{ $t('login.passwordRule') }}
              </fluent-text>
              <fluent-text slot="message" flag="pattern-mismatch" class="field-error">
                {{ $t('login.passwordRule') }}
              </fluent-text>
            </fluent-field>
            <fluent-field label-position="above">
              <label class="hide-required" slot="label">{{ $t('login.confirmPassword') }}</label>
              <fluent-text-input ref="confirmPasswordRef" slot="input" type="password" @blur="checkConfirmPassword"
                autocomplete="off" v-model="registerModel.confirmPassword"></fluent-text-input>
              <fluent-text slot="message" class="field-error">
                {{ confirmPasswordErrorText }}
              </fluent-text>
            </fluent-field>
            <fluent-button appearance="primary" type="submit" :disabled="registerLoading">
              <div v-show="!registerLoading">{{ $t('login.register') }}</div>
              <fluent-spinner v-show="registerLoading" size="tiny"></fluent-spinner>
            </fluent-button>
            <fluent-button @click="currentCard = 'login'">{{ $t('login.backToLogin') }}</fluent-button>
          </form>
        </div>
        <form v-else-if="currentCard === 'resetPassword'" class="card" @submit.prevent="onResetPasswordSendEmail"
          @keydown.enter.prevent>
          <label class="title">{{ $t('login.resetPasswordTitle') }}</label>
          <fluent-field label-position="above">
            <label class="hide-required" slot="label">{{ $t('login.email') }}</label>
            <fluent-text-input slot="input" :required="true" type="email" spellcheck="false" autocomplete="off"
              v-model="resetPasswordSendEmailModel.email"></fluent-text-input>
            <fluent-text slot="message" flag="value-missing" class="field-error">
              {{ $t('common.cannotBeEmpty') }}
            </fluent-text>
          </fluent-field>
          <fluent-button appearance="primary" type="submit"
            :disabled="resetPasswordSendEmailLoading || resetPasswordSendEmailSuccess">
            <div v-show="!resetPasswordSendEmailLoading">{{ $t('login.resetPasswordTitle') }}</div>
            <fluent-spinner v-show="resetPasswordSendEmailLoading" size="tiny"></fluent-spinner>
          </fluent-button>
          <fluent-button @click="currentCard = 'login'">{{ $t('login.backToLogin') }}</fluent-button>
        </form>
      </transition>
    </div>
  </div>
</template>

<style scoped>
.main {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  padding: 0;
  box-sizing: border-box;
}

.card {
  width: 100%;
  max-width: 440px;
  box-sizing: border-box;
  background-color: var(--colorNeutralBackground1);
  border-radius: 12px;
  box-shadow: var(--shadow16);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  padding: 16px;
}

.title {
  font-size: var(--fontSizeBase600);
  font-family: var(--fontFamilyBase);
  color: var(--colorNeutralForeground1);
}

.hide-required::after {
  content: '' !important;
}

fluent-button {
  width: 100%;
}

.scale-enter-active {
  transition: opacity var(--durationGentle) var(--curveDecelerateMid),
    transform var(--durationGentle) var(--curveDecelerateMid);
  transform-origin: center;
}

.scale-leave-active {
  transition: opacity var(--durationGentle) var(--curveAccelerateMid),
    transform var(--durationGentle) var(--curveAccelerateMid);
  transform-origin: center;
}

.scale-enter-from,
.scale-leave-to {
  opacity: 0;
  transform: scale(0.85);
}

.scale-enter-to,
.scale-leave-from {
  opacity: 1;
  transform: scale(1);
}
</style>