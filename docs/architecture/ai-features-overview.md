# AI Features Overview (Phase 7)

This document provides a consolidated reference for all AI features built in the FleetMind application during Phase 7.

| Feature Name | API Endpoint | Prompt Builder | Disclaimer | Degradation Behavior (No Provider) |
|--------------|--------------|----------------|------------|-----------------------------------|
| **Chat Assistant** | `POST /api/v1/ai/conversations/{id}/messages` | Inline in `AiController` | Standard UI notice | Displays standard error banner: "AI Provider is not configured or available." |
| **Voyage Summary** | `GET /api/v1/voyages/{id}/ai-summary` | `VoyageSummaryPromptBuilder.cs` | None | Returns empty/null summary or placeholder, UI hides section or shows "AI Unavailable". |
| **Incident Narrative** | `GET /api/v1/incidents/{id}/ai-narrative` | `IncidentNarrativePromptBuilder.cs` | None | Narrative section shows graceful "AI narrative unavailable" message. |
| **Maintenance Recs** | `GET /api/v1/ships/{id}/ai-maintenance-recommendations` | `MaintenanceRecommendationPromptBuilder.cs` | Standard Maintenance Disclaimer | Graceful fallback, section displays placeholder that AI is unconfigured. |
| **Cargo Risk Analysis**| `GET /api/v1/cargo/{id}/ai-risk-assessment` | `CargoRiskAnalysisPromptBuilder.cs` | **Strong** Safety Clearance Disclaimer | Risk section displays standard unavailable message cleanly. |
| **Analytics Insights** | `GET /api/v1/analytics/ai-insights` | `AnalyticsInsightsPromptBuilder.cs` | None | Narrative block on dashboard shows "AI Insights currently unavailable". |
| **NL Search** | `POST /api/v1/ai/natural-language-search` | `NaturalLanguageSearchPromptBuilder.cs` | None | Search bar rejects query with clear "AI Search is offline" or similar message. |
| **AI Usage Tracking**| `GET /api/v1/ai/usage-report` (Admin) | N/A (Decorator Pattern) | None | Usage table still functions (returns 0 usage or empty rows). |

*Note: All features are protected by `RateLimitedLoggingAiProviderDecorator`, which enforces strict hourly usage quotas universally.*
