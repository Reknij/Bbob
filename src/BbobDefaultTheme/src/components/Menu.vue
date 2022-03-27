<script setup lang="ts">
import { useRouter } from 'vue-router';
import Bbob from '../../../Bbob/JSApi/Bbob';
import { paths } from '../composition/routePaths'
let router = useRouter();

let props = defineProps({
  mode: {
    type: String,
    default: 'horizontal'
  },
  readyClick: {
    type: Function,
    default: undefined
  }
})
</script>

<template>
  <div v-if="props.mode == 'horizontal'" class="nav-wrapper">
    <h1 class="title" @click="router.push('/')">{{ Bbob.meta.blogName }}</h1>
    <div class="nav-container">
      <div :id="`nav-item-${p.path}`" :class="router.currentRoute.value.path == p.path?'nav-item-selected':''" class="nav-item" v-for="(p, i) in paths">
        <router-link class="nav-item-a" :to="p.path">{{ p.name }}</router-link>
      </div>
    </div>
  </div>
  <el-menu
    v-else-if="props.mode == 'vertical'"
    router
    :default-active="router.currentRoute.value.path"
    @select="props.readyClick && props.readyClick()"
  >
    <el-menu-item v-for="(p, i) in paths" :key="i" :index="p.path">{{ p.name }}</el-menu-item>
  </el-menu>
</template>

<style>
.title:hover {
  cursor: pointer;
  color: var(--theme-selected-color);
}
.title {
  color: var(--theme-font-color);
  margin-left: 20px;
}
.nav-wrapper {
  display: flex;
  justify-content: space-between;
  border-bottom: var(--theme-font-color) solid 1px;
}
.nav-container {
  display: flex;
  justify-content: flex-end;
  align-content: center;
}
.nav-item {
  padding: 0px 12px;
  display: inline-block;
  height: 100%;
  display: flex;
}
.nav-item-a {
  text-decoration: none;
  color: var(--theme-font-color);
  margin: auto;
}
.nav-item-a:hover {
  color: var(--theme-selected-color);
}
.nav-item-selected {
  border-bottom: var(--theme-selected-color) solid 3px;
  height: 99%;
  margin-bottom: 10px;
  font-size: large;
  font-weight: bold;
}
</style>
