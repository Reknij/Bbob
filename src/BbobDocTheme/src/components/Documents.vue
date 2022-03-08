<script setup lang="ts">
import { normal } from '../composition/changeSize'
import DocumentCategories from './DocumentCategories.vue'
import { clearCache, getArticle, rawHtml } from '../composition/documentData';
import { onMounted, onUpdated, watch } from 'vue';
import Bbob from '../../../Bbob/JSApi/Bbob';
import { onBeforeRouteLeave, useRouter } from 'vue-router';
import { routes } from '../router';

let router = useRouter();
if (router.currentRoute.value.name == routes[1].name) {
    let rpa = router.currentRoute.value.params.address as string;
    if (!rpa || rpa == 'default') rpa = 'default';
    getArticle(rpa);
}
onBeforeRouteLeave(() => {
    clearCache();
})
onUpdated(()=>clearCache())
onMounted(()=>{
    clearCache();
    drawHtml(rawHtml.value)
})
function drawHtml(value: string) {
    let htmlContent = document.getElementById('htmlContent')
    if (htmlContent) {
        htmlContent.innerHTML = value;
        Bbob.api.executeScriptElements(htmlContent);
    }
}
watch(() => rawHtml.value, drawHtml);
</script>

<template>
    <el-row>
        <el-col v-if="normal" :span="4">
            <DocumentCategories v-if="normal"></DocumentCategories>
        </el-col>
        <el-col v-if="normal" :span="1"></el-col>
        <el-col :span="normal ? 19 : 24">
            <el-card id="articleContent">
                <span id="htmlContent">
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
