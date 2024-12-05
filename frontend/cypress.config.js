import { defineConfig } from 'cypress';

export default defineConfig({
  fixturesFolder: "cypress/fixtures",
  e2e: {
    baseUrl: "http://localhost:5173",  // Ensure this matches your app's URL
    setupNodeEvents(on, config) {
      // implement node event listeners here
    },
  },
});
