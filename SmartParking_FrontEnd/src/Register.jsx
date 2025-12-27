import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

function Register() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [erro, setErro] = useState("");
  const navigate = useNavigate();

  const handleRegister = async (e) => {
    e.preventDefault();
    setErro("");
    // URL da Cloud
    const API_URL = "https://smartparking-api-diogo.azurewebsites.net/api";

    try {
      const response = await fetch(`${API_URL}/Auth/Register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
      });

      if (response.ok) {
        alert("Conta criada com sucesso! Faça login agora.");
        navigate('/'); 
      } else {
        const textoErro = await response.text();
        setErro(textoErro || "Erro ao criar conta.");
      }
    } catch (error) {
      console.error(error);
      setErro("Erro de conexão. O servidor está ligado?");
    }
  };

  return (
    <div style={containerStyle}>
      <div style={cardStyle}>
        
        <h1 style={{ fontSize: '2.5rem', marginBottom: '30px', fontWeight: 'bold' }}>📝 Registar</h1>
        
        <form onSubmit={handleRegister} style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          
          <input 
            type="text" 
            placeholder="Escolha um Username" 
            value={username} 
            onChange={(e) => setUsername(e.target.value)} 
            required 
            style={inputStyle}
          />
          
          <input 
            type="password" 
            placeholder="Escolha uma Password" 
            value={password} 
            onChange={(e) => setPassword(e.target.value)} 
            required 
            style={inputStyle}
          />
          
          <button type="submit" style={buttonStyle}>
            CRIAR CONTA
          </button>
        </form>
        
        {erro && <div style={errorStyle}>⚠️ {erro}</div>}

        <p style={{ marginTop: '30px', fontSize: '1rem', color: '#a1a1aa' }}>
          Já tem conta? <br/>
          <span onClick={() => navigate('/')} style={linkStyle}>
            Voltar ao Login
          </span>
        </p>

      </div>
    </div>
  );
}

// --- REUTILIZAÇÃO DOS MESMOS ESTILOS ---
const containerStyle = {
  display: 'flex',
  justifyContent: 'center',
  alignItems: 'center',
  height: '100vh',
  width: '100vw',
  backgroundColor: '#000000',
  color: 'white',
  fontFamily: 'Inter, system-ui, sans-serif'
};

const cardStyle = {
  backgroundColor: '#0a0a0a',
  padding: '60px 40px',
  borderRadius: '24px',
  border: '1px solid #27272a',
  width: '100%',
  maxWidth: '450px',
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
  fontSize: '1.1rem',
  outline: 'none',
  boxSizing: 'border-box'
};

const buttonStyle = {
  width: '100%',
  padding: '16px',
  backgroundColor: '#22c55e', // Verde para registo
  color: 'white',
  border: 'none',
  borderRadius: '12px',
  fontSize: '1.1rem',
  fontWeight: '700',
  cursor: 'pointer',
  marginTop: '10px'
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
  color: '#3b82f6',
  cursor: 'pointer',
  fontWeight: '600',
  textDecoration: 'none',
  marginTop: '5px',
  display: 'inline-block'
};

export default Register;