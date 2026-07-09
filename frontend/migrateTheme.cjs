const fs = require('fs');
const path = require('path');

const srcDir = path.join(__dirname, 'src');

function walk(dir, callback) {
    fs.readdirSync(dir).forEach(f => {
        let dirPath = path.join(dir, f);
        let isDirectory = fs.statSync(dirPath).isDirectory();
        if (isDirectory) {
            walk(dirPath, callback);
        } else if (f.endsWith('.tsx') || f.endsWith('.ts')) {
            callback(dirPath);
        }
    });
}

const mapClasses = {
    'bg-slate-900': 'bg-background',
    'bg-slate-800': 'bg-surface',
    'bg-slate-700/50': 'bg-surface-hover',
    'hover:bg-slate-700/50': 'hover:bg-surface-hover',
    'bg-slate-700': 'bg-surface-hover',
    'hover:bg-slate-700': 'hover:bg-surface-hover',
    'border-slate-700': 'border-border',
    'border-slate-800': 'border-border',
    'border-slate-600': 'border-border',
    'ring-slate-700': 'ring-border',
    'divide-slate-700': 'divide-border',
    'text-slate-400': 'text-text-muted',
    'hover:text-slate-400': 'hover:text-text-muted',
    'text-slate-500': 'text-text-muted',
    'text-slate-300': 'text-text-primary',
    'hover:text-slate-300': 'hover:text-text-primary',
    'text-slate-200': 'text-text-primary',
    'hover:text-slate-200': 'hover:text-text-primary',
    'bg-white/5': 'bg-surface-hover',
    'hover:bg-white/10': 'hover:bg-surface-hover',
    'ring-white/10': 'ring-border',
    'bg-gray-900': 'bg-surface',
    'text-gray-400': 'text-text-muted'
};

const badgeClasses = [
    'bg-indigo-600', 'bg-indigo-500', 'bg-red-600', 'bg-red-500', 'bg-emerald-600', 'bg-emerald-500',
    'bg-blue-600', 'bg-blue-500', 'bg-yellow-600', 'bg-yellow-500', 'bg-amber-600', 'bg-amber-500'
];

walk(srcDir, (filePath) => {
    let content = fs.readFileSync(filePath, 'utf-8');
    let original = content;

    // We will do a generic replace on className strings.
    // A bit naive but effective for our needs.
    content = content.replace(/className=["']([^"']+)["']/g, (match, classStr) => {
        let classes = classStr.split(/\s+/);
        let hasSolidBg = classes.some(c => badgeClasses.includes(c) || c.startsWith('bg-indigo') || c.startsWith('bg-red') || c.startsWith('bg-emerald') || c.startsWith('bg-amber'));
        
        let newClasses = classes.map(c => {
            if (mapClasses[c]) {
                return mapClasses[c];
            }
            if (c === 'text-white' && !hasSolidBg) {
                return 'text-text-primary';
            }
            if (c === 'hover:text-white' && !hasSolidBg) {
                return 'hover:text-text-primary';
            }
            return c;
        });
        
        return match.replace(classStr, newClasses.join(' '));
    });

    // Also handle className={`...`} template literals (more complex, but we'll just do a global replace for safe classes)
    content = content.replace(/className=\{`([^`]+)`\}/g, (match, classStr) => {
        let classes = classStr.split(/\s+/);
        let hasSolidBg = classes.some(c => badgeClasses.includes(c) || c.startsWith('bg-indigo-') || c.startsWith('bg-red-') || c.startsWith('bg-emerald-'));
        
        let newClasses = classes.map(c => {
            if (mapClasses[c]) {
                return mapClasses[c];
            }
            // Be careful with template logic like ${isActive ? 'text-white' : 'text-slate-400'}
            // Actually this simplistic approach won't parse JS logic inside `${...}` perfectly, 
            // but for standard classes it works.
            if (c === 'text-white' && !hasSolidBg) {
                return 'text-text-primary';
            }
            return c;
        });
        return match.replace(classStr, newClasses.join(' '));
    });

    // Global replace for leftover classes not in simple strings (e.g., inside ternaries inside className={})
    Object.keys(mapClasses).forEach(k => {
        // match class surrounded by whitespace, quotes, or backticks
        let regex = new RegExp(`(?<=['"\`\\s])${k.replace(/\//g, '\\/')}(?=['"\`\\s])`, 'g');
        content = content.replace(regex, mapClasses[k]);
    });

    if (content !== original) {
        fs.writeFileSync(filePath, content, 'utf-8');
        console.log(`Updated ${filePath}`);
    }
});
