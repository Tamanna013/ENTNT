const fs = require('fs');
const path = require('path');

function processDir(dir) {
    const files = fs.readdirSync(dir);
    for (const file of files) {
        const fullPath = path.join(dir, file);
        if (fs.statSync(fullPath).isDirectory()) {
            processDir(fullPath);
        } else if (file.endsWith('.tsx') || file.endsWith('.ts')) {
            processFile(fullPath);
        }
    }
}

function processFile(filePath) {
    let content = fs.readFileSync(filePath, 'utf-8');
    let original = content;

    // We already fixed the FormModal signatures. Now we fix ALL usages.
    content = content.replace(/cargoToEdit=\{/g, 'cargo={');
    content = content.replace(/containerToEdit=\{/g, 'container={');
    
    // For DocumentFormModal, FuelFormModal, MaintenanceFormModal used in Tables/Pages
    content = content.replace(/<DocumentFormModal([^>]+)initialData=\{/g, '<DocumentFormModal$1document={');
    content = content.replace(/<FuelFormModal([^>]+)initialData=\{/g, '<FuelFormModal$1fuelLog={');
    content = content.replace(/<MaintenanceFormModal([^>]+)initialData=\{/g, '<MaintenanceFormModal$1maintenanceRecord={');
    content = content.replace(/<MaintenanceFormModal([^>]+)mode=['"]?(create|edit)['"]?/g, '<MaintenanceFormModal$1');

    if (content !== original) {
        fs.writeFileSync(filePath, content, 'utf-8');
        console.log(`Updated ${filePath}`);
    }
}

processDir(path.join(__dirname, 'frontend/src'));
