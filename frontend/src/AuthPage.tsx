import {useEffect,useState,FormEvent} from 'react';
import {isAxiosError} from 'axios';
import {api} from './api';

type RegistrationStatus={registeredAdmins:number;maximumAdmins:number;canRegister:boolean};
export function accountError(error:unknown){
 if(isAxiosError(error)){
  const data=error.response?.data as {message?:string;errors?:string[]|Record<string,string[]>}|undefined;
  const details=Array.isArray(data?.errors)?data.errors:Object.values(data?.errors??{}).flat();
  return [data?.message,...details].filter(Boolean).join(' ')||'Unable to complete the request. Please try again.';
 }
 return 'Unable to complete the request. Please try again.';
}
export default function AuthPage({onSignedIn,noticeMessage}:{onSignedIn:(token:string,role:string)=>void;noticeMessage?:string}){
 const [status,setStatus]=useState<RegistrationStatus|null>(null);
 const [register,setRegister]=useState(false);
 const [email,setEmail]=useState('');
 const [password,setPassword]=useState('');
 const [name,setName]=useState('');
 const [error,setError]=useState('');
 const [notice,setNotice]=useState('');
 const [busy,setBusy]=useState(false);
 const [checking,setChecking]=useState(true);
 async function refreshStatus(){
  const {data}=await api.get<RegistrationStatus>('/auth/registration-status');setStatus(data);
  if(!data.canRegister)setRegister(false);
 }
 useEffect(()=>{
  let active=true;
  async function check(){
   try{const {data}=await api.get<RegistrationStatus>('/auth/registration-status');if(active){setStatus(data);if(!data.canRegister)setRegister(false)}}
   catch{if(active)setStatus(null)}finally{if(active)setChecking(false)}
  }
  void check();const onFocus=()=>void check();window.addEventListener('focus',onFocus);
  return ()=>{active=false;window.removeEventListener('focus',onFocus)};
 },[]);
 async function submit(event:FormEvent){
  event.preventDefault();setBusy(true);setError('');setNotice('');
  try{
   if(register){
    await api.post('/auth/register',{email,password,displayName:name});
    setRegister(false);setPassword('');setNotice('Admin account created. Please log in.');
    try{await refreshStatus()}catch{setStatus(null)}
   }else{
    const {data}=await api.post<{token:string;roles:string[]}>('/auth/login',{email,password});
    onSignedIn(data.token,data.roles.includes('Admin')?'Admin':'Staff');
   }
  }catch(error){setError(accountError(error));if(register)try{await refreshStatus()}catch{setStatus(null)}}
  finally{setBusy(false)}
 }
 return <main className="login"><section className="auth-intro"><div className="auth-brand"><span className="brand-mark">IT</span>Campus IT Service Desk</div><div className="section-kicker">A BETTER CONNECTED CAMPUS</div><h1>Less confusion.<br/>Better support.</h1><p>A simple place to report technical problems, follow their progress, and keep your campus moving.</p><div className="auth-benefits"><span>01 <strong>Report a problem</strong></span><span>02 <strong>Follow the progress</strong></span><span>03 <strong>Get back to what matters</strong></span></div><small>Designed for your campus community.</small></section><section className="panel auth-panel"><div className="section-kicker">WELCOME TO YOUR WORKSPACE</div><h1>Campus IT Service Desk</h1>
  <p>Report technical issues, track resolutions, and monitor campus support.</p>
  <h2>{register?'Register an Admin':'Sign in'}</h2>
  {noticeMessage&&<p className="notice" role="status">{noticeMessage}</p>}{error&&<p className="error" role="alert">{error}</p>}
  {notice&&<p className="notice" role="status">{notice}</p>}
  <form onSubmit={submit}>
   {register&&<label>Display name<input autoComplete="name" value={name} onChange={e=>setName(e.target.value)} required maxLength={100}/></label>}
   <label>Email<input type="email" autoComplete="username" value={email} onChange={e=>setEmail(e.target.value)} required/></label>
   <label>Password<input type="password" autoComplete={register?'new-password':'current-password'} value={password} onChange={e=>setPassword(e.target.value)} minLength={register?10:undefined} required/></label>
   {register&&<small>Use at least 10 characters, uppercase and lowercase letters, a number, and a symbol.</small>}
   <button disabled={busy}>{busy?'Please wait…':register?'Register Admin':'Login'}</button>
  </form>
  {status?.canRegister&&<><button className="link" disabled={busy} onClick={()=>{setRegister(!register);setError('');setNotice('')}}>{register?'Already registered? Log in':'Register an Admin'}</button>
   <small>{status.registeredAdmins} of {status.maximumAdmins} Admin accounts registered.</small></>}
  {status&&!status.canRegister&&<p className="notice" role="status">Maximum of 2 admins are already registered. You can no longer register more.</p>}
  {checking&&<small>Checking Admin registration availability…</small>}
  {!checking&&!status&&<small>Registration availability could not be checked. Refresh the page to try again.</small>}
  <small>Staff accounts are created by an Admin. Contact your Admin for access.</small>
 </section></main>;
}
