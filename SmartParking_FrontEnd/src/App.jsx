import { useState, useEffect } from 'react';
import './App.css';

function App() {
  const [lugares, setLugares] = useState([]);
  const [erro, setErro] = useState("");

  // 🔴 IMPORTANTE: Cole o seu Token JWT aqui dentro das aspas!
  const TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjEiLCJyb2xlIjoiQWRtaW4iLCJuYmYiOjE3NjY2MDQ3MDYsImV4cCI6MTc2NjYxMTkwNiwiaWF0IjoxNzY2NjA0NzA2fQ.m_f5I_rr5qnANNLidVhPpJ3zI221qnll4jf4QJq0z5E"; 

  useEffect(() => {
    // Esta função vai bater à porta do seu Backend
    const fetchLugares = async () => {
      try {
        const response = await fetch('http://localhost:5158/api/Lugares', {
          method: 'GET',
          headers: {
            'Authorization': `Bearer ${TOKEN}`, // Aqui enviamos o crachá de segurança
            'Content-Type': 'application/json'
          }
        });

        if (response.ok) {
          const data = await response.json();
          console.log("Dados recebidos:", data); // Para ver no Inspecionar do navegador
          setLugares(data);
        } else {
          setErro("Erro ao buscar dados: " + response.status);
        }
      } catch (error) {
        setErro("O Backend parece desligado! (Erro de conexão)");
        console.error(error);
      }
    };

    fetchLugares();
  }, []);

  return (
    <div style={{ padding: '40px', fontFamily: 'Arial, sans-serif' }}>
      <h1>🚗 SmartParking Dashboard</h1>
      
      {erro && <div style={{ color: 'red', marginBottom: '20px' }}>⚠️ {erro}</div>}

      <div style={{ display: 'flex', gap: '20px', flexWrap: 'wrap' }}>
        
        {/* Aqui criamos um cartão para cada lugar que veio da API */}
        {lugares.map((lugar) => {
          // Lógica: Se existe sensor E o estado é true, então está ocupado
          const isOcupado = lugar.sensor?.estado === true;
          // Se não tem sensor instalado, fica cinzento
          const semSensor = !lugar.sensor;

          return (
            <div key={lugar.id} style={{ 
              border: '2px solid #333', 
              borderRadius: '12px',
              padding: '20px',
              width: '180px',
              textAlign: 'center',
              boxShadow: '0 4px 8px rgba(0,0,0,0.1)',
              color: 'black', // Garante que o texto é preto
              // A Mágica das Cores acontece aqui:
              backgroundColor: semSensor ? '#f0f0f0' : (isOcupado ? '#ffcccc' : '#ccffcc')
            }}>
              <h2 style={{ margin: '0 0 10px 0' }}>{lugar.numeroLugar}</h2>
              <p>Piso: {lugar.piso}</p>
              
              {/* Mostra o estado em texto também */}
              <p style={{ fontWeight: 'bold' }}>
                {semSensor ? "⚠️ Sem Sensor" : (isOcupado ? "⛔ OCUPADO" : "✅ LIVRE")}
              </p>
            </div>
          )
        })}

      </div>
    </div>
  );
}

export default App; 