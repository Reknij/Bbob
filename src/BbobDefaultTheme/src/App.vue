<script setup lang="ts">
import Menu from './components/Menu.vue'
import Footer from './components/Footer.vue';
import { setMaxWidth, normal } from './composition/changeSize';
import MenuSmall from './components/MenuSmall.vue';
import { scrollToDownInvoke } from './composition/functionsRegister';
import { watch } from 'vue';

setMaxWidth(768);
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
    <el-card id="app-container" v-infinite-scroll="scrollToDownInvoke">
        <el-container>
            <el-header style="height: auto;">
                <Menu v-if="normal"></Menu>
                <MenuSmall v-else></MenuSmall>
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
    max-width: 1024px;
}
#mainContent{
    padding: var(--mainContentPadding);
}
</style>
