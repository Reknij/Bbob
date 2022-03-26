<script setup lang="ts">
import Menu from './components/Menu.vue'
import Footer from './components/Footer.vue';
import { setMaxWidth, normal } from './composition/changeSize';
import MenuSmall from './components/MenuSmall.vue';
import { scrollToDownInvoke } from './composition/functionsRegister';
import { watch } from 'vue';
import Bbob from '../../Bbob/JSApi/Bbob';

setMaxWidth(768);
function setElMainWidth(normal: boolean) {
    let element = document.documentElement as HTMLElement;
    if (element) {
        if (normal) element.style.setProperty('--mainContentPadding', '20px');
        else element.style.setProperty('--mainContentPadding', '20px 5px');
    }
    else console.log('is null')
}
setElMainWidth(normal.value);
watch(() => normal.value, () => setElMainWidth(normal.value));

let hasBackground = false;
let dt = Bbob.meta.extra.defaultTheme;
if (dt) {
    if (dt.background) hasBackground = true;
}
</script>

<template>
    <img class="background" v-if="hasBackground" :src="dt.background" />
    <div class="background backgroundApp"></div>
    <el-container v-infinite-scroll="scrollToDownInvoke" class="app-container center" style="min-height: 100vh;">
        <el-header style="height: auto;">
            <div class="app-container">
                <Menu v-if="normal"></Menu>
                <MenuSmall v-else></MenuSmall>
            </div>
        </el-header>
        <el-main id="mainContent">
            <div class="app-container">
                <router-view></router-view>
            </div>
        </el-main>
        <el-footer style="height: auto;">
            <Footer></Footer>
        </el-footer>
    </el-container>
</template>

<style>
:root {
    --mainContentPadding: 20px;
}
.app-container {
    max-width: 1024px;
}
#mainContent {
    padding: var(--mainContentPadding);
}
.background {
    position: fixed;
    right: 0px;
    bottom: 0px;
    min-width: 100%;
    min-height: 100%;
    height: auto;
    width: auto;
    z-index: -10;
}
.backgroundApp {
    background-color: rgba(255, 255, 255, 0.6);
    min-width: 1024px;
    top: 50%;
    left: 50%;
    -ms-transform: translate(-50%, -50%);
    transform: translate(-50%, -50%);
}
.center {
    margin: 0px auto !important;
}
body{
    margin: 0px;
}
</style>
