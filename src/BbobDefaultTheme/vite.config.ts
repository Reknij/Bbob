import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import importToCDN from 'vite-plugin-cdn-import'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    importToCDN({
      modules: [
        {
          name: 'vue',
          var: 'Vue',
          path: 'https://unpkg.com/vue@3.2.31/dist/vue.runtime.global.prod.js'
        },
        {
          name: 'vue-router',
          var: 'VueRouter',
          path: 'https://unpkg.com/vue-router@4.0.14/dist/vue-router.global.prod.js'
        },
        {
          name: 'element-plus',
          var: 'ElementPlus',
          path: 'https://unpkg.com/element-plus@2.1.0',
          css: 'https://unpkg.com/element-plus@2.1.0/dist/index.css'
        },
      ]
    })
  ],
  server: {
    host: true
  }
})
