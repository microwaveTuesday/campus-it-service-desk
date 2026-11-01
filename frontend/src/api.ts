import axios from 'axios';
export const api=axios.create({baseURL:import.meta.env.VITE_API_URL||'/api',timeout:12000});
api.interceptors.request.use(config=>{const token=sessionStorage.getItem('token');if(token)config.headers.Authorization=`Bearer ${token}`;return config});
api.interceptors.response.use(response=>response,error=>{
 if(error.response?.status===401&&error.config?.headers?.Authorization&&!error.config?.url?.includes('/auth/'))window.dispatchEvent(new Event('campus-session-ended'));
 return Promise.reject(error);
});
export type Category={id:number;name:string};
export type Ticket={id:number;requesterName:string;issue:string;location:string;status:string;dateReported:string;categoryId:number;category?:Category};
export type TicketInput=Omit<Ticket,'id'|'dateReported'|'category'>;
export type Profile={id:string;displayName:string;email:string;role:'Admin'|'Staff';canDeleteAdmin:boolean;activeAdminCount:number;deletionRequest:{id:number;status:string;requestedAt:string;reviewedAt?:string}|null};
export type DeletionRequest={id:number;userId:string;displayName:string;email:string;reason:string;requestedAt:string};
export function friendlyError(error:unknown){
 if(axios.isAxiosError(error)){
  const data=error.response?.data as {message?:string;errors?:string[]|Record<string,string[]>}|undefined;
  const details=Array.isArray(data?.errors)?data.errors:Object.values(data?.errors??{}).flat();
  if(data?.message||details.length)return [data?.message,...details].filter(Boolean).join(' ');
  if(error.response?.status===403)return 'You do not have permission to do that. Please contact an Admin.';
  if(error.response?.status===401)return 'Please sign in again to continue.';
  if(!error.response)return 'We could not connect to the service. Check that the system is running and try again.';
 }
 return 'Something went wrong. Please try again.';
}
