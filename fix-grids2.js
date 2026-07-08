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
    
    // Fix CargoDetailPage / ShipDetailPage
    content = content.replace(/grid-cols-1 lg:grid-cols-1 md:grid-cols-3/g, 'grid-cols-1 lg:grid-cols-3');
    
    // Fix ContainerDetailPage / VoyageDetailPage
    content = content.replace(/sm:grid-cols-1 md:grid-cols-3/g, 'grid-cols-1 sm:grid-cols-3');
    
    // Fix FleetDetailPage
    content = content.replace(/grid-cols-1 md:grid-cols-2 lg:grid-cols-1\s*\n*\s*md:grid-cols-2 lg:grid-cols-4/g, 'grid-cols-1 md:grid-cols-2 lg:grid-cols-4');
    
    // Fix MaintenanceDetailPage
    content = content.replace(/sm:grid-cols-1 md:grid-cols-2/g, 'grid-cols-1 sm:grid-cols-2');
    
    fs.writeFileSync(filePath, content, 'utf-8');
    console.log(`Processed ${filePath}`);
}

processDir(path.join(__dirname, 'frontend/src/pages'));
