document.addEventListener("DOMContentLoaded", () => {
  const ticker = document.querySelector(".ticker");
  const items = ticker.querySelectorAll(".ticker-item");

  if (!ticker || items.length === 0) {
    console.error("Ticker or items not found.");
    return;
  }

  // Calculate the cumulative height for each frame
  const frameHeights = Array.from(items).map(item => item.offsetHeight);
  const cumulativeHeights = frameHeights.map((_, i) =>
    frameHeights.slice(0, i).reduce((sum, height) => sum + height, 0)
  );

  console.log("Frame Heights:", frameHeights);
  console.log("Cumulative Heights:", cumulativeHeights);

  // Set the height of the ticker-wrap dynamically to match the first frame
  const tickerWrap = document.querySelector(".ticker-wrap");
  tickerWrap.style.height = `${frameHeights[0]}px`;
  const maxHeight = Math.max(...frameHeights);
  tickerWrap.style.height = `${maxHeight}px`;
  items.forEach(item => item.style.height = `${maxHeight}px`);

  // Create a dynamic keyframes style for scrolling and pausing
  const style = document.createElement("style");
  style.type = "text/css";

  const frameCount = items.length;
  const animationDuration = frameCount * 5; // 5 seconds per frame


const keyframes = `
  @keyframes roll-up {
    ${Array.from({ length: frameCount }, (_, i) => {
      const percentPerFrame = 100 / frameCount;
      const pauseStart = i * percentPerFrame;
      const pauseEnd = pauseStart + percentPerFrame * 0.7; // 70% pause, 30% scroll
      const scrollEnd = (i + 1) * percentPerFrame;
      const yStart = -cumulativeHeights[i];
      const yEnd = -(cumulativeHeights[i] + frameHeights[i]);

      return `
        ${pauseStart}% { transform: translateY(${yStart}px); }
        ${pauseEnd}% { transform: translateY(${yStart}px); }
        ${scrollEnd}% { transform: translateY(${yEnd}px); }
      `;
    }).join('')}
    100% { transform: translateY(0); }
  }
`;

  style.innerHTML = `
    .ticker {
      animation: roll-up ${animationDuration}s linear infinite;
    }
    .ticker:hover {
      animation-play-state: paused;
    }
    ${keyframes}
  `;
  document.head.appendChild(style);
});