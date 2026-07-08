const fs = require('fs');
const path = require('path');

function processDir(dir) {
    const files = fs.readdirSync(dir);
    for (const file of files) {
        const fullPath = path.join(dir, file);
        if (fs.statSync(fullPath).isDirectory()) {
            processDir(fullPath);
        } else if (file.endsWith('DetailPage.tsx')) {
            processFile(fullPath);
        }
    }
}

function processFile(filePath) {
    let content = fs.readFileSync(filePath, 'utf-8');
    
    // Replace grid-cols-2 with grid-cols-1 md:grid-cols-2 if not already responsive
    content = content.replace(/(?<!md:)grid-cols-2/g, 'grid-cols-1 md:grid-cols-2');
    
    // Replace grid-cols-3 with grid-cols-1 md:grid-cols-3
    content = content.replace(/(?<!md:)grid-cols-3/g, 'grid-cols-1 md:grid-cols-3');
    
    // Replace grid-cols-4 with grid-cols-1 md:grid-cols-4
    content = content.replace(/(?<!md:)grid-cols-4/g, 'grid-cols-1 md:grid-cols-2 lg:grid-cols-4');
    
    // Sometimes flex-row is used for side-by-side. 
    // Wait, replacing all flex-row might break things (e.g. small icon alignments). 
    // Let's stick to grid-cols which are explicitly mentioned and usually represent large page sections.

    // Let's also check for w-1/2 or similar side-by-side layouts, but grid is the primary issue.
    
    fs.writeFileSync(filePath, content, 'utf-8');
    console.log(`Processed ${filePath}`);
}

processDir(path.join(__dirname, 'frontend/src/pages'));
