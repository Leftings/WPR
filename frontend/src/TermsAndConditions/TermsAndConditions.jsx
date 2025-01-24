import React from 'react';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import '../index.css';

function TermsAndConditions() {
    return (
        <>
            <GeneralHeader />
            <div className="container">
                <section className="section">
                    <h1 className="title-primary">CarandAll - Algemene Voorwaarden</h1>
                    <p className="last-updated">Laatst bijgewerkt: 26 november 2024</p>
                </section>

                <section className="section">
                    <h2>1. Huurcontract</h2>
                    <ul>
                        <li><strong>1.1 Geschiktheid:</strong> Om een auto te huren bij CarandAll, moet je minimaal 21 jaar oud zijn (of de minimale leeftijd die vereist is volgens de huurwetgeving). Je moet ook een geldig rijbewijs hebben voor ten minste een jaar en voldoen aan andere vereisten die specifiek zijn voor het model auto dat je wilt huren.</li>
                        <li><strong>1.2 Huurperiode:</strong> De huurperiode begint wanneer je het voertuig ophaalt en eindigt wanneer het voertuig wordt teruggebracht volgens de voorwaarden in je boekingsovereenkomst. Alle huurperiodes zijn afhankelijk van beschikbaarheid.</li>
                        <li><strong>1.3 Huurkosten:</strong> De totale huurprijs wordt berekend op basis van het automodel, de huurperiode en eventuele extra diensten (bijv. verzekering, GPS, extra bestuurder). Kosten kunnen varieren op basis van locatie, vraag en het moment van boeken.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>2. Gebruik van het voertuig</h2>
                    <ul>
                        <li><strong>2.1 Geautoriseerde bestuurders:</strong> Alleen de persoon die in de huurovereenkomst is vermeld, mag het voertuig besturen. Extra bestuurders moeten bij de boeking worden aangegeven en moeten voldoen aan dezelfde geschiktheidsvereisten als de hoofdbestuurder.</li>
                        <li><strong>2.2 Verboden gebruik:</strong> Het voertuig mag niet worden gebruikt voor de volgende doeleinden:
                            <ul>
                                <li>Off-road rijden, racen of andere illegale activiteiten</li>
                                <li>Vervoer van gevaarlijke stoffen of vee</li>
                                <li>Subverhuur of het toestaan dat onbevoegden het voertuig besturen</li>
                                <li>Rijden onder invloed van alcohol of drugs</li>
                            </ul>
                        </li>
                        <li><strong>2.3 Kilometerbeperkingen:</strong> Elke huur bevat een bepaald aantal kilometers. Overmatige kilometers zullen extra kosten met zich meebrengen, zoals vermeld in je huurovereenkomst.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>3. Verzekering en Aansprakelijkheid</h2>
                    <ul>
                        <li><strong>3.1 Verzekeringsdekking:</strong> CarandAll biedt optionele verzekeringsdekking voor je huur, inclusief Collision Damage Waiver (CDW) en Diebstahlschutz. Als je de verzekering afwijst, ben je volledig verantwoordelijk voor de kosten van schade of verlies van het voertuig.</li>
                        <li><strong>3.2 Aansprakelijkheid:</strong> Je bent verantwoordelijk voor het voertuig en de staat ervan tijdens de huurperiode. Je gaat ermee akkoord de kosten van schade, verlies of diefstal te dekken, ongeacht of de schade door jou is veroorzaakt.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>4. Boeking en Annuleringen</h2>
                    <ul>
                        <li><strong>4.1 Boeking:</strong> Boekingen moeten via onze website of klantenservice worden gemaakt. Een boekingsbevestiging wordt naar het opgegeven e-mailadres gestuurd.</li>
                        <li><strong>4.2 Annuleringen en Terugbetalingen:</strong> Annuleringen moeten minstens 24 uur voor de geplande ophaaltijd worden gemaakt om een volledige terugbetaling te ontvangen. Annuleringen binnen 24 uur voor de ophaaltijd kunnen een annuleringsvergoeding tot 50% van de huurprijs met zich meebrengen.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>5. Teruggave van het voertuig</h2>
                    <ul>
                        <li><strong>5.1 Terugbrengstaat:</strong> Voertuigen moeten op de datum en tijd die in je huurovereenkomst zijn aangegeven, worden teruggebracht. Voertuigen moeten in dezelfde staat worden teruggebracht als bij het ophalen, met een volle tank benzine, tenzij anders afgesproken.</li>
                        <li><strong>5.2 Te late teruggave:</strong> Te late teruggave brengt extra kosten met zich mee, berekend op basis van een uurtarief of dagtarief, zoals gespecificeerd in je huurovereenkomst. Als het voertuig meer dan 12 uur te laat wordt teruggebracht, kunnen extra kosten voor een extra huurdag in rekening worden gebracht.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>6. Betaling en Kosten</h2>
                    <ul>
                        <li><strong>6.1 Betalingsmethoden:</strong> Wij accepteren alle grote creditcards en debetkaarten. Betaling moet worden gedaan bij de boeking of bij het ophalen, afhankelijk van de voorwaarden van je huur.</li>
                        <li><strong>6.2 Extra kosten:</strong> Extra kosten kunnen van toepassing zijn voor te late teruggave, extra bestuurders, eenrichtingsverhuur of speciale verzoeken (bijv. kinderzitjes, GPS-apparaten). Alle kosten worden tijdens het boekingsproces bekendgemaakt.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>7. Privacy en Gegevensbescherming</h2>
                    <ul>
                        <li><strong>7.1 Gegevensverzameling:</strong> Door gebruik te maken van onze diensten, ga je akkoord met onze verzameling en het gebruik van persoonsgegevens zoals uiteengezet in ons privacybeleid. We gebruiken deze gegevens om je boekingen te verwerken, diensten te leveren en met je te communiceren.</li>
                        <li><strong>7.2 Gegevensbeveiliging:</strong> We nemen redelijke maatregelen om je persoonlijke gegevens en betalingsinformatie te beschermen tegen ongeautoriseerde toegang, openbaarmaking, wijziging of vernietiging.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>8. Wijzigingen en Beeindiging</h2>
                    <ul>
                        <li><strong>8.1 Wijzigingen:</strong> CarandAll behoudt zich het recht voor om deze algemene voorwaarden op elk moment te wijzigen. Wijzigingen worden op onze website gepubliceerd en zijn van kracht vanaf de datum van publicatie.</li>
                        <li><strong>8.2 Beeindiging:</strong> CarandAll behoudt zich het recht voor om de huurovereenkomst te beeindigen in geval van overtreding van de voorwaarden of indien de veiligheid van de bestuurder of het voertuig in gevaar is.</li>
                    </ul>
                </section>
            </div>
            <GeneralFooter />
        </>
    );
};

export default TermsAndConditions;
