<script setup>
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import { NPopover, NDatePicker } from 'naive-ui'

const props = defineProps({
  modelValue: String
})
const emit = defineEmits(['update:modelValue'])

watch(() => props.modelValue, (val) => {
  dateString.value = val?.split('T')[0]
})

const show = ref(false)
const dateString = ref(props.modelValue)
const datepicker = ref(null)

const onFocus = () => {
  show.value = true
}

const onClickOutside = (e) => {
  const inputEl = document.querySelector('fluent-text-input')
  const dpEl = datepicker.value?.$el
  if (
    !inputEl.contains(e.target) &&
    !dpEl?.contains(e.target)
  ) {
    show.value = false
    emit('update:modelValue', dateString.value)
  }
}

onMounted(() => window.addEventListener('pointerdown', onClickOutside))
onBeforeUnmount(() => window.removeEventListener('pointerdown', onClickOutside))

const onDateUpdate = (e) => {
  dateString.value = e
  show.value = false
  emit('update:modelValue', dateString.value)
}
</script>

<template>
  <n-popover placement="bottom-start" trigger="manual" :show="show" :to="false" :show-arrow="false">
    <template #trigger>
      <fluent-text-input autocomplete="off" v-model="dateString" @focus="onFocus" readonly>
        <span slot="end">
          <fluent-button v-if="dateString" appearance="transparent" icon-only @click="dateString = null">
            <svg aria-hidden="true" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg"
              style="width: 18px;height: 18px;">
              <path
                d="m4.09 4.22.06-.07a.5.5 0 0 1 .63-.06l.07.06L10 9.29l5.15-5.14a.5.5 0 0 1 .63-.06l.07.06c.18.17.2.44.06.63l-.06.07L10.71 10l5.14 5.15c.18.17.2.44.06.63l-.06.07a.5.5 0 0 1-.63.06l-.07-.06L10 10.71l-5.15 5.14a.5.5 0 0 1-.63.06l-.07-.06a.5.5 0 0 1-.06-.63l.06-.07L9.29 10 4.15 4.85a.5.5 0 0 1-.06-.63l.06-.07-.06.07Z">
              </path>
            </svg>
          </fluent-button>
        </span>
      </fluent-text-input>
    </template>
    <n-date-picker ref="datepicker" panel type="date" :formatted-value="dateString"
      :on-update:formatted-value="onDateUpdate" />
  </n-popover>
</template>

<style scoped>
::v-deep(.n-date-panel-calendar) {
  width: 200px;
}
</style>