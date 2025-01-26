describe('Log in Page', () => {
    beforeEach(() => {
        cy.visit('/login'); // Visit the login page
    });

    it('should render the login page correctly', () => {
        // Check for header
        cy.get('h1').contains('Login');
        // Check for login button
        cy.get('button').contains('Login');
        // Check for the user type dropdown
        cy.get('select#userType').should('exist');
        // Check for email and password input fields
        cy.get('input#user').should('exist');
        cy.get('input#password').should('exist');
    });

    it('should show an error when required fields are missing', () => {
        // Click the login button without filling fields
        cy.get('button').contains('Login').click();
        // Check for error message
        cy.get('p').contains('Vul een emailadres en wachtwoord in').should('be.visible');
    });

    it('should show an error when an invalid account or password is entered', () => {
        cy.intercept('POST', '**/api/Login/login', {
            statusCode: 401, // Unauthorized
        }).as('loginRequest');
        // Fill in invalid credentials
        cy.get('input#user').type('invalid@test.nl');
        cy.get('input#password').type('wrongpassword');

        // Click the login button
        cy.get('button').contains('Login').click();

        // Wait for the mocked API request
        cy.wait('@loginRequest');

        // Check for error message
        cy.get('p').contains('invalid@test.nl is geen geldig account of het wachtwoord klopt niet').should('be.visible');
    });

    it('should show an error when too many failed login attempts occur', () => {
        cy.intercept('POST', '**/api/Login/login', {
            statusCode: 401, // Unauthorized
        }).as('loginRequest');
        
        // Simulate failed login attempts
        for (let i = 0; i < 10; i++) {
            cy.get('input#user').type('test@test.nl');
            cy.get('input#password').type('wrongpassword');
            cy.get('button').contains('Login').click();
        }
        // Reload the page and attempt login again
        cy.reload();
        cy.get('button').contains('Login').click();
        // Check for block message
        cy.get('p').contains('Teveel mislukte inlogpogingen. Probeer het over 5 minuten opnieuw.').should('be.visible');
    });

    it('should successfully log in as a Customer', () => {
        cy.intercept('POST', '**/api/Login/login', {
            statusCode: 401, // Unauthorized
        }).as('loginRequest');

        cy.intercept('GET', '**/api/Login/CheckSession', {
            statusCode: 401, // Unauthorized
        }).as('loginRequest');
        
        // Fill in valid customer credentials
        cy.get('input#user').type('customer@test.nl');
        cy.get('input#password').type('customerpassword');
        // Intercept login API call
        cy.intercept('POST', '**/api/Login/login', {
            statusCode: 200,
        }).as('loginRequest');
        // Click the login button
        cy.get('button').contains('Login').click();
        // Wait for login API call
        cy.wait('@loginRequest');
        // Check if navigation occurred
        cy.url().should('eq', Cypress.config('baseUrl') + '/');
    });

    it('should successfully log in as an Employee and navigate to the correct page', () => {
        cy.intercept('POST', '**/api/Login/login', {
            statusCode: 401, // Unauthorized
        }).as('loginRequest');

        cy.intercept('GET', '**/api/Login/CheckSessionStaff', {
            statusCode: 401, // Unauthorized
        }).as('loginRequest');
        
        // Select Employee user type
        cy.get('select#userType').select('Employee');
        // Fill in valid employee credentials
        cy.get('input#user').type('employee@test.nl');
        cy.get('input#password').type('employeepassword');
        // Mock API responses for Employee login
        cy.intercept('POST', '**/api/Login/login', { statusCode: 200 }).as('loginRequest');
        cy.intercept('GET', '**/api/Cookie/GetKindEmployee', { statusCode: 200, body: { officeType: 'Back' } }).as('officeRequest');
        // Click the login button
        cy.get('button').contains('Login').click();
        // Wait for API calls
        cy.wait('@loginRequest');
        cy.wait('@officeRequest');
        // Check if navigation occurred
        cy.url().should('eq', Cypress.config('baseUrl') + '/BackOfficeEmployee');
    });

    it('should successfully log in as a Vehicle Manager', () => {
        cy.intercept('POST', '**/api/Login/login', {
            statusCode: 401, // Unauthorized
        }).as('loginRequest');
        
        // Select Vehicle Manager user type
        cy.get('select#userType').select('VehicleManager');
        // Fill in valid vehicle manager credentials
        cy.get('input#user').type('manager@test.nl');
        cy.get('input#password').type('managerpassword');
        // Mock API responses for Vehicle Manager login
        cy.intercept('POST', '**/api/Login/login', { statusCode: 200 }).as('loginRequest');
        cy.intercept('GET', '**/api/Login/CheckSessionVehicleManager', { statusCode: 200 }).as('vehicleManagerRequest');
        // Click the login button
        cy.get('button').contains('Login').click();
        // Wait for API calls
        cy.wait('@loginRequest');
        cy.wait('@vehicleManagerRequest');
        // Check if navigation occurred
        cy.url().should('eq', Cypress.config('baseUrl') + '/VehicleManager');
    });
});
