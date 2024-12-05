describe('Sign Up Page', () => {

    beforeEach(() => {
        // Set up the test environment (clear any session, etc.)
        cy.visit('/signUp');  // Adjust the URL as needed (assuming /signup is the route for the sign-up page)
    });

    it('should render sign-up page correctly', () => {
        cy.get('h1').contains('Aanmelden'); // Check for page title
        cy.get('button').contains('Particulier'); // Check for "Particulier" button
        cy.get('button').contains('Zakelijk'); // Check for "Zakelijk" button
    });

    it('should toggle between account types', () => {
        // Test toggling between account types
        cy.get('button').contains('Particulier').click(); // Click "Particulier"
        cy.get('input#firstName').should('exist'); // Should display personal account fields
        cy.get('input#kvk').should('not.exist'); // Should not show KVK field for "Particulier"

        cy.get('button').contains('Zakelijk').click(); // Click "Zakelijk"
        cy.get('input#kvk').should('exist'); // Should show KVK field for "Zakelijk"
        cy.get('input#dateOfBirth').should('not.exist'); // Should not show date of birth field for "Zakelijk"
    });

    it('should show an error when required fields are missing', () => {
        cy.get('button').contains('Particulier').click(); // Click "Particulier"

        // Click the "Maak Account" button without filling the form
        cy.get('button').contains('Maak Account').click();

        // Check if the validation error appears
        cy.get('p').contains('Bepaalde verplichte veld(en) zijn niet ingevuld.');
    });

    it('should show an error when passwords do not match', () => {
        cy.get('button').contains('Particulier').click(); // Click "Particulier"

        // Fill in the form but with mismatched passwords
        cy.get('input#firstName').type('John');
        cy.get('input#lastName').type('Doe');
        cy.get('input#email').type('john.doe@example.com');
        cy.get('input#adres').type('123 Main St');
        cy.get('input#phonenumber').type('1234567890');
        cy.get('input#password').type('password123');
        cy.get('input#passwordConfirm').type('password456');

        // Click "Maak Account"
        cy.get('button').contains('Maak Account').click();

        // Check if password mismatch error appears
        cy.get('p').contains('Wachtwoorden komen niet overeen.');
    });

    it('should submit the form for personal account', () => {
        // Mocking the fetch API for form submission
        cy.intercept('POST', '/api/SignUp/signUpPersonal', {
            statusCode: 200,
            body: { message: "Success" }
        }).as('signupRequest');

        cy.get('button').contains('Particulier').click(); // Click "Particulier"

        // Fill in the form with valid details
        cy.get('input#firstName').type('John');
        cy.get('input#lastName').type('Doe');
        cy.get('input#email').type('john.doe@example.com');
        cy.get('input#adres').type('123 Main St');
        cy.get('input#phonenumber').type('1234567890');
        cy.get('input#dateOfBirth').type('2000-01-01');
        cy.get('input#password').type('password123');
        cy.get('input#passwordConfirm').type('password123');

        // Submit the form
        cy.get('button').contains('Maak Account').click();

        // Wait for the API call to complete and verify the success
        cy.wait('@signupRequest').its('response.statusCode').should('eq', 200);

        // Check if the success navigation happens (you can check URL or some element on the next page)
        cy.url().should('include', '/login');  // Assuming it navigates to the login page
    });

    it('should show an error for KVK field validation', () => {
        cy.get('button').contains('Zakelijk').click(); // Click "Zakelijk"

        // Fill the form with invalid KVK number
        cy.get('input#firstName').type('John');
        cy.get('input#lastName').type('Doe');
        cy.get('input#email').type('john.doe@example.com');
        cy.get('input#adres').type('123 Main St');
        cy.get('input#phonenumber').type('1234567890');
        cy.get('input#kvk').type('1234567'); // Invalid KVK number (7 digits instead of 8)
        cy.get('input#password').type('password123');
        cy.get('input#passwordConfirm').type('password123');

        // Submit the form
        cy.get('button').contains('Maak Account').click();

        // Check if KVK validation error appears
        cy.get('p').contains('KVK number must be 8 digits.');
    });
});

