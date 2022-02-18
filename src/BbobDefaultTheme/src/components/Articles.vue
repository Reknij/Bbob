<script setup lang="ts">
import { Ref, ref, watch } from 'vue';
import { onBeforeRouteLeave, onBeforeRouteUpdate } from 'vue-router';
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
let hasArticles = ref(props.mode == 'scroll' && Bbob.blog.nextFileLinks.length > 0);
const load = () => {
    if (props.mode == 'scroll'){
        Bbob.api.nextLinkInfos((linkArray) => {
        if (linkArray) {
            mainLinks.value.push(...linkArray);
            Bbob.blog.links = mainLinks.value;
        }
        else {
            hasArticles.value = false;
        }
    })
    }
}
let indexEvent = -1;
onBeforeRouteUpdate(()=>{
    indexEvent = scrollToDownEventRegist(load)
})
onBeforeRouteLeave(()=>{
    if (indexEvent != -1){
        scrollToDownEventUnRegist(indexEvent);
    }
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
                        class="articleTitle"
                        :to="`/article?address=${link.address}`"
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
                >{{ tag }}</el-tag>
            </el-card>
        </el-timeline-item>
    </el-timeline>
    <h4 v-if="hasArticles" style="text-align: center;">Articles is end~</h4>
</template>

<style>
.tagItem {
    margin-left: 5px;
}
.articleTitle {
    text-decoration: none;
    color: #303133;
}
.articleTitle:hover {
    text-decoration: none;
    color: #409eff;
}
</style>
