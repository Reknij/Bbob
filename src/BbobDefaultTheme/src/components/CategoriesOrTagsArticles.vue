<script setup lang="ts">
import { ref, watch } from 'vue';
import Bbob from '../../../Bbob/JSApi/Bbob'
import Articles from './Articles.vue';

let props = defineProps({
    mode: {
        type: String,
        default: 'categories'
    }
})
let title = ref('Categories')
let cAndT = ref(Bbob.blog.categories)
let clickFunc = Bbob.api.getLinkInfosWithCategory;
let mode = ref('warning')
let source = ref<any[]>([])
let checked = ref("");
const setSource = (linkArray: any[]) => {
    source.value = linkArray;
}
const clickItem = (status: boolean, value: string) => {
    if (status) {
        checked.value = value;
        clickFunc(value, setSource)
    }
    else {
        checked.value = "";
        source.value = [];
    }
}
const changeMode = () => {
    source.value = [];
    checked.value = "";
    if (props.mode == 'categories') {
        title.value = 'Categories'
        clickFunc = Bbob.api.getLinkInfosWithCategory;
        cAndT.value = Bbob.blog.categories;
        mode.value = 'warning';
    }
    else if (props.mode == 'tags') {
        title.value = 'Tags'
        clickFunc = Bbob.api.getLinkInfosWithTag;
        cAndT.value = Bbob.blog.tags;
        mode.value = 'success';
    }
}
changeMode();
watch(() => props.mode, changeMode)

</script>

<template>
    <el-divider content-position="left">
        <el-tag effect="dark" :type="mode">{{ title }}</el-tag>
    </el-divider>
    <el-check-tag
        v-if="cAndT.length"
        class="tagItem"
        v-for="(value, index) in cAndT"
        :key="index"
        :checked="checked == value"
        @change="(status: boolean) => clickItem(status, value)"
    >{{ value }}</el-check-tag>
    <Articles style="margin-top: 10px;" :source="source" define-source mode="all"></Articles>
</template>

<style>
.tagItem {
    margin-bottom: 5px;
}
</style>
