const fs = require('fs');
const path = require('path');

const statsPath = path.join(__dirname, 'frontend/dist/stats.json');
const stats = JSON.parse(fs.readFileSync(statsPath, 'utf8'));

// The rollup-plugin-visualizer raw-data format is a tree.
// At the root, children are the output chunks.
const root = stats;

// Find index.js chunk
let indexChunk = null;
for (const child of root.children) {
    if (child.name && child.name.includes('index-')) {
        indexChunk = child;
        break;
    }
}

if (indexChunk) {
    console.log(`Found index chunk: ${indexChunk.name} (size: ${indexChunk.size})`);
    // List top 10 modules in index chunk
    let modules = [];
    
    function collectModules(node) {
        if (!node.children || node.children.length === 0) {
            modules.push(node);
        } else {
            for (const c of node.children) {
                collectModules(c);
            }
        }
    }
    
    collectModules(indexChunk);
    
    modules.sort((a, b) => b.size - a.size);
    
    console.log("Top 20 largest modules in index.js:");
    for (let i = 0; i < Math.min(20, modules.length); i++) {
        console.log(`- ${modules[i].name} : ${(modules[i].size / 1024).toFixed(2)} KB`);
    }
} else {
    console.log("Could not find index chunk.");
}
