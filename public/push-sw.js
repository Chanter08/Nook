self.addEventListener("push", event => {
 if (!event.data) return;
 let msg; try { msg = event.data.json(); } catch { return; }
 const title = typeof msg.title === "string" ? msg.title : "Nook";
 const body = typeof msg.body === "string" ? msg.body : "";
 const path = typeof msg.url === "string" && /^\/(?!\/)/.test(msg.url) ? msg.url : "/";
 event.waitUntil(self.registration.showNotification(title, { body, icon: "/nook-icon.svg", tag: "nook-" + title + body, data: { path } }));
});
self.addEventListener("notificationclick", event => {
 event.notification.close();
 const target = new URL(event.notification.data?.path || "/", self.location.origin).href;
 event.waitUntil((async () => {
   const tabs = await clients.matchAll({ type: "window", includeUncontrolled: true });
   const tab = tabs.find(t => new URL(t.url).origin === self.location.origin);
   if (tab) { await tab.navigate(target); await tab.focus(); }
   else await clients.openWindow(target);
 })());
});
