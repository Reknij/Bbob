<script setup lang="ts">
import { onMounted } from 'vue';
import Bbob from '../../../Bbob/JSApi/Bbob'
import { language } from '../Languages/LanguageHelper';

onMounted(() => {
    Bbob.meta.extra.prerenderNow = true;
})
let dt = Bbob.meta.extra.defaultTheme;
let hasAvatar = false;
if (dt) {
    if (dt.avatar) {
        if (dt.avatar.source) hasAvatar = true;
        if (dt.avatar.type) {
            let element = document.documentElement as HTMLElement;
            if (dt.avatar.type == 'circle') element.style.setProperty('--theme-avatar-radius', '50%')
            if (dt.avatar.type == 'square') element.style.setProperty('--theme-avatar-radius', '0%')
        }
    }
}
</script>

<template>
    <div>
        <el-card>
            <img v-if="hasAvatar" :src="dt.avatar.source" class="avatarImage" />
            <svg
                v-else
                class="avatarImage"
                viewBox="0 0 1024 1024"
                xmlns="http://www.w3.org/2000/svg"
                data-v-ba633cb8
            >
                <path
                    fill="currentColor"
                    d="M512 512a192 192 0 1 0 0-384 192 192 0 0 0 0 384zm0 64a256 256 0 1 1 0-512 256 256 0 0 1 0 512zm320 320v-96a96 96 0 0 0-96-96H288a96 96 0 0 0-96 96v96a32 32 0 1 1-64 0v-96a160 160 0 0 1 160-160h448a160 160 0 0 1 160 160v96a32 32 0 1 1-64 0z"
                />
            </svg>
            <h1 :title="language.author" style="text-align: center;">{{ Bbob.meta.author }}</h1>
        </el-card>
        <el-card style="margin-top: 10px;">
            <template #header>
                <h2>{{ language.description }}</h2>
            </template>
            <p>{{ Bbob.meta.description }}</p>
        </el-card>
        <el-card style="margin-top: 10px;">
            <template #header>
                <h2>{{ language.aboutBlog }}</h2>
            </template>
            <p>{{ Bbob.meta.about }}</p>
        </el-card>
        <el-card style="margin-top: 10px;">
            <template #header>
                <h2>{{ language.lastBuild }}</h2>
            </template>
            <p>{{ Bbob.meta.lastBuild }}</p>
        </el-card>
    </div>
</template>

<style>
.avatarImage {
    margin-left: auto;
    margin-right: auto;
    margin-top: 15px;
    border-radius: var(--theme-avatar-radius);
    border: 1px solid var(--theme-font-color);
    display: block;
    width: 150px;
    height: 150px;
}
</style>
