<script setup>
import { ref, computed, h } from 'vue'
import { useI18n } from 'vue-i18n'
import { project as projectApi, addMessage } from '@/api'
import { NDataTable, NScrollbar, NPagination } from 'naive-ui'

const { t } = useI18n()

const props = defineProps({
  projectTypeList: { type: Array, default: () => [] }
})

const dialog = ref(null)
const project = ref({})
const progress = ref({})
const overlay = ref(false)
const progressPageRequest = ref({ page: 1, count: 5 })
const progressPagedList = ref({})

const deleteProgressDialog = ref(null)
const deleteProgressData = ref({})
const saving = ref(false)
const deleting = ref(false)

const progressFormKey = ref(0)

const progressColumns = computed(() => [
  { title: t('progress.current'), key: 'currentProgress', minWidth: 60 },
  { title: t('progress.nextReport'), key: 'nextReportProgress', minWidth: 120 },
  {
    title: t('progress.createdAt'), key: 'creationTime', minWidth: 120,
    render(row) { return h('fluent-text', {}, { default: () => new Date(row.creationTime + 'Z').toLocaleString() }) }
  },
  {
    title: t('common.action'), key: 'action', minWidth: 40,
    render(row) {
      return h('div', {}, {
        default: () => [
          h('fluent-button', {
            onClick: () => { deleteProgressData.value = row; deleteProgressDialog.value.show() },
            appearance: 'transparent', iconOnly: true, title: t('common.delete')
          }, h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', style: 'width:18px;height:18px' }, [
            h('path', { d: 'M10 5H14C14 3.89543 13.1046 3 12 3C10.8954 3 10 3.89543 10 5ZM8.5 5C8.5 3.067 10.067 1.5 12 1.5C13.933 1.5 15.5 3.067 15.5 5H21.25C21.6642 5 22 5.33579 22 5.75C22 6.16421 21.6642 6.5 21.25 6.5H19.9309L18.7589 18.6112C18.5729 20.5334 16.9575 22 15.0263 22H8.97369C7.04254 22 5.42715 20.5334 5.24113 18.6112L4.06908 6.5H2.75C2.33579 6.5 2 6.16421 2 5.75C2 5.33579 2.33579 5 2.75 5H8.5ZM10.5 9.75C10.5 9.33579 10.1642 9 9.75 9C9.33579 9 9 9.33579 9 9.75V17.25C9 17.6642 9.33579 18 9.75 18C10.1642 18 10.5 17.6642 10.5 17.25V9.75ZM14.25 9C14.6642 9 15 9.33579 15 9.75V17.25C15 17.6642 14.6642 18 14.25 18C13.8358 18 13.5 17.6642 13.5 17.25V9.75C13.5 9.33579 13.8358 9 14.25 9ZM6.73416 18.4667C6.84577 19.62 7.815 20.5 8.97369 20.5H15.0263C16.185 20.5 17.1542 19.62 17.2658 18.4667L18.4239 6.5H5.57608L6.73416 18.4667Z' })
          ]))
        ]
      })
    }
  }
])

const getProgressList = async (id) => {
  await requestList()
  if (progressPagedList.value.totalCount && progressPageRequest.value.page > 1 && ((progressPageRequest.value.page - 1) * progressPageRequest.value.count >= progressPagedList.value.totalCount)) {
    progressPageRequest.value.page--
    await requestList()
  }
  async function requestList() {
    progressPagedList.value = await projectApi.getProgressList(id, { skip: (progressPageRequest.value.page - 1) * progressPageRequest.value.count, count: progressPageRequest.value.count })
  }
}

const open = async (data) => {
  project.value = JSON.parse(JSON.stringify(data))
  progressPageRequest.value.page = 1
  await getProgressList(project.value.id)
  progress.value = {}
  progressFormKey.value++
  overlay.value = true
  setTimeout(() => dialog.value.show(), 10)
}

const close = () => {
  if (!dialog.value) return
  dialog.value.hide()
  setTimeout(() => { overlay.value = false }, 10)
}

const onSaveProgress = async () => {
  saving.value = true
  try {
    await projectApi.createProgress(project.value.id, progress.value)
    progress.value = {}
    progressFormKey.value++
    addMessage(t('progress.created'), 'success')
    await getProgressList(project.value.id)
  } finally {
    saving.value = false
  }
}

const onDeleteProgress = async () => {
  deleting.value = true
  try {
    await projectApi.deleteProgress(project.value.id, deleteProgressData.value.id)
    deleteProgressDialog.value.hide()
    addMessage(t('progress.deleted'), 'success')
    await getProgressList(project.value.id)
  } finally {
    deleting.value = false
  }
}

defineExpose({ open })
</script>

