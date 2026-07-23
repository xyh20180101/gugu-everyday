<script setup>
const props = defineProps({
  selected: { type: Boolean, default: false }
})
function onClick(e) {
  e.currentTarget.dispatchEvent(
    new CustomEvent('change', { bubbles: true, composed: true })
  )
}
</script>

<template>
  <div role="menuitem" class="menu-item" :class="{ selected: props.selected }" @click="onClick">
    <slot name="icon"></slot>
    <slot></slot>
  </div>
</template>

<style scoped>
.menu-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: var(--borderRadiusMedium);
  cursor: pointer;
  transition: background-color 0.2s ease, color 0.2s ease;
  color: var(--colorNeutralForeground2);
  font: var(--fontWeightRegular) var(--fontSizeBase300) / var(--lineHeightBase300) var(--fontFamilyBase);
  fill: currentcolor;
}

.menu-item:hover,
.menu-item.selected {
  background-color: var(--colorNeutralBackground1Hover);
  color: var(--colorNeutralForeground1Selected);
}
</style>