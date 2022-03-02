<script setup lang="ts">
import { ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import Bbob, { LinkInfo } from '../../../Bbob/JSApi/Bbob';
import { rawHtml, toc } from '../composition/documentData';

let activeName = ref('0');
const router = useRouter();
let props = defineProps({
    readyClick: {
        type: Function,
        default: undefined
    }
})
let rpa = router.currentRoute.value.params.address as string;
if (!rpa || rpa == '') rpa = 'default';
getArticle(rpa);

let clickDoc = (link: LinkInfo) => {
    console.log(link.address)
    router.replace({ params: { address: link.address } });
    getArticle(link.address);
}
function getArticle(address: string) {
    if (address == 'default')return;
    let a =address;
    const sa = Bbob.meta.extra.shortAddress;
    if (sa) {
        a = `${sa.startOfAddress}${address}${sa.endOfAddress}`;
    }
    Bbob.api.getArticleFromAddress(a, (article) => {
        if (article.contentParsed) {
            rawHtml.value = article.contentParsed;
        }
        if (article.toc) {
            toc.value = article.toc;
        }
        activeName.value = address;
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
                        :name="link.address"
                    >
                        <template #title style="word-wrap: break-all;">
                            <h4 v-if="activeName == link.address">{{ link.title }}</h4>
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
.el-collapse-item__header{
    line-height:normal;
    padding: 5px 0px;
}
</style>
