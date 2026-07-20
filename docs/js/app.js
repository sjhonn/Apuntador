window.apuntadorStorage = {
  get: key => localStorage.getItem(key),
  set: (key, value) => localStorage.setItem(key, value),
  remove: key => localStorage.removeItem(key)
};
window.apuntadorTheme = {
  apply: theme => { document.documentElement.dataset.theme = theme === 'light' ? 'light' : 'dark'; }
};
window.apuntadorFiles = {
  download: (name, content, type) => {
    const blob = new Blob([content], { type });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a'); a.href = url; a.download = name; a.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  },
  readSelected: input => new Promise((resolve, reject) => {
    const file = input?.files?.[0]; if (!file) return reject(new Error('No file'));
    const reader = new FileReader(); reader.onload = () => resolve(reader.result); reader.onerror = reject; reader.readAsText(file);
  })
};
