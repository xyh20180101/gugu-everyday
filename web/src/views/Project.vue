<script setup>
import { onMounted, ref, computed, h } from 'vue'
import { useI18n } from 'vue-i18n'
import { project as projectApi, projectType as projectTypeApi, addMessage } from '@/api'
import { NDataTable, NScrollbar, NPagination } from 'naive-ui'
import GuguProjectForm from '@/components/GuguProjectForm.vue'
import GuguProgressDialog from '@/components/GuguProgressDialog.vue'

const { t } = useI18n()

const filter = ref({ isArchived: false })
const projectTypeList = ref([])
const pageRequest = ref({ page: 1, count: 10 })
const pagedList = ref({})

const projectFormDialog = ref(null)
const progressDialog = ref(null)
const archiveDialog = ref(null)
const deleteDialog = ref(null)
const project = ref({})
const archiving = ref(false)
const deleting = ref(false)

const getProjectTypeList = async () => {
  projectTypeList.value = (await projectTypeApi.getList({ count: 1000 })).items
}

const getList = async () => {
  await requestList()
  if (pagedList.value.totalCount && pageRequest.value.page > 1 && ((pageRequest.value.page - 1) * pageRequest.value.count >= pagedList.value.totalCount)) {
    pageRequest.value.page--
    await requestList()
  }
  async function requestList() {
    pagedList.value = await projectApi.getList({ isArchived: filter.value.isArchived, skip: (pageRequest.value.page - 1) * pageRequest.value.count, count: pageRequest.value.count })
  }
}

onMounted(async () => {
  await getProjectTypeList()
  await getList()
})

const openDialog = (is_create, data) => {
  projectFormDialog.value.open(is_create, data)
}

const openProgressDialog = async (row) => {
  await progressDialog.value.open(row)
}

const openArchiveDialog = (data) => {
  project.value = JSON.parse(JSON.stringify(data))
  archiveDialog.value.show()
}

const onArchive = async () => {
  archiving.value = true
  try {
    await projectApi.setIsArchived(project.value.id, { isArchived: true })
    archiveDialog.value.hide()
    addMessage(t('project.archived'), 'success')
    await getList()
  } finally {
    archiving.value = false
  }
}

const onUnarchive = async (row) => {
  await projectApi.setIsArchived(row.id, { isArchived: false })
  addMessage(t('project.unarchived'), 'success')
  await getList()
}

const openDeleteDialog = (data) => {
  project.value = JSON.parse(JSON.stringify(data))
  deleteDialog.value.show()
}

const onDelete = async () => {
  deleting.value = true
  try {
    await projectApi.delete(project.value.id)
    deleteDialog.value.hide()
    addMessage(t('project.deleted'), 'success')
    await getList()
  } finally {
    deleting.value = false
  }
}

