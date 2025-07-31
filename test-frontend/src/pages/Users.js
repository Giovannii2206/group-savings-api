import React, { useState } from 'react';
import axios from 'axios';
const API_BASE = 'http://localhost:5000/api';

export default function Users() {
  const [users, setUsers] = useState([]);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const createUser = async () => {
    const res = await axios.post(`${API_BASE}/User`, { email, password });
    setUsers([...users, res.data]);
  };

  return (
    <div>
      <h2>Users</h2>
      <input placeholder="Email" value={email} onChange={e => setEmail(e.target.value)} />
      <input placeholder="Password" type="password" value={password} onChange={e => setPassword(e.target.value)} />
      <button onClick={createUser}>Create User</button>
      <ul>
        {users.map((u, i) => <li key={i}>{u.email} (ID: {u.id || u.userId})</li>)}
      </ul>
    </div>
  );
}
