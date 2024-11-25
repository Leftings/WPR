import React from 'react';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './TermsAndConditions.css'; // Update the CSS file to match the component name.

function TermsAndConditions() { // Fixed the name here
    return (
        <>
            <GeneralHeader />
            <div className="container">
                <header className="header">
                    <h1>CarandAll - Terms and Conditions</h1>
                    <p>Last Updated: November 21, 2024</p>
                </header>

                <section className="section">
                    <h2>1. Rental Agreement</h2>
                    <ul>
                        <li><strong>1.1 Eligibility:</strong> To rent a car from CarandAll, you must be at least 21 years old (or the minimum age required by the jurisdiction of rental). You must also hold a valid driver’s license for at least one year and meet any other requirements specific to the car model you wish to rent.</li>
                        <li><strong>1.2 Rental Period:</strong> The rental period starts when you pick up the vehicle and ends when the vehicle is returned in accordance with the terms specified in your booking agreement. All rental periods are subject to availability.</li>
                        <li><strong>1.3 Rental Charges:</strong> The total rental charge will be calculated based on the car model, rental duration, and any additional services (e.g., insurance, GPS, extra driver). Charges are subject to change based on location, demand, and time of booking.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>2. Vehicle Use</h2>
                    <ul>
                        <li><strong>2.1 Authorized Drivers:</strong> Only the person listed in the rental agreement may drive the vehicle. Any additional drivers must be declared at the time of booking, and they must meet the same eligibility requirements as the primary driver.</li>
                        <li><strong>2.2 Prohibited Use:</strong> The vehicle must not be used for any of the following purposes:
                            <ul>
                                <li>Off-roading, racing, or any illegal activity</li>
                                <li>Transporting hazardous materials or livestock</li>
                                <li>Subletting or allowing unauthorized drivers to operate the vehicle</li>
                                <li>Driving under the influence of alcohol or drugs</li>
                            </ul>
                        </li>
                        <li><strong>2.3 Mileage Limit:</strong> Each rental includes a set number of miles. Excess mileage will incur additional fees, which will be clearly outlined in your rental agreement.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>3. Insurance and Liability</h2>
                    <ul>
                        <li><strong>3.1 Insurance Coverage:</strong> CarandAll offers optional insurance coverage for your rental, including Collision Damage Waiver (CDW) and Theft Protection. If you decline insurance, you will be held responsible for the full cost of any damage to or loss of the vehicle.</li>
                        <li><strong>3.2 Liability:</strong> You are responsible for the vehicle and its condition during the rental period. You agree to cover any costs related to damage, theft, or loss, whether or not the damage is caused by you.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>4. Booking and Cancellations</h2>
                    <ul>
                        <li><strong>4.1 Booking Process:</strong> Bookings must be made through our website or customer service center. A booking confirmation will be sent to the provided email address.</li>
                        <li><strong>4.2 Cancellations and Refunds:</strong> Cancellations must be made at least 24 hours before the scheduled pick-up time to receive a full refund. Cancellations made within 24 hours of pick-up may incur a cancellation fee up to 50% of the rental price.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>5. Vehicle Returns</h2>
                    <ul>
                        <li><strong>5.1 Return Condition:</strong> Vehicles must be returned on the date and time specified in your rental agreement. Vehicles must be returned in the same condition in which they were rented, with a full tank of gas unless otherwise agreed.</li>
                        <li><strong>5.2 Late Returns:</strong> Late returns will incur additional charges, calculated based on an hourly or daily rate, as specified in your rental agreement. If the vehicle is returned more than 12 hours late, you may be charged for an additional rental day.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>6. Payment and Fees</h2>
                    <ul>
                        <li><strong>6.1 Payment Methods:</strong> We accept all major credit cards and debit cards. Payment must be made at the time of booking or at pick-up, depending on the terms of your rental.</li>
                        <li><strong>6.2 Additional Fees:</strong> Additional fees may apply for late returns, extra drivers, one-way rentals, or special requests (e.g., baby seats, GPS devices). All fees will be disclosed during the booking process.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>7. Privacy and Data Protection</h2>
                    <ul>
                        <li><strong>7.1 Data Collection:</strong> By using our Services, you agree to our collection and use of personal data as outlined in our Privacy Policy. We use this data to process your bookings, provide services, and communicate with you.</li>
                        <li><strong>7.2 Data Security:</strong> We take reasonable steps to protect your personal and payment information from unauthorized access, disclosure, alteration, or destruction.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>8. Limitation of Liability</h2>
                    <ul>
                        <li><strong>8.1 Limitation of Liability:</strong> CarandAll will not be liable for any indirect, special, or consequential damages, including but not limited to loss of profits or loss of rental income, arising out of your use or inability to use our Services or vehicles.</li>
                        <li><strong>8.2 Force Majeure:</strong> CarandAll shall not be held liable for delays or failure to perform its obligations under these Terms due to unforeseen circumstances, including but not limited to natural disasters, pandemics, government actions, or civil unrest.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>9. Modifications to the Terms</h2>
                    <p>CarandAll reserves the right to update or modify these Terms at any time. Any changes will be posted on this page with the updated date. Your continued use of our Services after any modifications will constitute your acceptance of the updated Terms.</p>
                </section>

                <section className="section">
                    <h2>10. Governing Law and Dispute Resolution</h2>
                    <ul>
                        <li><strong>10.1 Governing Law:</strong> These Terms are governed by and construed in accordance with the laws of the jurisdiction where your rental takes place.</li>
                        <li><strong>10.2 Dispute Resolution:</strong> Any disputes arising under or in connection with these Terms will be resolved through binding arbitration in the jurisdiction where your rental occurs, unless otherwise agreed upon by both parties.</li>
                    </ul>
                </section>

                <section className="section">
                    <h2>11. Contact Information</h2>
                    <p>For any questions or concerns about these Terms or your rental experience, please contact us at:</p>
                    <p><strong>CarandAll Customer Service</strong><br />
                        Email: support@carandall.com<br />
                        Phone: 1-800-123-4567</p>
                </section>

                <GeneralFooter />
            </div>
        </>
    );
};

export default TermsAndConditions;
