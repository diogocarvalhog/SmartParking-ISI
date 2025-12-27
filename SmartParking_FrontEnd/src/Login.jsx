import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

function Login() {
  const [username, setUsername] = useState(""); 
  const [password, setPassword] = useState("");
  const [erro, setErro] = useState("");
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    setErro("");
  const API_URL = "https://smartparking-api-diogo.azurewebsites.net/api";
    try {
      const response = await fetch(`${API_URL}/Auth/Login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password }) 
      });

      if (response.ok) {
        const data = await response.json();
        
        // Suporta token com t minúsculo ou maiúsculo
        const tokenRecebido = data.token || data.Token;
        // Suporta role com r minúsculo ou maiúsculo
        const roleRecebida = data.role || data.Role || "User"; 

        if (tokenRecebido) {
            localStorage.setItem("meuToken", tokenRecebido);
            
            // --- GUARDA A ROLE AQUI ---
            localStorage.setItem("userRole", roleRecebida); 
            
            navigate('/dashboard'); 
        } 

      } else {
        setErro("Login falhou! Verifique o Username e a Password.");
      }
    } catch (error) {
      console.error(error);
      setErro("Erro de conexão. O servidor está ligado?");
    }
  };

  return (
    <div style={{ padding: '40px', maxWidth: '400px', margin: 'auto', textAlign: 'center', fontFamily: 'Arial' }}>
      <h1>🔐 Entrar</h1>
      <form onSubmit={handleLogin} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
        
        <input 
          type="text" 
          placeholder="Username (ex: admin)" 
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          required 
          style={{ padding: '12px', borderRadius: '5px', border: '1px solid #ccc' }}
        />
        
        <input 
          type="password" 
          placeholder="Password" 
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required 
          style={{ padding: '12px', borderRadius: '5px', border: '1px solid #ccc' }}
        />
        
        <button type="submit" style={{ 
          padding: '12px', 
          background: '#007bff', 
          color: 'white', 
          border: 'none', 
          borderRadius: '5px',
          cursor: 'pointer',
          fontWeight: 'bold'
        }}>
          ENTRAR
        </button>
      </form>
      
      {erro && <p style={{ color: 'red', marginTop: '10px' }}>⚠️ {erro}</p>}

      {/* 🆕 NOVO: Link para o Registo */}
      <p style={{ marginTop: '20px', fontSize: '0.9em' }}>
        Ainda não tem conta? <br/>
        <span 
          onClick={() => navigate('/register')} 
          style={{ color: '#007bff', cursor: 'pointer', textDecoration: 'underline', fontWeight: 'bold' }}>
          Criar conta nova
        </span>
      </p>

    </div>
  );
}

export default Login;