describe('Sign Up Page', () => {

    beforeEach(() => {
        cy.visit('/signUp');  // URL om te testen
    });

    it('should render sign-up page correctly', () => {
        cy.get('h1').contains('Aanmelden');
        cy.get('button').contains('Particulier');
        cy.get('button').contains('Zakelijk');
    });

    it('should toggle between account types', () => {
        // Test toggle tussen account types
        cy.get('button').contains('Particulier').click(); 
        cy.get('input#dateOfBirth').should('exist'); // SDate of birth veld voor particuliet moet bestaan
        cy.get('input#kvk').should('not.exist'); // KVK veld voor particulier moet niet bestaan

        cy.get('button').contains('Zakelijk').click(); 
        cy.get('input#kvk').should('exist'); // KVK veld voor zakelijk account moet bestaan
        cy.get('input#dateOfBirth').should('not.exist'); // Date of birth veld voor zakelijk moet niet bestaan
    });

    it('should show an error when required fields are missing', () => {
        cy.get('button').contains('Particulier').click();
        cy.get('button').contains('Maak Account').click();
        cy.get('p').contains('Bepaalde verplichte veld(en) zijn niet ingevuld.');
    });

    it('should show an error when date of birth is too young or too old', () => {
        cy.get('button').contains('Particulier').click();

        cy.get('input#firstName').type('John');
        cy.get('input#lastName').type('Doe');
        cy.get('input#email').type('john.doe@example.com');
        cy.get('input#adres').type('123 Main St');
        cy.get('input#dateOfBirth').type('2020-01-01'); // DoB is te jong
        cy.get('input#phonenumber').type('1234567890');
        cy.get('input#password').type('password123');
        cy.get('input#passwordConfirm').type('password123');

        cy.get('button').contains('Maak Account').click();
        cy.get('p').contains('The birthday is invalid.'); // kijk voor error
        
        cy.get('input#dateOfBirth').clear().type('1010-01-01');
        cy.get('p').contains('The birthday is invalid.'); // kijk voor error
    });


    it('should show an error when passwords do not match', () => {
        cy.get('button').contains('Particulier').click();

        cy.get('input#firstName').type('John');
        cy.get('input#lastName').type('Doe');
        cy.get('input#email').type('john.doe@example.com');
        cy.get('input#adres').type('123 Main St');
        cy.get('input#dateOfBirth').type('2000-01-01');
        cy.get('input#phonenumber').type('1234567890');
        cy.get('input#password').type('password123');
        cy.get('input#passwordConfirm').type('password456'); // mismatch password test

        cy.get('button').contains('Maak Account').click();
        cy.get('p').contains('Wachtwoorden komen niet overeen.'); // kijk voor error
    });

    it('should show an error when password does not meet the complexity requirements', () => {
        cy.get('button').contains('Particulier').click();

        // Testing passwords that do not meet the complexity requirements
        cy.get('input#firstName').type('John');
        cy.get('input#lastName').type('Doe');
        cy.get('input#email').type('john.doe@example.com');
        cy.get('input#adres').type('123 Main St');
        cy.get('input#dateOfBirth').type('2000-01-01');
        cy.get('input#phonenumber').type('0623023057');
        cy.get('input#password').type('short'); // Invalid password (too short)
        cy.get('input#passwordConfirm').type('short');

        cy.get('button').contains('Maak Account').click();
        cy.get('p').contains('Password must be at least 8 characters.');

        cy.get('input#password').clear().type('password'); // Password without number
        cy.get('input#passwordConfirm').clear().type('password');

        cy.get('button').contains('Maak Account').click();
        cy.get('p').contains('Password must contain at least one number.');

        cy.get('input#password').clear().type('password1'); // Password without uppercase
        cy.get('input#passwordConfirm').clear().type('password1');

        cy.get('button').contains('Maak Account').click();
        cy.get('p').contains('Password must contain at least one upper case letter.');

        cy.get('input#password').clear().type('Password1'); // Password without symbol
        cy.get('input#passwordConfirm').clear().type('Password1');

        cy.get('button').contains('Maak Account').click();
        cy.get('p').contains('Password must contain at least one symbol');
    });
    
    it('should show an error when phone number is not the correct format', () => {
        cy.get('button').contains('Particulier').click();

        cy.get('input#firstName').type('John');
        cy.get('input#lastName').type('Doe');
        cy.get('input#email').type('john.doe@example.com');
        cy.get('input#adres').type('123 Main St');
        cy.get('input#dateOfBirth').type('2000-01-01');
        cy.get('input#phonenumber').type('1234567890'); // invalid phone number, does not start with 06
        cy.get('input#password').type('Wachtwoord-01');
        cy.get('input#passwordConfirm').type('Wachtwoord-01');

        cy.get('button').contains('Maak Account').click();
        cy.get('p').contains('The phone number is invalid.');
        
        cy.get('input#phonenumber').clear().type('06230230577'); // invalid phone number, begint wel met 06 maar heeft meer dan 10 cijfers
        cy.get('p').contains('The phone number is invalid.');
        
        
        cy.get('button').contains('Zakelijk').click();
        
        cy.get('input#phonenumber').clear().type('1234567890'); // invalid phone number, does not start with 06
        cy.get('input#kvk').type('88888888');
        cy.get('button').contains('Maak Account').click();
        
        cy.get('p').contains('The phone number is invalid.');

        cy.get('input#phonenumber').clear().type('06230230577'); // invalid phone number, begint wel met 06 maar heeft meer dan 10 cijfers
        cy.get('p').contains('The phone number is invalid.');

    })
    
    
    it('should submit the form for personal account', () => {
        // Mocking Fetch API
        cy.intercept('POST', '/api/SignUp/signUpPersonal', {
            statusCode: 200,
            body: { message: "Success" }
        }).as('signupRequest');

        cy.get('button').contains('Particulier').click();

        // Correct ingevulde formulier
        cy.get('input#firstName').type('John');
        cy.get('input#lastName').type('Doe');
        cy.get('input#email').type('john.doe@example.com');
        cy.get('input#adres').type('123 Main St');
        cy.get('input#phonenumber').type('1234567890');
        cy.get('input#dateOfBirth').type('2000-01-01');
        cy.get('input#password').type('password123');
        cy.get('input#passwordConfirm').type('password123');

        // Maak account met correcte gegevens
        cy.get('button').contains('Maak Account').click();

        // Wacht op response van de API call
        cy.wait('@signupRequest').its('response.statusCode').should('eq', 200);

        cy.url().should('include', '/login');  // kijk of die correct wordt doorgestuurd naar de juiste pagina
    });

    it('should show an error for KVK field validation', () => {
        cy.get('button').contains('Zakelijk').click(); // Click "Zakelijk"

        cy.get('input#firstName').type('John');
        cy.get('input#lastName').type('Doe');
        cy.get('input#email').type('john.doe@example.com');
        cy.get('input#adres').type('123 Main St');
        cy.get('input#phonenumber').type('1234567890');
        cy.get('input#kvk').type('1234567'); // Onjuist KVK number (7 digits inplaats van 8)
        cy.get('input#password').type('password123');
        cy.get('input#passwordConfirm').type('password123');

        cy.get('button').contains('Maak Account').click();

        // Kijk of KVK validatie error er is
        cy.get('p').contains('KVK number must be 8 digits.');
    });

    it('should submit the form for Zakelijk account', () => {
        // Mocking Fetch API
        cy.intercept('POST', '/api/SignUp/signUpEmployee', {
            statusCode: 200,
            body: { message: "Success" }
        }).as('signupRequest');

        cy.get('button').contains('Zakelijk').click();

        // Correct ingevulde formulier
        cy.get('input#firstName').type('John');
        cy.get('input#lastName').type('Doe');
        cy.get('input#email').type('john.doe@example.com');
        cy.get('input#adres').type('123 Main St');
        cy.get('input#phonenumber').type('0623023057');
        cy.get('input#kvk').type('88888888');
        cy.get('input#password').type('Wachtwoord-01');
        cy.get('input#passwordConfirm').type('Wachtwoord-01');

        // Maak account met correcte gegevens
        cy.get('button').contains('Maak Account').click();

        // Wacht op response van de API call
        cy.wait('@signupRequest').its('response.statusCode').should('eq', 200);

        cy.url().should('include', '/login');  // kijk of die correct wordt doorgestuurd naar de juiste pagina
    });
});

