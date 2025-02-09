import React from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import './index.css';

import Login from './login/loginEmployee.jsx';
import Home from './home/home.jsx';
import SignUp from './signUp/signUp.jsx';
import UserSettings from './userSettings/userSettings.jsx';
import GeneralSalePage from './GeneralSalePage/GeneralSalePage.jsx';
import TermsAndConditions from './TermsAndConditions/TermsAndConditions.jsx';
import CarDetailPage from './IndividualCarPage/IndividualSalePage.jsx';
import Abonnement from './Abonnement/Abonnement.jsx';
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
import WagenparkBeheerderOverzichtPage from './wagenparkBeheerderOverzichtPage/wagenparkBeheerderOverzichtPage.jsx';
import ViewRentalData from './employeeBackOffice/viewRentalData/viewRentalData.jsx';
import ReviewBusiness from './employeeFrontOffice/addBusiness/reviewBusiness.jsx';
import IntakeForm from './employeeFrontOffice/intakeForm/intakeForm.jsx'
import ChangeBusinessSettings from './vehicleManager/changeSettings/changeBusinessSettings.jsx';
import AddSubscription from './employeeBackOffice/addSubscription/addSubscription.jsx'
import AddVehicleManager from './vehicleManager/addVehicleManager.jsx';


createRoot(document.getElementById('root')).render(
    <Router>
        {}
        <ToastContainer
            position="top-right"
            autoClose={3000}
            hideProgressBar={false}
            newestOnTop={true}
            closeOnClick
            pauseOnHover
            draggable
        />
        <Routes>
            <Route path="/" element={<Home />}></Route>
            <Route path="/login" element={<Login />}></Route>
            <Route path="/signUp" element={<SignUp />}></Route>
            <Route path="/userSettings" element={<UserSettings />}></Route>
            <Route path="/vehicles" element={<GeneralSalePage />}></Route>
            <Route path="/TermsAndConditions" element={<TermsAndConditions />}></Route>
            <Route path="/abonnement" element={<Abonnement />} />
            <Route path="/vehicle/:frameNr" element={<CarDetailPage />} />
            <Route path="/backOfficeEmployee" element={<EmployeeBackOffice />}></Route>
            <Route path="/backOfficeEmployee/addVehicle" element={<AddVehicle />}></Route>
            <Route path="/backOfficeEmployee/addEmployee" element={<AddEmployee />}></Route>
            <Route path="/backOfficeEmployee/addSubscription" element={<AddSubscription />}></Route>
            <Route path="/frontOfficeEmployee" element={<EmployeeFrontOffice />}></Route>
            <Route path="/frontOfficeEmployee/reviewHireRequest" element={<ReviewHireRequest />}></Route>
            <Route path="/buy" element={<PayPage />} />
            <Route path="/overviewRental" element={<CarRentalOverview />} />
            <Route path="/confirmationPage" element={<ConfirmationPage />}></Route>
            <Route path="/changeRental" element={<ChangeRental />} />
            <Route path="/wagenparkBeheerderOverzichtPage" element={<WagenparkBeheerderOverzichtPage />} />
            <Route path="/vehicleManager" element={<VehicleManager />}></Route>
            <Route path="/vehicleManager/reviewHireRequest" element={<ReviewHireRequestVehicleManager />}></Route>
            <Route path="/frontOfficeEmployee/reviewBusiness" element={<ReviewBusiness />}></Route>
            <Route path="/backOfficeEmployee/viewRentalData" element={<ViewRentalData />}></Route>
            <Route path="/vehicleManager/changeBusinessSettings" element={<ChangeBusinessSettings />}></Route>
            <Route path="*" element={<Home />}></Route>
            <Route path="/frontOfficeEmployee/intakeForm" element={<IntakeForm />}></Route>
            <Route path="/vehicleManager/addVehicleManager" element={<AddVehicleManager />} />
        </Routes>
    </Router>
);
