<script setup lang="ts">
import { onBeforeMount, onMounted, ref, watch } from 'vue';
import { onBeforeRouteLeave, useRoute, useRouter } from 'vue-router';
import Bbob, { Article } from '../../../Bbob/JSApi/Bbob'
import { normal } from '../composition/changeSize';
import { language } from '../Languages/LanguageHelper';
import { List, Calendar } from '@element-plus/icons-vue'

let isLoading = ref(true);
let articleLoadingMs = 300;
const route = useRoute();
const router = useRouter();
let articleId = route.params.articleId ? route.params.articleId as string : '';
let article = ref<Article>({
    title: "loading article...",
    date: "loading article...",
    contentParsed: ""
});
let htmlContent = ref<HTMLElement | null>(null);
let artDate = ref('')
onBeforeMount(async () => {
    let start = new Date().getTime();
    article.value = await Bbob.api.getArticleByIdAsync(articleId)
    let date = `<span style="text-decoration: underline dashed; color: var(--theme-date-color);">${article.value.date}</span>`;
    if (language.postedOn.includes('${date}')) {
        artDate.value = ' ' + language.postedOn.replace('${date}', date);
    }
    else {
        artDate.value = ' ' + `${language.postedOn} ${date}`;
    }
    watch(htmlContent, () => {
        if (htmlContent.value) {
            Bbob.api.executeScriptElements(htmlContent.value as any);
            if (location.hash) location.href = location.hash;
            else window.scrollTo(0, 0);
        }
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
})

onBeforeRouteLeave(() => {
    document.title = Bbob.meta.blogName
    return true;
});

let tocDrawer = ref(false)
function tocClick(event: Event) {
    tocDrawer.value = true;
    document.getElementById('tocBtn')?.blur();
}
</script>

<template>
    <div id="content">
        <el-card>
            <el-skeleton :animated="true" :loading="isLoading" :rows="20">
                <template #default>
                    <el-page-header id="backBtn" @back="router.push('/')" :title="language.back"></el-page-header>
                    <span class="articleTitle">{{ article.title }}</span>

                    <div class="articleDate">
                        <el-icon class="calendarIcon">
                            <calendar />
                        </el-icon>
                        <span v-html="artDate" />
                    </div>

                    <div style="text-align: center; margin-top: 5px;">
                        <el-tag v-if="article.categories" class="tagItem"
                            v-for="(category, index) in article.categories" :key="index" type="warning"
                            @click="router.push(`/filter/categories?checked=${category}`)">{{ category }}</el-tag>
                        <el-tag v-if="article.tags" class="tagItem" v-for="(tag, index) in article.tags" :key="index"
                            type="success" @click="router.push(`/filter/tags?checked=${tag}`)">{{ tag }}</el-tag>
                    </div>
                    <el-divider></el-divider>
                    <span ref="htmlContent" v-html="article.contentParsed"></span>
                </template>
            </el-skeleton>
        </el-card>

        <el-card id="relativeCard">
            <a v-if="article.nextArticle"
                :href="Bbob.meta.extra.shortAddress ? `/article/${article.nextArticle.id}/` : `/article?address=${article.nextArticle.id}`">
                <p>
                    {{
                            language.nextArticle
                    }}:
                    {{
                            article.nextArticle.title
                    }}
                </p>
            </a>
            <el-divider v-if="article.nextArticle"></el-divider>
            <a v-if="article.previousArticle"
                :href="Bbob.meta.extra.shortAddress ? `/article/${article.previousArticle.id}/` : `/article?address=${article.previousArticle.id}`">
                <p>
                    {{
                            language.previousArticle
                    }}:
                    {{
                            article.previousArticle.title
                    }}
                </p>
            </a>
        </el-card>

        <el-drawer :size="normal ? '30' : '75%'" v-if="article.toc" v-model="tocDrawer" direction="ltr"
            :lock-scroll="false">
            <template #title>
                <h4 style="color: var(--theme-font-color);">{{ article.title }}</h4>
            </template>
            <template #default>
                <div>
                    <span @click="tocDrawer = false" v-html="article.toc"></span>
                </div>
            </template>
        </el-drawer>

        <el-button id="tocBtn" class="fix" @click="tocClick" type="primary">
            <el-icon>
                <list />
            </el-icon>
        </el-button>
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
    margin: 10px auto;
    max-width: calc(100% - var(--el-card-padding));
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
}

.calendarIcon {
    position: relative;
    bottom: -1px;
}

.fix {
    border-radius: 50%;
    width: 40px;
    height: 40px;
    position: fixed;
    right: max(calc(calc(100% - 1024px) / 2 + 15px), 15px);
    bottom: 80px;
    box-shadow: 0 0 6px rgb(0 0 0 / 30%);
    background-color: var(--theme-background-color);
    color: var(--theme-font-color);
    border-width: 0px;
}

.fix:hover {
    background-color: var(--theme-selected-color);
}

.fix:focus {
    background-color: var(--theme-background-color);
    color: var(--theme-font-color);
}

#relativeCard {
    margin-top: 10px;
}

#relativeCard a {
    color: var(--theme-font-color);
    text-decoration: none;
}

#relativeCard a:hover {
    color: var(--theme-selected-color);
    text-decoration: underline;
}
</style>
