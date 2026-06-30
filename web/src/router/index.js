import { createRouter, createWebHistory } from 'vue-router'
import Login from '@/views/Login.vue'
import MainLayout from '@/layouts/MainLayout.vue'
import Home from '../views/Home.vue'
import Project from '@/views/Project.vue'
import ProjectType from '@/views/ProjectType.vue'
import Settings from '@/views/Settings.vue'
import About from '../views/About.vue'
import PublicShowPage from '@/views/PublicShowPage.vue'
import ResetPassword from '@/views/ResetPassword.vue'
import Activate from '@/views/Activate.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/admin',
      component: MainLayout,
      children: [
        { path: 'home', name: 'home', component: Home },
        { path: 'projecttype', name: 'projecttype', component: ProjectType },
        { path: 'project', name: 'project', component: Project },
        { path: 'settings', name: 'settings', component: Settings },
        { path: 'about', name: 'about', component: About },
      ]
    },
    {
      path: '/admin/login', name: 'login', component: Login
    },
    {
      path: '/admin/reset-password', name: 'resetpassword', component: ResetPassword
    },
    {
      path: '/admin/activate', name: 'activate', component: Activate
    },
    {
      path: '/:idhash?', name: 'publicshowpage', component: PublicShowPage
    }
  ],
})

export default router
