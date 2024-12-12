import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './reviewHireRequest.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL_EMPLOYEE ?? 'http://localhost:5276';

function ReviewHireRequest() {
  return (
    <>
      <GeneralHeader>
      </GeneralHeader>

      <div className='body'>
        
      </div>

      <GeneralFooter>
      </GeneralFooter>
    </>
  );
}

export default ReviewHireRequest;
