import { defineConfig } from 'cypress';

export default defineConfig({
  fixturesFolder: "cypress/fixtures",
  e2e: {
    baseUrl: "http://localhost:5173",
    viewportWidth: 1280,
    viewportHeight: 1024,
    setupNodeEvents(on, config) {
    },
  },
});
