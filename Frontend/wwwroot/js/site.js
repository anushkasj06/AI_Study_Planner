(function () {
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
}());