<script setup lang="ts">
import { ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import Bbob from '../../../Bbob/JSApi/Bbob';
import { routes } from '../router'
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
let defaultActive = ref(routes[0].defaultPath);
function getDefaultActive(){
  for (const route of routes) {
    if (route.name == router.currentRoute.value.name){
      defaultActive.value = route.defaultPath;
    }
  }
}
getDefaultActive();
watch(()=>router.currentRoute.value, ()=>{
  getDefaultActive();
})
</script>

<template>
  <div v-if="props.mode == 'horizontal'" class="nav-wrapper">
    <h1 class="title" @click="router.push('/')">{{ Bbob.meta.blogName }}</h1>
    <div class="nav-container">
      <div :id="`nav-item-${p.name}`" :class="router.currentRoute.value.name == p.name?'nav-item-selected':''" class="nav-item" v-for="(p, i) in routes">
        <router-link class="nav-item-a" :to="p.defaultPath">{{ p.name }}</router-link>
      </div>
    </div>
  </div>
  <el-menu
    v-else-if="props.mode == 'vertical'"
    router
    :default-active="defaultActive"
    @select="props.readyClick && props.readyClick()"
  >
    <el-menu-item v-for="(p, i) in routes" :key="i" :index="p.defaultPath">{{ p.name }}</el-menu-item>
  </el-menu>
</template>

<style>
.title:hover {
  cursor: pointer;
  color: #409EFF
}
.title {
  color: #313331
}
div.menu {
  width: 35px;
  height: 5px;
  background-color: black;
  margin: 6px 0;
}
.nav-wrapper {
  display: flex;
  justify-content: space-between;
  border-bottom: #dcdfe6 solid 1px;
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
  color: #313331;
  margin: auto;
}
.nav-item-a:hover {
  color: #409EFF;
}
.nav-item-selected {
  border-bottom: #409eff solid 2px;
  margin-bottom: 10px;
}
</style>