const projectColumns = computed(() => [
  { title: t('common.name'), key: 'name', minWidth: 60 },
  {
    title: t('common.type'), key: 'typeId', minWidth: 60,
    render(row) { return h('fluent-text', {}, { default: () => projectTypeList.value.find(pt => pt.id === row.typeId)?.name ?? '' }) }
  },
  { title: t('common.description'), key: 'description', minWidth: 120 },
  { title: t('common.order'), key: 'order', minWidth: 60 },
  {
    title: t('common.color'), key: 'color', minWidth: 60,
    render(row) { return h('div', { style: 'height:18px;width:18px;background-color:' + row.color }) }
  },
  {
    title: t('project.startTime'), key: 'startTime', minWidth: 100,
    render(row) { return h('fluent-text', {}, { default: () => row.startTime?.split('T')[0] }) }
  },
  {
    title: t('project.endTime'), key: 'endTime', minWidth: 100,
    render(row) { return h('fluent-text', {}, { default: () => row.endTime?.split('T')[0] }) }
  },
  {
    title: t('project.isPublic'), key: 'isPublic', minWidth: 80,
    render(row) {
      return row.isPublic ? h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 20 20', style: 'width:18px;height:18px;fill:currentcolor;' }, [
        h('path', { d: 'M4.53033 12.9697C4.23744 12.6768 3.76256 12.6768 3.46967 12.9697C3.17678 13.2626 3.17678 13.7374 3.46967 14.0303L7.96967 18.5303C8.26256 18.8232 8.73744 18.8232 9.03033 18.5303L20.0303 7.53033C20.3232 7.23744 20.3232 6.76256 20.0303 6.46967C19.7374 6.17678 19.2626 6.17678 18.9697 6.46967L8.5 16.9393L4.53033 12.9697Z' })
      ]) : null
    }
  },
  {
    title: t('common.action'), key: 'action', minWidth: 160,
    render(row) {
      return h('div', {}, {
        default: () => [
          h('fluent-button', {
            onClick: async () => { await openProgressDialog(row) },
            appearance: 'transparent', iconOnly: true, disabled: row.isArchived, title: t('common.progress')
          }, h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 20 20', style: 'width:18px;height:18px' }, [
            h('path', { d: 'M18.95,14.89l0.85-0.79l-0.67-1.15l-1.12,0.33c-0.31-0.28-0.67-0.48-1.07-0.62l-0.28-1.16h-1.33l-0.27,1.17 c-0.39,0.13-0.75,0.34-1.05,0.61l-1.14-0.34L12.2,14.1l0.87,0.81c-0.11,0.54-0.05,0.97,0,1.21l-0.87,0.83l0.67,1.15l1.16-0.36 c0.3,0.26,0.64,0.46,1.03,0.59l0.27,1.17h1.33l0.28-1.16c0.39-0.13,0.75-0.33,1.05-0.6l1.15,0.35l0.67-1.15l-0.86-0.81 C19.07,15.53,18.98,15.07,18.95,14.89z M16.01,17c-0.83,0-1.5-0.67-1.5-1.5s0.67-1.5,1.5-1.5s1.5,0.67,1.5,1.5S16.84,17,16.01,17z M3.8,10c0.37,0,0.69,0.27,0.74,0.64c0.3,2.61,2.45,4.67,5.09,4.85l0.86,1.49C10.33,16.99,10.17,17,10,17 c-3.58,0-6.53-2.68-6.95-6.15C3,10.4,3.35,10,3.8,10z M8,7.25C8,7.66,7.66,8,7.25,8H4C3.45,8,3,7.55,3,7V3.75C3,3.34,3.34,3,3.75,3 S4.5,3.34,4.5,3.75v1.96C5.78,4.07,7.76,3,10,3c3.87,0,7,3.13,7,7h-1.5c0-3.03-2.47-5.5-5.5-5.5c-1.7,0-3.22,0.78-4.22,2h1.47 C7.66,6.5,8,6.84,8,7.25z M10,6c0.41,0,0.75,0.34,0.75,0.75v2.63l1.55,1.55l-0.78,1.34l-2.05-2.05c-0.14-0.14-0.22-0.33-0.22-0.53 V6.75C9.25,6.34,9.59,6,10,6z' })
          ])),
          h('fluent-button', {
            onClick: () => openDialog(false, row),
            appearance: 'transparent', iconOnly: true, disabled: row.isArchived, title: t('common.edit')
          }, h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', style: 'width:18px;height:18px' }, [
            h('path', { d: 'M20.9519 3.0481C19.5543 1.65058 17.2885 1.65064 15.8911 3.04825L3.94103 14.9997C3.5347 15.4061 3.2491 15.9172 3.116 16.4762L2.02041 21.0777C1.96009 21.3311 2.03552 21.5976 2.21968 21.7817C2.40385 21.9659 2.67037 22.0413 2.92373 21.981L7.52498 20.8855C8.08418 20.7523 8.59546 20.4666 9.00191 20.0601L20.952 8.10861C22.3493 6.71112 22.3493 4.4455 20.9519 3.0481ZM16.9518 4.10884C17.7634 3.29709 19.0795 3.29705 19.8912 4.10876C20.7028 4.9204 20.7029 6.23632 19.8913 7.04801L19 7.93946L16.0606 5.00012L16.9518 4.10884ZM15 6.06084L17.9394 9.00018L7.94119 18.9995C7.73104 19.2097 7.46668 19.3574 7.17755 19.4263L3.76191 20.2395L4.57521 16.8237C4.64402 16.5346 4.79168 16.2704 5.00175 16.0603L15 6.06084Z' })
          ])),
          h('fluent-button', {
            onClick: async () => { row.isArchived ? onUnarchive(row) : openArchiveDialog(row) },
            appearance: 'transparent', iconOnly: true, title: row.isArchived ? t('common.unarchive') : t('common.archive')
          }, h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 -960 960 960', style: 'width:22px;height:22px' }, [
            h('path', {
              d: !row.isArchived
                ? 'M480.21-552.46q-10.98 0-18.59 7.38-7.62 7.39-7.62 18.33v134.29l-48.92-48.92q-8.31-8.31-18.39-8.5-10.07-.2-18.77 8.5-8.69 8.69-8.69 18.57 0 9.89 8.69 18.58l89.91 89.91q9.71 9.7 22.35 9.7 12.65 0 22.13-9.84l89.77-89.77q8.3-8.31 8.5-18.39.19-10.07-8.5-18.76-8.7-8.7-18.58-8.7-9.88 0-18.58 8.7L506-392.46v-134.29q0-10.94-7.41-18.33-7.4-7.38-18.38-7.38ZM216-627.85v399.54q0 5.39 3.46 8.85t8.85 3.46h503.38q5.39 0 8.85-3.46t3.46-8.85v-399.54H216ZM231.39-164q-26.63 0-47.01-20.38T164-231.39v-439.38q0-12.84 4.87-24.5 4.86-11.65 14.59-21.5l60.16-60.92q9.84-9.85 21.05-14.08t23.79-4.23h382.31q12.58 0 23.98 4.23T716-777.69L776.54-716q9.73 9.85 14.59 21.69 4.87 11.85 4.87 24.7v438.22q0 26.63-20.38 47.01T728.61-164H231.39Zm-9.77-515.84H738l-57.62-59.93q-1.92-1.92-4.42-3.08-2.5-1.15-5.19-1.15H288.85q-2.69 0-5.2 1.15-2.5 1.16-4.42 3.08l-57.61 59.93ZM480-421.92Z'
                : 'M479.79-301.62q10.98 0 18.59-7.38 7.62-7.38 7.62-18.33v-134.29l48.92 47.93q8.31 8.3 18.39 8.5 10.07.19 18.77-8.5 8.69-8.7 8.69-18.58 0-9.88-8.69-18.58l-89.91-89.3q-9.71-9.7-22.35-9.7-12.65 0-22.13 9.85l-89.77 89.15q-8.3 8.31-8 18.39.31 10.07 9 18.77 8.7 8.69 18.58 8.69 9.88 0 18.58-8.69L454-461.62v134.29q0 10.95 7.41 18.33 7.4 7.38 18.38 7.38ZM216-627.85v399.54q0 5.39 3.46 8.85t8.85 3.46h503.38q5.39 0 8.85-3.46t3.46-8.85v-399.54H216ZM231.39-164q-26.63 0-47.01-20.38T164-231.39v-439.38q0-12.84 4.87-24.5 4.86-11.65 14.59-21.5l60.16-60.92q9.84-9.85 21.05-14.08t23.79-4.23h382.31q12.58 0 23.98 4.23T716-777.69L776.54-716q9.73 9.85 14.59 21.69 4.87 11.85 4.87 24.7v438.22q0 26.63-20.38 47.01T728.61-164H231.39Zm-9.77-515.84H738l-57.62-59.93q-1.92-1.92-4.42-3.08-2.5-1.15-5.19-1.15H288.85q-2.69 0-5.2 1.15-2.5 1.16-4.42 3.08l-57.61 59.93ZM480-421.92Z'
            })
          ])),
          h('fluent-button', {
            onClick: () => openDeleteDialog(row),
            appearance: 'transparent', iconOnly: true, title: t('common.delete')
          }, h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', style: 'width:18px;height:18px' }, [
            h('path', { d: 'M10 5H14C14 3.89543 13.1046 3 12 3C10.8954 3 10 3.89543 10 5ZM8.5 5C8.5 3.067 10.067 1.5 12 1.5C13.933 1.5 15.5 3.067 15.5 5H21.25C21.6642 5 22 5.33579 22 5.75C22 6.16421 21.6642 6.5 21.25 6.5H19.9309L18.7589 18.6112C18.5729 20.5334 16.9575 22 15.0263 22H8.97369C7.04254 22 5.42715 20.5334 5.24113 18.6112L4.06908 6.5H2.75C2.33579 6.5 2 6.16421 2 5.75C2 5.33579 2.33579 5 2.75 5H8.5ZM10.5 9.75C10.5 9.33579 10.1642 9 9.75 9C9.33579 9 9 9.33579 9 9.75V17.25C9 17.6642 9.33579 18 9.75 18C10.1642 18 10.5 17.6642 10.5 17.25V9.75ZM14.25 9C14.6642 9 15 9.33579 15 9.75V17.25C15 17.6642 14.6642 18 14.25 18C13.8358 18 13.5 17.6642 13.5 17.25V9.75C13.5 9.33579 13.8358 9 14.25 9ZM6.73416 18.4667C6.84577 19.62 7.815 20.5 8.97369 20.5H15.0263C16.185 20.5 17.1542 19.62 17.2658 18.4667L18.4239 6.5H5.57608L6.73416 18.4667Z' })
          ]))
        ]
      })
    }
  }
])
</script>

