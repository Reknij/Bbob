<script setup lang="ts">
import { ref, watch } from 'vue';
import Bbob, { ArchiveMonth, LinkInfo } from '../../../Bbob/JSApi/Bbob'
import router from '../router';
import Articles from './Articles.vue';

let props = defineProps({
    mode: {
        type: String,
        default: 'categories'
    }
})
let title = ref('Categories')
let filterTags = ref(Bbob.blog.categories)
let clickFunc: Function = Bbob.api.getLinkInfosWithCategory;
let mode = ref('warning')
let source = ref<any[]>([])
const setSource = (linkArray: any[]) => {
    source.value = linkArray;
}

function changeMode(changeTo: string) {
    if (changeTo == 'categories') {
        title.value = 'Categories'
        clickFunc = Bbob.api.getLinkInfosWithCategory;
        filterTags.value = Bbob.blog.categories;
        mode.value = 'warning';
    }
    else if (changeTo == 'tags') {
        title.value = 'Tags'
        clickFunc = Bbob.api.getLinkInfosWithTag;
        filterTags.value = Bbob.blog.tags;
        mode.value = 'success';
    }
    else if (changeTo == 'archives') {
        title.value = 'Archives'
        clickFunc = (year: number, callback: Function)=> {
            Bbob.blog.archives.forEach(archiveYear => {
                if (archiveYear.year == year){
                    Bbob.api.getLinkInfosWithArchiveAddress(archiveYear.address, (archiveMonthArray: ArchiveMonth[])=>{
                        let linkArray: LinkInfo[] = [];
                        archiveMonthArray.forEach(archiveMonth => {
                            linkArray.push(...archiveMonth.link);
                        });
                        callback(linkArray);
                    })
                }
            });
        }
        let archives: string[] = [];
        Bbob.blog.archives.forEach(archiveYear => {
            archives.push(archiveYear.year.toString());
        });
        filterTags.value = archives
        mode.value = "";
    }
}
function directChecked() {
    if (router.currentRoute.value.query.checked) {
        clickFunc(router.currentRoute.value.query.checked as string, setSource)
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
    <el-divider content-position="left">
        <el-tag effect="dark" :type="mode">{{ title }}</el-tag>
    </el-divider>
    <el-check-tag
        v-if="filterTags.length"
        class="tagItem"
        v-for="(value, index) in filterTags"
        :key="index"
        :checked="router.currentRoute.value.query.checked == value"
        @change="(status: boolean) => router.replace(status?{ query: { checked: value } }:{query:{}})"
    >{{ value }}</el-check-tag>
    <Articles :source="source" define-source mode="all"></Articles>
</template>

<style>
.tagItem {
    margin-bottom: 5px;
}
</style>
