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
        path: '/documents',
        component: Documents,
    },
    {
        path: '/about',
        component: About,
    }
]

const router = createRouter({
    history: createWebHistory(Bbob.meta.publicPath),
    routes
})

export default router;