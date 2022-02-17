<script setup lang="ts">
import { normal } from '../composition/changeSize'
import DocumentCategories from './DocumentCategories.vue'
import { rawHtml } from '../composition/documentData';
import { ref, watch } from 'vue';
import hljs from '../composition/gethljs';

watch(()=>rawHtml.value, (value)=>{
let htmlContent = document.getElementById('htmlContent')
    if (htmlContent){
        htmlContent.innerHTML = value;
        hljs.highlightAll();
    }
});
</script>

<template>
    <el-row>
        <el-col v-if="normal" :span="4">
            <DocumentCategories v-if="normal"></DocumentCategories>
        </el-col>
        <el-col v-if="normal" :span="1"></el-col>
        <el-col :span="normal?19:24">
            <el-card id="articleContent">
                <span id="htmlContent">
                    <h1 style="text-align: center;">
                        Welcome to documents!!!
                    </h1>
                </span>
            </el-card>
        </el-col>
    </el-row>
</template>

<style>
.articlesTitle {
    margin-left: 15px;
}
#articleContent {
    max-width: 768px;
    margin-left: auto;
    margin-right: auto;
}
</style>
