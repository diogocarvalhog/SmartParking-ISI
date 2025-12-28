import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { FiHome, FiSettings, FiLogOut, FiPlus, FiMapPin, FiLayers, FiTrash2 } from "react-icons/fi";

function Admin() {
  const [parques, setParques] = useState([]);
  const [lugares, setLugares] = useState([]); // Novo estado para listar lugares
  const [novoParque, setNovoParque] = useState({ nome: "", localizacao: "", latitude: 0, longitude: 0, isExterior: true });
  const [novoLugar, setNovoLugar] = useState({ numeroLugar: "", piso: 0, parqueId: "" });
  
  const navigate = useNavigate();
  const API_URL = "https://smartparking-api-diogo.azurewebsites.net/api";

  useEffect(() => {
    const role = localStorage.getItem("userRole");
    if (role !== "Admin") { navigate('/dashboard'); return; }
    fetchParques();
    fetchLugares(); // Carregar lugares ao iniciar
  }, [navigate]);

  const fetchParques = async () => {
    const token = localStorage.getItem("meuToken");
    try {
        const res = await fetch(`${API_URL}/Parques`, { headers: { 'Authorization': `Bearer ${token}` } });
        if (res.ok) setParques(await res.json());
    } catch(e) { console.error(e); }
  };

  const fetchLugares = async () => {
    const token = localStorage.getItem("meuToken");
    try {
        const res = await fetch(`${API_URL}/Lugares`, { headers: { 'Authorization': `Bearer ${token}` } });
        if (res.ok) setLugares(await res.json());
    } catch(e) { console.error(e); }
  };

  const buscarCoordenadas = async () => {
    if (!novoParque.localizacao) { alert("Indique a cidade."); return; }
    const apiKey = "1c61f7257faf7285ff220f8abdfae717"; 
    try {
      const res = await fetch(`https://api.openweathermap.org/geo/1.0/direct?q=${novoParque.localizacao}&limit=1&appid=${apiKey}`);
      const dados = await res.json();
      if (dados.length > 0) {
        setNovoParque(prev => ({ ...prev, latitude: dados[0].lat, longitude: dados[0].lon }));
      } else { alert("Cidade não encontrada."); }
    } catch (e) { 
      console.error(e);
      alert("Erro de conexão ao buscar coordenadas."); 
    }
  };

  const handleCriarParque = async (e) => {
    e.preventDefault();
    const token = localStorage.getItem("meuToken");
    const res = await fetch(`${API_URL}/Parques`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
      body: JSON.stringify(novoParque)
    });
    if (res.ok) { fetchParques(); setNovoParque({ nome: "", localizacao: "", latitude: 0, longitude: 0, isExterior: true }); alert("Parque criado."); }
  };

  const handleRemoverParque = async (id) => {
    if (!window.confirm("Tem a certeza que deseja remover este parque? Todos os lugares associados serão apagados.")) return;
    const token = localStorage.getItem("meuToken");
    try {
      const res = await fetch(`${API_URL}/Parques/${id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (res.ok) { alert("Parque removido."); fetchParques(); fetchLugares(); }
    } catch (e) { console.error(e); }
  };

  const handleCriarLugar = async (e) => {
    e.preventDefault();
    const token = localStorage.getItem("meuToken");
    const resLugar = await fetch(`${API_URL}/Lugares`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
      body: JSON.stringify({ numeroLugar: novoLugar.numeroLugar, piso: parseInt(novoLugar.piso), parqueId: parseInt(novoLugar.parqueId) })
    });

    if (resLugar.ok) {
        const lugarData = await resLugar.json();
        await fetch(`${API_URL}/Sensores`, {
             method: 'POST',
             headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
             body: JSON.stringify({ lugarId: lugarData.id, tipo: "Presenca", estado: false })
        });
        setNovoLugar({ ...novoLugar, numeroLugar: "" });
        alert("Lugar adicionado.");
        fetchLugares(); // Atualizar lista de lugares
    }
  };

  const handleRemoverLugar = async (id) => {
    if (!window.confirm("Tem a certeza que deseja remover este lugar?")) return;
    const token = localStorage.getItem("meuToken");
    try {
      const res = await fetch(`${API_URL}/Lugares/${id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (res.ok) { alert("Lugar removido."); fetchLugares(); }
    } catch (e) { console.error(e); }
  };

  const handleLogout = () => { localStorage.clear(); navigate("/"); };

  return (
    <div style={{ display: 'flex', width: '100vw', height: '100vh', background: '#000000', color: 'white', overflow: 'hidden' }}>
      
      <aside style={{ width: '340px', height: '100vh', display: 'flex', flexDirection: 'column', borderRight: '1px solid #27272a', backgroundColor: '#000000', padding: '30px', boxSizing: 'border-box' }}>
        <div style={{ flexShrink: 0, display: 'flex', alignItems: 'center', gap: '15px', marginBottom: '40px' }}>
          <div style={{ width: '45px', height: '45px', background: '#fff', borderRadius: '12px', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'black', fontWeight: 'bold', fontSize: '1.4rem' }}>S</div>
          <span style={{ fontSize: '1.6rem', fontWeight: '800', letterSpacing: '-1px', color: 'white' }}>SmartParking</span>
        </div>
        <nav style={{ flex: 1, overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: '15px', paddingRight: '10px' }}>
          <div style={menuItem} onClick={() => navigate('/dashboard')}><FiHome size={24} /> <span>Início</span></div>
          <div style={menuItemActive}><FiSettings size={24} /> <span>Administração</span></div>
        </nav>
        <div style={{ flexShrink: 0, marginTop: '30px', borderTop: '1px solid #27272a', paddingTop: '20px' }}>
            <button onClick={handleLogout} style={menuItemDanger}><FiLogOut size={22} /> <span>Terminar Sessão</span></button>
        </div>
      </aside>

      <main style={{ flex: 1, height: '100vh', overflowY: 'auto', padding: '50px', backgroundColor: '#000000', boxSizing: 'border-box' }}>
        <div style={{ marginBottom: '50px' }}>
            <h1 style={{ fontSize: '2.5rem', fontWeight: '800', marginBottom: '10px', color: 'white' }}>Administração</h1>
            <p style={{ color: '#a1a1aa', fontSize: '1.1rem' }}>Gestão de parques e lugares</p>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(450px, 1fr))', gap: '30px', marginBottom: '60px' }}>
          <div style={cardStyle}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '30px' }}><FiMapPin size={22} color="#3b82f6" /> <h3 style={{ fontSize: '1.5rem', fontWeight: '700', margin: 0 }}>Novo Parque</h3></div>
            <form onSubmit={handleCriarParque} style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
              <input placeholder="Nome do Parque" value={novoParque.nome} onChange={e => setNovoParque({...novoParque, nome: e.target.value})} required style={inputStyle} />
              <div style={{ display: 'flex', gap: '10px' }}>
                <input placeholder="Cidade" value={novoParque.localizacao} onChange={e => setNovoParque({...novoParque, localizacao: e.target.value})} required style={{ ...inputStyle, flex: 1 }} />
                <button type="button" onClick={buscarCoordenadas} style={searchButtonStyle}>🔍</button>
              </div>
              <button type="submit" style={primaryButtonStyle}><FiPlus size={20} /> Criar Parque</button>
            </form>
          </div>

          <div style={cardStyle}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '30px' }}><FiLayers size={22} color="#22c55e" /> <h3 style={{ fontSize: '1.5rem', fontWeight: '700', margin: 0 }}>Adicionar Lugar</h3></div>
            <form onSubmit={handleCriarLugar} style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
              <select onChange={e => setNovoLugar({...novoLugar, parqueId: e.target.value})} required style={inputStyle}>
                <option value="">Selecione o Parque...</option>
                {parques.map(p => <option key={p.id} value={p.id}>{p.nome}</option>)}
              </select>
              <input placeholder="Número do Lugar" value={novoLugar.numeroLugar} onChange={e => setNovoLugar({...novoLugar, numeroLugar: e.target.value})} required style={inputStyle} />
              <input type="number" placeholder="Piso" onChange={e => setNovoLugar({...novoLugar, piso: e.target.value})} required style={inputStyle} />
              <button type="submit" style={primaryButtonStyle}><FiPlus size={20} /> Adicionar Lugar</button>
            </form>
          </div>
        </div>

        {/* LISTA DE PARQUES */}
        <h3 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '25px' }}>Parques Existentes</h3>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '25px', marginBottom: '50px' }}>
          {parques.map(parque => (
            <div key={parque.id} style={itemCard}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                  <h4 style={{ fontSize: '1.2rem', fontWeight: '700', margin: 0 }}>{parque.nome}</h4>
                  <p style={{ color: '#a1a1aa', margin: '5px 0' }}>📍 {parque.localizacao}</p>
                </div>
                <button onClick={() => handleRemoverParque(parque.id)} style={deleteBtn}><FiTrash2 size={18} /></button>
              </div>
            </div>
          ))}
        </div>

        {/* LISTA DE LUGARES */}
        <h3 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '25px' }}>Lugares Existentes</h3>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '25px' }}>
          {lugares.map(lugar => (
            <div key={lugar.id} style={itemCard}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                  <h4 style={{ fontSize: '1.2rem', fontWeight: '700', margin: 0 }}>Lugar {lugar.numeroLugar}</h4>
                  <p style={{ color: '#a1a1aa', margin: '5px 0' }}>Piso {lugar.piso} - {parques.find(p => p.id === lugar.parqueId)?.nome || "Parque ID: " + lugar.parqueId}</p>
                </div>
                <button onClick={() => handleRemoverLugar(lugar.id)} style={deleteBtn}><FiTrash2 size={18} /></button>
              </div>
            </div>
          ))}
        </div>
      </main>
    </div>
  );
}

