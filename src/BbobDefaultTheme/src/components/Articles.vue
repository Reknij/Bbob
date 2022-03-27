<script setup lang="ts">
import { onBeforeMount, onBeforeUnmount, onMounted, Ref, ref, watch } from 'vue';
import { onBeforeRouteLeave, onBeforeRouteUpdate, useRouter } from 'vue-router';
import Bbob, { LinkInfo } from '../../../Bbob/JSApi/Bbob';
import { scrollToDownEventRegist, scrollToDownEventUnRegist } from '../composition/functionsRegister';

const props = defineProps({
    mode: {
        type: String,
        default: 'default'
    },
    source: {
        type: Array,
        default: []
    },
    defineSource: {
        type: Boolean,
        default: false
    }
})
let router = useRouter();
let mainLinks: Ref<any[]> = ref([]);
if (props.defineSource) {
    watch(() => props.source, () => mainLinks.value = props.source)
}
else {
    mainLinks.value = Bbob.blog.links;
}

if (mainLinks.value.length > 0) {
    mainLinks.value[0].size = "large"
    mainLinks.value[0].type = "primary"
}
if (props.mode == 'default' && mainLinks.value.length > 6) {
    mainLinks.value = mainLinks.value.slice(0, 5);
}
let articlesEnd = ref(props.mode == 'scroll' && Bbob.blog.nextFileLinks.length == 0);
const load = () => {
    if (props.mode == 'scroll') {
        Bbob.api.nextLinkInfos((linkArray) => {
            if (linkArray) {
                mainLinks.value.push(...linkArray);
                Bbob.blog.links = mainLinks.value;
            }
            else {
                articlesEnd.value = true;
            }
        })
    }
}
let indexEvent = -1;
onBeforeMount(() => {
    indexEvent = scrollToDownEventRegist(load)
})
onBeforeUnmount(() => {
    if (indexEvent != -1) {
        scrollToDownEventUnRegist(indexEvent);
    }
})
onMounted(() => {
    Bbob.meta.extra.prerenderNow = true;
})
</script>

<template>
    <el-timeline style="margin-top: 10px;">
        <el-timeline-item
            v-for="(link, index) in mainLinks"
            :key="index"
            :size="link.size"
            :type="link.type"
            :timestamp="link.date"
        >
            <el-card>
                <h3>
                    <router-link
                        class="articlesTitle"
                        :to="Bbob.meta.extra.shortAddress ? `/article/${link.address}` : `/article?address=${link.address}`"
                    >{{ link.title }}</router-link>
                </h3>
                <el-divider v-if="link.categories" content-position="left">
                    <el-tag effect="dark" type="warning">Categories</el-tag>
                </el-divider>
                <el-tag
                    v-if="link.categories"
                    class="tagItem"
                    v-for="(category, index) in link.categories"
                    :key="index"
                    type="warning"
                    @click="router.push(`/filter/categories?checked=${category}`)"
                >{{ category }}</el-tag>
                <el-divider v-if="link.tags" content-position="left">
                    <el-tag effect="dark" type="success">Tags</el-tag>
                </el-divider>
                <el-tag
                    v-if="link.tags"
                    class="tagItem"
                    v-for="(tag, index) in link.tags"
                    :key="index"
                    type="success"
                    @click="router.push(`/filter/tags?checked=${tag}`)"
                >{{ tag }}</el-tag>
            </el-card>
        </el-timeline-item>
    </el-timeline>
    <h4 v-if="articlesEnd" style="text-align: center;">Articles is end~</h4>
</template>

<style>
.tagItem {
    margin-left: 5px;
    cursor: pointer;
}
.articlesTitle {
    text-decoration: none;
    color: var(--theme-font-color);
}
.articlesTitle:hover {
    text-decoration: none;
    color: var(--theme-selected-color);
}
.el-timeline {
        padding-inline-start: 0px;
}
.el-timeline-item__timestamp{
    color: var(--theme-date-color);
    margin-left: 15px;
}
</style>
