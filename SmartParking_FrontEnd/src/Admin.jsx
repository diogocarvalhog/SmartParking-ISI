import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { FiHome, FiSettings, FiLogOut, FiPlus, FiMapPin, FiLayers, FiTrash2, FiFilter } from "react-icons/fi";

function Admin() {
  const [parques, setParques] = useState([]);
  const [lugares, setLugares] = useState([]);
  const [parqueFiltro, setParqueFiltro] = useState(""); // Estado para o filtro de lugares
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

  const handleRemoverParque = async (id) => {
    if (!window.confirm("Tem a certeza? Todos os lugares deste parque serão apagados.")) return;
    const token = localStorage.getItem("meuToken");
    try {
      const res = await fetch(`${API_URL}/Parques/${id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (res.ok) { fetchParques(); fetchLugares(); }
    } catch (e) { console.error(e); }
  };

  const handleRemoverLugar = async (id) => {
    if (!window.confirm("Remover este lugar?")) return;
    const token = localStorage.getItem("meuToken");
    try {
      const res = await fetch(`${API_URL}/Lugares/${id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (res.ok) fetchLugares();
    } catch (e) { console.error(e); }
  };

  const handleCriarParque = async (e) => {
    e.preventDefault();
    const token = localStorage.getItem("meuToken");
    const res = await fetch(`${API_URL}/Parques`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
      body: JSON.stringify(novoParque)
    });
    if (res.ok) { fetchParques(); setNovoParque({ nome: "", localizacao: "", latitude: 0, longitude: 0, isExterior: true }); }
  };

  const handleCriarLugar = async (e) => {
    e.preventDefault();
    const token = localStorage.getItem("meuToken");
    const resLugar = await fetch(`${API_URL}/Lugares`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
      body: JSON.stringify({ ...novoLugar, piso: parseInt(novoLugar.piso), parqueId: parseInt(novoLugar.parqueId) })
    });
    if (resLugar.ok) {
        const data = await resLugar.json();
        await fetch(`${API_URL}/Sensores`, {
             method: 'POST',
             headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
             body: JSON.stringify({ lugarId: data.id, tipo: "Presenca", estado: false })
        });
        setNovoLugar({ ...novoLugar, numeroLugar: "" });
        fetchLugares();
    }
  };

  const handleLogout = () => { localStorage.clear(); navigate("/"); };

  // Filtragem lógica dos lugares baseada na seleção
  const lugaresFiltrados = parqueFiltro 
    ? lugares.filter(l => l.parqueId === parseInt(parqueFiltro))
    : [];

  return (
    <div style={{ display: 'flex', width: '100vw', height: '100vh', background: '#000000', color: 'white', overflow: 'hidden' }}>
      
      <aside style={sidebarStyle}>
        <div style={logoStyle}>
          <div style={avatarStyle}>S</div>
          <span style={{ fontSize: '1.6rem', fontWeight: '800' }}>SmartParking</span>
        </div>
        <nav style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: '10px' }}>
          <div style={menuItem} onClick={() => navigate('/dashboard')}><FiHome size={22} /> Início</div>
          <div style={menuItemActive}><FiSettings size={22} /> Administração</div>
        </nav>
        <button onClick={handleLogout} style={logoutBtn}><FiLogOut size={22} /> Sair</button>
      </aside>

      <main style={{ flex: 1, overflowY: 'auto', padding: '50px' }}>
        <h1 style={{ fontSize: '2.5rem', fontWeight: '800', marginBottom: '40px' }}>Administração</h1>

        <div style={gridForms}>
          {/* Form Novo Parque */}
          <div style={cardStyle}>
            <h3 style={cardTitle}><FiMapPin /> Novo Parque</h3>
            <form onSubmit={handleCriarParque} style={formStyle}>
              <input placeholder="Nome" value={novoParque.nome} onChange={e => setNovoParque({...novoParque, nome: e.target.value})} style={inputStyle} required />
              <input placeholder="Cidade" value={novoParque.localizacao} onChange={e => setNovoParque({...novoParque, localizacao: e.target.value})} style={inputStyle} required />
              <button type="submit" style={btnPrimary}>Criar Parque</button>
            </form>
          </div>

          {/* Form Novo Lugar */}
          <div style={cardStyle}>
            <h3 style={cardTitle}><FiLayers /> Novo Lugar</h3>
            <form onSubmit={handleCriarLugar} style={formStyle}>
              <select onChange={e => setNovoLugar({...novoLugar, parqueId: e.target.value})} style={inputStyle} required>
                <option value="">Selecionar Parque...</option>
                {parques.map(p => <option key={p.id} value={p.id}>{p.nome}</option>)}
              </select>
              <input placeholder="Nº Lugar" value={novoLugar.numeroLugar} onChange={e => setNovoLugar({...novoLugar, numeroLugar: e.target.value})} style={inputStyle} required />
              <input type="number" placeholder="Piso" onChange={e => setNovoLugar({...novoLugar, piso: e.target.value})} style={inputStyle} required />
              <button type="submit" style={btnSuccess}>Adicionar Lugar</button>
            </form>
          </div>
        </div>

        {/* LISTAGEM DE PARQUES */}
        <h3 style={sectionTitle}>Parques Registados</h3>
        <div style={gridItems}>
          {parques.map(p => (
            <div key={p.id} style={itemCard}>
              <span>{p.nome} (ID: {p.id})</span>
              <button onClick={() => handleRemoverParque(p.id)} style={trashBtn}><FiTrash2 /></button>
            </div>
          ))}
        </div>

        {/* GESTÃO DE LUGARES COM FILTRO (Conforme pedido) */}
        <div style={{ marginTop: '60px', borderTop: '1px solid #27272a', paddingTop: '40px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '25px' }}>
            <h3 style={{ margin: 0, fontSize: '1.8rem', fontWeight: '700' }}>Lugares Existentes</h3>
            
            <div style={{ display: 'flex', alignItems: 'center', gap: '15px', background: '#0a0a0a', padding: '10px 20px', borderRadius: '12px', border: '1px solid #27272a' }}>
              <FiFilter color="#a1a1aa" />
              <select 
                value={parqueFiltro} 
                onChange={(e) => setParqueFiltro(e.target.value)}
                style={{ background: 'transparent', color: 'white', border: 'none', outline: 'none', cursor: 'pointer', fontSize: '1rem' }}
              >
                <option value="">Escolha um Parque para gerir...</option>
                {parques.map(p => <option key={p.id} value={p.id}>{p.nome}</option>)}
              </select>
            </div>
          </div>

          {parqueFiltro === "" ? (
            <div style={emptyState}>Selecione um parque acima para visualizar e remover os seus lugares.</div>
          ) : (
            <div style={gridItems}>
              {lugaresFiltrados.length > 0 ? (
                lugaresFiltrados.map(l => (
                  <div key={l.id} style={itemCardLugar}>
                    <div>
                      <div style={{ fontWeight: '700', fontSize: '1.1rem' }}>Lugar {l.numeroLugar}</div>
                      <div style={{ color: '#a1a1aa', fontSize: '0.9rem' }}>Piso {l.piso}</div>
                    </div>
                    <button onClick={() => handleRemoverLugar(l.id)} style={trashBtn}><FiTrash2 /></button>
                  </div>
                ))
              ) : (
                <div style={emptyState}>Este parque ainda não tem lugares registados.</div>
              )}
            </div>
          )}
        </div>
      </main>
    </div>
  );
}

