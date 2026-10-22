
export default {
  bootstrap: () => import('./main.server.mjs').then(m => m.default),
  inlineCriticalCss: true,
  baseHref: '/',
  locale: undefined,
  routes: [
  {
    "renderMode": 2,
    "preload": [
      "chunk-KZ7Y2OEN.js"
    ],
    "route": "/"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-ZUGYAKMS.js",
      "chunk-BCSDSUXB.js"
    ],
    "route": "/products/vacation-home-insurance"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-L6SH7EAG.js",
      "chunk-CMMTSM52.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/auth/login"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-7CFLCPE5.js",
      "chunk-RZ6HN63M.js",
      "chunk-DM33AHU5.js",
      "chunk-CMMTSM52.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/auth/register/customer"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-ERPQUCC6.js",
      "chunk-KA2DCRAA.js",
      "chunk-RZ6HN63M.js",
      "chunk-DM33AHU5.js",
      "chunk-CMMTSM52.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/auth/register/claims-manager"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-W7XRP2FZ.js",
      "chunk-DM33AHU5.js",
      "chunk-CMMTSM52.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/auth/forgot-password"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-QCA5S25J.js",
      "chunk-RZ6HN63M.js",
      "chunk-DM33AHU5.js",
      "chunk-CMMTSM52.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/auth/reset-password"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-ZC6URE54.js",
      "chunk-CMMTSM52.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/auth/mfa/verify"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-XDNFU2D6.js"
    ],
    "route": "/auth/claims-manager/application-status"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-ZM3E3UCH.js",
      "chunk-KA2DCRAA.js",
      "chunk-CMMTSM52.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/auth/claims-manager/reapply"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-YMAWKXZ5.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/account/security/change-password"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-AE5FGVDP.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/account/security/mfa"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-QQ6FJAIX.js"
    ],
    "route": "/access-denied"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-BT2BVAOU.js",
      "chunk-X5PGRLQR.js",
      "chunk-HLSKIOLE.js",
      "chunk-OCCJAXZN.js"
    ],
    "route": "/customer/dashboard"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-VD5MALDH.js",
      "chunk-HLSKIOLE.js",
      "chunk-OCCJAXZN.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/customer/properties"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-7ASI3W64.js",
      "chunk-X5PGRLQR.js",
      "chunk-HLSKIOLE.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/customer/profile"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-373R6HCD.js",
      "chunk-AC4GTW44.js",
      "chunk-6NZ7JFZA.js",
      "chunk-HLSKIOLE.js",
      "chunk-BCSDSUXB.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/customer/quotes"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-ZNRASXKM.js",
      "chunk-AC4GTW44.js",
      "chunk-EHCWU7RU.js",
      "chunk-6NZ7JFZA.js",
      "chunk-HLSKIOLE.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/customer/policies"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-JFAKJ6YV.js",
      "chunk-NRUGC7MI.js",
      "chunk-BN5RJHA6.js",
      "chunk-EHCWU7RU.js",
      "chunk-6NZ7JFZA.js",
      "chunk-KZGRWDQ7.js",
      "chunk-6HQAK6RD.js",
      "chunk-HLSKIOLE.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/customer/claims"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-YULN6ZTQ.js",
      "chunk-2ZHXGIJO.js",
      "chunk-KZGRWDQ7.js",
      "chunk-6HQAK6RD.js",
      "chunk-X5PGRLQR.js"
    ],
    "route": "/admin/dashboard"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-FFL7C4QT.js",
      "chunk-2ZHXGIJO.js",
      "chunk-KZGRWDQ7.js",
      "chunk-6HQAK6RD.js",
      "chunk-X5PGRLQR.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/admin/claims-managers/pending"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-URDPEK4U.js",
      "chunk-2ZHXGIJO.js",
      "chunk-KZGRWDQ7.js",
      "chunk-6HQAK6RD.js",
      "chunk-X5PGRLQR.js",
      "chunk-OCCJAXZN.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/admin/customers"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-6TTQ2DJH.js",
      "chunk-XGVYMIPD.js",
      "chunk-NFRHO7N6.js",
      "chunk-2ZHXGIJO.js",
      "chunk-KZGRWDQ7.js",
      "chunk-6HQAK6RD.js",
      "chunk-X5PGRLQR.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/admin/quotes"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-TQZLYZOI.js",
      "chunk-XGVYMIPD.js",
      "chunk-NFRHO7N6.js",
      "chunk-2ZHXGIJO.js",
      "chunk-KZGRWDQ7.js",
      "chunk-6HQAK6RD.js",
      "chunk-X5PGRLQR.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/admin/products"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-JMZPCLI7.js",
      "chunk-2ZHXGIJO.js",
      "chunk-KZGRWDQ7.js",
      "chunk-6HQAK6RD.js",
      "chunk-X5PGRLQR.js",
      "chunk-CMMTSM52.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/admin/pricing"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-MWQVUIK5.js",
      "chunk-BN5RJHA6.js",
      "chunk-NFRHO7N6.js",
      "chunk-2ZHXGIJO.js",
      "chunk-KZGRWDQ7.js",
      "chunk-6HQAK6RD.js",
      "chunk-X5PGRLQR.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/admin/operations"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-PMK4YY33.js"
    ],
    "route": "/claims-manager"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-PMK4YY33.js",
      "chunk-NJUK5G3X.js",
      "chunk-LQ5WDJUR.js",
      "chunk-6HQAK6RD.js"
    ],
    "route": "/claims-manager/dashboard"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-PMK4YY33.js",
      "chunk-EIG4CYIF.js",
      "chunk-LQ5WDJUR.js",
      "chunk-NRUGC7MI.js",
      "chunk-BN5RJHA6.js",
      "chunk-6HQAK6RD.js",
      "chunk-HJNG36WW.js"
    ],
    "route": "/claims-manager/claims"
  },
  {
    "renderMode": 2,
    "preload": [
      "chunk-PMK4YY33.js",
      "chunk-VIO7HKHS.js"
    ],
    "route": "/claims-manager/profile"
  },
  {
    "renderMode": 2,
    "redirectTo": "/",
    "route": "/**"
  }
],
  entryPointToBrowserMapping: undefined,
  assets: {
    'index.csr.html': {size: 10178, hash: '6c0184c8b4db7ad0d1f657c512cc450ff35e8ea41767cce3b633331765dd1189', text: () => import('./assets-chunks/index_csr_html.mjs').then(m => m.default)},
    'index.server.html': {size: 1318, hash: '65f395da5f60451a6146070e098479e600ec1fc73efcb40eea51b0d055fddfde', text: () => import('./assets-chunks/index_server_html.mjs').then(m => m.default)},
    'auth/login/index.html': {size: 32717, hash: '0dab7caada9ef0621a439d8aaed3d63eeea08b3d793f187a27c4e65d385e4024', text: () => import('./assets-chunks/auth_login_index_html.mjs').then(m => m.default)},
    'auth/register/customer/index.html': {size: 33039, hash: 'f878569fd8ee0b910419f41c22c29316d21c0ba492fded0fc1a60e9ad9108f57', text: () => import('./assets-chunks/auth_register_customer_index_html.mjs').then(m => m.default)},
    'index.html': {size: 90307, hash: '4131a9352340b0a0cd13203a754797e1023080bf5895f5e6486ba785dff3887f', text: () => import('./assets-chunks/index_html.mjs').then(m => m.default)},
    'auth/claims-manager/application-status/index.html': {size: 255, hash: '5234ad79cf291c29ba8d07c20adde4a3127024cc986f9539e639dc7c44a33e9b', text: () => import('./assets-chunks/auth_claims-manager_application-status_index_html.mjs').then(m => m.default)},
    'account/security/change-password/index.html': {size: 405, hash: 'e50fa491d8c5f9d2f65c0e878fa0ad0c141d39d9bf239299940b5a1b06338d62', text: () => import('./assets-chunks/account_security_change-password_index_html.mjs').then(m => m.default)},
    'customer/properties/index.html': {size: 360, hash: '53017647dd860e19bd4cc9f2f21e6da667543debc5e71e1592d8170da96549c9', text: () => import('./assets-chunks/customer_properties_index_html.mjs').then(m => m.default)},
    'access-denied/index.html': {size: 336, hash: '17520341075fb25eacf99bbb5f69d65a3afce76770aa580c08ad1c2d2e52ff2b', text: () => import('./assets-chunks/access-denied_index_html.mjs').then(m => m.default)},
    'customer/quotes/index.html': {size: 348, hash: 'd430e81a45e62ef6732882f212e71a29cd16f1d2cc625fa4e58594c98486e471', text: () => import('./assets-chunks/customer_quotes_index_html.mjs').then(m => m.default)},
    'customer/claims/index.html': {size: 348, hash: '8fb215ce643544d4cd7b75fc7bddb969c515a13d3d0bb38d19f71460cc40552b', text: () => import('./assets-chunks/customer_claims_index_html.mjs').then(m => m.default)},
    'admin/quotes/index.html': {size: 339, hash: '251e54230cf6138cd2d3449f1f5f1e6da59ff4afc149f4a2df90caa9dda448f2', text: () => import('./assets-chunks/admin_quotes_index_html.mjs').then(m => m.default)},
    'admin/claims-managers/pending/index.html': {size: 396, hash: '3807e32aa090e87a61f594baa7b3ff1a8b1b33f9cacc5b5dbe7dd07123c2fb65', text: () => import('./assets-chunks/admin_claims-managers_pending_index_html.mjs').then(m => m.default)},
    'admin/pricing/index.html': {size: 342, hash: '1a4bd0c6dc0740bfed17e67310211ff307023ff9f6535483c59b602f5c87ae98', text: () => import('./assets-chunks/admin_pricing_index_html.mjs').then(m => m.default)},
    'claims-manager/claims/index.html': {size: 366, hash: '3e713cbbec4edc4708dc63398bbf1d79d50bff0663303a11ced0b5c5a0e68e19', text: () => import('./assets-chunks/claims-manager_claims_index_html.mjs').then(m => m.default)},
    'claims-manager/index.html': {size: 375, hash: 'f2585bbe009ad65e79a2df362c2a3f84bff163728682d9a29ada855f819a6e50', text: () => import('./assets-chunks/claims-manager_index_html.mjs').then(m => m.default)},
    'auth/claims-manager/reapply/index.html': {size: 255, hash: '5234ad79cf291c29ba8d07c20adde4a3127024cc986f9539e639dc7c44a33e9b', text: () => import('./assets-chunks/auth_claims-manager_reapply_index_html.mjs').then(m => m.default)},
    'customer/dashboard/index.html': {size: 357, hash: '50811c1e5f87bbec292b2c689ecae9394ab489ff33c65d82364a5254c1a80f7c', text: () => import('./assets-chunks/customer_dashboard_index_html.mjs').then(m => m.default)},
    'customer/policies/index.html': {size: 354, hash: '1fcd0a47db4fdd209022bb5ef37e5f04ed1f4d4500b703aedca4491b68c83bc5', text: () => import('./assets-chunks/customer_policies_index_html.mjs').then(m => m.default)},
    'auth/reset-password/index.html': {size: 28112, hash: '448fee2a291d7d3bee6c2fc4d44e79be7b4e584f6752acef9a1959b6cf0befcf', text: () => import('./assets-chunks/auth_reset-password_index_html.mjs').then(m => m.default)},
    'admin/customers/index.html': {size: 348, hash: '82002407b616ca6792bfb5a73bc351a1ef65b81de77ad70d556b0f0e6d0ff337', text: () => import('./assets-chunks/admin_customers_index_html.mjs').then(m => m.default)},
    'auth/register/claims-manager/index.html': {size: 35372, hash: '8646346b745e77d81f766f0ee40df6a298678fd2a2038eb6ec5c6815617f8ff8', text: () => import('./assets-chunks/auth_register_claims-manager_index_html.mjs').then(m => m.default)},
    'admin/operations/index.html': {size: 351, hash: '0fd533947a88736b405e61c4b7ae7a8dc3302d7d3aa4b48b71f1c8c0bcf0d466', text: () => import('./assets-chunks/admin_operations_index_html.mjs').then(m => m.default)},
    'claims-manager/profile/index.html': {size: 369, hash: '0d0a2888fc431638660f954181c037c664b813c0c719f93bdd20286a66a7e84f', text: () => import('./assets-chunks/claims-manager_profile_index_html.mjs').then(m => m.default)},
    'auth/mfa/verify/index.html': {size: 255, hash: '5234ad79cf291c29ba8d07c20adde4a3127024cc986f9539e639dc7c44a33e9b', text: () => import('./assets-chunks/auth_mfa_verify_index_html.mjs').then(m => m.default)},
    'customer/profile/index.html': {size: 351, hash: 'd2969e8a586624316ecab4b4108da0c93d207f1eea3d1f4c962092b91cab99c4', text: () => import('./assets-chunks/customer_profile_index_html.mjs').then(m => m.default)},
    'admin/products/index.html': {size: 345, hash: 'ba30845040159a50e3d577618d93a47ea3a7ca165723b9e118b082fc8e42487f', text: () => import('./assets-chunks/admin_products_index_html.mjs').then(m => m.default)},
    'admin/dashboard/index.html': {size: 348, hash: 'd6727ce28d013cba435906e41bc48f6dd074fdd3037a1f03cc01f3ad6e41a7e2', text: () => import('./assets-chunks/admin_dashboard_index_html.mjs').then(m => m.default)},
    'account/security/mfa/index.html': {size: 369, hash: '5f215959c7d615a2232e2adbcf4beac81c3cf7afc95f8138b34ccabe75919a03', text: () => import('./assets-chunks/account_security_mfa_index_html.mjs').then(m => m.default)},
    'claims-manager/dashboard/index.html': {size: 375, hash: 'f2585bbe009ad65e79a2df362c2a3f84bff163728682d9a29ada855f819a6e50', text: () => import('./assets-chunks/claims-manager_dashboard_index_html.mjs').then(m => m.default)},
    'auth/forgot-password/index.html': {size: 29696, hash: '10dc8348fb84dfc1d3f9d562d4d1436bbad297a110598094aa74bf5e6dcd186d', text: () => import('./assets-chunks/auth_forgot-password_index_html.mjs').then(m => m.default)},
    'products/vacation-home-insurance/index.html': {size: 41698, hash: 'faf396f9d166d07bd86976e452cf03bf382ed011f5ba9351706cb38c40e5d0c5', text: () => import('./assets-chunks/products_vacation-home-insurance_index_html.mjs').then(m => m.default)},
    'styles-FQDCUBRE.css': {size: 68507, hash: 'Etl9bdXiYWE', text: () => import('./assets-chunks/styles-FQDCUBRE_css.mjs').then(m => m.default)}
  },
};
