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

    // Fix FileUpload prop usage
    content = content.replace(/isUploading=\{/g, 'isLoading={');
    content = content.replace(/isUploading /g, 'isLoading ');

    // Standardize 'isSubmitting' to 'isLoading' in FormModals
    // Note: React Hook Form uses 'isSubmitting' internally inside 'formState'. 
    // We only want to standardize the prop on FormModal components themselves.
    if (filePath.endsWith('FormModal.tsx')) {
        content = content.replace(/isSubmitting/g, 'isLoading');
        
        // Standardize edit prop names
        if (filePath.endsWith('CargoFormModal.tsx')) {
            content = content.replace(/cargoToEdit/g, 'cargo');
        } else if (filePath.endsWith('ContainerFormModal.tsx')) {
            content = content.replace(/containerToEdit/g, 'container');
        } else if (filePath.endsWith('DocumentFormModal.tsx')) {
            content = content.replace(/initialData/g, 'document');
        } else if (filePath.endsWith('FuelFormModal.tsx')) {
            content = content.replace(/initialData/g, 'fuelLog');
        } else if (filePath.endsWith('MaintenanceFormModal.tsx')) {
            content = content.replace(/initialData/g, 'maintenanceRecord');
            // Remove 'mode' prop logic
            content = content.replace(/mode\s*:\s*'create'\s*\|\s*'edit';?/, '');
            content = content.replace(/mode(,?)/g, ''); // remove from destructuring
            content = content.replace(/mode === 'edit'/g, '!!maintenanceRecord');
            content = content.replace(/mode === 'create'/g, '!maintenanceRecord');
            content = content.replace(/isEditMode = (.*);/, 'isEditMode = !!maintenanceRecord;');
        }
    }

    // Now if the prop names changed in the FormModal, we have to fix where they are CALLED (i.e., the Pages).
    if (filePath.endsWith('Page.tsx')) {
        content = content.replace(/isSubmitting=\{/g, 'isLoading={');
        
        content = content.replace(/cargoToEdit=\{/g, 'cargo={');
        content = content.replace(/containerToEdit=\{/g, 'container={');
        
        if (filePath.endsWith('DocumentsPage.tsx')) {
            content = content.replace(/initialData=\{/g, 'document={');
        } else if (filePath.endsWith('FuelPage.tsx')) {
            content = content.replace(/initialData=\{/g, 'fuelLog={');
        } else if (filePath.endsWith('MaintenancePage.tsx') || filePath.endsWith('ShipDetailPage.tsx')) {
            content = content.replace(/initialData=\{/g, 'maintenanceRecord={');
            // Remove mode prop
            content = content.replace(/mode=['"]edit['"]/g, '');
            content = content.replace(/mode=['"]create['"]/g, '');
            content = content.replace(/mode=\{[^}]+\}/g, '');
        }
    }

    if (content !== original) {
        fs.writeFileSync(filePath, content, 'utf-8');
        console.log(`Updated ${filePath}`);
    }
}

processDir(path.join(__dirname, 'frontend/src'));
