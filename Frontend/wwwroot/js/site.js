(function () {
  const csrfToken = document.querySelector('meta[name="request-verification-token"]')?.getAttribute('content') ?? '';

  const uiLogPrefix = '[AIStudyPlanner UI]';

  logClientInfo(`initialized on ${window.location.pathname}`);

  initializeMermaidSupport();
  initializeAlerts();
  initializeNotifications();
  initializeAssistant();
  initializeNotesPage();
  initializeWebPush();
  initializeReminderStudio();

  function initializeAlerts() {
    const alerts = document.querySelectorAll('.alert');
    if (!alerts.length) {
      return;
    }

    window.setTimeout(() => {
      alerts.forEach((alert) => {
        alert.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
        alert.style.opacity = '0';
        alert.style.transform = 'translateY(-4px)';
      });
    }, 5000);
  }

  function initializeNotifications() {
    const toggleButton = document.getElementById('notificationToggleBtn');
    const panel = document.getElementById('notificationPanel');
    const list = document.getElementById('notificationList');
    const markAllButton = document.getElementById('markAllNotificationsReadBtn');

    if (!toggleButton || !panel || !list || !markAllButton) {
      return;
    }

    toggleButton.addEventListener('click', async () => {
      try {
        logClientDebug('notifications panel toggle requested');
        panel.classList.toggle('hidden');
        if (!panel.classList.contains('hidden')) {
          logClientDebug('notifications panel opened, loading data');
          await loadNotifications(list);
        }
      } catch (error) {
        logClientError('loading notifications', error);
      }
    });

    markAllButton.addEventListener('click', async () => {
      try {
        logClientDebug('mark all notifications as read');
        await apiRequest('/app/notifications/mark-all-read', {
          method: 'POST'
        });

        await loadNotifications(list);
      } catch (error) {
        logClientError('marking all notifications as read', error);
      }
    });
  }

  async function loadNotifications(list) {
    const notifications = await apiRequest('/app/notifications?includeRead=true', {
      method: 'GET'
    });

    list.innerHTML = '';

    if (!notifications.length) {
      list.innerHTML = '<article class="empty-state compact"><h3>No notifications</h3><p>You are all caught up.</p></article>';
      return;
    }

    notifications.forEach((item) => {
      const card = document.createElement('article');
      card.className = 'reminder-card stacked';

      const badgeClass = item.isRead ? 'muted' : 'brand';
      card.innerHTML = `
        <div>
          <p class="reminder-title">${escapeHtml(item.title)}</p>
          <p class="reminder-message">${escapeHtml(item.message)}</p>
          <p class="reminder-meta">${formatDate(item.createdAt)}</p>
        </div>
        <div class="reminder-actions">
          <span class="pill ${badgeClass}">${item.type}</span>
          ${item.isRead ? '' : '<button type="button" class="btn btn-secondary small">Mark read</button>'}
        </div>`;

      if (!item.isRead) {
        const button = card.querySelector('button');
        button.addEventListener('click', async () => {
          try {
            await apiRequest(`/app/notifications/${item.id}/mark-read`, {
              method: 'POST'
            });

            await loadNotifications(list);
          } catch (error) {
            logClientError('marking notification as read', error);
          }
        });
      }

      list.appendChild(card);
    });
  }

  function initializeAssistant() {
    const fab = document.getElementById('assistantFab');
    const panel = document.getElementById('assistantPanel');
    const expandButton = document.getElementById('assistantExpandBtn');
    const closeButton = document.getElementById('assistantCloseBtn');
    const form = document.getElementById('assistantForm');
    const input = document.getElementById('assistantInput');
    const messages = document.getElementById('assistantMessages');
    const shortcuts = document.querySelectorAll('.assistant-chip[data-assistant-prompt]');
    const mindMapPanel = document.getElementById('assistantMindMapPanel');
    const mindMapContainer = document.getElementById('assistantMindMap');
    const mindMapProvider = document.getElementById('assistantMindMapProvider');

    if (!fab || !panel || !expandButton || !closeButton || !form || !input || !messages) {
      return;
    }

    shortcuts.forEach((button) => {
      button.addEventListener('click', () => {
        input.value = button.getAttribute('data-assistant-prompt') ?? '';
        input.focus();
      });
    });

    fab.addEventListener('click', async () => {
      try {
        logClientDebug('assistant panel toggle requested');
        panel.classList.toggle('hidden');
      } catch (error) {
        logClientError('opening assistant panel', error);
      }
    });

    expandButton.addEventListener('click', () => {
      const isExpanded = panel.classList.toggle('assistant-panel--expanded');
      expandButton.textContent = isExpanded ? 'Restore' : 'Maximize';
      expandButton.setAttribute('aria-pressed', String(isExpanded));
      document.body.classList.toggle('assistant-window-open', isExpanded);
    });

    closeButton.addEventListener('click', () => {
      panel.classList.add('hidden');
      panel.classList.remove('assistant-panel--expanded');
      expandButton.textContent = 'Maximize';
      expandButton.setAttribute('aria-pressed', 'false');
      document.body.classList.remove('assistant-window-open');
    });

    form.addEventListener('submit', async (event) => {
      event.preventDefault();

      const messageText = input.value.trim();
      if (!messageText) {
        return;
      }

      try {
        appendMessage(messages, messageText, 'user');
        input.value = '';
        logClientDebug('assistant chat message sent');

        const response = await apiRequest('/app/assistant/chat', {
          method: 'POST',
          body: {
            message: messageText
          }
        });

        appendAssistantResponse(messages, response);
        await renderAssistantMindMap(mindMapPanel, mindMapContainer, mindMapProvider, response.mindMapMermaid, response.provider, response.usedFallback);

        logClientDebug('assistant chat response received');
      } catch (error) {
        logClientError('sending assistant chat message', error);
      }
    });
  }

  function appendMessage(container, message, sender) {
    const item = document.createElement('article');
    item.className = `assistant-message ${sender}`;
    item.textContent = message;
    container.appendChild(item);
    container.scrollTop = container.scrollHeight;
  }

  function appendAssistantResponse(container, response) {
    appendMessage(container, response.reply || 'I did not receive a response.', 'bot');

    if (Array.isArray(response.suggestions) && response.suggestions.length > 0) {
      const suggestionLine = response.suggestions.map((x) => x.label).filter(Boolean).join(' | ');
      if (suggestionLine) {
        appendMessage(container, `Suggested next actions: ${suggestionLine}`, 'bot');
      }
    }
  }

  function initializeNotesPage() {
    const noteCards = document.querySelectorAll('.note-diagram[data-mermaid]');
    if (!noteCards.length) {
      return;
    }

    noteCards.forEach((diagramNode) => {
      const diagram = diagramNode.getAttribute('data-mermaid') ?? diagramNode.textContent ?? '';
      if (!diagram.trim()) {
        return;
      }

      renderMermaidDiagram(diagramNode, diagram).catch((error) => {
        logClientError('rendering note graph', error);
        diagramNode.textContent = diagram;
      });
    });

    const chips = document.querySelectorAll('.notes-template-chip[data-note-template]');
    const promptInput = document.querySelector('.notes-compose-card textarea[name="Prompt"]');

    if (!promptInput) {
      return;
    }

    chips.forEach((button) => {
      button.addEventListener('click', () => {
        promptInput.value = button.getAttribute('data-note-template') ?? '';
        promptInput.focus();
      });
    });
  }

  async function renderAssistantMindMap(panel, container, providerChip, diagram, provider, usedFallback) {
    if (!panel || !container) {
      return;
    }

    const hasDiagram = typeof diagram === 'string' && diagram.trim().length > 0;
    if (!hasDiagram) {
      panel.classList.add('hidden');
      container.innerHTML = '';
      return;
    }

    panel.classList.remove('hidden');
    if (providerChip) {
      providerChip.textContent = usedFallback ? 'Fallback' : (provider || 'Groq');
    }
    container.innerHTML = '';

    const diagramNode = document.createElement('div');
    diagramNode.className = 'mermaid assistant-mermaid';
    diagramNode.textContent = diagram;
    container.appendChild(diagramNode);

    await renderMermaidDiagram(diagramNode, diagram);
  }

  function initializeMermaidSupport() {
    if (!window.mermaid) {
      return;
    }

    try {
      window.mermaid.initialize({
        startOnLoad: false,
        securityLevel: 'strict',
        theme: 'neutral',
        flowchart: { useMaxWidth: true, htmlLabels: true }
      });
    } catch (error) {
      logClientError('initializing mermaid', error);
    }
  }

  async function renderMermaidDiagram(container, diagram) {
    if (!window.mermaid || !container) {
      return;
    }

    let target = container.classList.contains('mermaid') ? container : container.querySelector('.mermaid');
    if (!target) {
      target = document.createElement('div');
      target.className = 'mermaid';
      target.textContent = diagram;
      container.appendChild(target);
    } else {
      target.textContent = diagram;
    }

    if (!target) {
      return;
    }

    try {
      await window.mermaid.run({ nodes: [target] });
    } catch (error) {
      logClientError('rendering mermaid diagram', error);
      throw error;
    }
  }

  function initializeWebPush() {
    const button = document.getElementById('enablePushBtn');
    if (!button) {
      return;
    }

    if (!('serviceWorker' in navigator) || !('PushManager' in window) || !('Notification' in window)) {
      button.disabled = true;
      button.textContent = 'Web alerts unavailable';
      return;
    }

    button.addEventListener('click', async () => {
      try {
        const permission = await Notification.requestPermission();
        if (permission !== 'granted') {
          button.textContent = 'Permission denied';
          return;
        }

        const keyResponse = await apiRequest('/app/notifications/webpush/public-key', { method: 'GET' });
        if (!keyResponse.publicKey) {
          button.textContent = 'Push key missing';
          return;
        }

        const registration = await navigator.serviceWorker.register('/sw.js');
        let subscription = await registration.pushManager.getSubscription();

        if (!subscription) {
          subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(keyResponse.publicKey)
          });
        }

        const json = subscription.toJSON();
        await apiRequest('/app/notifications/webpush/subscribe', {
          method: 'POST',
          body: {
            endpoint: json.endpoint,
            p256dh: json.keys?.p256dh ?? '',
            auth: json.keys?.auth ?? ''
          }
        });

        button.textContent = 'Web alerts enabled';
        logClientInfo('web push enabled successfully');
      } catch (error) {
        logClientError('enabling web push', error);
      }
    });
  }

  function initializeReminderStudio() {
    const aiDraftButton = document.getElementById('aiReminderDraftBtn');
    const promptInput = document.getElementById('aiReminderPromptInput');
    const titleInput = document.getElementById('reminderTitleInput');
    const messageInput = document.getElementById('reminderMessageInput');
    const dateInput = document.getElementById('reminderDateInput');
    const channelInAppInput = document.getElementById('reminderChannelInAppInput');
    const channelEmailInput = document.getElementById('reminderChannelEmailInput');
    const channelPushInput = document.getElementById('reminderChannelBrowserPushInput');
    const draftMeta = document.getElementById('aiReminderDraftMeta');
    const templateButtons = document.querySelectorAll('.reminder-template-chip');

    if (!aiDraftButton || !titleInput || !messageInput || !dateInput) {
      return;
    }

    if (!dateInput.value) {
      dateInput.value = toLocalDateTimeInputValue(new Date(Date.now() + 60 * 60 * 1000));
    }

    templateButtons.forEach((button) => {
      button.addEventListener('click', () => {
        const template = button.getAttribute('data-template') ?? '';
        if (promptInput) {
          promptInput.value = template;
          promptInput.focus();
        }
      });
    });

    aiDraftButton.addEventListener('click', async () => {
      try {
        aiDraftButton.setAttribute('disabled', 'disabled');
        aiDraftButton.textContent = 'Generating...';

        const preferredReminderDateTime = dateInput.value
          ? new Date(dateInput.value).toISOString()
          : null;

        const draft = await apiRequest('/app/reminders/ai-draft', {
          method: 'POST',
          body: {
            prompt: promptInput?.value?.trim() ?? '',
            preferredReminderDateTime,
            preferEmail: Boolean(channelEmailInput?.checked),
            preferBrowserPush: Boolean(channelPushInput?.checked)
          }
        });

        if (typeof draft.title === 'string' && draft.title.length > 0) {
          titleInput.value = draft.title;
        }

        if (typeof draft.message === 'string' && draft.message.length > 0) {
          messageInput.value = draft.message;
        }

        if (draft.reminderDateTime) {
          dateInput.value = toLocalDateTimeInputValue(new Date(draft.reminderDateTime));
        }

        if (Array.isArray(draft.recommendedChannels)) {
          const channels = draft.recommendedChannels.map((x) => String(x));
          if (channelInAppInput) {
            channelInAppInput.checked = channels.includes('InApp');
          }
          if (channelEmailInput) {
            channelEmailInput.checked = channels.includes('Email');
          }
          if (channelPushInput) {
            channelPushInput.checked = channels.includes('BrowserPush');
          }
        }

        if (draftMeta) {
          draftMeta.textContent = draft.reasoning
            ? `AI reasoning: ${draft.reasoning}`
            : 'AI draft generated successfully.';
        }

        logClientInfo('ai reminder draft generated');
      } catch (error) {
        if (draftMeta) {
          draftMeta.textContent = 'AI draft failed. Please adjust prompt and try again.';
        }

        logClientError('generating ai reminder draft', error);
      } finally {
        aiDraftButton.removeAttribute('disabled');
        aiDraftButton.textContent = 'Generate with AI';
      }
    });
  }

  async function apiRequest(url, options) {
    const requestOptions = {
      method: options.method,
      headers: {
        'RequestVerificationToken': csrfToken
      }
    };

    if (options.body) {
      requestOptions.headers['Content-Type'] = 'application/json';
      requestOptions.body = JSON.stringify(options.body);
    }

    logClientDebug(`api request ${options.method} ${url}`);

    const response = await fetch(url, requestOptions);
    if (!response.ok) {
      const text = await response.text();
      logClientError(`request failed ${options.method} ${url}`, text);
      throw new Error(text || `Request failed: ${response.status}`);
    }

    logClientDebug(`api response ${response.status} ${options.method} ${url}`);

    if (response.status === 204) {
      return {};
    }

    return response.json();
  }

  function logClientError(context, error) {
    console.error(`${uiLogPrefix} ${context}`, error);
  }

  function logClientInfo(message) {
    console.info(`${uiLogPrefix} ${message}`);
  }

  function logClientDebug(message) {
    console.debug(`${uiLogPrefix} ${message}`);
  }

  function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
      .replace(/-/g, '+')
      .replace(/_/g, '/');

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; i += 1) {
      outputArray[i] = rawData.charCodeAt(i);
    }

    return outputArray;
  }

  function toLocalDateTimeInputValue(date) {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hour = String(d.getHours()).padStart(2, '0');
    const minute = String(d.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hour}:${minute}`;
  }

  function formatDate(value) {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return value;
    }

    return new Intl.DateTimeFormat('en-IN', {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(date);
  }

  function escapeHtml(text) {
    return String(text)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }
}());