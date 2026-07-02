<script setup>
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { auth } from '@/api'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()

const validLoading = ref(true)
onMounted(async () => {
  await auth.resetPasswordValid({ token: route.query.token })
  validLoading.value = false
})

const resetPasswordModel = ref({
  password: '',
  confirmPassword: ''
})

const confirmPasswordErrorText = ref('')
const checkConfirmPassword = () => {
  const success = resetPasswordModel.value.confirmPassword === resetPasswordModel.value.password
  if (success) {
    confirmPasswordErrorText.value = ''
  } else {
    confirmPasswordErrorText.value = t('resetPassword.passwordMismatch')
  }
  return success
}

const resetPasswordLoading = ref(false)
const onResetPassword = async () => {
  try {
    resetPasswordLoading.value = true
    if (!checkConfirmPassword())
      return
    await auth.resetPassword({
      token: route.query.token,
      password: resetPasswordModel.value.password
    })
    showSuccess.value = true
  }
  finally {
    resetPasswordLoading.value = false
  }
}

const showSuccess = ref(false)
</script>

<template>
  <div class="main">
    <fluent-spinner v-if="validLoading" size="tiny"></fluent-spinner>
    <form v-else class="card" @submit.prevent="onResetPassword" @keydown.enter.prevent>
      <label class="title">{{ $t('resetPassword.title') }}</label>
      <fluent-field label-position="above">
        <label class="hide-required" slot="label">{{ $t('resetPassword.password') }}</label>
        <fluent-text-input slot="input" :required="true" pattern="^(?=.{6,20}$)[\x21-\x7E]+$" type="password" autocomplete="off"
          v-model="resetPasswordModel.password"></fluent-text-input>
        <fluent-text slot="message" flag="value-missing" class="field-error">
          {{ $t('resetPassword.passwordRule') }}
        </fluent-text>
        <fluent-text slot="message" flag="pattern-mismatch" class="field-error">
          {{ $t('resetPassword.passwordRule') }}
        </fluent-text>
      </fluent-field>
      <fluent-field label-position="above">
        <label class="hide-required" slot="label">{{ $t('resetPassword.confirmPassword') }}</label>
        <fluent-text-input ref="confirmPasswordRef" slot="input" type="password" @blur="checkConfirmPassword"
          autocomplete="off" v-model="resetPasswordModel.confirmPassword"></fluent-text-input>
        <fluent-text slot="message" class="field-error">
          {{ confirmPasswordErrorText }}
        </fluent-text>
      </fluent-field>
      <fluent-button v-if="!showSuccess" appearance="primary" type="submit" :disabled="resetPasswordLoading">
        <div v-show="!resetPasswordLoading">{{ $t('resetPassword.submit') }}</div>
        <fluent-spinner v-show="resetPasswordLoading" size="tiny"></fluent-spinner>
      </fluent-button>
      <div v-else style="display: flex;flex-direction: column;gap: 16px;align-items: center;">
        <fluent-text style="display: flex;align-items: center;gap: 8px;">
          <svg width="24" height="24" viewBox="0 0 24 24" style="color:var(--colorPaletteGreenForeground1)" fill="currentcolor" xmlns="http://www.w3.org/2000/svg"> <path d="M12 2C17.5228 2 22 6.47715 22 12C22 17.5228 17.5228 22 12 22C6.47715 22 2 17.5228 2 12C2 6.47715 6.47715 2 12 2ZM15.2197 8.96967L10.75 13.4393L8.78033 11.4697C8.48744 11.1768 8.01256 11.1768 7.71967 11.4697C7.42678 11.7626 7.42678 12.2374 7.71967 12.5303L10.2197 15.0303C10.5126 15.3232 10.9874 15.3232 11.2803 15.0303L16.2803 10.0303C16.5732 9.73744 16.5732 9.26256 16.2803 8.96967C15.9874 8.67678 15.5126 8.67678 15.2197 8.96967Z"/> </svg>
          {{ $t('resetPassword.success') }}
        </fluent-text>
        <fluent-button @click="router.push({ name: 'login' })">{{ $t('resetPassword.goToLogin') }}</fluent-button>
      </div>
    </form>
  </div>
</template>

<style scoped>
.main {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  gap: 100px;
  align-items: center;
  justify-content: center;
  padding: 16px;
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
</style>