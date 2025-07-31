import React, { useState } from 'react';
import axios from 'axios';
const API_BASE = 'http://localhost:5000/api';

export default function Reports() {
  const [byUser, setByUser] = useState([]);
  const [byGroup, setByGroup] = useState([]);
  const [bySession, setBySession] = useState([]);

  const fetchReport = async (type) => {
    const res = await axios.get(`${API_BASE}/ContributionReports/${type}`);
    if (type === 'ByUser') setByUser(res.data);
    if (type === 'ByGroup') setByGroup(res.data);
    if (type === 'BySession') setBySession(res.data);
  };

  return (
    <div>
      <h2>Contribution Reports</h2>
      <button onClick={() => fetchReport('ByUser')}>By User</button>
      <button onClick={() => fetchReport('ByGroup')}>By Group</button>
      <button onClick={() => fetchReport('BySession')}>By Session</button>
      <div style={{marginTop:20}}>
        <h3>By User</h3>
        <pre>{JSON.stringify(byUser, null, 2)}</pre>
        <h3>By Group</h3>
        <pre>{JSON.stringify(byGroup, null, 2)}</pre>
        <h3>By Session</h3>
        <pre>{JSON.stringify(bySession, null, 2)}</pre>
      </div>
    </div>
  );
}
