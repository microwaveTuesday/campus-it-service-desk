import {useEffect,useId,useState} from 'react';
import {api} from '../api';

type Attachment={id:number;originalName:string;size:number};
type Preview={status:'idle'|'loading'}|{status:'ready';url:string}|{status:'error'};

export default function AttachmentItem({file,requestId,onDownload}:{file:Attachment;requestId:number;onDownload:()=>Promise<void>}){
 const [open,setOpen]=useState(false);
 const [preview,setPreview]=useState<Preview>({status:'idle'});
 const [attempt,setAttempt]=useState(0);
 const previewId=useId();
 const isImage=/\.(png|jpe?g)$/i.test(file.originalName);

 useEffect(()=>{
  if(!open)return;
  const controller=new AbortController();
  let objectUrl:string|undefined;
  let active=true;
  setPreview({status:'loading'});
  api.get<Blob>(`/requests/${requestId}/attachments/${file.id}`,{responseType:'blob',signal:controller.signal})
   .then(response=>{
    if(!active)return;
    // Render only supported raster images, never a document or an error response.
    if(!['image/png','image/jpeg'].includes(response.data.type.toLowerCase())){
     setPreview({status:'error'});return;
    }
    objectUrl=URL.createObjectURL(response.data);
    setPreview({status:'ready',url:objectUrl});
   })
   .catch(()=>{if(active)setPreview({status:'error'})});
  return ()=>{
   active=false;
   controller.abort();
   if(objectUrl)URL.revokeObjectURL(objectUrl);
  };
 },[open,attempt,requestId,file.id]);

 function toggle(){setPreview({status:open?'idle':'loading'});setOpen(!open)}
 function retry(){setPreview({status:'loading'});setAttempt(value=>value+1)}

 return <li className="attachment-item">
  <div className="attachment-heading">
   <div className="attachment-details"><strong>{file.originalName}</strong><small>{Math.max(1,Math.round(file.size/1024))} KB</small></div>
   <div className="attachment-actions">
    {isImage&&<button type="button" className="secondary small" aria-expanded={open} aria-controls={previewId} onClick={toggle}>{open?'Hide preview':'Preview photo'}</button>}
    <button type="button" className="secondary small" onClick={()=>void onDownload()}>Download</button>
   </div>
  </div>
  {open&&<div id={previewId} className="attachment-preview">
   {preview.status==='loading'&&<p role="status">Loading photo…</p>}
   {preview.status==='ready'&&<img src={preview.url} alt={`Uploaded photo: ${file.originalName}`} onError={()=>setPreview({status:'error'})}/>}
   {preview.status==='error'&&<div><p role="alert">We couldn’t preview this photo. Try again or use Download to open the file.</p><button type="button" className="secondary small" onClick={retry}>Try again</button></div>}
  </div>}
 </li>;
}
