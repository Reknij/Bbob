<script setup lang="ts">
import Bbob from '../../../Bbob/JSApi/Bbob'

</script>

<template>
    <div style="text-align: center;">
        <span class="copyright" v-html="Bbob.meta.copyright"></span>
        <br />
        <a
            class="extraLink"
            :href="link.address"
            v-if="Bbob.meta.extra.extraLinks"
            v-for="(link, i) in Bbob.meta.extra.extraLinks"
            :key="i"
        >{{ link.name }}</a>
    </div>
</template>

<style>
.extraLink{
    margin: 5px 0px 2px 2px;
    text-decoration: underline;
    color: var(--theme-font-color);
}
.extraLink:hover{
    color: var(--theme-selected-color);
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
