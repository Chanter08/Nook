# Nook Web Push: deployment checklist

1. Generate a permanent VAPID key pair using `npx web-push generate-vapid-keys` and keep the private key secret.
2. Set `WebPush__PublicKey`, `WebPush__PrivateKey` and `WebPush__Subject` (for example `mailto:admin@example.com`) on the API host/container. Do not commit them. Replacing these keys invalidates existing browser subscriptions.
3. Run the EF migration `20261108100000_AddWebPushReminders` with the existing deployment migration procedure.
4. Serve Nook via HTTPS with the UI and API on the same origin. `/push-sw.js` must be served as JavaScript; outbound HTTPS to push gateways must be allowed.
5. Keep the API online and run **one** dispatcher instance. The background worker scans every 5 minutes; sleeping/down hosts miss the chosen hour. Add a distributed lease/outbox before horizontally scaling.
6. In Nook open **Us → Notifications**, enable, select categories and preferred reminder hour, then press **Send test**. On iPhone (iOS 16.4+), install to Home Screen from Safari and open via its icon.

Schedule: meals scheduled today, incomplete chores due today, external calendar events tomorrow, pantry items expiring tomorrow. Events require the existing `GoogleCalendar:IcalUrl` setting. All notification hours are local to Europe/Malta and automatically adjust for DST. Delivery history prevents repeats on later scans; upstream push providers may delay notifications. Per-device tokens protect preferences, but Nook has no household user authentication: secure public exposure and apply rate limits before making this a multi-user public service. No guarantee of exactly-once delivery across crashes or concurrent API hosts.
