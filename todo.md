# TODO

按"价值 / 成本" 排序，每条包含：目标、改动点、验收标准。

---

## 1. 进度时间线 / 历史曲线

**目标**：在首页和项目详情展示项目进度随时间变化的趋势，让创作者看到产出节奏，也让展示页更有"作品成长感"。

**改动点**：
- 后端
  - `ProjectController.GetProgressList` 已按 `CreationTime` 排序，接口本身够用；如果量大再加 `from` / `to` 参数和聚合接口（如 `/api/project/{id}/progress-series`）。
  - `ProjectDto` 已有 `Progresses` 列表；工序类项目（`ProgressType.Step`）聚合为"每个工序停留天数"阶梯图，存储 / 计算在 `ExtraData` 或单独 `ProgressSeriesDto`。
- 前端
  - 复用项目已有 ECharts 依赖，新组件 `GuguProgressChart.vue`。
  - 进度类型项目：折线图，X = `creationTime`，Y = `currentProgress`（若是数值型；纯文本如"第50章"做原始标签展示）。
  - 工序类型项目：横向时间线 / 阶梯图，颜色与项目色一致。
  - 在 `views/Project.vue` 项目卡片下方、`views/PublicShowPage.vue` 卡片下方各放一份。

**验收**：
- 进度类项目能看到至少 2 个进度点时呈现折线，1 个点时呈现单点标注。
- 工序类项目能看到阶段切换的时间线。
- 公开页未登录访问也能看到图表。