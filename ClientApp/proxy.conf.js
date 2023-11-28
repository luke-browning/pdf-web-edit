const target = "http://localhost:5114";

const PROXY_CONFIG = [
  {
    context: ["/api/**"],
    target: target,
    secure: false,
    ws: true
  },
];

module.exports = PROXY_CONFIG;
