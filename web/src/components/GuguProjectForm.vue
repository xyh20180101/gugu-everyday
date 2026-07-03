<script setup>
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { project as projectApi, addMessage } from '@/api'
import { NScrollbar, NColorPicker, NTimeline, NTimelineItem } from 'naive-ui'
import GuguDatePicker from '@/components/GuguDatePicker.vue'

const { t } = useI18n()

const props = defineProps({
  projectTypeList: { type: Array, default: () => [] }
})
const emit = defineEmits(['saved'])

const dialog = ref(null)
const project = ref({ extraData: {} })
const stepsString = ref('')
const isCreate = ref(true)
const overlay = ref(false)
const saving = ref(false)

const open = (is_create, data) => {
  isCreate.value = is_create
  if (is_create) {
    project.value = { color: '#115ea3', isPublic: true, extraData: {} }
    stepsString.value = ''
  } else {
    project.value = JSON.parse(JSON.stringify(data))
    stepsString.value = project.value.extraData.steps?.join('\n') ?? ''
  }
  overlay.value = true
  setTimeout(() => dialog.value.show(), 10)
}

const onTypeChange = (event) => {
  project.value.typeId = event.target.value
  project.value.extraData = props.projectTypeList.find(p => p.id === project.value.typeId)?.extraData ?? {}
  if (project.value.extraData.steps)
    stepsString.value = project.value.extraData.steps.join('\n')
}

const close = () => {
  dialog.value.hide()
  setTimeout(() => { overlay.value = false }, 10)
}

const onSave = async () => {
  saving.value = true
  try {
    if (isCreate.value)
      await projectApi.create(project.value)
    else
      await projectApi.update(project.value.id, project.value)
    close()
    addMessage(t('project.saved'), 'success')
    emit('saved')
  } finally {
    saving.value = false
  }
}

defineExpose({ open })
</script>

