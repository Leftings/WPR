describe('SignUp Page', () => {
    beforeEach(() => {
        // Intercept the GET request to fetch subscriptions and mock the response
        cy.intercept('GET', '/api/Subscription/GetSubscriptions', {
            statusCode: 200,
            body: {
                data: ['Basic', 'Premium', 'Pro'] // Example subscriptions
            }
        }).as('getSubscriptions');

        cy.intercept('GET', '**/api/Login/CheckSession', {
            statusCode: 401, // Unauthorized
        }).as('loginRequest');

        // Visit the SignUp page
        cy.visit('/signup');

        // Wait for the intercepted request to complete
        cy.wait('@getSubscriptions');
    });

    it('should render the SignUp page correctly', () => {
        // Check for page title
        cy.get('h1').contains('Aanmelden Particulier').should('be.visible');

        // Check for account type buttons
        cy.get('button').contains('Particulier').should('be.visible');
        cy.get('button').contains('Zakelijk').should('be.visible');

        // Check for form fields when "Particulier" is selected
        cy.get('input#firstName').should('exist');
        cy.get('input#lastName').should('exist');
        cy.get('input#email').should('exist');
        cy.get('input#password').should('exist');
        cy.get('input#passwordConfirm').should('exist');
        cy.get('input#inputStreet').should('exist');
        cy.get('input#inputNumber').should('exist');
        cy.get('input#phonenumber').should('exist');
        cy.get('input#dateOfBirth').should('exist');
    });

    it('should switch between "Particulier" and "Zakelijk" account types', () => {
        // Switch to "Zakelijk"
        cy.get('button').contains('Zakelijk').click();
        cy.get('h1').contains('Aanmelden Bedrijf').should('be.visible');

        // Check for fields relevant to "Zakelijk"
        cy.get('input#inputSubscriptionType').should('exist');
        cy.get('input#inputBusinessName').should('exist');
        cy.get('input#inputKvK').should('exist');
        cy.get('input#inputDomain').should('exist');
        cy.get('input#inputContactEmail').should('exist');

        // Switch back to "Particulier"
        cy.get('button').contains('Particulier').click();
        cy.get('h1').contains('Aanmelden Particulier').should('be.visible');
    });

    it('should show error messages for missing fields', () => {
        // Submit form with empty fields (e.g., for "Particulier")
        cy.get('button').contains('Bevestig').click();

        // Check for error messages
        cy.get('.error-message').should('exist');
        cy.get('.error-message').contains('Voornaam is verplicht');
        cy.get('.error-message').contains('Achternaam is verplicht');
        cy.get('.error-message').contains('E-mail is verplicht');
        cy.get('.error-message').contains('Wachtwoord is verplicht');
        cy.get('.error-message').contains('Herhaal wachtwoord is verplicht');
        cy.get('.error-message').contains('Straatnaam is verplicht');
        cy.get('.error-message').contains('Nummer is verplicht');
        cy.get('.error-message').contains('Telefoonnummer is verplicht');
        cy.get('.error-message').contains('Geboortedatum is verplicht');
    });

    it('should validate password confirmation correctly', () => {
        cy.get('input#password').type('password123');
        cy.get('input#passwordConfirm').type('password123');
        cy.get('button').contains('Bevestig').click();

        // Assuming successful signup, check if the user is redirected to login page
        cy.url().should('include', '/login');
    });

    it('should submit the form with valid data', () => {
        cy.intercept('POST', '**/api/SignUp/signUp', {
            statusCode: 401, // Unauthorized
        }).as('signUp');
        
        // Provide valid data for "Particulier"
        cy.get('input#firstName').type('John');
        cy.get('input#lastName').type('Doe');
        cy.get('input#email').type('johndoe@example.com');
        cy.get('input#password').type('password123');
        cy.get('input#passwordConfirm').type('password123');
        cy.get('input#inputStreet').type('Main Street');
        cy.get('input#inputNumber').type('123');
        cy.get('input#phonenumber').type('1234567890');
        cy.get('input#dateOfBirth').type('1990-01-01');

        // Submit the form
        cy.get('button').contains('Bevestig').click();

        // Assuming successful signup, check if the user is redirected to login page
        cy.url().should('include', '/login');
    });
});
