<script setup lang="ts">
import { onBeforeMount, onMounted, ref, watch } from 'vue';
import { onBeforeRouteLeave, useRoute, useRouter } from 'vue-router';
import Bbob, { Article } from '../../../Bbob/JSApi/Bbob'
import { normal } from '../composition/changeSize';
import { language } from '../Languages/LanguageHelper';

let isLoading = ref(true);
let articleLoadingMs = 300;
const route = useRoute();
const router = useRouter();
let address = route.params.address ? route.params.address as string : '';
if (!Bbob.meta.extra.shortAddress) {
    address = route.query.address as string;
}
let article = ref<Article>({
    title: "loading article...",
    date: "loading article...",
    contentParsed: ""
});
let htmlContent = ref(null);
let artDate = ref('')
onBeforeMount(async () => {
    let start = new Date().getTime();
    article.value = await Bbob.api.getArticleFromAddressAsync(address)
    let date = `<span style="text-decoration: underline dashed;">${article.value.date}</span>`;
    if (language.postedOn.includes('${date}')) {
        artDate.value = language.postedOn.replace('${date}', date);
    }
    else {
        artDate.value = `${language.postedOn} ${date}`;
    }
    watch(htmlContent, () => {
        if (htmlContent.value) Bbob.api.executeScriptElements(htmlContent.value as any);
    })
    let diff = (new Date().getTime() - start); //milliseconds interval
    document.title = `${article.value.title} - ${Bbob.meta.blogName}`;
    let wait = articleLoadingMs - diff;
    if (wait > 0) {
        setTimeout(() => {
            isLoading.value = false;
        }, wait);
    }
    else {
        isLoading.value = false;
    }

    Bbob.meta.extra.prerenderNow = true;
    window.scrollTo(0, 0);
})

onBeforeRouteLeave(() => {
    document.title = Bbob.meta.blogName
    return true;
});

let tocDrawer = ref(false)
</script>

<template>
    <div id="content">
        <el-card>
            <el-skeleton :animated="true" :loading="isLoading" :rows="20">
                <template #default>
                    <el-page-header id="backBtn" @back="router.push('/')" :title="language.back"></el-page-header>
                    <span class="articleTitle">{{ article.title }}</span>

                    <span class="articleDate" v-html="artDate" />
                    <div style="text-align: center; margin-top: 5px;">
                        <el-tag
                            v-if="article.categories"
                            class="tagItem"
                            v-for="(category, index) in article.categories"
                            :key="index"
                            type="warning"
                            @click="router.push(`/filter/categories?checked=${category}`)"
                        >{{ category }}</el-tag>
                        <el-tag
                            v-if="article.tags"
                            class="tagItem"
                            v-for="(tag, index) in article.tags"
                            :key="index"
                            type="success"
                            @click="router.push(`/filter/tags?checked=${tag}`)"
                        >{{ tag }}</el-tag>
                    </div>
                    <el-divider></el-divider>
                    <span ref="htmlContent" v-html="article.contentParsed"></span>
                </template>
            </el-skeleton>
        </el-card>

        <el-drawer
            :size="normal ? '50%' : '100%'"
            v-if="article.toc"
            v-model="tocDrawer"
            direction="ltr"
        >
            <template #title>
                <h4 style="color: var(--theme-font-color);">{{ article.title }}</h4>
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
</template>

<style>
#content {
    margin-left: auto;
    margin-right: auto;
}
.toc-item a {
    color: var(--theme-font-color);
    text-decoration: none;
}
.toc-item a:hover {
    color: var(--theme-selected-color);
    text-decoration: underline;
}
#backBtn {
    color: var(--theme-font-color);
}
#backBtn:hover {
    color: var(--theme-selected-color);
}
#content img {
    display: block;
    margin: 0px auto;
    max-width: calc(768px - var(--el-card-padding) * 2);
}
.tagItem {
    margin-left: 5px;
    cursor: pointer;
}
.toc-number {
    display: none;
}
.articleTitle {
    text-align: center;
    display: block;
    font-size: xx-large;
    font-weight: bold;
    color: var(--theme-font-color);
}
.articleDate {
    text-align: center;
    display: block;
    font-size: small;
    color: var(--theme-date-color);
}
</style>
