import {useCallback,useEffect,useState} from 'react';
import AuthPage from './AuthPage';
import StaffManagement from './StaffManagement';
import OverviewPage from './pages/OverviewPage';
import RequestsPage from './pages/RequestsPage';
import CategoriesPage from './pages/CategoriesPage';
import SettingsPage,{Theme} from './pages/SettingsPage';
import {api,Profile,friendlyError} from './api';
import {Icon,Notice} from './components/UI';
const navigation=[{id:'overview',label:'Operations overview',caption:'Your support desk at a glance'},{id:'requests',label:'Service requests',caption:'Report and follow up on campus issues'},{id:'categories',label:'Manage categories',caption:'Organize the support your campus needs',admin:true},{id:'staff',label:'Staff management',caption:'Manage your team and review account requests',admin:true},{id:'settings',label:'Settings',caption:'Your account, appearance, and preferences'}];
function currentRoute(){return window.location.hash.replace(/^#\/?/,'')||'overview'}
function initialTheme():Theme{const value=localStorage.getItem('campus-theme');return value==='light'||value==='dark'?value:window.matchMedia('(prefers-color-scheme: dark)').matches?'dark':'light'}
export default function App(){
 const [theme,setTheme]=useState<Theme>(initialTheme);const [token,setToken]=useState(()=>sessionStorage.getItem('token'));const [profile,setProfile]=useState<Profile|null>(null);const [profileError,setProfileError]=useState('');const [authNotice,setAuthNotice]=useState('');const [route,setRoute]=useState(currentRoute);const [menuOpen,setMenuOpen]=useState(false);
 useEffect(()=>{document.documentElement.dataset.theme=theme;localStorage.setItem('campus-theme',theme)},[theme]);
 const signOut=useCallback((notice='')=>{sessionStorage.removeItem('token');sessionStorage.removeItem('role');setToken(null);setProfile(null);setProfileError('');setAuthNotice(notice);window.location.hash='/overview'},[]);
 useEffect(()=>{const ended=()=>signOut('Your session has ended. Please sign in again.');window.addEventListener('campus-session-ended',ended);return ()=>window.removeEventListener('campus-session-ended',ended)},[signOut]);
 useEffect(()=>{const changed=()=>{setRoute(currentRoute());setMenuOpen(false);window.scrollTo({top:0})};window.addEventListener('hashchange',changed);return ()=>window.removeEventListener('hashchange',changed)},[]);
 const refreshProfile=useCallback(async()=>{if(!token)return;const r=await api.get<Profile>('/account/me');setProfile(r.data);setProfileError('')},[token]);
 useEffect(()=>{if(!token)return;let active=true;api.get<Profile>('/account/me').then(r=>{if(active){setProfile(r.data);setProfileError('')}}).catch(e=>{if(active)setProfileError(friendlyError(e))});return ()=>{active=false}},[token]);
 useEffect(()=>{const current=navigation.find(n=>n.id===route.split('/')[0]);document.title=(token&&current?current.label+' | ':'')+'Campus IT Service Desk'},[route,token]);
 useEffect(()=>{if(!menuOpen)return;const escape=(e:KeyboardEvent)=>{if(e.key==='Escape')setMenuOpen(false)};window.addEventListener('keydown',escape);return ()=>window.removeEventListener('keydown',escape)},[menuOpen]);
 const toggleTheme=()=>setTheme(value=>value==='light'?'dark':'light');
 if(!token)return <><button className="auth-theme secondary" onClick={toggleTheme}><Icon name={theme==='dark'?'sun':'moon'}/>{theme==='dark'?'Light mode':'Dark mode'}</button><AuthPage noticeMessage={authNotice} onSignedIn={(value,role)=>{sessionStorage.setItem('token',value);sessionStorage.setItem('role',role);setAuthNotice('');setToken(value)}}/></>;
 if(!profile)return <main className="connection-screen"><div className="panel"><div className="brand-mark">IT</div><h1>Campus IT Service Desk</h1>{profileError?<><Notice error>{profileError}</Notice><button onClick={()=>void refreshProfile().catch(e=>setProfileError(friendlyError(e)))}>Try again</button><button className="secondary" onClick={()=>signOut()}>Return to sign in</button></>:<p role="status">Opening your support desk…</p>}</div></main>;
 const pageId=route.split('/')[0];const item=navigation.find(n=>n.id===pageId);const allowed=item&&(!item.admin||profile.role==='Admin');
 const initials=profile.displayName.trim().split(/\s+/).slice(0,2).map(x=>x[0]).join('').toUpperCase();
 return <div className="app-shell"><a className="skip-link" href="#main-content" onClick={event=>{event.preventDefault();document.getElementById("main-content")?.focus()}}>Skip to main content</a>
 <aside className={'sidebar '+(menuOpen?'open':'')} aria-label="Main navigation"><a href="#/overview" className="brand"><span className="brand-mark">IT</span><span>Campus IT<small>SERVICE DESK</small></span></a><div className="sidebar-label">WORKSPACE</div>
 <nav>{navigation.filter(n=>!n.admin||profile.role==='Admin').map(n=><a href={'#/'+n.id} key={n.id} aria-current={pageId===n.id?'page':undefined} className={pageId===n.id?'active':''}><Icon name={n.id}/><span>{n.label}</span>{pageId===n.id&&<span className="nav-indicator"/>}</a>)}</nav>
 <div className="sidebar-bottom"><div className="support-note"><strong>Here to keep campus moving.</strong><span>Simple tools. Clear progress. Better support.</span></div><button className="sidebar-theme" onClick={toggleTheme}><Icon name={theme==='dark'?'sun':'moon'}/>{theme==='dark'?'Switch to light mode':'Switch to dark mode'}</button><div className="sidebar-profile"><span className="avatar">{initials}</span><div><strong>{profile.displayName}</strong><small>{profile.role==='Admin'?'Administrator':'Staff member'}</small></div></div><button className="sign-out" onClick={()=>signOut()}><Icon name="logout"/>Sign out</button></div></aside>
 {menuOpen&&<button className="menu-backdrop" aria-label="Close navigation" onClick={()=>setMenuOpen(false)}/>}
 <div className="main-shell"><header className="topbar"><button className="icon-button mobile-menu" aria-label="Open navigation" aria-expanded={menuOpen} onClick={()=>setMenuOpen(!menuOpen)}><Icon name="menu"/></button><span className="topbar-location">Campus IT Service Desk <span>/ {item?.label??'Page not found'}</span></span><span className="role-label">{profile.role==='Admin'?'Admin workspace':'Staff workspace'}</span></header>
 <main id="main-content" className="page-content" tabIndex={-1}><div className="page-heading"><div><div className="section-kicker">YOUR CAMPUS, CONNECTED</div><h1>{allowed?item.label:'Page not available'}</h1><p>{allowed?item.caption:'Choose a page from the navigation to continue.'}</p></div><span className="workspace-pill"><span/>Support workspace</span></div>
 {!allowed?<section className="panel"><h2>This page is not available for your account.</h2><a className="button" href="#/overview">Go to overview</a></section>:pageId==='overview'?<OverviewPage profile={profile} theme={theme}/>:pageId==='requests'?<RequestsPage key={route} profile={profile} startNew={route==='requests/new'}/>:pageId==='categories'?<CategoriesPage/>:pageId==='staff'?<StaffManagement/>:<SettingsPage profile={profile} theme={theme} setTheme={setTheme} onRefresh={refreshProfile} onDeleted={()=>signOut('Your account has been deleted.')}/>}
 <footer className="page-footer">Campus IT Service Desk <span>Making campus support easier, one request at a time.</span></footer></main></div></div>
}
