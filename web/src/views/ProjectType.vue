<script setup>
import { onMounted, h, ref, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { projectType as projectTypeApi, addMessage } from '@/api'
import { NDataTable, NScrollbar, NTimeline, NTimelineItem, NPagination } from 'naive-ui'

const { t } = useI18n()

const projectTypeColumns = computed(() => [
  {
    title: t('common.name'),
    key: 'name',
    minWidth: 60
  },
  {
    title: t('projectType.progressType'),
    key: 'progressType',
    minWidth: 80,
    render(row) {
      return h('fluent-text', {}, {
        default: () => {
          switch (row.progressType) {
            case 0: return t('projectType.progressTypeProgress')
            case 1: return t('projectType.progressTypeStep')
            default: return ''
          }
        }
      })
    }
  },
  {
    title: t('projectType.progressAttr'),
    key: 'extraData',
    minWidth: 80,
    render(row) {
      return h('fluent-text', {}, {
        default: () => {
          switch (row.progressType) {
            case 0: return `${row.extraData.totalProgress ?? ''}${row.extraData.progressUnit ?? ''}`
            case 1: return `${row.extraData.steps ?? ''}`
            default: return ''
          }
        }
      })
    }
  },
  {
    title: t('common.action'),
    key: 'action',
    minWidth: 80,
    render(row) {
      return h(
        'div',
        { style: 'display: flex;' },
        {
          default: () => [
            h(
              'fluent-button',
              {
                onClick: () => openDialog(false, row),
                appearance: 'transparent',
                iconOnly: true,
                title: t('common.edit')
              },
              h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', style: 'width:18px;height:18px' }, [
                h('path', { d: 'M20.9519 3.0481C19.5543 1.65058 17.2885 1.65064 15.8911 3.04825L3.94103 14.9997C3.5347 15.4061 3.2491 15.9172 3.116 16.4762L2.02041 21.0777C1.96009 21.3311 2.03552 21.5976 2.21968 21.7817C2.40385 21.9659 2.67037 22.0413 2.92373 21.981L7.52498 20.8855C8.08418 20.7523 8.59546 20.4666 9.00191 20.0601L20.952 8.10861C22.3493 6.71112 22.3493 4.4455 20.9519 3.0481ZM16.9518 4.10884C17.7634 3.29709 19.0795 3.29705 19.8912 4.10876C20.7028 4.9204 20.7029 6.23632 19.8913 7.04801L19 7.93946L16.0606 5.00012L16.9518 4.10884ZM15 6.06084L17.9394 9.00018L7.94119 18.9995C7.73104 19.2097 7.46668 19.3574 7.17755 19.4263L3.76191 20.2395L4.57521 16.8237C4.64402 16.5346 4.79168 16.2704 5.00175 16.0603L15 6.06084Z' })
              ])
            ),
            h(
              'fluent-button',
              {
                onClick: () => { deleteData.value = row; deleteDialog.value.show() },
                appearance: 'transparent',
                iconOnly: true,
                title: t('common.delete')
              },
              h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', style: 'width:18px;height:18px' }, [
                h('path', { d: 'M10 5H14C14 3.89543 13.1046 3 12 3C10.8954 3 10 3.89543 10 5ZM8.5 5C8.5 3.067 10.067 1.5 12 1.5C13.933 1.5 15.5 3.067 15.5 5H21.25C21.6642 5 22 5.33579 22 5.75C22 6.16421 21.6642 6.5 21.25 6.5H19.9309L18.7589 18.6112C18.5729 20.5334 16.9575 22 15.0263 22H8.97369C7.04254 22 5.42715 20.5334 5.24113 18.6112L4.06908 6.5H2.75C2.33579 6.5 2 6.16421 2 5.75C2 5.33579 2.33579 5 2.75 5H8.5ZM10.5 9.75C10.5 9.33579 10.1642 9 9.75 9C9.33579 9 9 9.33579 9 9.75V17.25C9 17.6642 9.33579 18 9.75 18C10.1642 18 10.5 17.6642 10.5 17.25V9.75ZM14.25 9C14.6642 9 15 9.33579 15 9.75V17.25C15 17.6642 14.6642 18 14.25 18C13.8358 18 13.5 17.6642 13.5 17.25V9.75C13.5 9.33579 13.8358 9 14.25 9ZM6.73416 18.4667C6.84577 19.62 7.815 20.5 8.97369 20.5H15.0263C16.185 20.5 17.1542 19.62 17.2658 18.4667L18.4239 6.5H5.57608L6.73416 18.4667Z' })
              ])
            )
          ]
        }
      )
    }
  },
])

const pageRequest = ref({ page: 1, count: 10 })
const pagedList = ref({})
const getList = async () => {
  await requestList()
  if (pagedList.value.totalCount && pageRequest.value.page > 1 && ((pageRequest.value.page - 1) * pageRequest.value.count >= pagedList.value.totalCount)) {
    pageRequest.value.page--
    await requestList()
  }
  async function requestList() {
    pagedList.value = await projectTypeApi.getList({ skip: (pageRequest.value.page - 1) * pageRequest.value.count, count: pageRequest.value.count })
  }
}
onMounted(async () => {
  await getList()
})

