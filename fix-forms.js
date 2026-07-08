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
    
    // Process input, select, textarea with {...register('fieldName')}
    content = content.replace(/<(input|select|textarea)([^>]*?)\{\.\.\.register\('([^']+)'\)\}([^>]*?)>/g, (match, tag, before, fieldName, after) => {
        if (match.includes('aria-invalid')) return match; // skip if already processed
        return `<${tag}${before}{...register('${fieldName}')} aria-invalid={!!(errors as any)?.${fieldName}} aria-describedby={(errors as any)?.${fieldName} ? '${fieldName}-error' : undefined}${after}>`;
    });

    // Process error <p> tags
    // e.g. {errors.name && <p className="...">
    // or {errors?.name?.message && <p ...>
    // or {(errors as any).firstName && <p ...>
    // We want to extract the field name and add id="fieldName-error" to <p>
    content = content.replace(/\{([^}&]+)&&\s*<p([^>]*)>/g, (match, condition, pAttrs) => {
        if (pAttrs.includes('id=')) return match; // skip if already processed
        
        // condition is like `errors.name ` or `errors?.name?.message ` or `(errors as any).firstName ` or `errors && (errors as any).firstName `
        let fieldName = null;
        
        // try to find the actual field name after `errors.` or `errors?.` or `(errors as any).`
        const matchField = condition.match(/(?:errors|errors as any|errors as any\)|errors\))(?:\?\.|\.)([a-zA-Z0-9_]+)/);
        if (matchField && matchField[1]) {
            fieldName = matchField[1];
        }

        if (fieldName) {
            return `{${condition}&& <p id="${fieldName}-error"${pAttrs}>`;
        }
        return match;
    });

    fs.writeFileSync(filePath, content, 'utf-8');
    console.log(`Processed ${filePath}`);
}

processDir(path.join(__dirname, 'frontend/src/components'));
