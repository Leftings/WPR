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

createRoot(document.getElementById('root')).render(
  <Router>
    <Routes>
      <Route path="/" element={<Login />}></Route>
      <Route path="/home" element={<Home />}></Route>
      <Route path="/signUp" element={<SignUp />}></Route>
            <Route path="/userSettings" element={<UserSettings />}></Route>
            <Route path="/GeneralSalePage" element={<GeneralSalePage />}></Route>
            <Route path="/TermsAndConditions" element={<TermsAndConditions />}></Route>
    </Routes>
  </Router>
);

