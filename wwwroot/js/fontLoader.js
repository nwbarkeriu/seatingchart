window.ensureFontLoaded = (fontName, fontUrl) => {
    // Remove spaces and special chars for CSS font-family
    const cssFontName = fontName.replace(/[^a-zA-Z0-9]/g, '');
    if (document.fonts.check(`1em ${cssFontName}`)) return;

    // Only add if not already present
    if (!document.getElementById('font-' + cssFontName)) {
        const style = document.createElement('style');
        style.id = 'font-' + cssFontName;
        style.innerHTML = `
        @font-face {
            font-family: '${cssFontName}';
            src: url('${fontUrl}');
            font-weight: normal;
            font-style: normal;
        }`;
        document.head.appendChild(style);
    }
};