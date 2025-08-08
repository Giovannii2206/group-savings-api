import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import Home from './pages/Home';
import Users from './pages/Users';
import Groups from './pages/Groups';
import Sessions from './pages/Sessions';
import Contributions from './pages/Contributions';
import Reports from './pages/Reports';

function App() {
  return (
    <Router>
      <nav style={{ padding: 16, borderBottom: '1px solid #ccc', marginBottom: 20 }}>
        <Link to="/" style={{ marginRight: 16 }}>Home</Link>
        <Link to="/users" style={{ marginRight: 16 }}>Users</Link>
        <Link to="/groups" style={{ marginRight: 16 }}>Groups</Link>
        <Link to="/sessions" style={{ marginRight: 16 }}>Sessions</Link>
        <Link to="/contributions" style={{ marginRight: 16 }}>Contributions</Link>
        <Link to="/reports">Reports</Link>
      </nav>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/users" element={<Users />} />
        <Route path="/groups" element={<Groups />} />
        <Route path="/sessions" element={<Sessions />} />
        <Route path="/contributions" element={<Contributions />} />
        <Route path="/reports" element={<Reports />} />
      </Routes>
    </Router>
  );
}

export default App;
