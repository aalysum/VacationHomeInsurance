import {
  Routes
} from '@angular/router';


import {
  guestGuard
} from './core/guards/guest.guard';


import {
  authGuard
} from './core/guards/auth.guard';


import {
  adminGuard
} from './core/guards/admin.guard';


import {
  customerGuard
} from './core/guards/customer.guard';


import {
  claimsManagerGuard
} from './core/guards/claims-manager.guard';


import {
  mfaFlowGuard
} from './core/guards/mfa-flow.guard';


import {
  claimsManagerApplicationFlowGuard
} from './core/guards/claims-manager-application-flow.guard';


import {
  claimsManagerReapplicationGuard
} from './core/guards/claims-manager-reapplication.guard';


export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',


    loadComponent: () =>
      import(
        './features/public/landing/landing.component'
      ).then(
        component =>
          component.LandingComponent
      ),


    title:
      'SecureSphere Insurance'
  },


  {
    path: 'products/vacation-home-insurance',
    loadComponent: () =>
      import(
        './features/public/vacation-home/vacation-home.component'
      ).then(
        component => component.VacationHomeComponent
      ),
    title: 'Vacation Home Insurance | SecureSphere'
  },


  {
    path:
      'auth/login',


    canActivate: [
      guestGuard
    ],


    loadComponent: () =>
      import(
        './features/auth/login/login.component'
      ).then(
        component =>
          component.LoginComponent
      ),


    title:
      'Sign In | SecureSphere'
  },


  {
    path:
      'auth/register/customer',


    canActivate: [
      guestGuard
    ],


    loadComponent: () =>
      import(
        './features/auth/customer-registeration/customer-registration.component'
      ).then(
        component =>
          component.CustomerRegistrationComponent
      ),


    title:
      'Customer Registration | SecureSphere'
  },


  {
    path:
      'auth/register/claims-manager',


    canActivate: [
      guestGuard
    ],


    loadComponent: () =>
      import(
        './features/auth/claims-manager-registration/claims-manager-registration.component'
      ).then(
        component =>
          component.ClaimsManagerRegistrationComponent
      ),


    title:
      'Claims Manager Registration | SecureSphere'
  },


  {
    path:
      'auth/forgot-password',


    canActivate: [
      guestGuard
    ],


    loadComponent: () =>
      import(
        './features/auth/forgot-password/forgot-password.component'
      ).then(
        component =>
          component.ForgotPasswordComponent
      ),


    title:
      'Forgot Password | SecureSphere'
  },


  {
    path:
      'auth/reset-password',


    canActivate: [
      guestGuard
    ],


    loadComponent: () =>
      import(
        './features/auth/reset-password/reset-password.component'
      ).then(
        component =>
          component.ResetPasswordComponent
      ),


    title:
      'Reset Password | SecureSphere'
  },


  {
    path:
      'auth/mfa/verify',


    canActivate: [
      mfaFlowGuard
    ],


    loadComponent: () =>
      import(
        './features/auth/mfa-verification/mfa-verification.component'
      ).then(
        component =>
          component.MfaVerificationComponent
      ),


    title:
      'Verify MFA | SecureSphere'
  },


  {
    path:
      'auth/claims-manager/application-status',


    canActivate: [
      claimsManagerApplicationFlowGuard
    ],


    loadComponent: () =>
      import(
        './features/auth/claims-manager-application-status/claims-manager-application-status.component'
      ).then(
        component =>
          component
            .ClaimsManagerApplicationStatusComponent
      ),


    title:
      'Application Status | SecureSphere'
  },


  {
    path:
      'auth/claims-manager/reapply',


    canActivate: [
      claimsManagerReapplicationGuard
    ],


    loadComponent: () =>
      import(
        './features/auth/claims-manager-reapplication/claims-manager-reapplication.component'
      ).then(
        component =>
          component
            .ClaimsManagerReapplicationComponent
      ),


    title:
      'Claims Manager Reapplication | SecureSphere'
  },


  {
    path:
      'account/security/change-password',


    canActivate: [
      authGuard
    ],


    loadComponent: () =>
      import(
        './features/auth/change-password/change-password.component'
      ).then(
        component =>
          component.ChangePasswordComponent
      ),


    title:
      'Change Password | SecureSphere'
  },


  {
    path:
      'account/security/mfa',


    canActivate: [
      authGuard
    ],


    loadComponent: () =>
      import(
        './features/auth/mfa-management/mfa-management.component'
      ).then(
        component =>
          component.MfaManagementComponent
      ),


    title:
      'Multi-Factor Authentication | SecureSphere'
  },


  {
    path:
      'access-denied',


    canActivate: [
      authGuard
    ],


    loadComponent: () =>
      import(
        './shared/pages/access-denied/access-denied.component'
      ).then(
        component =>
          component.AccessDeniedComponent
      ),


    title:
      'Access Denied | SecureSphere'
  },


