export default defineNuxtRouteMiddleware((to) => {
  if (to.path === '/') {
    return navigateTo('/front/home', { redirectCode: 301 })
  }
})