// Estilos
const sidebarStyle = { width: '340px', borderRight: '1px solid #27272a', padding: '30px', display: 'flex', flexDirection: 'column' };
const logoStyle = { display: 'flex', alignItems: 'center', gap: '15px', marginBottom: '50px' };
const avatarStyle = { width: '45px', height: '45px', background: 'white', color: 'black', borderRadius: '12px', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 'bold' };
const menuItem = { display: 'flex', alignItems: 'center', gap: '15px', padding: '15px', borderRadius: '12px', cursor: 'pointer', color: '#a1a1aa', fontWeight: '600' };
const menuItemActive = { ...menuItem, background: '#18181b', color: 'white' };
const logoutBtn = { ...menuItem, color: '#ef4444', marginTop: 'auto', border: 'none', background: 'transparent', textAlign: 'left' };
const cardStyle = { background: '#0a0a0a', padding: '30px', borderRadius: '24px', border: '1px solid #27272a' };
const cardTitle = { display: 'flex', alignItems: 'center', gap: '10px', fontSize: '1.4rem', marginBottom: '20px' };
const inputStyle = { width: '100%', padding: '12px', borderRadius: '10px', background: '#18181b', border: '1px solid #27272a', color: 'white', marginBottom: '10px' };
const btnPrimary = { width: '100%', padding: '12px', borderRadius: '10px', background: '#3b82f6', color: 'white', border: 'none', fontWeight: 'bold', cursor: 'pointer' };
const btnSuccess = { ...btnPrimary, background: '#22c55e' };
const gridForms = { display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '30px', marginBottom: '50px' };
const gridItems = { display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '20px' };
const itemCard = { display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '20px', background: '#0a0a0a', borderRadius: '15px', border: '1px solid #27272a' };
const itemCardLugar = { ...itemCard, borderLeft: '4px solid #3b82f6' };
const trashBtn = { background: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', border: 'none', padding: '8px', borderRadius: '8px', cursor: 'pointer' };
const sectionTitle = { fontSize: '1.8rem', marginBottom: '20px' };
const emptyState = { gridColumn: '1 / -1', textAlign: 'center', padding: '40px', color: '#71717a', background: '#050505', borderRadius: '15px', border: '1px dashed #27272a' };

export default Admin;