<script setup>
import { ref, watch, inject, nextTick, onBeforeUnmount } from 'vue'
import * as echarts from 'echarts/core'
import { LineChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, MarkAreaComponent } from 'echarts/components'
import { SVGRenderer } from 'echarts/renderers'
import { getProgressText } from '@/public'
import { NPopover } from 'naive-ui'

echarts.use([LineChart, GridComponent, TooltipComponent, MarkAreaComponent, SVGRenderer])

const props = defineProps({
  project: { type: Object, required: true }
})
const emit = defineEmits(['reminder'])

const chartDom = ref(null)
let chart = null
let resizeObserver = null

const themeName = inject('themeName')
watch(themeName, (val) => {
  if (chart && chartDom.value) {
    console.log(themeName.value)
    chart.dispose()
    chart = echarts.init(chartDom.value, val === 'webDarkTheme' ? 'dark' : 'light')
    update()
  }
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', resize)
  if (resizeObserver) {
    resizeObserver.disconnect()
    resizeObserver = null
  }
  if (chart) {
    chart.dispose()
    chart = null
  }
})

const initChart = async (show) => {
  if (!show) {
    // 关闭时清理 observer
    if (resizeObserver) {
      resizeObserver.disconnect()
      resizeObserver = null
    }
    if (chart) {
      chart.dispose()
      chart = null // 确保设为 null，下次点开才能重新进入初始化
    }
    return
  }
  await nextTick()
  if (chartDom.value) {
    resizeObserver = new ResizeObserver((entries) => {
      const { width, height } = entries[0].contentRect
      if (width === 0 || height === 0) return
      if (!chart) {
        chart = echarts.init(chartDom.value, themeName.value === 'webDarkTheme' ? 'dark' : 'light')
        window.removeEventListener('resize', resize)
        window.addEventListener('resize', resize)
        update()
      }
    })
    resizeObserver.observe(chartDom.value)
  }
}

const update = () => {
  const progresses = props.project.progresses ?? []
  const progressesMinTime = progresses.length > 0 ? new Date(Math.min(...progresses.map(p => new Date(p.creationTime)))) : null
  const progressesMaxTime = progresses.length > 0 ? new Date(Math.max(...progresses.map(p => new Date(p.creationTime)))) : null
  const startTime = props.project.startTime
    ? progressesMinTime ? new Date(Math.min(new Date(props.project.startTime), progressesMinTime)) : new Date(props.project.startTime)
    : progressesMinTime
  const endTime = props.project.endTime
    ? progressesMaxTime ? new Date(Math.max(new Date(props.project.endTime), progressesMaxTime)) : new Date(props.project.endTime)
    : progressesMaxTime

  const data = progresses.map(p => ({
    value: [new Date(p.creationTime), p.currentProgressValue ?? 0]
  }))
  const markAreaData = [[
    { xAxis: new Date(props.project.startTime) },
    { xAxis: new Date(props.project.endTime) }
  ]]

  const options = {
    backgroundColor: 'transparent',
    animation: false,
    grid: {
      left: 64,
      right: 32,
      bottom: 32,
      top: 16
    },
    xAxis: {
      type: 'time',
      min: startTime,
      max: endTime,
      axisLabel: {
        formatter: (value) => {
          const d = new Date(value)
          return `${d.getMonth() + 1}/${d.getDate()}`
        }
      },
      splitLine: { show: false }
    },
    yAxis: {
      type: 'value',
      min: 0,
      max: 1,
      axisLabel: {
        formatter: (v) => `${(v * 100).toFixed(0)}%`
      },
      splitLine: { show: false }
    },
    series: [{
      type: 'line',
      data: data,
      smooth: true,
      symbol: 'circle',
      symbolSize: 6,
      lineStyle: { width: 2, color: props.project.color },
      itemStyle: { color: props.project.color },
      markArea: {
        silent: true,
        itemStyle: {
          color: 'var(--colorNeutralStrokeAlpha)'
        },
        data: markAreaData
      }
    }]
  }

  chart.setOption(options)
}