const dialog = ref(null)
const projectType = ref({ progressType: 0, extraData: {} })
const stepsString = ref('')
const isCreate = ref(true)
const projectTypeOverlay = ref(false)
const saving = ref(false)
const deleting = ref(false)

const openDialog = (is_create, data) => {
  isCreate.value = is_create
  if (is_create) {
    projectType.value = {
      name: '',
      progressType: 0,
      extraData: {}
    }
    stepsString.value = ''
  }
  else {
    projectType.value = JSON.parse(JSON.stringify(data))
    stepsString.value = data.extraData.steps?.join('\n') ?? ''
  }
  projectTypeOverlay.value = true
  setTimeout(() => {
    dialog.value.show()
  }, 10)
}
const onSave = async () => {
  saving.value = true
  try {
    if (isCreate.value)
      await projectTypeApi.create(projectType.value)
    else
      await projectTypeApi.update(projectType.value.id, projectType.value)
    closeDialog()
    addMessage(t('projectType.saved'), 'success')
    await getList()
  } finally {
    saving.value = false
  }
}

const closeDialog = () => {
  dialog.value.hide()
  setTimeout(() => {
    projectTypeOverlay.value = false
  }, 10)
}

const deleteDialog = ref(null)
const deleteData = ref('')

const onDelete = async () => {
  deleting.value = true
  try {
    await projectTypeApi.delete(deleteData.value.id)
    deleteDialog.value.hide()
    addMessage(t('projectType.deleted'), 'success')
  } finally {
    deleting.value = false
  }
  await getList()
}
</script>

