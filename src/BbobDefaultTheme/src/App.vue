<script setup lang="ts">
import Menu from './components/Menu.vue'
import Footer from './components/Footer.vue';
import { setMaxWidth, normal } from './composition/changeSize';
import MenuSmall from './components/MenuSmall.vue';
import { scrollToDownInvoke } from './composition/functionsRegister';
import { changeToDark } from './composition/modeChange';
import { computed, onMounted, ref, VideoHTMLAttributes, watch } from 'vue';
import Bbob from '../../Bbob/JSApi/Bbob';

setMaxWidth(768);

let hasBackground = false;
let source = ref('');
let isVideo = false;
let dt = Bbob.meta.extra.defaultTheme;
if (dt) {
    let element = document.documentElement as HTMLElement;
    if (dt.background) {
        hasBackground = true;
        element.style.setProperty('--theme-cover-min-width', '1024px');
        source.value = dt.background.sourceH;
        if (dt.background.isVideo) isVideo = dt.background.isVideo;
    }
    if (dt.mode) {
        if (dt.mode == 'dark') changeToDark();
    }
}

function setElMainWidth(normal: boolean) {
    let element = document.documentElement as HTMLElement;
    if (element) {
        if (normal) {
            element.style.setProperty('--mainContentPadding', '20px');
            if (dt && dt.background && dt.background.sourceH) source.value = dt.background.sourceH;
            replay();
        }
        else {
            element.style.setProperty('--mainContentPadding', '20px 8px');
            if (dt && dt.background && dt.background.sourceV) source.value = dt.background.sourceV;
            replay();
        }
    }
    else console.log('is null')
}
setElMainWidth(normal.value);
watch(() => normal.value, () => setElMainWidth(normal.value));
function replay() {
    let video = document.getElementById('videoBg') as HTMLVideoElement;
    if (video) video.load();
}
</script>

<template>
    <img
        class="background bgToCenter"
        style="background-size: cover;"
        v-if="hasBackground && !isVideo"
        :src="source"
    />
    <video
        id="videoBg"
        class="background bgToCenter"
        autoplay
        muted
        loop
        v-else-if="hasBackground && isVideo"
    >
        <source :src="source" type="video/mp4" />
        <source :src="source" type="video/webm" />
        <source :src="source" type="video/ogg" />
    </video>
    <div class="background cover" id="app-cover"></div>
    <el-container
        v-infinite-scroll="scrollToDownInvoke"
        :infinite-scroll-distance="60"
        class="app-container center"
        style="min-height: 100vh;"
    >
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
    <el-backtop :bottom="130" />
</template>

<style>
:root {
    --mainContentPadding: 20px;
    --theme-cover-color: rgba(255, 255, 255, 0.6);
    --theme-font-color: #313331;
    --theme-date-color: #8a8a8a;
    --theme-selected-color: #409eff;
    --theme-background-color: #ffffff99;
    --theme-border-color: #ebeef5;
    --theme-cover-min-width: 100%;
    --theme-drawer-background-color: #ffffff;
    --color: var(--theme-font-color);
}
.app-container {
    max-width: 1024px;
}
.bgToCenter {
    height: 100%;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
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
.cover {
    background-color: var(--theme-cover-color);
    min-width: var(--theme-cover-min-width);
    top: 50%;
    left: 50%;
    -ms-transform: translate(-50%, -50%);
    transform: translate(-50%, -50%);
}
.center {
    margin: 0px auto !important;
}
body {
    margin: 0px;
}
.el-card__body {
    padding: 10px 20px;
}
.el-card {
    --el-card-border-color: var(--theme-border-color);
    --el-card-border-radius: 4px;
    --el-card-padding: 20px;
    --el-card-bg-color: var(--theme-background-color);
    color: var(--theme-font-color);
}
.el-drawer {
    background-color: var(--theme-drawer-background-color);
}
.el-divider__text {
    background-color: transparent;
}
.el-menu {
    background-color: var(--theme-background-color);
}
.el-menu-item {
    color: var(--theme-font-color);
}
.el-menu-item.is-active {
    color: var(--theme-selected-color);
}
#app {
    color: var(--theme-font-color) !important;
}
.el-backtop {
    box-shadow: 0 0 6px rgb(0 0 0 / 30%);
    background-color: var(--theme-background-color);
    right: max(calc(calc(100% - 1024px) / 2 + 15px), 15px) !important;
}
.el-backtop:hover {
    background-color: var(--theme-selected-color);
}

div,
input,
textarea,
button,
select,
a {
  -webkit-tap-highlight-color: rgba(0,0,0,0);
}
</style>
