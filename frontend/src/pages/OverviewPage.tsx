import {useEffect,useState} from 'react';
import {Bar} from 'react-chartjs-2';
import {Chart as ChartJS,CategoryScale,LinearScale,BarElement,Tooltip,Legend} from 'chart.js';
import {api,Profile,friendlyError} from '../api';
import {Notice,EmptyState,Icon} from '../components/UI';
import {Theme} from './SettingsPage';
import WeatherCard from '../components/WeatherCard';
ChartJS.register(CategoryScale,LinearScale,BarElement,Tooltip,Legend);
type Dashboard={total:number;byStatus:{status:string;count:number}[];byCategory:{category:string;count:number}[]};
export default function OverviewPage({profile,theme}:{profile:Profile;theme:Theme}){
 const [data,setData]=useState<Dashboard|null>(null);const [error,setError]=useState('');
 useEffect(()=>{let active=true;api.get<Dashboard>('/dashboard').then(r=>{if(active)setData(r.data)}).catch(e=>{if(active)setError(friendlyError(e))});return ()=>{active=false}},[]);
 const count=(status:string)=>data?.byStatus.find(x=>x.status===status)?.count??0;
 const ink=theme==='dark'?'#c8d8e4':'#536879';const grid=theme==='dark'?'#263b4d':'#e7eef2';
 return <>{error&&<Notice error>{error}</Notice>}<section className="welcome-banner"><div><div className="section-kicker">CAMPUS SUPPORT, MADE SIMPLE</div><h2>Welcome, {profile.displayName.split(' ')[0]}.</h2><p>A clear view of what needs attention across your campus.</p><a className="button" href="#/requests/new"><Icon name="plus"/>Create a service request</a></div><div className="welcome-mark" aria-hidden="true"><Icon name="requests"/></div></section>
 <section className="stats" aria-label="Service request totals">{[{label:'Total requests',value:data?.total??'—',hint:'All reported issues',tone:'teal'},{label:'Pending',value:count('Pending'),hint:'Waiting for support',tone:'amber'},{label:'In progress',value:count('In Progress'),hint:'Currently being worked on',tone:'blue'},{label:'Resolved',value:count('Resolved'),hint:'Support completed',tone:'green'}].map(s=><article className="stat-card" key={s.label}><span className={'stat-dot '+s.tone}/><span>{s.label}</span><strong>{s.value}</strong><small>{s.hint}</small></article>)}</section>
 <div className="overview-grid"><section className="panel"><div className="panel-heading"><div><div className="section-kicker">WORKLOAD</div><h2>Requests by status</h2></div><a className="text-link" href="#/requests">View all requests →</a></div>{!data?<p role="status">Loading your dashboard…</p>:data.total===0?<EmptyState title="Your support desk is ready">Create your first service request to see the chart here.</EmptyState>:<div className="chart-wrap" role="img" aria-label={`Requests: ${count('Pending')} pending, ${count('In Progress')} in progress, ${count('Resolved')} resolved.`}><Bar options={{responsive:true,maintainAspectRatio:false,plugins:{legend:{display:false}},scales:{x:{ticks:{color:ink},grid:{display:false}},y:{beginAtZero:true,ticks:{precision:0,color:ink},grid:{color:grid}}}}} data={{labels:['Pending','In Progress','Resolved'],datasets:[{label:'Requests',data:[count('Pending'),count('In Progress'),count('Resolved')],backgroundColor:['#e6aa45','#478fae','#23a389'],borderRadius:7,maxBarThickness:80}]}}/></div>}</section>
 <section className="panel quick-guide"><div className="section-kicker">GET STARTED</div><h2>Need a hand?</h2><ol><li><strong>Report the problem</strong><span>Tell us what happened and where.</span></li><li><strong>Add supporting files</strong><span>A photo or document helps explain the issue.</span></li><li><strong>Keep the status updated</strong><span>Mark the request resolved when the problem is fixed.</span></li></ol><WeatherCard accountId={profile.id}/></section></div></>
}

