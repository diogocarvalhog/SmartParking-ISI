import './index.css';
import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import App from './App.jsx'
import Login from './Login.jsx'
import Register from './Register.jsx' // <--- IMPORTANTE
import Admin from './Admin.jsx';

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/register" element={<Register />} /> {/* <--- IMPORTANTE */}
        <Route path="/dashboard" element={<App />} />
        <Route path="/admin" element={<Admin />} />
      </Routes>
    </BrowserRouter>
  </React.StrictMode>,
)