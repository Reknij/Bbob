<script setup lang="ts">
import Bbob from '../../../Bbob/JSApi/Bbob';
import { language } from '../Languages/LanguageHelper';
import Articles from './Articles.vue'
document.title = Bbob.meta.blogName;
let hasDescription = false;
let dt = Bbob.meta.extra.defaultTheme;
let welcome = language.welcome;
if (dt) {
  if (dt.description) hasDescription = true;
  if (dt.welcome) welcome = dt.welcome;
}

</script>

<template>
  <div>
    <el-card class="app-container">
      <h1 style="text-align: center;">{{welcome}}</h1>
      <p style="text-align: center;" v-if="hasDescription">{{dt.description}}</p>
    </el-card>
    <Articles></Articles>
  </div>
</template>

<style>
</style>
