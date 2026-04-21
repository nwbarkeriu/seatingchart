function initTicker() {
  var ticker = document.querySelector(".ticker");
  var items = ticker ? ticker.querySelectorAll(".ticker-item") : [];

  if (!ticker || items.length < 2) {
    setTimeout(initTicker, 500);
    return;
  }

  if (ticker.dataset.tickerInit === "true") return;
  ticker.dataset.tickerInit = "true";

  var halfCount = Math.floor(items.length / 2);
  // Measure actual width of the first half (original items)
  var totalWidth = 0;
  for (var i = 0; i < halfCount; i++) {
    totalWidth += items[i].offsetWidth;
  }

  var speed = 60; // pixels per second
  var offset = 0;
  var lastTime = performance.now();

  function animate(now) {
    var dt = (now - lastTime) / 1000;
    lastTime = now;
    offset += speed * dt;
    // When we've scrolled past all original items, reset seamlessly
    if (offset >= totalWidth) {
      offset -= totalWidth;
    }
    ticker.style.transform = "translateX(-" + offset + "px)";
    requestAnimationFrame(animate);
  }

  ticker.style.transition = "none";
  requestAnimationFrame(animate);
}

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", initTicker);
} else {
  initTicker();
}