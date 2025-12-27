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
    const API_URL = "https://smartparking-api-diogo.azurewebsites.net/api";
    try {
      const response = await fetch(`${API_URL}/Auth/Register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
      });

      if (response.ok) {
        alert("Conta criada com sucesso! Faça login agora.");
        navigate('/'); // Redireciona para o Login
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
    <div style={{ padding: '40px', maxWidth: '400px', margin: 'auto', textAlign: 'center', fontFamily: 'Arial' }}>
      <h1>📝 Registar</h1>
      <form onSubmit={handleRegister} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
        
        <input 
          type="text" 
          placeholder="Escolha um Username" 
          value={username} 
          onChange={(e) => setUsername(e.target.value)} 
          required 
          style={{ padding: '12px', borderRadius: '5px', border: '1px solid #ccc' }}
        />
        
        <input 
          type="password" 
          placeholder="Escolha uma Password" 
          value={password} 
          onChange={(e) => setPassword(e.target.value)} 
          required 
          style={{ padding: '12px', borderRadius: '5px', border: '1px solid #ccc' }}
        />
        
        <button type="submit" style={{ 
          padding: '12px', 
          background: '#28a745', 
          color: 'white', 
          border: 'none', 
          borderRadius: '5px',
          cursor: 'pointer', 
          fontWeight: 'bold' 
        }}>
          CRIAR CONTA
        </button>
      </form>
      
      {erro && <p style={{ color: 'red', marginTop: '10px' }}>⚠️ {erro}</p>}

      <p style={{ marginTop: '20px' }}>
        Já tem conta? <span onClick={() => navigate('/')} style={{ color: 'blue', cursor: 'pointer', textDecoration: 'underline' }}>Voltar ao Login</span>
      </p>
    </div>
  );
}

export default Register;