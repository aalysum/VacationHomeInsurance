import './polyfills.server.mjs';
var r={production:!1,apiGatewayBaseUrl:"http://localhost:7000"};function i(t){let e=r.apiGatewayBaseUrl.replace(/\/+$/,""),n=t.startsWith("/")?t:`/${t}`;return`${e}${n}`}export{r as a,i as b};
