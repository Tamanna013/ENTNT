const fs = require('fs');
const path = require('path');

function processDir(dir) {
    const files = fs.readdirSync(dir);
    for (const file of files) {
        const fullPath = path.join(dir, file);
        if (fs.statSync(fullPath).isDirectory()) {
            processDir(fullPath);
        } else if (file.endsWith('FormModal.tsx')) {
            processFile(fullPath);
        }
    }
}

function processFile(filePath) {
    let content = fs.readFileSync(filePath, 'utf-8');
    const interfaceMatch = content.match(/interface\s+[a-zA-Z0-9_]+Props\s*(?:<[^>]+>)?\s*\{[^}]+\}/g);
    console.log(`\n--- ${path.basename(filePath)} ---`);
    if (interfaceMatch) {
        interfaceMatch.forEach(m => console.log(m));
    }
}

processDir(path.join(__dirname, 'frontend/src/components'));
