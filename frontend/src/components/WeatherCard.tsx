import {useCallback,useEffect,useRef,useState} from 'react';
import {api,friendlyError} from '../api';
import {useWeatherLocation} from '../weatherLocation';
type Weather={available:boolean;temperatureC?:number;condition?:string;humidityPercent?:number|null;windKmh?:number|null;feelsLikeC?:number|null;checkedAt?:string;forecast?:{date:string;maxTemperatureC:number;minTemperatureC:number;rainChance:number|null;condition:string}[];message?:string};
const refreshEvery=15*60*1000;
export default function WeatherCard({accountId,showChangeLink=true}:{accountId:string;showChangeLink?:boolean}){
 const {location}=useWeatherLocation(accountId);const [data,setData]=useState<Weather|null>(null);const [busy,setBusy]=useState(false);const [error,setError]=useState('');const generation=useRef(0);const lastRefresh=useRef(0);
 const fetchWeather=useCallback(async()=>{
  const current=++generation.current;setBusy(true);setError('');
  try{const response=await api.get<Weather>('/weather',{params:{latitude:location.latitude,longitude:location.longitude}});if(current!==generation.current)return;lastRefresh.current=Date.now();if(!response.data.available){setError(response.data.message||'Weather is temporarily unavailable.');return}setData(response.data)}
  catch(e){if(current===generation.current)setError(friendlyError(e))}finally{if(current===generation.current)setBusy(false)}
 },[location.latitude,location.longitude]);
 useEffect(()=>{setData(null);void fetchWeather();const timer=window.setInterval(()=>{if(document.visibilityState==='visible')void fetchWeather()},refreshEvery);const visible=()=>{if(document.visibilityState==='visible'&&Date.now()-lastRefresh.current>=refreshEvery)void fetchWeather()};document.addEventListener('visibilitychange',visible);return ()=>{generation.current++;window.clearInterval(timer);document.removeEventListener('visibilitychange',visible)}},[fetchWeather]);
 return <div className="weather-card weather-live" aria-label="Weather and forecast">
  <div className="weather-heading"><span>LOCAL WEATHER & FORECAST</span><button className="weather-refresh" disabled={busy} onClick={()=>void fetchWeather()} aria-label="Refresh weather">{busy?'Updating…':'Refresh'}</button></div>
  <h3>{location.name}</h3><small className="weather-source-location">{location.source==='device'?'Approximate device location':location.source==='default'?'Default location':'Selected city'}</small>
  {!data&&!error&&<p role="status">Checking the latest weather…</p>}
  {error&&<p className="weather-error" role="status">{error}{data?' Showing the last successful update.':''}</p>}
  {data&&<><div className="weather-current"><strong>{Math.round(data.temperatureC!)}°C</strong><span>{data.condition}</span></div>
   <div className="weather-details">{data.feelsLikeC!=null&&<span>Feels like {Math.round(data.feelsLikeC)}°C</span>}{data.humidityPercent!=null&&<span>Humidity {Math.round(data.humidityPercent)}%</span>}{data.windKmh!=null&&<span>Wind {Math.round(data.windKmh)} km/h</span>}</div>
   {!!data.forecast?.length&&<div className="weather-forecast" aria-label="Three-day forecast">{data.forecast.map((day,index)=><div key={day.date}><span>{index===0?'Today':new Date(day.date+'T12:00:00').toLocaleDateString(undefined,{weekday:'short'})}</span><strong>{Math.round(day.maxTemperatureC)}° <small>/ {Math.round(day.minTemperatureC)}°</small></strong><small>{day.condition}</small>{day.rainChance!=null&&<small>Rain {day.rainChance}%</small>}</div>)}</div>}
   {data.checkedAt&&<small className="weather-updated">Updated {new Date(data.checkedAt).toLocaleTimeString(undefined,{hour:'numeric',minute:'2-digit'})}{error?' · Last successful update':''}</small>}
  </>}
  <div className="weather-footer"><a href="https://open-meteo.com/" target="_blank" rel="noreferrer">Weather by Open-Meteo</a>{showChangeLink&&<a href="#/settings">Change location</a>}</div><small>Refreshes automatically every 15 minutes while this page is open.</small>
 </div>
}
