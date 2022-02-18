<script setup lang="ts">
import Bbob from '../../../Bbob/JSApi/Bbob'

</script>

<template>
    <p class="copyright">
        © 2022 {{Bbob.meta.author}} Powered by <a class="beautifulLink" href="https://github.com/Reknij/Bbob">Bbob</a> & default v1.0
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
