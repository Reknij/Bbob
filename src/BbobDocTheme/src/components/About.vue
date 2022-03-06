<script setup lang="ts">
import Bbob from '../../../Bbob/JSApi/Bbob'

</script>

<template>
    <div>
        <el-card style="margin-top: 10px;">
            <template #header>
                <h2>About {{Bbob.meta.blogName}}</h2>
            </template>
            <p>{{ Bbob.meta.about }}</p>
        </el-card>
        <el-card style="margin-top: 10px;">
            <template #header>
                <h2>Last build</h2>
            </template>
            <p>{{ Bbob.meta.lastBuild }}</p>
        </el-card>
    </div>
</template>

<style>
</style>
