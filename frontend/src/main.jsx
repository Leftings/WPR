import React from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom'; 
import './index.css';
import Login from './login/loginEmployee.jsx';
import Home from './home/home.jsx';
import SignUp from './signUp/signUp.jsx';

createRoot(document.getElementById('root')).render(
  <Router>
    <Routes>
      <Route path="/" element={<Login />}></Route>
      <Route path="/home" element={<Home />}></Route>
        <Route path="/signUp" element={<SignUp />}></Route>
    </Routes>
  </Router>
);

