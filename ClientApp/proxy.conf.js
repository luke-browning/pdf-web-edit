const { env } = require("process");

const target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS
  ? env.ASPNETCORE_URLS.split(";")[0]
  : "https://localhost:7114";

const PROXY_CONFIG = [
  {
    context: ["/api/**"],
    target: target,
    secure: false,
    ws: true
  },
];

module.exports = PROXY_CONFIG;
