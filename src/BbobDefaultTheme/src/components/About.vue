<script setup lang="ts">
import { onMounted } from 'vue';
import Bbob from '../../../Bbob/JSApi/Bbob'
import { language } from '../Languages/LanguageHelper';

onMounted(() => {
    Bbob.meta.extra.prerenderNow = true;
})
</script>

<template>
    <div>
        <el-card>
            <template #header>
                <h2>{{ language.description }}</h2>
            </template>
            <p>{{ Bbob.meta.description }}</p>
        </el-card>
        <el-card style="margin-top: 10px;">
            <template #header>
                <h2>{{ language.author }}</h2>
            </template>
            <p>{{ Bbob.meta.author }}</p>
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
</style>
