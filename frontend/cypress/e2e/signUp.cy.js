describe('SignUp Page', () => {
    const backendUrl = 'http://localhost:5165'; // Adjust based on your environment

    beforeEach(() => {
        
        cy.intercept('GET', '**/api/Login/CheckSession', {
            statusCode: 401, // Unauthorized
        }).as('loginRequest');

        cy.intercept('GET', '**/api/Subscription/GetSubscriptions', {
            statusCode: 401, // Unauthorized
        }).as('subscriptionRequest');
        
        
        cy.visit('/signUp');// Adjust based on your routing setup
    });

    it('should render the sign-up page with the correct form fields', () => {
        // Test that the initial state is correct (Private selected)
        cy.get('h1').should('contain.text', 'Aanmelden Particulier');
        cy.get('input#firstName').should('exist');
        cy.get('input#lastName').should('exist');
        cy.get('input#email').should('exist');
        cy.get('input#password').should('exist');
        cy.get('input#passwordConfirm').should('exist');
        cy.get('input#inputStreet').should('exist');
        cy.get('input#inputNumber').should('exist');
        cy.get('input#inputExtra').should('exist');
        cy.get('input#phonenumber').should('exist');
        cy.get('input#dateOfBirth').should('exist');

        // Check if Business is correctly hidden
        cy.get('label[for="inputSubscriptionType"]').should('not.exist');
    });

    it('should show validation errors for missing required fields (Private)', () => {
        cy.get('button.cta-button').contains('Bevestig').click();

        // Test for error messages when no fields are filled
        cy.get('.error-message').should('exist');
        cy.get('.error-message li').should('contain.text', 'firstName is niet ingevuld');
        cy.get('.error-message li').should('contain.text', 'lastName is niet ingevuld');
        cy.get('.error-message li').should('contain.text', 'password1 is niet ingevuld');
        cy.get('.error-message li').should('contain.text', 'password2 is niet ingevuld');
        cy.get('.error-message li').should('contain.text', 'street is niet ingevuld');
        cy.get('.error-message li').should('contain.text', 'number is niet ingevuld');
        cy.get('.error-message li').should('contain.text', 'phonenumber is niet ingevuld');
        cy.get('.error-message li').should('contain.text', 'dateOfBirth is niet ingevuld');
    });

    it('should switch to business form when Business account type is selected and employee is selected', () => {
        cy.get('button.cta-button').contains('Zakelijk').click();

        // Verify Business form fields
        cy.get('h1').should('contain.text', 'Aanmelden Bedrijf');
        cy.get('input#inputEmailBusiness').should('exist');
        cy.get('input#inputPasswordBusiness1').should('exist');
        cy.get('input#inputPasswordBusiness2').should('exist');;
    });

    it('should show validation errors for missing required fields (Business)', () => {
        cy.get('button.cta-button').contains('Zakelijk').click();

        // Simulate the business account form submission
        cy.get('button.cta-button').contains('Bevestig').click();

        // Test for error messages when required fields are missing
        cy.get('.error-message').should('exist');
        cy.get('.error-message li').should('contain.text', 'email is niet ingevuld');
        cy.get('.error-message li').should('contain.text', 'password1 is niet ingevuld');
        cy.get('.error-message li').should('contain.text', 'password2 is niet ingevuld');
    });

    it('should submit the form successfully for a Private account', () => {
        cy.intercept('POST', '**/api/SignUp/signUp', {
            statusCode: 200, // Successful
        }).as('signupRequest');
        
        cy.get('input#firstName').type('John');
        cy.get('input#lastName').type('Doe');
        cy.get('input#email').type('john.doe@example.com');
        cy.get('input#password').type('Password-123');
        cy.get('input#passwordConfirm').type('Password-123');
        cy.get('input#inputStreet').type('Main St');
        cy.get('input#inputNumber').type('123');
        cy.get('input#inputExtra').type('Apt 4B');
        cy.get('input#phonenumber').type('0656756756');
        cy.get('input#dateOfBirth').type('1990-01-01');


        cy.get('button.cta-button').contains('Bevestig').click();

        // Wait for the request and verify the successful redirection
        cy.wait('@signupRequest').its('response.statusCode').should('eq', 200);
        cy.url().should('include', '/login');
    });
    
    
    

    // it('should submit the form successfully for a Business account', () => {
    //     cy.intercept('POST', '**/api/AddBusiness/addBusiness', {
    //         statusCode: 200, // Successful
    //     }).as('businessSignupRequest');
    //
    //     // Mock the GET subscriptions response to return some options
    //     cy.intercept('GET', '**/api/Subscription/GetSubscriptions', {
    //         statusCode: 200,
    //         body: {
    //             data: ['Standard', 'Premium', 'Business'], // Mocked subscription options
    //         },
    //     }).as('getSubscriptionsRequest');
    //
    //     // Trigger business signup process
    //     cy.get('button.cta-button').contains('Zakelijk').click();
    //     cy.get('button.cta-button').contains('Bedrijf').click();
    //
    //     // Wait for subscriptions to be fetched
    //     cy.wait('@getSubscriptionsRequest');
    //
    //     // Ensure the subscription dropdown is populated
    //     cy.get('select#inputSubscriptionType').should('have.length', 3);  // Check if there are 3 options
    //
    //     // Select 'Standard' subscription
    //     cy.get('select#inputSubscriptionType').select('Standard');
    //
    //     // Continue filling in the form for the business account
    //     cy.get('input#inputBusinessName').type('My Business');
    //     cy.get('input#inputKvK').type('12345678');
    //     cy.get('input#inputDomain').type('mybusiness.com');
    //     cy.get('input#inputContactEmail').type('contact@mybusiness.com');
    //     cy.get('input#inputStreet').type('Business St');
    //     cy.get('input#inputNumber').type('101');
    //     cy.get('input#inputExtra').type('Suite 100');
    //
    //     // Submit the form
    //     cy.get('button.cta-button').contains('Bevestig').click();
    //
    //     // Wait for the request and verify the successful redirection
    //     cy.wait('@businessSignupRequest').its('response.statusCode').should('eq', 200);
    //     cy.url().should('include', '/');
    // });
    

    it('should show error message for invalid email format', () => {
        cy.intercept('POST', '**/api/SignUp/signUp', {
            statusCode: 400, // Successful
        }).as('signupRequest');
        
        cy.get('input#firstName').type('John');
        cy.get('input#lastName').type('Doe');
        cy.get('input#email').type('john.doe');
        cy.get('input#password').type('Password-123');
        cy.get('input#passwordConfirm').type('Password-123');
        cy.get('input#inputStreet').type('Main St');
        cy.get('input#inputNumber').type('123');
        cy.get('input#inputExtra').type('Apt 4B');
        cy.get('input#phonenumber').type('0656756756');
        cy.get('input#dateOfBirth').type('1990-01-01');
        cy.get('button.cta-button').contains('Bevestig').click();

        // Check if the error message appears
        cy.get('.error-message').should('exist');
        cy.get('.error-message li').should('contain.text', 'E-mail');
        cy.wait('@signupRequest').its('response.statusCode').should('eq', 400);
    });

    it('should show error for short KvK number in Business account', () => {
        cy.get('button.cta-button').contains('Zakelijk').click();
        cy.get('input#inputKvK').type('123');
        cy.get('button.cta-button').contains('Bevestig').click();

        // Check if the error message for KvK is shown
        cy.get('.error-message').should('exist');
        cy.get('.error-message li').should('contain.text', 'Te kort KvK nummer');
    });
});
