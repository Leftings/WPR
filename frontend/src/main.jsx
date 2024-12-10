import React from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
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
        </Routes>
    </Router>
);