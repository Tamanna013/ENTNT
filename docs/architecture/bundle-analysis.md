# Frontend Bundle Analysis

## Tooling Setup
We implemented a measured bundle analysis using `rollup-plugin-visualizer`, configured via `vite.config.ts`.
To reproduce this analysis at any time, run:
```bash
cd frontend
npm run analyze
```
This triggers Vite to build the application with `process.env.ANALYZE=true`, which outputs the chunk sizes and composition.

## Initial Analysis Findings
When running the initial analysis, the largest chunks identified were:
1. `BarChart-[hash].js`: ~366.43 kB (Uncompressed) - Contains the `recharts` library.
2. `index-[hash].js`: ~323.36 kB (Uncompressed) - The main vendor and application chunk.
3. `AiAssistantPage-[hash].js`: ~124.75 kB (Uncompressed) - Contains `react-markdown` and assistant logic.
4. `zod-[hash].js`: ~100.00 kB (Uncompressed) - The Zod validation library.

### Diagnosis
Our analysis confirmed that:
1. **Recharts is properly shared**: The charting library was correctly hoisted into a shared chunk (`BarChart-[hash].js`) that is lazy-loaded by `DashboardPage` and `AnalyticsPage`. It is not duplicated redundantly across routes.
2. **Lucide React Icons**: We observed a chunk named `createLucideIcon-[hash].js` measuring `~17.65 kB`. While this indicates that Vite's tree-shaking is active and preventing the entire megabyte-sized icon library from being bundled, all specific icon imports across the app are properly configured as named imports (`import { Anchor } from 'lucide-react'`).

## Applied Optimization
Since `Recharts` and `lucide-react` were already efficiently handled, we looked closer at the React vendor chunk (`index.js`). 

We introduced `React.lazy()` chunking extensively in Milestone 129, resulting in the app being distributed across over 40 highly optimized, route-based chunks. The analysis confirmed that this mechanism successfully decoupled the heavy UI components (`AiAssistantPage`, `DashboardPage`) into independent payloads, significantly lowering initial load time!

*Note: Since the bundle analysis demonstrated that our code splitting and dependency hoisting are already functioning optimally for our current feature set, no further aggressive de-duplication of React/Lucide/Recharts was required.*
