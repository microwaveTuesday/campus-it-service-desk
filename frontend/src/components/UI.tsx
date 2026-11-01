import {useEffect,useId,useRef,ReactNode} from 'react';
export function Notice({children,error=false}:{children:ReactNode;error?:boolean}){return <div className={error?'notice error':'notice'} role={error?'alert':'status'}>{children}</div>}
export function EmptyState({title,children}:{title:string;children:ReactNode}){return <div className="empty-state"><span className="empty-icon" aria-hidden="true">✧</span><h3>{title}</h3><p>{children}</p></div>}
export function Modal({title,description,children,onClose,busy=false}:{title:string;description?:string;children:ReactNode;onClose:()=>void;busy?:boolean}){
 const ref=useRef<HTMLDialogElement>(null);const id=useId();
 useEffect(()=>{const d=ref.current;d?.showModal();return ()=>d?.close()},[]);
 return <dialog ref={ref} aria-labelledby={id} aria-describedby={description?id+'-desc':undefined} onCancel={e=>{e.preventDefault();if(!busy)onClose()}}>
  <div className="modal-head"><h2 id={id}>{title}</h2><button type="button" className="icon-button" aria-label="Close dialog" disabled={busy} onClick={onClose}>×</button></div>
  {description&&<p id={id+'-desc'}>{description}</p>}{children}
 </dialog>
}
export function StatusBadge({value}:{value:string}){const tone=value==='Resolved'||value==='Enabled'?'success':value==='In Progress'||value==='Pending'?'pending':value==='Disabled'?'muted':'neutral';return <span className={'badge '+tone}>{value}</span>}
export function Icon({name}:{name:string}){
 const paths:Record<string,ReactNode>={
  overview:<><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></>,
  requests:<><path d="M8 4H5a2 2 0 0 0-2 2v14h18V6a2 2 0 0 0-2-2h-3"/><rect x="8" y="2" width="8" height="5" rx="1"/><path d="M7 12h10M7 16h7"/></>,
  categories:<><path d="M3 7V4h7l3 3h8v13H3z"/></>,
  staff:<><circle cx="9" cy="7" r="3"/><path d="M3 21v-3a6 6 0 0 1 12 0v3M16 4a3 3 0 0 1 0 6M18 13a5 5 0 0 1 3 5v3"/></>,
  settings:<><circle cx="12" cy="12" r="3"/><path d="m10 2-1 3-3 1-3-1-2 4 3 2v3l-2 2 2 4 3-1 3 1 1 3 4-1 1-3 3-1 3 1 2-4-3-2v-3l2-2-2-4-3 1-3-1-1-3z"/></>,
  moon:<path d="M20 15a8 8 0 0 1-11-11A9 9 0 1 0 20 15z"/>,
  sun:<><circle cx="12" cy="12" r="4"/><path d="M12 1v3m0 16v3M1 12h3m16 0h3M4 4l2 2m12 12 2 2M4 20l2-2M18 6l2-2"/></>,
  logout:<><path d="M10 3H4v18h6m5-14 5 5-5 5m-8-5h13"/></>,
  menu:<path d="M3 6h18M3 12h18M3 18h18"/>,
  plus:<path d="M12 5v14M5 12h14"/>,
 };
 return <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">{paths[name]??paths.requests}</svg>
}
