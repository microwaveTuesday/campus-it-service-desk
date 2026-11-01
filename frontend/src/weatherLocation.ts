import {useCallback,useEffect,useState} from 'react';
export type WeatherLocation={version:1;name:string;latitude:number;longitude:number;source:'city'|'device'|'default'};
export const defaultWeatherLocation:WeatherLocation={version:1,name:'Naga City, Philippines',latitude:13.62,longitude:123.19,source:'default'};
const key=(accountId:string)=>'campus-weather-v1:'+accountId;
function valid(value:unknown):value is WeatherLocation{
 if(!value||typeof value!=='object')return false;
 const v=value as Partial<WeatherLocation>;
 return v.version===1&&typeof v.name==='string'&&v.name.length>0&&v.name.length<=150&&typeof v.latitude==='number'&&Number.isFinite(v.latitude)&&v.latitude>=-90&&v.latitude<=90&&typeof v.longitude==='number'&&Number.isFinite(v.longitude)&&v.longitude>=-180&&v.longitude<=180&&['city','device','default'].includes(v.source??'');
}
function read(accountId:string):WeatherLocation{try{const value:unknown=JSON.parse(localStorage.getItem(key(accountId))??'null');return valid(value)?value:defaultWeatherLocation}catch{return defaultWeatherLocation}}
export function useWeatherLocation(accountId:string){
 const [location,setLocation]=useState<WeatherLocation>(()=>read(accountId));
 useEffect(()=>{setLocation(read(accountId));const changed=(event:Event)=>{if((event as CustomEvent<{accountId:string}>).detail?.accountId===accountId)setLocation(read(accountId))};const stored=(event:StorageEvent)=>{if(event.key===key(accountId)||event.key===null)setLocation(read(accountId))};window.addEventListener('campus-weather-location',changed);window.addEventListener('storage',stored);return ()=>{window.removeEventListener('campus-weather-location',changed);window.removeEventListener('storage',stored)}},[accountId]);
 const save=useCallback((value:WeatherLocation)=>{if(!valid(value))throw new Error('Choose a valid location.');const rounded={...value,latitude:Math.round(value.latitude*100)/100,longitude:Math.round(value.longitude*100)/100};localStorage.setItem(key(accountId),JSON.stringify(rounded));setLocation(rounded);window.dispatchEvent(new CustomEvent('campus-weather-location',{detail:{accountId}}))},[accountId]);
 const reset=useCallback(()=>{localStorage.removeItem(key(accountId));setLocation(defaultWeatherLocation);window.dispatchEvent(new CustomEvent('campus-weather-location',{detail:{accountId}}))},[accountId]);
 return {location,save,reset};
}
