const fs = require('fs');
const path = require('path');
const fontDir = './wwwroot/fonts';
const cssOut = './wwwroot/css/fonts.generated.css';
// Change this to your desired output folder (e.g., './fonts-data/fonts.json')
const jsonOut = './wwwroot/fonts/fonts.json';

// Ensure the output directory exists
const jsonDir = path.dirname(jsonOut);
if (!fs.existsSync(jsonDir)) {
    fs.mkdirSync(jsonDir, { recursive: true });
}

const fontFiles = fs.readdirSync(fontDir).filter(f => /\.(ttf|otf|woff2?)$/i.test(f));
let css = '';
let fontsJson = [];

fontFiles.forEach(file => {
    const fontName = path.parse(file).name.replace(/[^a-zA-Z0-9]/g, '');
    css += `
@font-face {
    font-family: '${fontName}';
    src: url('/fonts/${file}');
    font-weight: normal;
    font-style: normal;
}
`;
    fontsJson.push({ Name: fontName, File: file });
});

fs.writeFileSync(cssOut, css);
fs.writeFileSync(jsonOut, JSON.stringify(fontsJson, null, 2));
console.log('Generated CSS for fonts:', fontFiles);
console.log('Generated fonts.json with', fontsJson.length, 'fonts at', jsonOut);