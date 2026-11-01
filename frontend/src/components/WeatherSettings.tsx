import {useEffect,useRef,useState,FormEvent} from 'react';
import {api,friendlyError} from '../api';
import {useWeatherLocation,WeatherLocation} from '../weatherLocation';
import {Notice} from './UI';
import WeatherCard from './WeatherCard';
type Place={name:string;region:string;country:string;latitude:number;longitude:number};
export default function WeatherSettings({accountId}:{accountId:string}){
 const {location,save,reset}=useWeatherLocation(accountId);const [query,setQuery]=useState('');const [places,setPlaces]=useState<Place[]>([]);const [searched,setSearched]=useState(false);const [searching,setSearching]=useState(false);const [locating,setLocating]=useState(false);const [error,setError]=useState('');const [notice,setNotice]=useState('');const active=useRef(true);const searchVersion=useRef(0);
 useEffect(()=>{active.current=true;return ()=>{active.current=false;searchVersion.current++}},[]);
 function saveLocation(value:WeatherLocation){try{save(value);setError('');setNotice('Weather location updated. The forecast now uses this location.');setPlaces([]);setSearched(false)}catch{setError('Your browser could not save the location. Check browser storage settings and try again.')}}
 function useCurrentLocation(){
  setError('');setNotice('');if(!window.isSecureContext||!navigator.geolocation){setError('Device location is not available here. Choose a city below instead.');return}setPlaces([]);setSearched(false);setLocating(true);
  navigator.geolocation.getCurrentPosition(position=>{if(!active.current)return;setLocating(false);saveLocation({version:1,name:'Current device location',latitude:position.coords.latitude,longitude:position.coords.longitude,source:'device'})},problem=>{if(!active.current)return;setLocating(false);setError(problem.code===1?'Location access was not allowed. You can choose a city below instead.':problem.code===3?'Finding your location took too long. Try again or choose a city below.':'Your device could not find its location. Choose a city below instead.')},{enableHighAccuracy:false,timeout:12000,maximumAge:300000});
 }
 async function search(e:FormEvent){e.preventDefault();if(query.trim().length<2){setError('Enter at least two characters of a city name.');return}const version=++searchVersion.current;setSearching(true);setError('');setNotice('');setPlaces([]);setSearched(false);
  try{const r=await api.get<{available:boolean;locations:Place[];message?:string}>('/weather/locations',{params:{query:query.trim()}});if(!active.current||version!==searchVersion.current)return;if(!r.data.available){setError(r.data.message||'Location search is temporarily unavailable.');return}setPlaces(r.data.locations);setSearched(true)}catch(e){if(active.current&&version===searchVersion.current)setError(friendlyError(e))}finally{if(active.current&&version===searchVersion.current)setSearching(false)}
 }
 return <section className="panel" aria-labelledby="weather-settings-heading"><div className="section-kicker">WEATHER NEAR YOU</div><h2 id="weather-settings-heading">Weather location</h2><p>Choose a city or use your device location for current conditions and a three-day forecast.</p>
  {error&&<Notice error>{error}</Notice>}{notice&&<Notice>{notice}</Notice>}
  <div className="weather-location-summary"><span>Current selection</span><strong>{location.name}</strong><small>{location.source==='device'?'Approximate location from your device':location.source==='default'?'Default campus weather location':'City selected by you'}</small></div>
  <button className="secondary" disabled={locating||searching} onClick={useCurrentLocation}>{locating?'Finding your location…':'Use my current location'}</button>
  <p className="field-hint">Your browser will ask for permission if needed. Approximate coordinates are sent to our server and Open-Meteo for weather. The selection is saved only for this account on this browser. Your movements are not tracked.</p>
  <form onSubmit={search} className="weather-search"><label>Or search for a city or municipality<input type="search" minLength={2} maxLength={100} required placeholder="For example: Legazpi, Philippines" value={query} onChange={e=>{setQuery(e.target.value);setSearched(false);setPlaces([]);searchVersion.current++;setSearching(false)}}/></label><button disabled={searching||locating}>{searching?'Searching…':'Search cities'}</button></form>
  {searched&&places.length===0&&<p role="status">No matching cities were found. Try another city name or add the country.</p>}
  {places.length>0&&<ul className="weather-city-results" aria-label="Matching cities">{places.map(place=><li key={`${place.latitude}:${place.longitude}:${place.name}`}><div><strong>{place.name}</strong><small>{[place.region,place.country].filter(Boolean).join(', ')}</small></div><button className="secondary small" onClick={()=>saveLocation({version:1,name:[place.name,place.region,place.country].filter(Boolean).join(', ').slice(0,150),latitude:place.latitude,longitude:place.longitude,source:'city'})} aria-label={`Use ${place.name}, ${place.region}, ${place.country}`}>Use this location</button></li>)}</ul>}
  {location.source!=='default'&&<button className="link" disabled={locating||searching} onClick={()=>{try{reset();setNotice('Weather location reset to Naga City.');setError('')}catch{setError('Your browser could not reset the location.')}}}>Reset to Naga City</button>}
  <WeatherCard accountId={accountId} showChangeLink={false}/>
 </section>
}
