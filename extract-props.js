const fs = require('fs');
const path = require('path');

const uiDir = path.join(__dirname, 'frontend/src/components/ui');
const files = fs.readdirSync(uiDir).filter(f => f.endsWith('.tsx'));

for (const file of files) {
    const content = fs.readFileSync(path.join(uiDir, file), 'utf-8');
    const interfaceMatch = content.match(/interface\s+[a-zA-Z0-9_]+Props\s*(?:<[^>]+>)?\s*\{[^}]+\}/g);
    const typeMatch = content.match(/type\s+[a-zA-Z0-9_]+Props\s*=\s*(?:[^{]+|\{[^}]+\})/g);
    console.log(`\n--- ${file} ---`);
    if (interfaceMatch) {
        interfaceMatch.forEach(m => console.log(m));
    }
    if (typeMatch) {
        typeMatch.forEach(m => console.log(m));
    }
}
