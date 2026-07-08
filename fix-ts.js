const fs = require('fs');
const path = require('path');

function replaceInFile(filePath, searchRegex, replaceText) {
    let content = fs.readFileSync(filePath, 'utf-8');
    const original = content;
    content = content.replace(searchRegex, replaceText);
    if (content !== original) {
        fs.writeFileSync(filePath, content, 'utf-8');
        console.log(`Updated ${filePath}`);
    }
}

// Fix 'amber' -> 'yellow'
const widgetsPath = path.join(__dirname, 'frontend/src/components/dashboard/OpenIncidentsWidget.tsx');
replaceInFile(widgetsPath, /'amber'/g, "'yellow'");

const incidentsTablePath = path.join(__dirname, 'frontend/src/components/incidents/IncidentsTable.tsx');
replaceInFile(incidentsTablePath, /'amber'/g, "'yellow'");

const incidentDetailPath = path.join(__dirname, 'frontend/src/pages/incidents/IncidentDetailPage.tsx');
replaceInFile(incidentDetailPath, /'amber'/g, "'yellow'");

// Fix CrewDetailPage isUploading
const crewDetailPath = path.join(__dirname, 'frontend/src/pages/crew/CrewDetailPage.tsx');
replaceInFile(crewDetailPath, /isUploading/g, 'isLoading');

// Fix DocumentDetailPage UploadVersionModalProps
const uploadVersionModalPath = path.join(__dirname, 'frontend/src/components/documents/UploadVersionModal.tsx');
if (fs.existsSync(uploadVersionModalPath)) {
    replaceInFile(uploadVersionModalPath, /isUploading\?: boolean;/, 'isLoading?: boolean;');
    replaceInFile(uploadVersionModalPath, /isUploading = false/, 'isLoading = false');
    replaceInFile(uploadVersionModalPath, /isUploading\}/, 'isLoading}');
    replaceInFile(uploadVersionModalPath, /isUploading=\{isUploading\}/, 'isLoading={isLoading}');
}

// Fix MaintenanceDetailPage mode
const maintenanceDetailPath = path.join(__dirname, 'frontend/src/pages/maintenance/MaintenanceDetailPage.tsx');
replaceInFile(maintenanceDetailPath, /mode="edit"/g, '');

