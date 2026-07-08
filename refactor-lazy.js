const fs = require('fs');
const path = require('path');

const routerPath = path.join(__dirname, 'frontend/src/routes/AppRouter.tsx');
let content = fs.readFileSync(routerPath, 'utf8');

// Find all imports from '../pages/...'
const importRegex = /import\s+\{\s*([a-zA-Z0-9_]+)\s*\}\s+from\s+['"](\.\.\/pages\/[^'"]+)['"];/g;

// We need to keep React import if it's missing, but it's probably missing.
if (!content.includes("import React")) {
    content = "import React, { Suspense } from 'react';\n" + content;
} else if (!content.includes("Suspense")) {
    content = content.replace("import React", "import React, { Suspense }");
}

let newImports = [];
content = content.replace(importRegex, (match, componentName, importPath) => {
    return `const ${componentName} = React.lazy(() => import('${importPath}').then(m => ({ default: m.${componentName} })));`;
});

fs.writeFileSync(routerPath, content, 'utf8');
console.log("Refactoring complete");
