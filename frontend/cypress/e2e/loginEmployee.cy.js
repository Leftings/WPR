describe('Log in Page', () => {

    beforeEach(() => {
        cy.visit('/login');  // URL om te testen
    });

    it('should render the login page correctly', () => {
        cy.get('h1').contains('Klanten Login');
        cy.get('button').contains('Login');
    });

    it('should show an error when required fields are missing', () => {
        cy.get('button').contains('Login').click();
        cy.get('p').contains('Vul een emailadres en wachtwoord in');
    });

    it('should show an error when an account does not exist', () => {
        cy.get('input#user').type('test@test.nl')
        cy.get('input#password').type('testwachtwoord')
        
        cy.get('button').contains('Login').click();
        cy.get('p').contains('test@test.nl is geen geldig account of het wachtwoord klopt niet');
    });
});