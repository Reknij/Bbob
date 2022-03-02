import { createRouter, createWebHistory } from "vue-router";
import Home from '../components/Home.vue';
import Documents from '../components/Documents.vue';
import About from '../components/About.vue';
import Bbob from '../../../Bbob/JSApi/Bbob';

const routes = [
    {
        path: '/',
        component: Home
    },
    {
        path: '/documents/:address',
        component: Documents
    },
    {
        path: '/about',
        component: About,
    }
]

const router = createRouter({
    history: createWebHistory(Bbob.meta.baseUrl.replace(window.location.origin, "")),
    routes
})

export default router;