let observer;
let current = {
  authenticated: false,
  modules: {},
  permissions: {}
};

function setVisible(element, visible) {
  element.style.display = visible ? "" : "none";
  element.setAttribute("aria-hidden", visible ? "false" : "true");
  if ("disabled" in element && !visible) {
    element.disabled = true;
  }
}

function moduleKey(element) {
  const icon = element.querySelector('[class*="icon-"]');
  if (!icon) return null;
  const match = [...icon.classList].map(value => /^icon-(find|apply|approval|reports)$/.exec(value)).find(Boolean);
  return match?.[1] ?? null;
}

function apply() {
  const path = window.location.pathname.toLowerCase();
  if (!current.authenticated && path !== "/") {
    window.location.replace("/");
    return;
  }

  document.querySelectorAll(".dropdown-item, .map-action-button").forEach(element => {
    const key = moduleKey(element);
    if (key) setVisible(element, current.modules[key] === true);
  });

  document.querySelectorAll(".add-trip-button").forEach(element =>
    setVisible(element, current.permissions.create === true));

  document.querySelectorAll(".approve-button").forEach(element =>
    setVisible(element, current.permissions.approve === true));

  document.querySelectorAll(".trip-quick-actions button").forEach((element, index) => {
    const allowed = index === 0
      ? current.permissions.reject === true
      : index === 1
        ? current.permissions.delete === true
        : index === 2
          ? current.permissions.edit === true
          : current.permissions.arrangements === true;
    setVisible(element, allowed);
  });

  if (path === "/find" && current.modules.find !== true) {
    window.location.replace("/dashboard");
    return;
  }

  if (path === "/dashboard") {
    const requestedModule = new URLSearchParams(window.location.search).get("module");
    if (requestedModule && current.modules[requestedModule] !== true) {
      window.history.replaceState({}, "", "/dashboard");
      window.location.reload();
    }
  }
}

export function start(configuration) {
  current = configuration;
  observer?.disconnect();
  observer = new MutationObserver(apply);
  observer.observe(document.body, { childList: true, subtree: true });
  apply();
}
