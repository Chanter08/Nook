import { useEffect, useState } from "react";
import { Bell } from "lucide-react";
import { currentSubscription, disablePush, enablePush, getPushPreferences, testPush, updatePushPreferences } from "@/api/push";
import type { PushPreferences } from "@/api/push";
const categories=[["mealsEnabled","Meal planner"],["choresEnabled","Chore reminders"],["eventsEnabled","Special events"],["pantryEnabled","Pantry expiry"]] as const;
export default function NotificationSettings(){
 const [prefs,setPrefs]=useState<PushPreferences|null>(null);
 const [busy,setBusy]=useState(false);
 const [status,setStatus]=useState("");
 const supported=typeof window!=="undefined"&&"serviceWorker" in navigator&&"PushManager" in window&&"Notification" in window;
 useEffect(()=>{let active=true;void(async()=>{
  try{if(!await currentSubscription())return;const p=await getPushPreferences();if(active)setPrefs(p);}
  catch{if(active)setStatus("Re-enable notifications on this device.");}
 })();return()=>{active=false;};},[]);
 async function run(work:()=>Promise<void>){setBusy(true);setStatus("");try{await work();}catch(e){setStatus(e instanceof Error?e.message:"Something went wrong.");}finally{setBusy(false);}}
 async function save(next:PushPreferences){await run(async()=>{setPrefs(await updatePushPreferences(next));setStatus("Preferences saved.");});}
 return <section className="mt-10 rounded-3xl border border-stone-200 p-5 dark:border-white/10">
  <div className="flex items-center gap-2"><Bell size={20}/><h2 className="text-xl font-semibold">Notifications</h2></div>
  <p className="mt-2 text-sm text-stone-500 dark:text-stone-400">Enable on each device. Reminders arrive at the selected hour in Malta time. On iPhone, launch Nook from its Home Screen icon.</p>
  {!supported&&<p className="mt-3 text-sm text-amber-700">Web Push is unavailable in this browser.</p>}
  {!prefs?<button type="button" disabled={!supported||busy} onClick={()=>void run(async()=>{setPrefs(await enablePush());setStatus("Notifications enabled.");})} className="mt-4 rounded-full bg-emerald-900 px-4 py-2 text-sm text-white disabled:opacity-50">Enable notifications</button>:
   <div className="mt-5 space-y-4">
    {categories.map(([key,label])=><label key={key} className="flex items-center justify-between gap-4 text-sm"><span>{label}</span>
      <input type="checkbox" disabled={busy} checked={prefs[key]} onChange={e=>void save({...prefs,[key]:e.target.checked})} className="h-5 w-5 accent-emerald-800"/></label>)}
    <label className="flex items-center justify-between gap-3 text-sm"><span>Reminder hour (Malta)</span>
     <select disabled={busy} value={prefs.reminderHour} onChange={e=>void save({...prefs,reminderHour:Number(e.target.value)})}
       className="rounded-xl border border-stone-300 bg-transparent px-3 py-2 dark:border-white/20">
       {Array.from({length:24},(_,h)=><option key={h} value={h}>{String(h).padStart(2,"0")}:00</option>)}
     </select></label>
    <div className="flex flex-wrap gap-3">
     <button type="button" disabled={busy} onClick={()=>void run(async()=>{await testPush();setStatus("Test push sent.");})} className="rounded-full border px-4 py-2 text-sm dark:border-white/20">Send test</button>
     <button type="button" disabled={busy} onClick={()=>void run(async()=>{await disablePush();setPrefs(null);setStatus("Notifications disabled.");})} className="rounded-full border px-4 py-2 text-sm dark:border-white/20">Disable on this device</button>
    </div>
   </div>}
  {status&&<p role="status" className="mt-3 text-sm text-stone-500">{status}</p>}
 </section>;
}
