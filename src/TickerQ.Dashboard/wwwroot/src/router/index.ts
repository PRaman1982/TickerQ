import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import { getBasePath, requiresAuthentication } from '@/utilities/pathResolver'

const router = createRouter({
  history: createWebHistory(getBasePath()),
  routes: [
    {
      path: '/login',
      name: 'Login',
      component: () => import('../views/Login.vue'),
      meta: { requiresAuth: false, hideForAuthenticated: true },
    },
    {
      path: '/',
      name: 'Dashboard',
      component: () => import('../views/Dashboard.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/cron-tickers',
      name: 'CronTicker',
      component: () => import('../views/CronTicker.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/time-tickers',
      name: 'TimeTicker',
      component: () => import('../views/TimeTicker.vue'),
      meta: { requiresAuth: true },
    },
  ],
})

// Navigation guard for authentication
router.beforeEach(async (to, from, next) => {
  console.log('🔒 [Router Guard] Navigation to:', to.fullPath, '| from:', from.fullPath)

  // Always allow access to login page
  if (to.name === 'Login') {
    console.log('🔒 [Router Guard] Login page - allowing access')
    next()
    return
  }

  // Check if authentication is required globally
  const authRequired = requiresAuthentication()
  console.log('🔒 [Router Guard] Auth required globally?', authRequired)

  // If no authentication is required globally, allow all routes
  if (!authRequired) {
    console.log('🔒 [Router Guard] No auth required - allowing access')
    next()
    return
  }

  // Authentication is required - get auth store
  const authStore = useAuthStore()

  // Initialize auth if not already done
  if (!authStore.isInitialized) {
    console.log('🔒 [Router Guard] Auth not initialized - initializing...')
    try {
      await authStore.initializeAuth()
      console.log('🔒 [Router Guard] Auth initialized. isLoggedIn:', authStore.isLoggedIn)
    } catch (error) {
      console.error('🔒 [Router Guard] Auth initialization failed:', error)
      next({ name: 'Login' })
      return
    }
  }

  // Check authentication status
  const isAuthenticated = authStore.isLoggedIn
  console.log('🔒 [Router Guard] isAuthenticated:', isAuthenticated)

  if (isAuthenticated) {
    console.log('🔒 [Router Guard] User is authenticated - allowing access')
    next()
  } else {
    // If backend provided a custom unauthorized redirect URL, use it
    const config = authStore.getConfig()
    console.log('🔒 [Router Guard] Auth config:', JSON.stringify(config))
    const customRedirect = config?.unauthorizedRedirectUrl
    console.log('🔒 [Router Guard] Custom redirect URL:', customRedirect)

    if (customRedirect) {
      // Append the current original location as a redirect parameter so the external login can route back
      const currentPath = window.location.pathname
      const separator = customRedirect.includes('?') ? '&' : '?'
      const redirectTo = `${customRedirect}${separator}redirect=${encodeURIComponent(currentPath)}`
      console.log('🔒 [Router Guard] Redirecting to external login:', redirectTo)
      window.location.href = redirectTo
      return
    }

    console.log('🔒 [Router Guard] Falling back to internal Login view')
    // Fallback to internal login view
    next({ name: 'Login', query: { redirect: to.fullPath } })
  }
})

export default router