{
  path:
    'customer/dashboard',


  canActivate: [
    customerGuard
  ],


  loadComponent:
    () =>
      import(
        './features/customer/dashboard/customer-dashboard.component'
      ).then(
        component =>
          component.CustomerDashboardComponent
      ),


  title:
    'Customer Dashboard | SecureSphere'
},
{
  path:
    'customer/properties',


  canActivate: [
    customerGuard
  ],


  loadComponent:
    () =>
      import(
        './features/customer/properties/customer-properties.component'
      ).then(
        component =>
          component.CustomerPropertiesComponent
      ),


  title:
    'Vacation Properties | SecureSphere'
},
{
  path:
    'customer/profile',


  canActivate: [
    customerGuard
  ],


  loadComponent:
    () =>
      import(
        './features/customer/profile/customer-profile.component'
      ).then(
        component =>
          component.CustomerProfileComponent
      ),


  title:
    'Customer Profile | SecureSphere'
},
{
  path:
    'customer/quotes',


  canActivate: [
    customerGuard
  ],


  loadComponent:
    () =>
      import(
        './features/customer/quotes/customer-quotes.component'
      ).then(
        component =>
          component.CustomerQuotesComponent
      ),


  title:
    'Insurance Quotes | SecureSphere'
},
{
  path:
    'customer/policies',


  canActivate: [
    customerGuard
  ],


  loadComponent:
    () =>
      import(
        './features/customer/policies/customer-policies.component'
      ).then(
        component =>
          component.CustomerPoliciesComponent
      ),


  title:
    'Insurance Policies | SecureSphere'
},
{
  path:
    'customer/claims',


  canActivate: [
    customerGuard
  ],


  loadComponent:
    () =>
      import(
        './features/customer/claims/customer-claims.component'
      ).then(
        component =>
          component.CustomerClaimsComponent
      ),


  title:
    'Insurance Claims | SecureSphere'
},


{
  path:
    'admin/dashboard',


  canActivate: [
    adminGuard
  ],


  loadComponent: () =>
    import(
      './features/admin/dashboard/admin-dashboard.component'
    ).then(
      component =>
        component.AdminDashboardComponent
    ),


  title:
    'Admin Dashboard | SecureSphere'
},


{
  path:
    'admin/claims-managers/pending',


  canActivate: [
    adminGuard
  ],


  loadComponent: () =>
    import(
      './features/admin/claims-manager-applications/claims-manager-applications.component'
    ).then(
      component =>
        component
          .ClaimsManagerApplicationsComponent
    ),


  title:
    'Claims Manager Applications | SecureSphere'
},
{
  path:
    'admin/customers',


  canActivate: [
    adminGuard
  ],


  loadComponent: () =>
    import(
      './features/admin/customers/admin-customers.component'
    ).then(
      component =>
        component.AdminCustomersComponent
    ),


  title:
    'Customer Management | SecureSphere'
},


{
  path:
    'admin/quotes',


  canActivate: [
    adminGuard
  ],


  loadComponent: () =>
    import(
      './features/admin/quotes/admin-quotes.component'
    ).then(
      component =>
        component.AdminQuotesComponent
    ),


  title:
    'Quote Approvals | SecureSphere'
},
{
  path:
    'admin/products',


  canActivate: [
    adminGuard
  ],


  loadComponent: () =>
    import(
      './features/admin/products/admin-products.component'
    ).then(
      component =>
        component.AdminProductsComponent
    ),


  title:
    'Products & Coverages | SecureSphere'
},{
  path:
    'admin/pricing',


  canActivate: [
    adminGuard
  ],


  loadComponent: () =>
    import(
      './features/admin/pricing/admin-pricing.component'
    ).then(
      component =>
        component.AdminPricingComponent
    ),


  title:
    'Pricing Configuration | SecureSphere'
},
{
  path:
    'admin/operations',


  canActivate: [
    adminGuard
  ],


  loadComponent: () =>
    import(
      './features/admin/operations/admin-operations.component'
    ).then(
      component =>
        component.AdminOperationsComponent
    ),


  title:
    'Policy & Claim Operations | SecureSphere'
},
{
  path:
    'claims-manager',


  loadChildren:
    () =>
      import(
        './features/claims-manager/claims-manager.routes'
      ).then(
        module =>
          module
            .CLAIMS_MANAGER_ROUTES
      )
},
  {
    path: '**',
    redirectTo: ''
  }
];

