import React, { useState } from 'react';
import axios from 'axios';
const API_BASE = 'http://localhost:5000/api';

export default function Groups() {
  const [groups, setGroups] = useState([]);
  const [name, setName] = useState('');
  const [createdBy, setCreatedBy] = useState('');

  const createGroup = async () => {
    const res = await axios.post(`${API_BASE}/Group`, { name, createdBy });
    setGroups([...groups, res.data]);
  };

  return (
    <div>
      <h2>Groups</h2>
      <input placeholder="Group Name" value={name} onChange={e => setName(e.target.value)} />
      <input placeholder="CreatedBy UserId" value={createdBy} onChange={e => setCreatedBy(e.target.value)} />
      <button onClick={createGroup}>Create Group</button>
      <ul>
        {groups.map((g, i) => <li key={i}>{g.name} (ID: {g.id || g.groupId})</li>)}
      </ul>
    </div>
  );
}
