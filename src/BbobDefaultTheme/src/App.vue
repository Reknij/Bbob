<script setup lang="ts">
import Menu from './components/Menu.vue'
import Footer from './components/Footer.vue';
import { setMaxWidth, normal } from './composition/changeSize';
import MenuSmall from './components/MenuSmall.vue';
import {scrollToDownInvoke} from './composition/functionsRegister';

setMaxWidth(768);
</script>

<template>
    <el-card id="app-container" v-infinite-scroll="scrollToDownInvoke">
        <el-container>
        <el-header>
            <Menu v-if="normal"></Menu>
            <MenuSmall v-else></MenuSmall>
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
#app-container{
    margin: 0px auto !important;
    max-width: 1024px;
}
</style>
