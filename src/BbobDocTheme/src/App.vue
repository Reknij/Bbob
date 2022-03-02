<script setup lang="ts">
import Menu from './components/Menu.vue';
import Footer from './components/Footer.vue';
import MenuSmall from './components/MenuSmall.vue';
import { setMaxWidth, normal } from './composition/changeSize'
import DocumentCategories from './components/DocumentCategories.vue';
import { useRouter } from 'vue-router';
import { paths } from './composition/routePaths';
import { watch } from 'vue';

let router = useRouter();
setMaxWidth(768)
const ifInDocuments = () => {
  if (router.currentRoute.value.path == paths[1].path) return true;
  else return false;
}
function setElMainWidth(normal: boolean) {
    let element = document.documentElement as HTMLElement;
    if (element) {
        if (normal) element.style.setProperty('--mainContentPadding', '20px');
        else element.style.setProperty('--mainContentPadding', '20px 0px');
    }
    else console.log('is null')
}
setElMainWidth(normal.value);
watch(() => normal.value, () => setElMainWidth(normal.value));
</script>

<template>
  <el-card id="app-container">
    <el-container>
      <el-header style="height: auto;">
        <Menu v-if="normal"></Menu>
        <MenuSmall v-else #default="ms">
          <DocumentCategories v-if="ifInDocuments()" :ready-click="() => ms.offIt()"></DocumentCategories>
        </MenuSmall>
      </el-header>
      <el-main id="mainContent">
        <router-view></router-view>
      </el-main>
      <el-footer style="height: auto;">
        <Footer></Footer>
      </el-footer>
    </el-container>
  </el-card>
</template>

<style>
:root{
  --mainContentPadding: 20px;
}
#app-container {
  margin: 0px auto !important;
}
#mainContent{
  padding: var(--mainContentPadding);
}
</style>