<template>
  <div style="width: 100%;height: 0; flex: 1; display: flex;flex-direction: column;align-items: start;">
    <fluent-button appearance="primary" @click="() => openDialog(true)">
      <svg slot="start" width="24" height="24" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
        <path
          d="M12 3.25C12.4142 3.25 12.75 3.58579 12.75 4V11.25H20C20.4142 11.25 20.75 11.5858 20.75 12C20.75 12.4142 20.4142 12.75 20 12.75H12.75V20C12.75 20.4142 12.4142 20.75 12 20.75C11.5858 20.75 11.25 20.4142 11.25 20V12.75H4C3.58579 12.75 3.25 12.4142 3.25 12C3.25 11.5858 3.58579 11.25 4 11.25H11.25V4C11.25 3.58579 11.5858 3.25 12 3.25Z" />
      </svg>
      {{ $t('projectType.createTitle') }}
    </fluent-button>
    <form @submit.prevent="onSave" @cancel="() => dialog.hide()"
      @keydown.enter="(e) => { if (e.target.tagName === 'FLUENT-TEXTAREA') return; e.preventDefault() }">
      <transition name="fade">
        <div v-if="projectTypeOverlay" class="fake-modal"></div>
      </transition>
      <fluent-dialog ref="dialog" type="non-modal">
        <fluent-dialog-body>
          <fluent-button slot="close" appearance="transparent" icon-only aria-label="close" @click="closeDialog">
            <svg aria-hidden="true" width="20" height="20" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
              <path
                d="m4.09 4.22.06-.07a.5.5 0 0 1 .63-.06l.07.06L10 9.29l5.15-5.14a.5.5 0 0 1 .63-.06l.07.06c.18.17.2.44.06.63l-.06.07L10.71 10l5.14 5.15c.18.17.2.44.06.63l-.06.07a.5.5 0 0 1-.63.06l-.07-.06L10 10.71l-5.15 5.14a.5.5 0 0 1-.63.06l-.07-.06a.5.5 0 0 1-.06-.63l.06-.07L9.29 10 4.15 4.85a.5.5 0 0 1-.06-.63l.06-.07-.06.07Z">
              </path>
            </svg>
          </fluent-button>
          <h2 slot="title">{{ isCreate ? $t('projectType.createTitle') : $t('projectType.editTitle') }}</h2>
          <n-scrollbar style="height: 100%;max-height: 100%; padding: 0 20px;box-sizing: border-box;overflow: visible;"
            trigger="none">
            <div class="flex-col">
              <fluent-field label-position="above">
                <label slot="label">{{ $t('common.name') }}</label>
                <fluent-text-input slot="input" :required="true" spellcheck="false" autocomplete="off"
                  v-model="projectType.name"></fluent-text-input>
                <fluent-text slot="message" flag="value-missing" class="field-error">
                  {{ $t('common.cannotBeEmpty') }}
                </fluent-text>
              </fluent-field>
              <fluent-field label-position="above">
                <label slot="label">{{ $t('projectType.progressType') }}</label>
                <fluent-dropdown slot="input" type="select"
                  @change="(event) => { projectType.progressType = event.target.value }">
                  <fluent-listbox>
                    <fluent-option :value="0" :selected="projectType.progressType === 0"
                      :name="$t('projectType.progressTypeProgress')">
                      {{ $t('projectType.progressTypeProgress') }}
                    </fluent-option>
                    <fluent-option :value="1" :selected="projectType.progressType === 1"
                      :name="$t('projectType.progressTypeStep')">
                      {{ $t('projectType.progressTypeStep') }}
                    </fluent-option>
                  </fluent-listbox>
                </fluent-dropdown>
                <fluent-text slot="message" flag="value-missing" class="field-error">
                  {{ $t('common.cannotBeEmpty') }}
                </fluent-text>
              </fluent-field>
              <fluent-field label-position="above" v-if="projectType.progressType === 0">
                <label slot="label">{{ $t('project.totalProgress') }}</label>
                <fluent-text-input slot="input" spellcheck="false" autocomplete="off"
                  v-model="projectType.extraData.totalProgress"></fluent-text-input>
              </fluent-field>
              <fluent-field label-position="above" v-if="projectType.progressType === 0">
                <label slot="label">{{ $t('project.progressUnit') }}</label>
                <fluent-text-input slot="input" spellcheck="false" autocomplete="off"
                  v-model="projectType.extraData.progressUnit"></fluent-text-input>
              </fluent-field>
              <fluent-field label-position="above" v-show="projectType.progressType === 1">
                <label slot="label">{{ $t('project.steps') }}</label>
                <fluent-textarea block slot="input" spellcheck="false" :placeholder="$t('project.stepsPlaceholder')" v-model="stepsString"
                  @input="(event) => { projectType.extraData.steps = stepsString.split(/\r?\n/).filter(Boolean) }"></fluent-textarea>
              </fluent-field>
              <div style="width:100%;overflow: auto;" v-show="projectType.progressType === 1">
                <n-timeline slot="input" horizontal>
                  <n-timeline-item v-for="step in projectType.extraData.steps" :content="step" />
                </n-timeline>
              </div>
            </div>
          </n-scrollbar>
          <fluent-button slot="action" @click="closeDialog" :disabled="saving">
            {{ $t('common.cancel') }}
          </fluent-button>
          <fluent-button slot="action" appearance="primary" type="submit" :disabled="saving">
            <div v-show="!saving">{{ $t('common.save') }}</div>
            <fluent-spinner v-show="saving" size="tiny"></fluent-spinner>
          </fluent-button>
        </fluent-dialog-body>
      </fluent-dialog>
    </form>
    <fluent-dialog ref="deleteDialog" type="alert">
      <fluent-dialog-body>
        <fluent-button slot="close" appearance="transparent" icon-only aria-label="close">
          <svg aria-hidden="true" width="20" height="20" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
            <path
              d="m4.09 4.22.06-.07a.5.5 0 0 1 .63-.06l.07.06L10 9.29l5.15-5.14a.5.5 0 0 1 .63-.06l.07.06c.18.17.2.44.06.63l-.06.07L10.71 10l5.14 5.15c.18.17.2.44.06.63l-.06.07a.5.5 0 0 1-.63.06l-.07-.06L10 10.71l-5.15 5.14a.5.5 0 0 1-.63.06l-.07-.06a.5.5 0 0 1-.06-.63l.06-.07L9.29 10 4.15 4.85a.5.5 0 0 1-.06-.63l.06-.07-.06.07Z">
            </path>
          </svg>
        </fluent-button>
        <h2 slot="title">{{ $t('projectType.confirmDelete') }}</h2>
        <fluent-text style="padding: 0 20px;">{{ $t('projectType.confirmDeleteText') }} <fluent-text style="font-weight: bold;">{{ deleteData.name
            }}</fluent-text> ？</fluent-text>
        <fluent-button slot="action" appearance="primary" @click="onDelete" :disabled="deleting">
          <div v-show="!deleting">{{ $t('common.delete') }}</div>
          <fluent-spinner v-show="deleting" size="tiny"></fluent-spinner>
        </fluent-button>
        <fluent-button slot="action" @click="deleteDialog.hide()" :disabled="deleting">
          {{ $t('common.cancel') }}
        </fluent-button>
      </fluent-dialog-body>
    </fluent-dialog>
    <n-scrollbar style="flex: 1; margin: 16px 0;max-height: fit-content;">
      <n-data-table :columns="projectTypeColumns" :data="pagedList.items" :bordered="false">
        <template #empty>
          <fluent-text>{{ $t('common.none') }}</fluent-text>
        </template>
      </n-data-table>
    </n-scrollbar>
    <n-pagination style="align-self: center;margin-bottom: 16px;" v-model:page="pageRequest.page"
      :page-size="pageRequest.count" :item-count="pagedList.totalCount" @update:page="getList()" />
  </div>
</template>

<style scoped>
fluent-textarea {
  width: 100%;
  max-width: 100%;
  --min-block-size: 100px;
}

fluent-listbox[popover]:not([popover="manual"]) {
  display: none !important;
}
</style>