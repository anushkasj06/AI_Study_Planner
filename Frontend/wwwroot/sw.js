self.addEventListener('push', (event) => {
  if (!event.data) {
    return;
  }

  let payload;
  try {
    payload = event.data.json();
  } catch {
    payload = {
      title: 'AI Study Planner',
      body: event.data.text()
    };
  }

  const title = payload.title || 'AI Study Planner';
  const options = {
    body: payload.body || 'You have a new study reminder.',
    icon: payload.icon || '/favicon.ico',
    badge: payload.badge || '/favicon.ico',
    data: {
      url: '/app/dashboard'
    }
  };

  event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', (event) => {
  event.notification.close();

  event.waitUntil((async () => {
    const allClients = await clients.matchAll({ includeUncontrolled: true, type: 'window' });
    if (allClients.length > 0) {
      allClients[0].focus();
      allClients[0].navigate('/app/dashboard');
      return;
    }

    await clients.openWindow('/app/dashboard');
  })());
});
