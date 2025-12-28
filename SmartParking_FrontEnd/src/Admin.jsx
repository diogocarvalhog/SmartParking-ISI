import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { FiHome, FiSettings, FiLogOut, FiPlus, FiMapPin, FiLayers, FiTrash2, FiFilter } from "react-icons/fi";

function Admin() {
  const [parques, setParques] = useState([]);
  const [lugares, setLugares] = useState([]);
  const [parqueFiltro, setParqueFiltro] = useState(""); // Filtro para gerir lugares
  const [novoParque, setNovoParque] = useState({ nome: "", localizacao: "", latitude: 0, longitude: 0, isExterior: true });
  const [novoLugar, setNovoLugar] = useState({ numeroLugar: "", piso: 0, parqueId: "" });
  
  const navigate = useNavigate();
  const API_URL = "https://smartparking-api-diogo.azurewebsites.net/api";

  useEffect(() => {
    const role = localStorage.getItem("userRole");
    if (role !== "Admin") { navigate('/dashboard'); return; }
    fetchParques();
    fetchLugares();
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
    } catch (e) { alert("Erro de conexão ao buscar coordenadas."); }
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
    if (!window.confirm("Tem a certeza? Todos os lugares associados serão apagados.")) return;
    const token = localStorage.getItem("meuToken");
    try {
      const res = await fetch(`${API_URL}/Parques/${id}`, { method: 'DELETE', headers: { 'Authorization': `Bearer ${token}` } });
      if (res.ok) { fetchParques(); fetchLugares(); }
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
        fetchLugares();
    }
  };

  const handleRemoverLugar = async (id) => {
    if (!window.confirm("Remover este lugar?")) return;
    const token = localStorage.getItem("meuToken");
    try {
      const res = await fetch(`${API_URL}/Lugares/${id}`, { method: 'DELETE', headers: { 'Authorization': `Bearer ${token}` } });
      if (res.ok) fetchLugares();
    } catch (e) { console.error(e); }
  };

  const handleLogout = () => { localStorage.clear(); navigate("/"); };

  // Filtra os lugares apenas se um parque estiver selecionado
  const lugaresFiltrados = parqueFiltro 
    ? lugares.filter(l => l.parqueId === parseInt(parqueFiltro))
    : [];

  return (
    <div style={{ display: 'flex', width: '100vw', height: '100vh', background: '#000000', color: 'white', overflow: 'hidden' }}>
      
      {/* SIDEBAR */}
      <aside style={{ width: '340px', height: '100vh', display: 'flex', flexDirection: 'column', borderRight: '1px solid #27272a', backgroundColor: '#000000', padding: '30px', boxSizing: 'border-box' }}>
        <div style={{ flexShrink: 0, display: 'flex', alignItems: 'center', gap: '15px', marginBottom: '40px' }}>
          <div style={{ width: '45px', height: '45px', background: '#fff', borderRadius: '12px', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'black', fontWeight: 'bold', fontSize: '1.4rem' }}>S</div>
          <span style={{ fontSize: '1.6rem', fontWeight: '800', letterSpacing: '-1px', color: 'white' }}>SmartParking</span>
        </div>
        <nav style={{ flex: 1, overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: '15px' }}>
          <div style={menuItem} onClick={() => navigate('/dashboard')}><FiHome size={24} /> <span>Início</span></div>
          <div style={menuItemActive}><FiSettings size={24} /> <span>Administração</span></div>
        </nav>
        <div style={{ flexShrink: 0, marginTop: '30px', borderTop: '1px solid #27272a', paddingTop: '20px' }}>
            <button onClick={handleLogout} style={menuItemDanger}><FiLogOut size={22} /> <span>Terminar Sessão</span></button>
        </div>
      </aside>

      {/* MAIN CONTENT */}
      <main style={{ flex: 1, height: '100vh', overflowY: 'auto', padding: '50px', backgroundColor: '#000000', boxSizing: 'border-box' }}>
        <div style={{ marginBottom: '50px' }}>
            <h1 style={{ fontSize: '2.5rem', fontWeight: '800', marginBottom: '10px' }}>Administração</h1>
            <p style={{ color: '#a1a1aa', fontSize: '1.1rem' }}>Gestão de parques e lugares</p>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(450px, 1fr))', gap: '30px', marginBottom: '60px' }}>
          {/* Card: Novo Parque (Estilo do código 1) */}
          <div style={cardStyle}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '30px' }}><div style={{ color: '#3b82f6' }}><FiMapPin size={22} /></div><h3 style={{ fontSize: '1.5rem', fontWeight: '700', margin: 0 }}>Novo Parque</h3></div>
            <form onSubmit={handleCriarParque} style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
              <div><label style={labelStyle}>Nome do Parque</label><input placeholder="Ex: Smart Park IPCA" value={novoParque.nome} onChange={e => setNovoParque({...novoParque, nome: e.target.value})} required style={inputStyle} /></div>
              <div><label style={labelStyle}>Localização (Cidade)</label><div style={{ display: 'flex', gap: '10px' }}><input placeholder="Ex: Porto" value={novoParque.localizacao} onChange={e => setNovoParque({...novoParque, localizacao: e.target.value})} required style={{ ...inputStyle, flex: 1 }} /><button type="button" onClick={buscarCoordenadas} style={searchButtonStyle}>🔍</button></div></div>
              <button type="submit" style={primaryButtonStyle}><FiPlus size={20} /> Criar Parque</button>
            </form>
          </div>

          {/* Card: Adicionar Lugar (Estilo do código 1) */}
          <div style={cardStyle}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '30px' }}><div style={{ color: '#22c55e' }}><FiLayers size={22} /></div><h3 style={{ fontSize: '1.5rem', fontWeight: '700', margin: 0 }}>Adicionar Lugar</h3></div>
            <form onSubmit={handleCriarLugar} style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
              <div><label style={labelStyle}>Parque</label><select onChange={e => setNovoLugar({...novoLugar, parqueId: e.target.value})} required style={inputStyle}><option value="">Selecione o Parque...</option>{parques.map(p => <option key={p.id} value={p.id}>{p.nome}</option>)}</select></div>
              <div><label style={labelStyle}>Número do Lugar</label><input placeholder="Ex: A-01" value={novoLugar.numeroLugar} onChange={e => setNovoLugar({...novoLugar, numeroLugar: e.target.value})} required style={inputStyle} /></div>
              <div><label style={labelStyle}>Piso</label><input type="number" placeholder="0" onChange={e => setNovoLugar({...novoLugar, piso: e.target.value})} required style={inputStyle} /></div>
              <button type="submit" style={primaryButtonStyle}><FiPlus size={20} /> Adicionar Lugar</button>
            </form>
          </div>
        </div>

        {/* LISTA DE PARQUES */}
        <h3 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '25px' }}>Parques Existentes</h3>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '25px', marginBottom: '60px' }}>
          {parques.map(p => (
            <div key={p.id} style={itemCard}>
              <div><h4 style={{ margin: 0 }}>{p.nome}</h4><p style={{ color: '#a1a1aa', fontSize: '0.9rem', margin: '5px 0' }}>📍 {p.localizacao}</p></div>
              <button onClick={() => handleRemoverParque(p.id)} style={deleteBtn}><FiTrash2 size={18} /></button>
            </div>
          ))}
        </div>

        {/* GESTÃO DE LUGARES FILTRADA */}
        <div style={{ borderTop: '1px solid #27272a', paddingTop: '40px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '25px' }}>
            <h3 style={{ margin: 0, fontSize: '1.5rem', fontWeight: '700' }}>Gestão de Lugares</h3>
            <div style={filterContainer}>
              <FiFilter color="#a1a1aa" /><select value={parqueFiltro} onChange={(e) => setParqueFiltro(e.target.value)} style={filterSelect}><option value="">Escolha o parque...</option>{parques.map(p => <option key={p.id} value={p.id}>{p.nome}</option>)}</select>
            </div>
          </div>

          {parqueFiltro === "" ? (
            <div style={emptyState}>Selecione um parque para gerir os lugares.</div>
          ) : (
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '25px' }}>
              {lugaresFiltrados.length > 0 ? lugaresFiltrados.map(l => (
                <div key={l.id} style={itemCardLugar}>
                  <div><h4 style={{ margin: 0 }}>Lugar {l.numeroLugar}</h4><p style={{ color: '#a1a1aa', fontSize: '0.9rem', margin: 0 }}>Piso {l.piso}</p></div>
                  <button onClick={() => handleRemoverLugar(l.id)} style={deleteBtn}><FiTrash2 size={18} /></button>
                </div>
              )) : <div style={emptyState}>Sem lugares neste parque.</div>}
            </div>
          )}
        </div>
      </main>
    </div>
  );
}

