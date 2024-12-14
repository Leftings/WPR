import React from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter as Router, Route, Routes, BrowserRouter } from 'react-router-dom';
import './index.css';
import Login from './login/loginEmployee.jsx';
import Home from './home/home.jsx';
import SignUp from './signUp/signUp.jsx';
import UserSettings from './userSettings/userSettings.jsx';
import GeneralSalePage from './GeneralSalePage/GeneralSalePage.jsx';
import TermsAndConditions from './TermsAndConditions/TermsAndConditions.jsx';
import CarDetailPage from './IndividualCarPage/IndividualSalePage.jsx';
import AbonementUitlegPage from './AbonementUitlegPage/AbonementUitlegPage.jsx';
import EmployeeBackOffice from './employeeBackOffice/employeeBackOffice.jsx';
import AddVehicle from './employeeBackOffice/addVehicle/addVehicle.jsx';
import EmployeeFrontOffice from './employeeFrontOffice/employeeFrontOffice.jsx';
import ReviewHireRequest from './employeeFrontOffice/reviewHireRequest/reviewHireRequest.jsx';
import AddEmployee from './employeeBackOffice/addEmployee/addEmployee.jsx';
import CarRentalOverview from './CarRentalOverview/CarRentalOverview.jsx';
import PayPage from './PayPage/PayPage.jsx';
import ConfirmationPage from './confirmationPage/ConfirmationPage.jsx';
import ChangeRental from './changeRental/ChangeRental.jsx';
import VehicleManager from './vehicleManager/vehicleManager.jsx';
import ReviewHireRequestVehicleManager from './vehicleManager/reviewHireRequest/reviewHireRequest.jsx';
import WagenparkBeheerderOverzichtPage from './wagenparkBeheerderOverzichtPage/wagenparkBeheerderOverzichtPage.jsx'


createRoot(document.getElementById('root')).render(
    <Router>
        <Routes>
            <Route path="/" element={<Home />}></Route>
            <Route path="/login" element={<Login />}></Route>
            <Route path="/signUp" element={<SignUp />}></Route>
            <Route path="/userSettings" element={<UserSettings />}></Route>
            <Route path="/GeneralSalePage" element={<GeneralSalePage />}></Route>
            <Route path="/TermsAndConditions" element={<TermsAndConditions />}></Route>
            <Route path="/AbonementUitlegPage" element={<AbonementUitlegPage />} />
            <Route path="/vehicle/:frameNr" element={<CarDetailPage />} />
            <Route path="/backOfficeEmployee" element={<EmployeeBackOffice />}></Route>
            <Route path="/backOfficeEmployee/addVehicle" element={<AddVehicle />}></Route>
            <Route path="/backOfficeEmployee/addEmployee" element={<AddEmployee />}></Route>
            <Route path="/frontOfficeEmployee" element={<EmployeeFrontOffice />}></Route>
            <Route path="/frontOfficeEmployee/reviewHireRequest" element={<ReviewHireRequest />}></Route>
            <Route path="/buy" element={<PayPage />} />
            <Route path="/overviewRental" element={<CarRentalOverview />} />
            <Route path="/confirmationPage" element={<ConfirmationPage />}></Route>
            <Route path="/changeRental" element={<ChangeRental />} />
            <Route path="/wagenparkBeheerderOverzichtPage" element={<WagenparkBeheerderOverzichtPage />} />

        </Routes>
    </Router>
);