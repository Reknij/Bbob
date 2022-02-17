<script setup lang="ts">
import Menu from './components/Menu.vue';
import Footer from './components/Footer.vue';
import MenuSmall from './components/MenuSmall.vue';
import { setMaxWidth, normal } from './composition/changeSize'
import DocumentCategories from './components/DocumentCategories.vue';
import { useRouter } from 'vue-router';
import { paths } from './composition/routePaths';

let router = useRouter();
setMaxWidth(768)
const ifInDocuments = () => {
  if (router.currentRoute.value.path == paths[1].path) return true;
  else return false;
}
</script>

<template>
  <el-card id="app-container">
    <el-container>
      <el-header>
        <Menu v-if="normal"></Menu>
        <MenuSmall v-else #default="ms">
          <DocumentCategories v-if="ifInDocuments()" :ready-click="() => ms.offIt()"></DocumentCategories>
        </MenuSmall>
      </el-header>
      <el-main>
        <router-view></router-view>
      </el-main>
      <el-footer>
        <Footer></Footer>
      </el-footer>
    </el-container>
  </el-card>
</template>

<style>
#app-container {
  margin: 0px auto !important;
}
</style>