// Estilos Reutilizados
const cardStyle = { backgroundColor: '#0a0a0a', borderRadius: '24px', padding: '40px', border: '1px solid #27272a' };
const inputStyle = { width: '100%', padding: '15px 18px', background: '#18181b', color: 'white', border: '1px solid #27272a', borderRadius: '12px' };
const labelStyle = { display: 'block', fontSize: '0.95rem', color: '#a1a1aa', marginBottom: '8px' };
const primaryButtonStyle = { display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '10px', background: '#3b82f6', color: 'white', border: 'none', padding: '16px', borderRadius: '12px', fontWeight: '700', cursor: 'pointer', width: '100%' };
const searchButtonStyle = { padding: '15px 20px', background: '#3b82f6', color: 'white', border: 'none', borderRadius: '12px', cursor: 'pointer' };
const itemCard = { display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '25px', backgroundColor: '#0a0a0a', borderRadius: '20px', border: '1px solid #27272a' };
const itemCardLugar = { ...itemCard, borderLeft: '4px solid #3b82f6' };
const deleteBtn = { background: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', border: 'none', borderRadius: '8px', padding: '10px', cursor: 'pointer' };
const filterContainer = { display: 'flex', alignItems: 'center', gap: '10px', background: '#0a0a0a', padding: '10px 20px', borderRadius: '12px', border: '1px solid #27272a' };
const filterSelect = { background: 'transparent', color: 'white', border: 'none', outline: 'none', cursor: 'pointer' };
const emptyState = { gridColumn: '1 / -1', textAlign: 'center', padding: '40px', color: '#71717a', background: '#050505', borderRadius: '15px', border: '1px dashed #27272a' };
const menuItem = { display: 'flex', alignItems: 'center', gap: '15px', padding: '15px 20px', color: '#a1a1aa', borderRadius: '12px', cursor: 'pointer', fontWeight: '600', width: '100%' };
const menuItemActive = { ...menuItem, background: '#18181b', color: 'white' };
const menuItemDanger = { ...menuItem, color: '#ef4444', background: 'transparent', paddingLeft: '0' };

export default Admin;