<template>
  <div style="width:100%;height:0;flex:1;display:flex;flex-direction:column;align-items:start;">
    <div style="width:100%;display:flex;justify-content:space-between;">
      <fluent-button appearance="primary" @click="() => openDialog(true)">
        <svg slot="start" width="24" height="24" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
          <path d="M12 3.25C12.4142 3.25 12.75 3.58579 12.75 4V11.25H20C20.4142 11.25 20.75 11.5858 20.75 12C20.75 12.4142 20.4142 12.75 20 12.75H12.75V20C12.75 20.4142 12.4142 20.75 12 20.75C11.5858 20.75 11.25 20.4142 11.25 20V12.75H4C3.58579 12.75 3.25 12.4142 3.25 12C3.25 11.5858 3.58579 11.25 4 11.25H11.25V4C11.25 3.58579 11.5858 3.25 12 3.25Z" />
        </svg>
        {{ $t('project.createProject') }}
      </fluent-button>
      <div style="display:flex;align-items:center;gap:8px">
        <fluent-text>{{ $t('project.viewArchived') }}</fluent-text>
        <fluent-switch :checked="filter.isArchived" @change="(e) => { filter.isArchived = e.target.checked; getList() }" />
      </div>
    </div>

    <GuguProjectForm ref="projectFormDialog" :projectTypeList="projectTypeList" @saved="getList" />
    <GuguProgressDialog ref="progressDialog" :projectTypeList="projectTypeList" />

    <fluent-dialog ref="archiveDialog" type="alert">
      <fluent-dialog-body>
        <fluent-button slot="close" appearance="transparent" icon-only aria-label="close">
          <svg aria-hidden="true" width="20" height="20" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
            <path d="m4.09 4.22.06-.07a.5.5 0 0 1 .63-.06l.07.06L10 9.29l5.15-5.14a.5.5 0 0 1 .63-.06l.07.06c.18.17.2.44.06.63l-.06.07L10.71 10l5.14 5.15c.18.17.2.44.06.63l-.06.07a.5.5 0 0 1-.63.06l-.07-.06L10 10.71l-5.15 5.14a.5.5 0 0 1-.63.06l-.07-.06a.5.5 0 0 1-.06-.63l.06-.07L9.29 10 4.15 4.85a.5.5 0 0 1-.06-.63l.06-.07-.06.07Z" />
          </svg>
        </fluent-button>
        <h2 slot="title">{{ $t('project.confirmArchive') }}</h2>
        <div style="padding:0 20px;display:flex;flex-direction:column;gap:8px;">
          <fluent-text>{{ $t('project.confirmArchiveText') }} <fluent-text style="font-weight:bold;">{{ project.name }}</fluent-text> ？</fluent-text>
          <fluent-text>{{ $t('project.archiveNote') }}</fluent-text>
        </div>
        <fluent-button slot="action" appearance="primary" @click="onArchive" :disabled="archiving">
          <div v-show="!archiving">{{ $t('common.archive') }}</div>
          <fluent-spinner v-show="archiving" size="tiny"></fluent-spinner>
        </fluent-button>
        <fluent-button slot="action" @click="archiveDialog.hide()" :disabled="archiving">{{ $t('common.cancel') }}</fluent-button>
      </fluent-dialog-body>
    </fluent-dialog>

    <fluent-dialog ref="deleteDialog" type="alert">
      <fluent-dialog-body>
        <fluent-button slot="close" appearance="transparent" icon-only aria-label="close">
          <svg aria-hidden="true" width="20" height="20" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
            <path d="m4.09 4.22.06-.07a.5.5 0 0 1 .63-.06l.07.06L10 9.29l5.15-5.14a.5.5 0 0 1 .63-.06l.07.06c.18.17.2.44.06.63l-.06.07L10.71 10l5.14 5.15c.18.17.2.44.06.63l-.06.07a.5.5 0 0 1-.63.06l-.07-.06L10 10.71l-5.15 5.14a.5.5 0 0 1-.63.06l-.07-.06a.5.5 0 0 1-.06-.63l.06-.07L9.29 10 4.15 4.85a.5.5 0 0 1-.06-.63l.06-.07-.06.07Z" />
          </svg>
        </fluent-button>
        <h2 slot="title">{{ $t('project.confirmDelete') }}</h2>
        <div style="padding:0 20px;display:flex;flex-direction:column;gap:8px;">
          <fluent-text>{{ $t('project.confirmDeleteText') }} <fluent-text style="font-weight:bold;">{{ project.name }}</fluent-text> ？</fluent-text>
          <fluent-text>{{ $t('project.deleteNote') }}</fluent-text>
        </div>
        <fluent-button slot="action" appearance="primary" @click="onDelete" :disabled="deleting">
          <div v-show="!deleting">{{ $t('common.delete') }}</div>
          <fluent-spinner v-show="deleting" size="tiny"></fluent-spinner>
        </fluent-button>
        <fluent-button slot="action" @click="deleteDialog.hide()" :disabled="deleting">{{ $t('common.cancel') }}</fluent-button>
      </fluent-dialog-body>
    </fluent-dialog>

    <n-scrollbar style="flex:1;margin:16px 0;max-height:fit-content;">
      <n-data-table :columns="projectColumns" :data="pagedList.items" :bordered="false">
        <template #empty><fluent-text>{{ $t('common.none') }}</fluent-text></template>
      </n-data-table>
    </n-scrollbar>
    <n-pagination style="align-self:center;margin-bottom:16px;" v-model:page="pageRequest.page"
      :page-size="pageRequest.count" :item-count="pagedList.totalCount" @update:page="getList()" />
  </div>
</template>

<style scoped>
::v-deep(.n-scrollbar-rail) {
  z-index: 1 !important;
}
</style>
