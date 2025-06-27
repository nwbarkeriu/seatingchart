document.addEventListener("DOMContentLoaded", () => {
  const ticker = document.querySelector(".ticker");
  const items = ticker.querySelectorAll(".ticker-item");

  if (!ticker || items.length === 0) {
    console.error("Ticker or items not found.");
    return;
  }

  // Use the fixed height from CSS
  const itemHeight = items[0].offsetHeight;
  const frameCount = items.length;
  const animationDuration = frameCount * 4; // 2 seconds per frame

  // Set the height of the ticker-wrap dynamically
  const tickerWrap = document.querySelector(".ticker-wrap");
  tickerWrap.style.height = `${itemHeight}px`;
  items.forEach(item => item.style.height = `${itemHeight}px`);
  // Calculate the cumulative height for each frame
  const frameHeights = Array.from(items).map(item => item.offsetHeight);
  const cumulativeHeights = frameHeights.map((_, i) =>
    frameHeights.slice(0, i).reduce((sum, height) => sum + height, 0)
  );

  console.log("Frame Heights:", frameHeights);
  console.log("Cumulative Heights:", cumulativeHeights);

  // Create keyframes for scrolling and pausing
  const style = document.createElement("style");
  style.type = "text/css";

  const percentPerFrame = 100 / frameCount;
  let keyframes = `@keyframes scroll-vert-roll-up {`;

  for (let i = 0; i < frameCount; i++) {
    const pauseStart = i * percentPerFrame;
    const pauseEnd = pauseStart + percentPerFrame * 0.7; // 70% pause, 30% scroll
    const scrollEnd = (i + 1) * percentPerFrame;
    const manualOffset = 40; // Adjust this number as needed (positive or negative)
    const yStart = -i * itemHeight - manualOffset;
    const yEnd = -(i + 1) * itemHeight - manualOffset;

    keyframes += `
      ${pauseStart}% { transform: translateY(${yStart}px); }
      ${pauseEnd}% { transform: translateY(${yStart}px); }
      ${scrollEnd}% { transform: translateY(${yEnd}px); }
    `;
  }
  keyframes += `100% { transform: translateY(0); } }`;

  style.innerHTML = `
    .ticker {
      animation: scroll-vert-roll-up ${animationDuration}s linear infinite;
      animation-fill-mode: forwards;
    }
    .ticker:hover {
      animation-play-state: paused;
    }
    ${keyframes}
  `;
  document.head.appendChild(style);
});