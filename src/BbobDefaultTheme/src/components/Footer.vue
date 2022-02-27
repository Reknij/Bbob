<script setup lang="ts">
import Bbob from '../../../Bbob/JSApi/Bbob'

</script>

<template>
    <p class="copyright">
        © 2022 {{Bbob.meta.author}} Powered by <a class="beautifulLink" href="https://github.com/Reknij/Bbob">Bbob</a> & default
    </p>
    <p class="copyright">
        Last build: {{Bbob.meta.lastBuild}}
    </p>
</template>

<style>
.copyright{
    text-align: center;
}
.beautifulLink{
    text-decoration: none;
    color: #8300FF;
}
.beautifulLink:hover{
    text-decoration: underline;
}
</style>
