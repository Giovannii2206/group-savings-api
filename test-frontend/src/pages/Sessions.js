import React, { useState } from 'react';
import axios from 'axios';
const API_BASE = 'http://localhost:5000/api';

export default function Sessions() {
  const [sessions, setSessions] = useState([]);
  const [groupId, setGroupId] = useState('');
  const [targetAmount, setTargetAmount] = useState('');
  const [targetDate, setTargetDate] = useState('');
  const [startDate, setStartDate] = useState('');
  const [frequency, setFrequency] = useState('');
  const [status, setStatus] = useState('Active');

  const createSession = async () => {
    const res = await axios.post(`${API_BASE}/GroupSession`, {
      groupId, targetAmount: Number(targetAmount), targetDate, startDate, frequency, status
    });
    setSessions([...sessions, res.data]);
  };

  return (
    <div>
      <h2>Group Sessions</h2>
      <input placeholder="GroupId" value={groupId} onChange={e => setGroupId(e.target.value)} />
      <input placeholder="Target Amount" value={targetAmount} onChange={e => setTargetAmount(e.target.value)} />
      <input placeholder="Target Date (ISO)" value={targetDate} onChange={e => setTargetDate(e.target.value)} />
      <input placeholder="Start Date (ISO)" value={startDate} onChange={e => setStartDate(e.target.value)} />
      <input placeholder="Frequency" value={frequency} onChange={e => setFrequency(e.target.value)} />
      <input placeholder="Status" value={status} onChange={e => setStatus(e.target.value)} />
      <button onClick={createSession}>Create Session</button>
      <ul>
        {sessions.map((s, i) => <li key={i}>Session for Group {s.groupId} (ID: {s.id || s.groupSessionId})</li>)}
      </ul>
    </div>
  );
}
