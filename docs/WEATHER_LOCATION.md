# Weather location and forecast

In Settings, open Weather location. Use Search cities to find a real city or municipality and select Use this location beside the correct result. Or choose Use my current location and grant browser location permission when prompted. The overview automatically uses that selection.

The weather card shows current temperature, conditions, feels-like temperature, humidity, wind, and a three-day forecast with highs/lows and rain probability when available. It refreshes every 15 minutes while the page is open/visible and when returning to a stale page. Refresh requests a new update immediately. Updated shows when the provider response was fetched. Open-Meteo current conditions are model-based, not second-by-second readings from your device.

The selection is saved per account on this browser. It is not a global campus location and is not synchronized to another device. Reset to Naga City restores the original default. Logging out and back in retains that account's selection. Different accounts use different keys.

Device location is requested only after the user presses the button. There is no background GPS tracking. Latitude/longitude are rounded to two decimal places before saving and transmission. Approximate coordinates go to the application's authenticated API and Open-Meteo to retrieve weather; they are not added to the application's database. City search sends the typed city name to Open-Meteo's geocoding service. Browser permission denied, unavailable position or timeout leaves the existing selection unchanged and offers city search. Device geolocation requires a secure context (HTTPS or localhost). Device position may be approximate depending on browser/OS permissions; press the button again after moving if a new location is needed.

API: authenticated GET /api/weather accepts optional latitude and longitude (both or neither). Missing/invalid pairs, non-finite values and out-of-range coordinates return 400 before any upstream call. GET /api/weather/locations?query=... searches up to five places. Hostnames and query structure are fixed on the server. Responses are validated before use and upstream failure/timeout returns a safe unavailable response. Existing report/database functions do not wait for weather. No schema migration is required.

Manual verification: choose another city, confirm the forecast updates in Settings and on Overview, reload to confirm persistence, try Refresh, verify dark mode, and reset to Naga City. For device location, explicitly approve your own browser prompt; test denied permission by choosing Block and verify city search remains available. Tests must not silently grant or transmit a real person's location.

Sources: https://open-meteo.com/en/docs and https://open-meteo.com/en/docs/geocoding-api . Current weather variables, WMO condition labels, daily forecast variables and city-search parameters follow the provider's documentation.
