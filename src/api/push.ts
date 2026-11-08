import { apiRequest } from "@/api/client";
export type PushPreferences = { id:string; mealsEnabled:boolean; choresEnabled:boolean; eventsEnabled:boolean; pantryEnabled:boolean; reminderHour:number };
type Device={id:string;token:string};
const key="nook:push-device";
function identity():Device|null { try {return JSON.parse(localStorage.getItem(key)??"null") as Device|null;} catch{return null;} }
function auth(){return {"X-Push-Token":identity()?.token??""};}
function publicKeyBytes(base64:string):Uint8Array<ArrayBuffer>{
 const decoded=atob(base64.replace(/-/g,"+").replace(/_/g,"/"));
 const bytes=new Uint8Array(new ArrayBuffer(decoded.length));
 for(let i=0;i<decoded.length;i++)bytes[i]=decoded.charCodeAt(i);
 return bytes;
}
export async function currentSubscription():Promise<PushSubscription|null>{
 if(!("serviceWorker" in navigator))return null;
 const registration=await navigator.serviceWorker.getRegistration("/");
 return registration?.pushManager.getSubscription()??null;
}
export async function enablePush():Promise<PushPreferences>{
 if(!("serviceWorker" in navigator)||!("PushManager" in window)||!("Notification" in window))
  throw new Error("Web Push is unsupported. On iPhone add Nook to your Home Screen first.");
 if(Notification.permission==="denied")throw new Error("Allow Nook notifications in your device settings.");
 const {publicKey}=await apiRequest<{publicKey:string}>("/api/push/public-key");
 const registration=await navigator.serviceWorker.register("/push-sw.js",{scope:"/"});
 await navigator.serviceWorker.ready;
 if(await Notification.requestPermission()!=="granted")throw new Error("Notification permission was not granted.");
 let subscription=await registration.pushManager.getSubscription();
 if(!subscription)subscription=await registration.pushManager.subscribe({userVisibleOnly:true,applicationServerKey:publicKeyBytes(publicKey)});
 const result=await apiRequest<{id:string;token:string;preferences:PushPreferences}>("/api/push/subscriptions",{
  method:"POST",body:JSON.stringify(subscription.toJSON())
 });
 localStorage.setItem(key,JSON.stringify({id:result.id,token:result.token}));
 return result.preferences;
}
export async function getPushPreferences():Promise<PushPreferences|null>{
 const device=identity();if(!device)return null;
 return apiRequest<PushPreferences>(`/api/push/subscriptions/${device.id}`,{headers:auth()});
}
export async function updatePushPreferences(p:PushPreferences):Promise<PushPreferences>{
 return apiRequest<PushPreferences>(`/api/push/subscriptions/${p.id}`,{method:"PUT",headers:auth(),body:JSON.stringify(p)});
}
export async function testPush():Promise<void>{
 const device=identity();if(!device)throw new Error("Enable notifications first.");
 await apiRequest<void>(`/api/push/subscriptions/${device.id}/test`,{method:"POST",headers:auth()});
}
export async function disablePush():Promise<void>{
 const device=identity();
 if(device)await apiRequest<void>(`/api/push/subscriptions/${device.id}`,{method:"DELETE",headers:auth()});
 const sub=await currentSubscription();if(sub)await sub.unsubscribe();
 localStorage.removeItem(key);
}