const resize = () => {
  if (chart) chart.resize()
}

const getFontColor = (hex) => {
  if (hex.startsWith("#")) hex = hex.slice(1);
  if (hex.length === 3) hex = hex.split("").map(c => c + c).join("");
  const r = parseInt(hex.slice(0, 2), 16);
  const g = parseInt(hex.slice(2, 4), 16);
  const b = parseInt(hex.slice(4, 6), 16);
  return (0.299 * r + 0.587 * g + 0.114 * b) > 128 ? '#424242' : '#d6d6d6';
}
</script>

<template>
  <n-popover placement="bottom-center" trigger="click" :to="false" :show-arrow="false" width="trigger"
    @update:show="initChart">
    <template #trigger>
      <div class="project" :style="{ backgroundColor: project.color }">
        <div class="progress-fill"
          :style="{ width: ((project.progresses.at(-1)?.currentProgressValue ?? 0) * 100).toFixed(2) + '%', backgroundColor: '#fff' }">
        </div>
        <fluent-text class="left" weight="bold" :style="{ zIndex: 1, color: getFontColor(project.color) }"
          :title="project.name">{{ project.name }}</fluent-text>
        <div class="center">
          <fluent-text :style="{ zIndex: 1, color: getFontColor(project.color) }">{{ getProgressText(project)
          }}</fluent-text>
        </div>
        <fluent-button class="right" appearance="transparent" icon-only :title="$t('publicPage.remind')" @click.stop="emit('reminder', project.id)">
          <svg :style="{ width: '24px', height: '24px', color: getFontColor(project.color) }" width="24" height="24"
            viewBox="0 -960 960 960" xmlns="http://www.w3.org/2000/svg">
            <path
              d="M600-160q-134 0-227-93t-93-227q0-133 93-226.5T600-800q133 0 226.5 93.5T920-480q0 134-93.5 227T600-160Zm0-80q100 0 170-70t70-170q0-100-70-170t-170-70q-100 0-170 70t-70 170q0 100 70 170t170 70Zm40-256v-104q0-17-11.5-28.5T600-640q-17 0-28.5 11.5T560-600v121q0 8 3.5 15.5T572-451l91 91q12 12 28.5 12t28.5-12q12-12 12-28.5T720-417l-80-79ZM120-600q-17 0-28.5-11.5T80-640q0-17 11.5-28.5T120-680h80q17 0 28.5 11.5T240-640q0 17-11.5 28.5T200-600h-80ZM80-440q-17 0-28.5-11.5T40-480q0-17 11.5-28.5T80-520h120q17 0 28.5 11.5T240-480q0 17-11.5 28.5T200-440H80Zm40 160q-17 0-28.5-11.5T80-320q0-17 11.5-28.5T120-360h80q17 0 28.5 11.5T240-320q0 17-11.5 28.5T200-280h-80Zm480-200Z" />
          </svg>
        </fluent-button>
      </div>
    </template>
    <div class="chart" ref="chartDom"></div>
  </n-popover>
</template>

<style scoped>
.project {
  height: 30px;
  width: 100%;
  max-width: 1200px;
  border-radius: 4px;
  padding: 0 4px 0 8px;
  box-sizing: border-box;
  display: flex;
  justify-content: space-between;
  align-items: center;
  position: relative;
  overflow: hidden;
  cursor: pointer;
  box-shadow: var(--shadow4);
}

.left {
  position: absolute;
  left: 8px;
  max-width: 33%;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.center {
  position: absolute;
  left: 50%;
  transform: translateX(-50%);
  max-width: 33%;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.right {
  position: absolute;
  right: 4px;
}

.progress-fill {
  position: absolute;
  left: 0;
  bottom: 0;
  z-index: 0;
  height: 2px;
}

.chart {
  width: 100%;
  height: 240px;
}
</style>
