import {useEffect,useState,FormEvent} from 'react';
import {api,Profile,friendlyError} from '../api';
import {Modal,Notice} from '../components/UI';
import WeatherSettings from '../components/WeatherSettings';
export type Theme='light'|'dark';
export default function SettingsPage({profile,theme,setTheme,onRefresh,onDeleted}:{profile:Profile;theme:Theme;setTheme:(value:Theme)=>void;onRefresh:()=>Promise<void>;onDeleted:()=>void}){
 const [dialog,setDialog]=useState<'delete'|'request'|null>(null);const [password,setPassword]=useState('');const [reason,setReason]=useState('');const [confirmed,setConfirmed]=useState(false);const [busy,setBusy]=useState(false);const [error,setError]=useState('');const [notice,setNotice]=useState('');
 useEffect(()=>{void onRefresh().catch(e=>setError(friendlyError(e)))},[onRefresh]);
 async function submit(e:FormEvent){e.preventDefault();setBusy(true);setError('');try{
  if(dialog==='delete'){await api.delete('/account',{data:{password}});onDeleted();return}
  await api.post('/account/deletion-request',{password,reason});setDialog(null);setPassword('');setReason('');setNotice('Your request was sent to the Admins for review.');await onRefresh();
 }catch(e){setError(friendlyError(e))}finally{setBusy(false)}}
 async function cancel(){setBusy(true);setError('');try{await api.delete('/account/deletion-request');setNotice('Your deletion request was cancelled.');await onRefresh()}catch(e){setError(friendlyError(e))}finally{setBusy(false)}}
 function open(kind:'delete'|'request'){setError('');setNotice('');setPassword('');setConfirmed(false);setDialog(kind)}
 const pending=profile.deletionRequest?.status==='Pending';
 return <div className="settings-grid">
  {notice&&<Notice>{notice}</Notice>}{error&&!dialog&&<Notice error>{error}</Notice>}
  <section className="panel"><div className="section-kicker">YOUR ACCOUNT</div><h2>Account details</h2><dl className="profile-details"><div><dt>Name</dt><dd>{profile.displayName}</dd></div><div><dt>Email</dt><dd>{profile.email}</dd></div><div><dt>Account type</dt><dd>{profile.role==='Admin'?'Administrator':'Staff'}</dd></div></dl></section>
  <section className="panel"><div className="section-kicker">MAKE IT YOURS</div><h2>Appearance</h2><p>Choose the look that feels most comfortable. Your choice is saved on this browser.</p><div className="theme-options"><button aria-pressed={theme==='light'} className={'theme-option '+(theme==='light'?'selected':'')} onClick={()=>setTheme('light')}><span className="theme-preview light-preview" aria-hidden="true"/>Light mode</button><button aria-pressed={theme==='dark'} className={'theme-option '+(theme==='dark'?'selected':'')} onClick={()=>setTheme('dark')}><span className="theme-preview dark-preview" aria-hidden="true"/>Dark mode</button></div></section>
  <WeatherSettings accountId={profile.id}/>
  <section className="panel danger-zone"><div className="section-kicker">ACCOUNT ACCESS</div><h2>{profile.role==='Admin'?'Delete your account':'Request account deletion'}</h2>
   <p>Deleting an account removes its sign-in details. Existing service requests and attachments remain with the service desk.</p>
   {profile.role==='Admin'?<>{!profile.canDeleteAdmin?<Notice>You are the last active Admin. Your account cannot be deleted until another active Admin is available.</Notice>:<p>Another active Admin will remain. Deleting your account also makes one Admin registration slot available again.</p>}<button className="danger" disabled={!profile.canDeleteAdmin} onClick={()=>open('delete')}>Delete my account</button></>:
    <>{pending?<><Notice>Your request is waiting for an Admin. You can continue using your account until it is approved.</Notice><button className="secondary" disabled={busy} onClick={()=>void cancel()}>Cancel deletion request</button></>:<>{profile.deletionRequest&&<p className="subtle">Previous request: {profile.deletionRequest.status.toLowerCase()}. You can send a new request if needed.</p>}<p>An Admin must approve your request before your account is deleted.</p><button className="danger-outline" onClick={()=>open('request')}>Request account deletion</button></>}</>}
  </section>
  {dialog&&<Modal title={dialog==='delete'?'Delete your Admin account?':'Request account deletion?'} description={dialog==='delete'?'This permanently removes your account and signs you out. Another active Admin must remain.':'Your account stays available while an Admin reviews your request.'} busy={busy} onClose={()=>setDialog(null)}>
   <form onSubmit={submit}>{error&&<Notice error>{error}</Notice>}<label>Your current password<input type="password" autoComplete="current-password" required value={password} onChange={e=>setPassword(e.target.value)}/></label>
   {dialog==='request'&&<label>Reason (optional)<textarea maxLength={500} value={reason} onChange={e=>setReason(e.target.value)} placeholder="Tell your Admin why you would like to leave."/></label>}
   {dialog==='delete'&&<label className="checkbox"><input type="checkbox" required checked={confirmed} onChange={e=>setConfirmed(e.target.checked)}/>I understand that my account will be permanently deleted.</label>}
   <div className="dialog-actions"><button className="secondary" type="button" disabled={busy} onClick={()=>setDialog(null)}>Keep my account</button><button className={dialog==='delete'?'danger':''} disabled={busy}>{busy?'Please wait…':dialog==='delete'?'Permanently delete my account':'Send request to Admin'}</button></div>
   </form></Modal>}
 </div>
}
