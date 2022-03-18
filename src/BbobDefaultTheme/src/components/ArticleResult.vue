<script setup lang="ts">
import { ref, watch } from 'vue';
import { onBeforeRouteLeave, useRoute } from 'vue-router';
import Bbob, { Article } from '../../../Bbob/JSApi/Bbob'
import { normal } from '../composition/changeSize';

const noArticle = 'No exists article!';
const route = useRoute();
let address = route.params.address ? route.params.address as string : '';
if (Bbob.meta.extra.shortAddress) {
    address = `${Bbob.meta.extra.shortAddress.startOfAddress}${address}${Bbob.meta.extra.shortAddress.endOfAddress}`
}
else {
    address = route.query.address as string;
}
let article = ref<Article>({
    title: noArticle,
    date: noArticle,
})
Bbob.api.getArticleFromAddress(address, (art) => {
    let htmlContent = document.getElementById('htmlContent');
    article.value = art;
    if (htmlContent && art.contentParsed) {
        htmlContent.innerHTML = art.contentParsed;
        Bbob.meta.extra.prerenderNow = true;
        Bbob.api.executeScriptElements(htmlContent);
    }
    document.title = art.title;
})

onBeforeRouteLeave(() => {
    document.title = Bbob.meta.blogName
    return true;
});

let tocDrawer = ref(false)
</script>

<template>
    <div id="parent">
        <div id="content">
            <el-card>
                <span id="htmlContent"></span>
            </el-card>

            <el-drawer
                :size="normal ? '50%' : '100%'"
                v-if="article.toc"
                v-model="tocDrawer"
                direction="ltr"
            >
                <template #title>
                    <h4>Table of content</h4>
                </template>
                <template #default>
                    <div>
                        <span @click="tocDrawer = false" v-html="article.toc"></span>
                    </div>
                </template>
            </el-drawer>

            <el-affix style="margin-left: calc(100% - 60px);" :offset="80" position="bottom">
                <el-button @click="tocDrawer = true" type="primary">Toc</el-button>
            </el-affix>
        </div>
    </div>
</template>

<style>
#content {
    max-width: 768px;
    margin-left: auto;
    margin-right: auto;
}
.toc-item a {
    color: #303133;
    text-decoration: none;
}
.toc-item a:hover {
    color: #409eff;
    text-decoration: underline;
}
div#content img {
    display: block;
    margin: 0px auto;
    max-width: calc(768px - var(--el-card-padding) * 2);
}
</style>