<template>
  <form @submit.prevent="onSaveProgress" @keydown.enter.prevent>
    <transition name="fade">
      <div v-if="overlay" class="fake-modal"></div>
    </transition>
    <fluent-dialog ref="dialog" type="non-modal">
      <fluent-dialog-body>
        <fluent-button slot="close" appearance="transparent" icon-only aria-label="close" @click="close">
          <svg aria-hidden="true" width="20" height="20" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
            <path d="m4.09 4.22.06-.07a.5.5 0 0 1 .63-.06l.07.06L10 9.29l5.15-5.14a.5.5 0 0 1 .63-.06l.07.06c.18.17.2.44.06.63l-.06.07L10.71 10l5.14 5.15c.18.17.2.44.06.63l-.06.07a.5.5 0 0 1-.63.06l-.07-.06L10 10.71l-5.15 5.14a.5.5 0 0 1-.63.06l-.07-.06a.5.5 0 0 1-.06-.63l.06-.07L9.29 10 4.15 4.85a.5.5 0 0 1-.06-.63l.06-.07-.06.07Z" />
          </svg>
        </fluent-button>
        <h2 slot="title">{{ $t('progress.title') }}-{{ project.name }}</h2>
        <n-scrollbar style="height:100%;max-height:100%;padding:0 20px;box-sizing:border-box;overflow:visible;" trigger="none">
          <div class="flex-col">
            <n-data-table :columns="progressColumns" :data="progressPagedList.items" :bordered="false">
              <template #empty><fluent-text>{{ $t('common.none') }}</fluent-text></template>
            </n-data-table>
            <n-pagination style="align-self:center;" v-model:page="progressPageRequest.page"
              :page-size="progressPageRequest.count" :item-count="progressPagedList.totalCount"
              @update:page="getProgressList(project.id)" />
            <div class="flex-col" :key="progressFormKey">
              <fluent-field label-position="above">
                <label slot="label">{{ $t('progress.current') }}</label>
                <fluent-text-input v-if="project?.type?.progressType === 0 || !project?.extraData?.steps || project?.extraData?.steps?.length === 0"
                  slot="input" required spellcheck="false" autocomplete="off" v-model="progress.currentProgress" />
                <fluent-dropdown v-else slot="input" required type="combobox" :value="progress.currentProgress"
                  @change="(e) => progress.currentProgress = e.target.value">
                  <fluent-listbox>
                    <fluent-option v-for="step in project.extraData.steps" :value="step" :name="step">{{ step }}</fluent-option>
                  </fluent-listbox>
                </fluent-dropdown>
                <fluent-text slot="message" flag="value-missing" class="field-error">{{ $t('common.cannotBeEmpty') }}</fluent-text>
              </fluent-field>
              <fluent-field label-position="above">
                <label slot="label">{{ $t('progress.nextReport') }}</label>
                <fluent-text-input v-if="project?.type?.progressType === 0 || !project?.extraData?.steps || project?.extraData?.steps?.length === 0"
                  slot="input" spellcheck="false" autocomplete="off" v-model="progress.nextReportProgress" />
                <fluent-dropdown v-else slot="input" type="combobox" :value="progress.nextReportProgress"
                  @change="(e) => progress.nextReportProgress = e.target.value">
                  <fluent-listbox>
                    <fluent-option v-for="step in project.extraData.steps" :value="step" :name="step">{{ step }}</fluent-option>
                  </fluent-listbox>
                </fluent-dropdown>
              </fluent-field>
            </div>
          </div>
        </n-scrollbar>
        <fluent-button slot="action" @click="close" :disabled="saving">{{ $t('common.close') }}</fluent-button>
        <fluent-button slot="action" appearance="primary" type="submit" :disabled="saving">
          <div v-show="!saving">{{ $t('common.create') }}</div>
          <fluent-spinner v-show="saving" size="tiny"></fluent-spinner>
        </fluent-button>
      </fluent-dialog-body>
    </fluent-dialog>
  </form>
  <fluent-dialog ref="deleteProgressDialog" type="alert">
    <fluent-dialog-body>
      <fluent-button slot="close" appearance="transparent" icon-only aria-label="close">
        <svg aria-hidden="true" width="20" height="20" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
          <path d="m4.09 4.22.06-.07a.5.5 0 0 1 .63-.06l.07.06L10 9.29l5.15-5.14a.5.5 0 0 1 .63-.06l.07.06c.18.17.2.44.06.63l-.06.07L10.71 10l5.14 5.15c.18.17.2.44.06.63l-.06.07a.5.5 0 0 1-.63.06l-.07-.06L10 10.71l-5.15 5.14a.5.5 0 0 1-.63.06l-.07-.06a.5.5 0 0 1-.06-.63l.06-.07L9.29 10 4.15 4.85a.5.5 0 0 1-.06-.63l.06-.07-.06.07Z" />
        </svg>
      </fluent-button>
      <h2 slot="title">{{ $t('progress.confirmDelete') }}</h2>
      <div style="padding:0 20px;display:flex;flex-direction:column;gap:8px;">
        <fluent-text>{{ $t('progress.confirmDeleteText') }}</fluent-text>
      </div>
      <fluent-button slot="action" appearance="primary" @click="onDeleteProgress" :disabled="deleting">
        <div v-show="!deleting">{{ $t('common.delete') }}</div>
        <fluent-spinner v-show="deleting" size="tiny"></fluent-spinner>
      </fluent-button>
      <fluent-button slot="action" @click="deleteProgressDialog.hide()" :disabled="deleting">{{ $t('common.cancel') }}</fluent-button>
    </fluent-dialog-body>
  </fluent-dialog>
</template>
