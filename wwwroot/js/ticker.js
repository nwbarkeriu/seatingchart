function initTicker() {
  const ticker = document.querySelector(".ticker");
  const items = ticker ? ticker.querySelectorAll(".ticker-item") : [];

  if (!ticker || items.length < 2) {
    // Items not rendered yet (Blazor Server), retry shortly
    setTimeout(initTicker, 500);
    return;
  }

  // Avoid double-init
  if (ticker.dataset.tickerInit === "true") return;
  ticker.dataset.tickerInit = "true";

  const itemHeight = 120; // matches CSS .ticker-item height
  const pauseDuration = 4000; // ms to pause on each item
  const scrollDuration = 600; // ms for the scroll transition
  const halfCount = Math.floor(items.length / 2);
  let currentIndex = 0;

  // Set initial position
  ticker.style.transition = "none";
  ticker.style.transform = "translateY(0px)";

  function scrollToNext() {
    currentIndex++;

    // Smooth scroll to next item
    ticker.style.transition = "transform " + scrollDuration + "ms ease-in-out";
    ticker.style.transform = "translateY(-" + (currentIndex * itemHeight) + "px)";

    // When we've scrolled through all original items, reset seamlessly
    if (currentIndex >= halfCount) {
      setTimeout(function() {
        currentIndex = 0;
        ticker.style.transition = "none";
        ticker.style.transform = "translateY(0px)";
      }, scrollDuration);
    }
  }

  // Start the ticker loop
  setInterval(scrollToNext, pauseDuration + scrollDuration);
}

// Try on DOMContentLoaded and also immediately (for Blazor Server late rendering)
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", initTicker);
} else {
  initTicker();
}