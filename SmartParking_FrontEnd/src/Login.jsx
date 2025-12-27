import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { FiLock, FiUser } from "react-icons/fi"; // (Opcional) Ícones para ficar bonito, se tiveres instalado

function Login() {
  const [username, setUsername] = useState(""); 
  const [password, setPassword] = useState("");
  const [erro, setErro] = useState("");
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    setErro("");
    // URL da Cloud
    const API_URL = "https://smartparking-api-diogo.azurewebsites.net/api";
    
    try {
      const response = await fetch(`${API_URL}/Auth/Login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password }) 
      });

      if (response.ok) {
        const data = await response.json();
        const tokenRecebido = data.token || data.Token;
        const roleRecebida = data.role || data.Role || "User"; 

        if (tokenRecebido) {
            localStorage.setItem("meuToken", tokenRecebido);
            localStorage.setItem("userRole", roleRecebida); 
            navigate('/dashboard'); 
        } 

      } else {
        setErro("Login falhou! Verifique os dados.");
      }
    } catch (error) {
      console.error(error);
      setErro("Erro de conexão. O servidor está ligado?");
    }
  };

  return (
    <div style={containerStyle}>
      <div style={cardStyle}>
        
        <h1 style={{ fontSize: '2.5rem', marginBottom: '30px', fontWeight: 'bold' }}>🔐 Entrar</h1>
        
        <form onSubmit={handleLogin} style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          
          <input 
            type="text" 
            placeholder="Username" 
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required 
            style={inputStyle}
          />
          
          <input 
            type="password" 
            placeholder="Password" 
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required 
            style={inputStyle}
          />
          
          <button type="submit" style={buttonStyle}>
            ENTRAR
          </button>
        </form>
        
        {erro && <div style={errorStyle}>⚠️ {erro}</div>}

        <p style={{ marginTop: '30px', fontSize: '1rem', color: '#a1a1aa' }}>
          Ainda não tem conta? <br/>
          <span 
            onClick={() => navigate('/register')} 
            style={linkStyle}>
            Criar conta nova
          </span>
        </p>

      </div>
    </div>
  );
}

// --- ESTILOS MODERNOS (CSS-in-JS) ---
const containerStyle = {
  display: 'flex',
  justifyContent: 'center',
  alignItems: 'center',
  height: '100vh',
  width: '100vw',
  backgroundColor: '#000000',
  color: 'white',
  fontFamily: 'Inter, system-ui, Avenir, Helvetica, Arial, sans-serif'
};

const cardStyle = {
  backgroundColor: '#0a0a0a',
  padding: '60px 40px',
  borderRadius: '24px',
  border: '1px solid #27272a',
  width: '100%',
  maxWidth: '450px', // Mais largo
  textAlign: 'center',
  boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.5)'
};

const inputStyle = {
  width: '100%',
  padding: '16px 20px',
  backgroundColor: '#18181b',
  border: '1px solid #27272a',
  borderRadius: '12px',
  color: 'white',
  fontSize: '1.1rem', // Letra maior
  outline: 'none',
  boxSizing: 'border-box'
};

const buttonStyle = {
  width: '100%',
  padding: '16px',
  backgroundColor: '#3b82f6', // Azul bonito
  color: 'white',
  border: 'none',
  borderRadius: '12px',
  fontSize: '1.1rem',
  fontWeight: '700',
  cursor: 'pointer',
  marginTop: '10px',
  transition: 'background 0.2s'
};

const errorStyle = {
  marginTop: '20px',
  padding: '10px',
  backgroundColor: 'rgba(239, 68, 68, 0.1)',
  color: '#ef4444',
  borderRadius: '8px',
  fontSize: '0.95rem'
};

const linkStyle = {
  color: '#60a5fa',
  cursor: 'pointer',
  fontWeight: '600',
  textDecoration: 'none',
  marginTop: '5px',
  display: 'inline-block'
};

export default Login;