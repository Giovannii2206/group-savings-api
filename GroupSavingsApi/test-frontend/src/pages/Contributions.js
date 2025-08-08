import React, { useState } from 'react';
import axios from 'axios';
const API_BASE = 'http://localhost:5000/api';

export default function Contributions() {
  const [contributions, setContributions] = useState([]);
  const [groupSessionId, setGroupSessionId] = useState('');
  const [userId, setUserId] = useState('');
  const [amount, setAmount] = useState('');
  const [date, setDate] = useState('');
  const [type, setType] = useState('deposit');

  const [error, setError] = useState(null);
const [loading, setLoading] = useState(false);

const createContribution = async () => {
  setError(null);
  if (!groupSessionId || !userId || !amount || !date || !type) {
    setError('All fields are required.');
    return;
  }
  setLoading(true);
  try {
    const res = await axios.post(`${API_BASE}/Contribution`, {
      groupSessionId,
      userId,
      amount: Number(amount),
      date,
      type
    });
    setContributions([...contributions, res.data]);
    setGroupSessionId(''); setUserId(''); setAmount(''); setDate(''); setType('deposit');
  } catch (err) {
    setError(err.response?.data?.message || err.message || 'Submission failed');
  } finally {
    setLoading(false);
  }
};

  return (
    <div>
      <h2>Contributions</h2>
      <input placeholder="SessionId" value={groupSessionId} onChange={e => setGroupSessionId(e.target.value)} />
      <input placeholder="UserId" value={userId} onChange={e => setUserId(e.target.value)} />
      <input placeholder="Amount" value={amount} onChange={e => setAmount(e.target.value)} />
      <input placeholder="Date (ISO)" value={date} onChange={e => setDate(e.target.value)} />
      <input placeholder="Type" value={type} onChange={e => setType(e.target.value)} />
      <button onClick={createContribution}>Create Contribution</button>
      <ul>
        {contributions.map((c, i) => <li key={i}>User {c.userId} - {c.amount} to Session {c.groupSessionId}</li>)}
      </ul>
    </div>
  );
}
