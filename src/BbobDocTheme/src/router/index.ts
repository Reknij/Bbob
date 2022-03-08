import { createRouter, createWebHistory } from "vue-router";
import Home from '../components/Home.vue';
import Documents from '../components/Documents.vue';
import About from '../components/About.vue';
import Bbob from '../../../Bbob/JSApi/Bbob';

const routes = [
    {
        name: "Home",
        path: '/',
        defaultPath: "/",
        component: Home
    },
    {
        name: "Documents",
        path: '/documents/:address',
        defaultPath: "/documents/default",
        component: Documents
    },
    {
        name: "About",
        path: '/about',
        defaultPath: "/about",
        component: About,
    }
]

const router = createRouter({
    history: createWebHistory(Bbob.meta.baseUrl.replace(window.location.origin, "")),
    routes
})

export {
    routes,
    router
}