<template>
  <form @submit.prevent="onSave"
    @keydown.enter="(e) => { if (e.target.tagName === 'FLUENT-TEXTAREA') return; e.preventDefault() }">
    <transition name="fade">
      <div v-if="overlay" class="fake-modal"></div>
    </transition>
    <fluent-dialog ref="dialog" type="non-modal">
      <fluent-dialog-body>
        <fluent-button slot="close" appearance="transparent" icon-only aria-label="close" @click="close">
          <svg aria-hidden="true" width="20" height="20" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
            <path
              d="m4.09 4.22.06-.07a.5.5 0 0 1 .63-.06l.07.06L10 9.29l5.15-5.14a.5.5 0 0 1 .63-.06l.07.06c.18.17.2.44.06.63l-.06.07L10.71 10l5.14 5.15c.18.17.2.44.06.63l-.06.07a.5.5 0 0 1-.63.06l-.07-.06L10 10.71l-5.15 5.14a.5.5 0 0 1-.63.06l-.07-.06a.5.5 0 0 1-.06-.63l.06-.07L9.29 10 4.15 4.85a.5.5 0 0 1-.06-.63l.06-.07-.06.07Z" />
          </svg>
        </fluent-button>
        <h2 slot="title">{{ isCreate ? $t('project.createProject') : $t('project.editProject') }}</h2>
        <n-scrollbar style="height:100%;max-height:100%;padding:0 20px;box-sizing:border-box;overflow:visible;"
          trigger="none">
          <div class="flex-col">
            <fluent-field label-position="above">
              <label slot="label">{{ $t('common.name') }}</label>
              <fluent-text-input slot="input" :required="true" spellcheck="false" autocomplete="off"
                v-model="project.name" />
              <fluent-text slot="message" flag="value-missing" class="field-error">{{ $t('common.cannotBeEmpty')
              }}</fluent-text>
            </fluent-field>
            <fluent-field label-position="above">
              <label slot="label">{{ $t('common.type') }}</label>
              <fluent-dropdown slot="input" :required="true" type="combobox" :value="project.typeId"
                :disabled="!isCreate" @change="onTypeChange">
                <fluent-listbox>
                  <fluent-option v-for="pt in projectTypeList" :value="pt.id" :name="pt.name">
                    {{ pt.name }}
                  </fluent-option>
                </fluent-listbox>
              </fluent-dropdown>
              <fluent-text slot="message" flag="value-missing" class="field-error">{{ $t('common.cannotBeEmpty')
              }}</fluent-text>
            </fluent-field>
            <fluent-field label-position="above">
              <label slot="label">{{ $t('common.description') }}</label>
              <fluent-textarea block slot="input" spellcheck="false" v-model="project.description" />
            </fluent-field>
            <div class="flex-row">
              <fluent-field label-position="above">
                <label slot="label">{{ $t('common.color') }}</label>
                <n-color-picker slot="input" :modes="['hex']" v-model:value="project.color" :show-alpha="false"
                  :render-label="() => ''" />
              </fluent-field>
              <fluent-field label-position="above">
                <label slot="label">{{ $t('common.order') }}</label>
                <fluent-text-input slot="input" spellcheck="false" autocomplete="off" v-model="project.order" />
              </fluent-field>
            </div>
            <div class="flex-row">
              <div style="width:100%;">
                <label>{{ $t('project.startTime') }}</label>
                <gugu-date-picker slot="input" v-model="project.startTime" />
              </div>
              <div style="width:100%;">
                <label>{{ $t('project.endTime') }}</label>
                <gugu-date-picker slot="input" v-model="project.endTime" />
              </div>
            </div>
            <div class="flex-row">
              <div style="flex:1">
                <fluent-field label-position="above" style="width:fit-content;">
                  <label slot="label">{{ $t('project.isPublic') }}</label>
                  <fluent-switch slot="input" :checked="project.isPublic"
                    @change="(e) => project.isPublic = e.target.checked" />
                </fluent-field>
              </div>
              <div style="flex:1"><fluent-field label-position="above" style="width:fit-content;">
                  <label slot="label">{{ $t('project.isMask') }}</label>
                  <fluent-switch slot="input" :checked="project.isMask"
                    @change="(e) => project.isMask = e.target.checked" />
                </fluent-field></div>
            </div>
            <fluent-field label-position="above"
              v-if="projectTypeList.find(p => p.id === project.typeId)?.progressType === 0">
              <label slot="label">{{ $t('project.totalProgress') }}</label>
              <fluent-text-input slot="input" spellcheck="false" autocomplete="off"
                v-model="project.extraData.totalProgress" />
            </fluent-field>
            <fluent-field label-position="above"
              v-if="projectTypeList.find(p => p.id === project.typeId)?.progressType === 0">
              <label slot="label">{{ $t('project.progressUnit') }}</label>
              <fluent-text-input slot="input" spellcheck="false" autocomplete="off"
                v-model="project.extraData.progressUnit" />
            </fluent-field>
            <fluent-field label-position="above"
              v-show="projectTypeList.find(p => p.id === project.typeId)?.progressType === 1">
              <label slot="label">{{ $t('project.steps') }}</label>
              <fluent-textarea block slot="input" spellcheck="false" :placeholder="$t('project.stepsPlaceholder')"
                v-model="stepsString"
                @input="() => { project.extraData.steps = stepsString.split(/\r?\n/).filter(Boolean) }" />
            </fluent-field>
            <div style="width:100%;overflow: auto;" v-show="projectTypeList.find(p => p.id === project.typeId)?.progressType === 1">
              <n-timeline slot="input" horizontal>
                <n-timeline-item v-for="step in project.extraData.steps" :content="step" />
              </n-timeline>
            </div>
          </div>
        </n-scrollbar>
        <fluent-button slot="action" @click="close" :disabled="saving">{{ $t('common.cancel') }}</fluent-button>
        <fluent-button slot="action" appearance="primary" type="submit" :disabled="saving">
          <div v-show="!saving">{{ $t('common.save') }}</div>
          <fluent-spinner v-show="saving" size="tiny"></fluent-spinner>
        </fluent-button>
      </fluent-dialog-body>
    </fluent-dialog>
  </form>
</template>

<style scoped>
fluent-textarea {
  --min-block-size: 100px;
}
</style>
