import { createRouter, createWebHistory } from "vue-router";
import Home from '../components/Home.vue'
import Articles from '../components/Articles.vue'
import ArticleResult from '../components/ArticleResult.vue'
import Filter from '../components/Filter.vue'
import About from '../components/About.vue'
import Bbob from '../../../Bbob/JSApi/Bbob'
const routes = [
    {
        path: '/',
        component: Home
    },
    {
        path: '/articles/:mode',
        component: Articles,
        props: true 
    },
    {
        path: '/article/:articleId*',
        component: ArticleResult
    },
    {
        path: '/filter/:mode',
        component: Filter,
        props: true

    },
    {
        path: '/about',
        component: About
    }
]

const router = createRouter({
    history: createWebHistory(Bbob.meta.baseUrl.replace(window.location.origin, "")),
    routes
})

export default router;