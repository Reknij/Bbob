<script setup lang="ts">
import { ref } from 'vue';
import Bbob, { LinkInfo } from '../../../Bbob/JSApi/Bbob';
import { rawHtml, toc } from '../composition/documentData';

let activeName = ref('0');

let props = defineProps({
    readyClick: {
        type: Function,
        default: undefined
    }
})
let clickDoc = (link: LinkInfo) => {
    Bbob.api.getArticleFromAddress(link.address, (article) => {
        if (article.contentParsed) {
            rawHtml.value = article.contentParsed;
        }
        if (article.toc) {
            toc.value = article.toc;
        }
    })
    if (props.readyClick) {
        props.readyClick();
    }

}

let blogs: any = {}
for (let index = 0; index < Bbob.blog.categories.length; index++) {
    const category = Bbob.blog.categories[index];
    blogs[category.text] = ref<LinkInfo[]>();
    Bbob.api.getLinkInfosWithAddress(category.address, (linkArray) => {
        blogs[category.text].value = linkArray
    })
}

</script>

<template>
    <div>
        <div v-for="(category, indexCategory) in Bbob.blog.categories" :key="indexCategory">
            <h1>{{ category.text }}</h1>
            <div class="articlesTitle">
                <el-collapse accordion v-model="activeName">
                    <el-collapse-item
                        v-for="(link, index) in blogs[category.text].value"
                        @click="clickDoc(link)"
                        :key="index"
                        :name="`${category.text}/${index}`"
                    >
                        <template #title>
                            <h4 v-if="activeName == `${category.text}/${index}`">{{ link.title }}</h4>
                            <span v-else>{{ link.title }}</span>
                        </template>
                        <span v-html="toc"></span>
                    </el-collapse-item>
                </el-collapse>
            </div>
        </div>
    </div>
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
.toc-item a {
    color: #303133;
    text-decoration: none;
}
.toc-item a:hover {
    color: #409eff;
    text-decoration: underline;
}
.toc-number {
    display: none;
}
</style>
