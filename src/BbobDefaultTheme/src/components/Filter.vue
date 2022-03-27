<script setup lang="ts">
import { ref, watch } from 'vue';
import Bbob, { FilterSource, LinkInfo } from '../../../Bbob/JSApi/Bbob'
import router from '../router';
import Articles from './Articles.vue';

let props = defineProps({
    mode: {
        type: String,
        default: 'categories'
    }
})
let title = ref('Categories')
let filterTags = ref<FilterSource[]>([])
let mode = ref('warning')
let source = ref<any[]>([])
const setSource = (linkArray: any[]) => {
    source.value = linkArray;
}

function changeMode(changeTo: string) {
    if (changeTo == 'categories') {
        title.value = 'Categories'
        filterTags.value = Bbob.blog.categories;
        mode.value = 'warning';
    }
    else if (changeTo == 'tags') {
        title.value = 'Tags'
        filterTags.value = Bbob.blog.tags;
        mode.value = 'success';
    }
    else if (changeTo == 'archives') {
        title.value = 'Archives'
        filterTags.value = Bbob.blog.archives;
        mode.value = "";
    }
}
function checkIt(filter: FilterSource, status: boolean) {
    if (status) {
        Bbob.api.getLinkInfosWithAddress(filter.address, (linkArray) => {
            setSource(linkArray);
            router.replace({ query: { checked: filter.text } })
        })
    }
    else {
        setSource([]);
        router.replace({ query: {} })
    }
}
function directChecked() {
    if (router.currentRoute.value.query.checked) {
        filterTags.value.forEach(filter => {
            if (filter.text == router.currentRoute.value.query.checked) {
                Bbob.api.getLinkInfosWithAddress(filter.address, setSource)
                return;
            }
        });
    }
}
changeMode(props.mode);
directChecked();
watch(() => router.currentRoute.value, (value, oldValue) => {
    source.value = [];
    if (value.params.mode != oldValue.params.mode) {
        changeMode(value.params.mode as string);
        directChecked();
        return;
    }
    value.query.checked != oldValue.query.checked && directChecked();
})
</script>

<template>
    <h1 style="text-align: center; color: var(--theme-font-color);">{{title}}</h1>
    <el-divider ></el-divider>
    <el-check-tag
        v-if="filterTags.length"
        class="tagItem"
        v-for="(filter, index) in filterTags"
        :key="index"
        :checked="router.currentRoute.value.query.checked == filter.text"
        @change="(status: boolean) => checkIt(filter, status)"
    >{{ filter.text }}</el-check-tag>
    <Articles :source="source" define-source mode="all"></Articles>
</template>

<style>
.tagItem {
    margin-bottom: 5px;
}
</style>
