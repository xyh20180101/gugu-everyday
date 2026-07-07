<script setup>
import { inject, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { auth, userInfo as userInfoApi, addMessage } from '@/api'
import { NScrollbar } from 'naive-ui'
import { solvePow } from '@/public'

const router = useRouter()
const { t } = useI18n()

const parentUserInfo = inject('userInfo')
const userInfo = ref({ ...(parentUserInfo.value) })
watch(parentUserInfo, (val) => {
  userInfo.value = { ...val }
})

const userIdHash = ref('')

onMounted(async () => {
  const userIdHashResponse = await auth.userIdHash()
  userIdHash.value = userIdHashResponse.userIdHash
})

const saveLoading = ref(false)
const onSave = async () => {
  try {
    saveLoading.value = true
    const saveResponse = await userInfoApi.update(userInfo.value.id, userInfo.value)
    parentUserInfo.value = saveResponse
    addMessage(t('settings.saved'), 'success')
  }
  finally {
    saveLoading.value = false
  }
}

const openPublicShowPage = () => {
  window.open(`/${userIdHash.value}`, '_blank')
}

const copyPublicShowPage = () => {
  navigator.clipboard.writeText(`${window.location.origin}/${userIdHash.value}`)
    .then(() => {
      addMessage(t('settings.linkCopied'), 'success')
    })
    .catch(err => {
      addMessage(t('settings.linkCopyFailed'), 'error')
    })
}

const dialog = ref(null)
const updatePasswordModel = ref({})
const confirmPasswordErrorText = ref('')
const updatePasswordOverlay = ref(false)

const openUpdatePasswordDialog = () => {
  updatePasswordModel.value = {}
  updatePasswordOverlay.value = true
  setTimeout(() => {
    dialog.value.show()
  }, 10)
}

const closeUpdatePasswordDialog = () => {
  if (!dialog.value) return
  dialog.value.hide()
  setTimeout(() => {
    updatePasswordOverlay.value = false
  }, 10)
}

const checkConfirmPassword = () => {
  const success = updatePasswordModel.value.confirmPassword === updatePasswordModel.value.newPassword
  if (success) {
    confirmPasswordErrorText.value = ''
  } else {
    confirmPasswordErrorText.value = t('settings.passwordMismatch')
  }
  return success
}

const updatePasswordLoading = ref(false)
const onUpdatePassword = async () => {
  try {
    updatePasswordLoading.value = true
    if (!checkConfirmPassword())
      return
    const loginChallengeResponse = await auth.updatePasswordChallenge()
    const { counter, hash } = await solvePow(loginChallengeResponse.challenge, loginChallengeResponse.nonce)
    await auth.updatePassword({
      oldPassword: updatePasswordModel.value.oldPassword,
      newPassword: updatePasswordModel.value.newPassword,
      nonce: loginChallengeResponse.nonce,
      counter: counter.toString()
    })
    addMessage(t('settings.passwordChanged'), 'success', 4000)
    setTimeout(() => {
      router.push({ name: 'login' })
    }, 2000)
    closeUpdatePasswordDialog()
  }
  finally {
    updatePasswordLoading.value = false
  }
}
</script>

<template>
  <div style="height: 0px; flex: 1; margin-bottom: 16px; padding: 0;">
    <form style="height:100%;display: flex;flex-direction: column;align-items: start;gap: 16px;padding: 0;"
      @submit.prevent="onSave"
      @keydown.enter="(e) => { if (e.target.tagName === 'FLUENT-TEXTAREA') return; e.preventDefault() }">
      <n-scrollbar style="height:fit-content; padding: 0 16px; box-sizing: border-box;" trigger="none">
        <div style="display: flex;flex-direction: column;gap: 16px;">
          <fluent-field label-position="above">
            <label class="hide-required" slot="label">{{ $t('settings.userName') }}</label>
            <fluent-text-input slot="input" :required="true" spellcheck="false"
              v-model="userInfo.userName"></fluent-text-input>
            <fluent-text slot="message" flag="value-missing" class="field-error">
              {{ $t('common.cannotBeEmpty') }}
            </fluent-text>
          </fluent-field>
          <fluent-button style="width: fit-content;" @click="openUpdatePasswordDialog">{{ $t('settings.changePassword')
          }}</fluent-button>
          <fluent-field label-position="above">
            <label slot="label">{{ $t('settings.bulletin') }}</label>
            <fluent-textarea block slot="input" spellcheck="false" v-model="userInfo.bulletin"></fluent-textarea>
          </fluent-field>
          <fluent-divider style="width: 100%;"></fluent-divider>
          <div style="display: flex;gap: 16px">
            <fluent-button style="width: fit-content;" @click="openPublicShowPage">{{ $t('settings.openShowPage')
            }}</fluent-button>
            <fluent-button style="width: fit-content;" @click="copyPublicShowPage">{{ $t('settings.copyLink')
            }}</fluent-button>
          </div>
          <fluent-field label-position="above">
            <label class="hide-required" slot="label">{{ $t('settings.showPageTitle') }}</label>
            <fluent-text-input slot="input" :required="true" spellcheck="false"
              v-model="userInfo.showPageTitle"></fluent-text-input>
            <fluent-text slot="message" flag="value-missing" class="field-error">
              {{ $t('common.cannotBeEmpty') }}
            </fluent-text>
          </fluent-field>
          <div style="display: flex;gap: 16px">
            <div style="width: 96px;">
              <fluent-field label-position="above" style="width: fit-content;">
                <label slot="label">{{ $t('settings.enableShowPage') }}</label>
                <fluent-switch slot="input" :checked="userInfo.isShowPageEnabled"
                  @change="(event) => { userInfo.isShowPageEnabled = event.target.checked; }"></fluent-switch>
              </fluent-field>
            </div>
            <div style="width: 96px;">
              <fluent-field label-position="above" style="width: fit-content;">
                <label slot="label">{{ $t('settings.enableReminder') }}</label>
                <fluent-switch slot="input" :checked="userInfo.isAllowReminder"
                  @change="(event) => { userInfo.isAllowReminder = event.target.checked; }"></fluent-switch>
              </fluent-field>
            </div>
          </div>
          <fluent-field label-position="above">
            <label class="hide-required" slot="label">{{ $t('settings.mask') }}</label>
            <fluent-text-input slot="input" spellcheck="false" v-model="userInfo.mask"></fluent-text-input>
          </fluent-field>
          <fluent-divider style="width: 100%;"></fluent-divider>
          <fluent-text>{{ $t('settings.export') }}</fluent-text>
          <div slot="input" style="display: flex;gap: 16px;margin-top: -12px;">
            <fluent-button style="width: fit-content;" @click="userInfoApi.exportJson()">json</fluent-button>
            <fluent-button style="width: fit-content;" @click="userInfoApi.exportCsv()">csv</fluent-button>
          </div>
        </div>
      </n-scrollbar>
      <fluent-button appearance="primary" type="submit" style="margin-left: 16px;" :disabled="saveLoading">
        <div v-show="!saveLoading">{{ $t('common.save') }}</div>
        <fluent-spinner v-show="saveLoading" size="tiny"></fluent-spinner>
      </fluent-button>
    </form>
    <form @submit.prevent="onUpdatePassword" @keydown.enter.prevent>
      <transition name="fade">
        <div v-if="updatePasswordOverlay" class="fake-modal"></div>
      </transition>
      <fluent-dialog ref="dialog" type="non-modal">
        <fluent-dialog-body>
          <fluent-button slot="close" appearance="transparent" icon-only aria-label="close"
            @click="closeUpdatePasswordDialog">
            <svg aria-hidden="true" width="20" height="20" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
              <path
                d="m4.09 4.22.06-.07a.5.5 0 0 1 .63-.06l.07.06L10 9.29l5.15-5.14a.5.5 0 0 1 .63-.06l.07.06c.18.17.2.44.06.63l-.06.07L10.71 10l5.14 5.15c.18.17.2.44.06.63l-.06.07a.5.5 0 0 1-.63.06l-.07-.06L10 10.71l-5.15 5.14a.5.5 0 0 1-.63.06l-.07-.06a.5.5 0 0 1-.06-.63l.06-.07L9.29 10 4.15 4.85a.5.5 0 0 1-.06-.63l.06-.07-.06.07Z">
              </path>
            </svg>
          </fluent-button>
          <h2 slot="title">{{ $t('settings.changePassword') }}</h2>
          <n-scrollbar style="height: 100%;max-height: 100%; padding: 0 20px;box-sizing: border-box;overflow: visible;"
            trigger="none">
            <div class="flex-col">
              <fluent-field label-position="above">
                <label class="hide-required" slot="label">{{ $t('settings.oldPassword') }}</label>
                <fluent-text-input slot="input" :required="true" pattern="^[A-Za-z\d]{4,50}$" type="password"
                  autocomplete="off" v-model="updatePasswordModel.oldPassword"></fluent-text-input>
                <fluent-text slot="message" flag="value-missing" class="field-error">
                  {{ $t('settings.passwordRule') }}
                </fluent-text>
                <fluent-text slot="message" flag="pattern-mismatch" class="field-error">
                  {{ $t('settings.passwordRule') }}
                </fluent-text>
              </fluent-field>
              <fluent-field label-position="above">
                <label class="hide-required" slot="label">{{ $t('settings.newPassword') }}</label>
                <fluent-text-input slot="input" :required="true" pattern="^[A-Za-z\d]{4,50}$" type="password"
                  autocomplete="off" v-model="updatePasswordModel.newPassword"></fluent-text-input>
                <fluent-text slot="message" flag="value-missing" class="field-error">
                  {{ $t('settings.passwordRule') }}
                </fluent-text>
                <fluent-text slot="message" flag="pattern-mismatch" class="field-error">
                  {{ $t('settings.passwordRule') }}
                </fluent-text>
              </fluent-field>
              <fluent-field label-position="above">
                <label class="hide-required" slot="label">{{ $t('login.confirmPassword') }}</label>
                <fluent-text-input ref="confirmPasswordRef" slot="input" type="password" @blur="checkConfirmPassword"
                  autocomplete="off" v-model="updatePasswordModel.confirmPassword"></fluent-text-input>
                <fluent-text slot="message" class="field-error">
                  {{ confirmPasswordErrorText }}
                </fluent-text>
              </fluent-field>
            </div>
          </n-scrollbar>
          <fluent-button slot="action" @click="closeUpdatePasswordDialog">
            {{ $t('common.cancel') }}
          </fluent-button>
          <fluent-button slot="action" appearance="primary" type="submit" :disabled="updatePasswordLoading">
            <div v-show="!updatePasswordLoading">{{ $t('common.save') }}</div>
            <fluent-spinner v-show="updatePasswordLoading" size="tiny"></fluent-spinner>
          </fluent-button>
        </fluent-dialog-body>
      </fluent-dialog>
    </form>
  </div>
</template>

<style scoped>
.title {
  font-size: var(--fontSizeBase600);
  font-family: var(--fontFamilyBase);
  color: var(--colorNeutralForeground1);
}

fluent-textarea {
  width: 100%;
  max-width: 100%;
  --min-block-size: 200px;
}

.hide-required::after {
  content: '' !important;
}
</style>