const itemCard = { backgroundColor: '#0a0a0a', borderRadius: '15px', padding: '20px', border: '1px solid #27272a' };
const deleteBtn = { background: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', border: 'none', borderRadius: '8px', padding: '10px', cursor: 'pointer', transition: '0.2s' };
const menuItem = { display: 'flex', alignItems: 'center', gap: '15px', padding: '15px 20px', color: '#a1a1aa', borderRadius: '12px', cursor: 'pointer', fontSize: '1.1rem', fontWeight: '600', border: 'none', background: 'transparent', width: '100%', justifyContent: 'flex-start' };
const menuItemActive = { ...menuItem, background: '#18181b', color: 'white' };
const menuItemDanger = { ...menuItem, marginTop: '10px', color: '#ef4444' };
const cardStyle = { backgroundColor: '#0a0a0a', borderRadius: '24px', padding: '40px', border: '1px solid #27272a' };
const inputStyle = { width: '100%', padding: '15px 18px', background: '#18181b', color: 'white', border: '1px solid #27272a', borderRadius: '12px', fontSize: '1rem' };
const primaryButtonStyle = { display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '10px', background: '#3b82f6', color: 'white', border: 'none', padding: '16px 24px', fontWeight: '700', cursor: 'pointer', borderRadius: '12px', width: '100%' };
const searchButtonStyle = { padding: '15px 20px', background: '#3b82f6', color: 'white', border: 'none', borderRadius: '12px', cursor: 'pointer' };

export default Admin;