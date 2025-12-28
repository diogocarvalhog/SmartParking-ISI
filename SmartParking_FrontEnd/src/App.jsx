import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { FiHome, FiSettings, FiLogOut, FiCloud, FiDatabase, FiServer, FiDownload } from "react-icons/fi";

function App() {
  const [parques, setParques] = useState([]);
  const [parqueSelecionado, setParqueSelecionado] = useState("");
  const [lugares, setLugares] = useState([]);
  const [weather, setWeather] = useState(null);
  const [loadingId, setLoadingId] = useState(null);

  const navigate = useNavigate();

  const BASE_URL = "https://smartparking-api-diogo.azurewebsites.net";
  const API_URL = `${BASE_URL}/api`;
  const SOAP_URL = `${BASE_URL}/Service.asmx`;

  const userRole = localStorage.getItem("userRole");

  const handleDownloadXml = async () => {
    if (!parqueSelecionado) return;
    const parqueId = Number(parqueSelecionado);
    const soapEnvelope = `
      <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/">
        <soapenv:Header/>
        <soapenv:Body>
          <tem:GetParqueDetalhe>
            <tem:id>${parqueId}</tem:id>
          </tem:GetParqueDetalhe>
        </soapenv:Body>
      </soapenv:Envelope>
    `.trim();

    try {
      const response = await fetch(SOAP_URL, {
        method: 'POST',
        headers: {
          'Content-Type': 'text/xml; charset=utf-8',
          'SOAPAction': 'http://tempuri.org/ISoapService/GetParqueDetalhe'
        },
        body: soapEnvelope
      });
      const xmlText = await response.text();
      if (!response.ok) {
        console.error('SOAP HTTP error:', response.status, xmlText);
        alert("Erro ao descarregar XML via SOAP.");
        return;
      }
      const blob = new Blob([xmlText], { type: 'text/xml' });
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = `Parque_${parqueId}_Dados.xml`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(link.href);
    } catch (e) {
      console.error(e);
      alert("Erro ao descarregar XML via SOAP.");
    }
  };

  useEffect(() => {
    const token = localStorage.getItem("meuToken");
    if (!token) { navigate("/"); return; }
    fetch(`${API_URL}/Parques`, { headers: { 'Authorization': `Bearer ${token}` } })
      .then(res => res.json())
      .then(data => { setParques(data); if (data.length > 0) setParqueSelecionado(data[0].id); })
      .catch(() => { });
  }, [navigate]);

  const carregarDados = () => {
    if (!parqueSelecionado) return;
    const token = localStorage.getItem("meuToken");
    fetch(`${API_URL}/Lugares`, { headers: { 'Authorization': `Bearer ${token}` } })
      .then(res => res.json())
      .then(data => { if (Array.isArray(data)) setLugares(data.filter(l => l.parqueId == parqueSelecionado)); });
    fetch(`${API_URL}/Parques/${parqueSelecionado}/weather`, { headers: { 'Authorization': `Bearer ${token}` } })
      .then(res => res.json())
      .then(data => setWeather(data))
      .catch(() => setWeather(null));
  };

  useEffect(() => {
    carregarDados();
    const interval = setInterval(carregarDados, 5000);
    return () => clearInterval(interval);
  }, [parqueSelecionado]);

  const handleToggle = async (sensorId) => {
    setLoadingId(sensorId);
    const token = localStorage.getItem("meuToken");
    try {
      await fetch(`${API_URL}/Sensores/${sensorId}/toggle`, { method: 'POST', headers: { 'Authorization': `Bearer ${token}` } });
      carregarDados();
    } catch (e) {
    } finally {
      setLoadingId(null);
    }
  };

  const handleLogout = () => { localStorage.clear(); navigate("/"); };
  const ocupados = lugares.filter(l => l.sensor?.estado === true).length;
  const livres = lugares.length - ocupados;

  return (
    <div style={{ display: 'flex', width: '100vw', height: '100vh', background: '#000000', color: 'white', overflow: 'hidden' }}>
      <aside style={{ width: '340px', height: '100vh', display: 'flex', flexDirection: 'column', borderRight: '1px solid #27272a', backgroundColor: '#000000', padding: '30px', boxSizing: 'border-box' }}>
        <div style={{ flexShrink: 0, display: 'flex', alignItems: 'center', gap: '15px', marginBottom: '40px' }}>
          <div style={{ width: '45px', height: '45px', background: '#fff', borderRadius: '12px', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'black', fontWeight: 'bold', fontSize: '1.4rem' }}>S</div>
          <span style={{ fontSize: '1.6rem', fontWeight: '800', letterSpacing: '-1px', color: 'white' }}>SmartParking</span>
        </div>
        <nav style={{ flex: 1, overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: '15px', paddingRight: '10px' }}>
          <div style={menuItemActive}><FiHome size={24} /> <span>Início</span></div>
          {userRole === "Admin" && (
            <div style={menuItem} onClick={() => navigate('/admin')}><FiSettings size={24} /> <span>Administração</span></div>
          )}
        </nav>
        <div style={{ flexShrink: 0, marginTop: '30px', borderTop: '1px solid #27272a', paddingTop: '20px' }}>
          <p style={{ fontSize: '0.8rem', color: '#71717a', marginBottom: '10px', fontWeight: '600' }}>LOCALIZAÇÃO</p>
          <select value={parqueSelecionado} onChange={(e) => setParqueSelecionado(e.target.value)} style={darkSelect}>
            {parques.map(p => <option key={p.id} value={p.id}>{p.nome}</option>)}
          </select>
          <button onClick={handleLogout} style={menuItemDanger}><FiLogOut size={22} /> <span>Terminar Sessão</span></button>
        </div>
      </aside>

      <main style={{ flex: 1, height: '100vh', overflowY: 'auto', padding: '50px', backgroundColor: '#000000', boxSizing: 'border-box' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '50px' }}>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '14px' }}>
              <h1 style={{ fontSize: '2.5rem', fontWeight: '800', marginBottom: '10px', color: 'white' }}>Visão Geral</h1>
              <button type="button" onClick={handleDownloadXml} style={{ width: '44px', height: '44px', borderRadius: '12px', background: '#0a0a0a', border: '1px solid #27272a', color: '#d4d4d8', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center' }}><FiDownload size={20} /></button>
            </div>
            <p style={{ color: '#a1a1aa', fontSize: '1.1rem' }}>Monitorização em tempo real</p>
          </div>
          {weather && (
            <div style={{ display: 'flex', alignItems: 'center', gap: '15px', padding: '10px 25px', background: '#0a0a0a', borderRadius: '100px', border: '1px solid #27272a' }}>
              <FiCloud color="#a1a1aa" size={26} />
              <span style={{ fontWeight: '700', fontSize: '1.3rem' }}>{Math.round(weather.temp)}°C</span>
              <span style={{ color: '#71717a', fontSize: '1rem' }}>{weather.city}</span>
            </div>
          )}
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '30px', marginBottom: '60px' }}>
          <div style={statCard}><span style={labelStat}>Capacidade</span><div style={numberStat}>{lugares.length}</div></div>
          <div style={statCard}><span style={labelStat}>Livres</span><div style={{ ...numberStat, color: '#22c55e' }}>{livres}</div></div>
          <div style={statCard}><span style={labelStat}>Ocupados</span><div style={{ ...numberStat, color: '#ef4444' }}>{ocupados}</div></div>
        </div>

        <h3 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '25px' }}>Mapa de Ocupação</h3>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '25px', paddingBottom: '50px' }}>
          {lugares.map(lugar => {
            const isOcupado = lugar.sensor?.estado === true;
            const temSensor = !!lugar.sensor;

            // --- LÓGICA DE BLOQUEIO CORRIGIDA ---
            // O botão desativa se for USER e o lugar estiver OCUPADO
            const canAction = userRole === "Admin" || !isOcupado;

            return (
              <div key={lugar.id} style={{ backgroundColor: '#0a0a0a', borderRadius: '20px', padding: '30px', border: '1px solid #27272a', minHeight: '180px', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <div><h4 style={{ fontSize: '1.5rem', fontWeight: '700', margin: 0 }}>{lugar.numeroLugar}</h4><span style={{ color: '#71717a' }}>Piso {lugar.piso}</span></div>
                  <div style={{ width: '45px', height: '45px', borderRadius: '50%', background: !temSensor ? '#27272a' : (isOcupado ? 'rgba(239, 68, 68, 0.1)' : 'rgba(34, 197, 94, 0.1)'), display: 'flex', alignItems: 'center', justifyContent: 'center', color: !temSensor ? '#52525b' : (isOcupado ? '#ef4444' : '#22c55e') }}><FiServer size={22} /></div>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: '30px' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                    <div style={{ width: '12px', height: '12px', borderRadius: '50%', background: !temSensor ? '#52525b' : (isOcupado ? '#ef4444' : '#22c55e') }}></div>
                    <span style={{ color: '#d4d4d4' }}>{!temSensor ? "Sem Sensor" : (isOcupado ? "Ocupado" : "Livre")}</span>
                  </div>
                  {temSensor && (
                    <button
                      onClick={() => handleToggle(lugar.sensor.id)}
                      disabled={loadingId === lugar.sensor.id || !canAction}
                      style={{
                        padding: '12px 25px', borderRadius: '10px', 
                        background: isOcupado ? '#3f3f46' : '#22c55e', 
                        color: 'white', fontWeight: '600', 
                        cursor: canAction ? 'pointer' : 'not-allowed', 
                        border: 'none',
                        opacity: canAction ? 1 : 0.5 // Feedback visual para bloqueio
                      }}
                    >
                      {isOcupado ? "Ocupado" : "Entrar"}
                    </button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      </main>
    </div>
  );
}

// Estilos mantidos
const menuItem = { display: 'flex', alignItems: 'center', gap: '15px', padding: '15px 20px', color: '#a1a1aa', borderRadius: '12px', cursor: 'pointer', fontSize: '1.1rem', fontWeight: '600' };
const menuItemActive = { ...menuItem, background: '#18181b', color: 'white' };
const menuItemDanger = { ...menuItem, color: '#ef4444', background: 'transparent' };
const darkSelect = { width: '100%', padding: '15px', background: '#0a0a0a', color: 'white', border: '1px solid #27272a', borderRadius: '12px', marginBottom: '15px' };
const statCard = { backgroundColor: '#0a0a0a', borderRadius: '24px', padding: '35px', border: '1px solid #27272a' };
const labelStat = { color: '#a1a1aa', fontSize: '1.1rem' };
const numberStat = { fontSize: '4rem', fontWeight: '800' };

export default App;