<script setup lang="ts">
import Bbob from '../../../Bbob/JSApi/Bbob'

</script>

<template>
    <div style="text-align: center;">
        <span class="copyright" v-html="Bbob.meta.copyright"></span>
         <br>
        <el-tag color="white" class="extraLink" v-if="Bbob.meta.extra.extraLinks" v-for="(link, i) in Bbob.meta.extra.extraLinks" :key="i">
            <a :href="link.address">{{link.name}}</a>
        </el-tag>
    </div>
</template>

<style>
.extraLink {
    margin: 5px 0px 2px 2px;
}
.extraLink a {
    text-decoration: none;
    color: black;
}
.copyright {
    margin: auto 0px;
    text-align: center;
}
.bbob-project-link {
    text-decoration: none;
    color: #8300ff;
}
.bbob-project-link:hover {
    text-decoration: underline;
}
</style>
