import {chromium} from 'playwright';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import {fileURLToPath} from 'node:url';
import {randomUUID} from 'node:crypto';

// Run only against the disposable local stack described in README.md.
const ui=process.env.PREVIEW_UI_URL||'http://localhost:4310';
const api=process.env.PREVIEW_API_URL||'http://localhost:4810';
for(const [address,port] of [[ui,'4310'],[api,'4810']]){
 const parsed=new URL(address);
 assert.ok(['localhost','127.0.0.1'].includes(parsed.hostname)&&parsed.port===port,'Use the isolated preview test ports, never the live application.');
}
const suffix=randomUUID();
const password=`Test!Aa9${randomUUID()}`;
async function request(path,body,token){
 const response=await fetch(api+'/api'+path,{method:body?'POST':'GET',headers:{...(body?{'Content-Type':'application/json'}:{}),...(token?{Authorization:`Bearer ${token}`}:{})},body:body?JSON.stringify(body):undefined});
 assert.ok(response.ok,`${path}: HTTP ${response.status}`);
 return response.status===204?null:response.json();
}
await request('/auth/register',{email:`admin-${suffix}@example.test`,password,displayName:'Synthetic Preview Admin'});
const admin=(await request('/auth/login',{email:`admin-${suffix}@example.test`,password})).token;
const staffEmail=`staff-${suffix}@example.test`;
await request('/staff',{email:staffEmail,password,displayName:'Synthetic Preview Staff'},admin);
const category=await request('/categories',{name:'Synthetic photo preview'},admin);
const ticket=await request('/requests',{requesterName:'Synthetic requester',issue:'Photo preview verification',location:'Test lab',categoryId:category.id,status:'Pending'},admin);
const browser=await chromium.launch({headless:true,...(process.env.PREVIEW_BROWSER_CHANNEL?{channel:process.env.PREVIEW_BROWSER_CHANNEL}:{})});
const context=await browser.newContext({viewport:{width:1280,height:900},acceptDownloads:true});
const page=await context.newPage();
const failures=[];page.on('pageerror',error=>failures.push(error.message));
const checks=[];
function passed(name){checks.push(name);console.log('PASS '+name)}
await context.addInitScript(()=>{
 const create=URL.createObjectURL.bind(URL),revoke=URL.revokeObjectURL.bind(URL);
 window.__previewUrls=new Set();
 URL.createObjectURL=blob=>{const url=create(blob);window.__previewUrls.add(url);return url};
 URL.revokeObjectURL=url=>{window.__previewUrls.delete(url);revoke(url)};
});
try{
 await page.goto(ui+'/#/requests');
 await page.getByLabel('Email',{exact:true}).fill(staffEmail);
 await page.getByLabel('Password',{exact:true}).fill(password);
 await page.getByRole('button',{name:'Login',exact:true}).click();
 await page.getByRole('heading',{name:'Service requests',exact:true}).waitFor();
 await page.getByRole('button',{name:`Files for request ${ticket.id}`,exact:true}).click();
 const dialog=page.getByRole('dialog');
 for(const name of ['wide.png','portrait.JPG']){
  await dialog.locator('input[type=file]').setInputFiles(fileURLToPath(new URL('./fixtures/'+name,import.meta.url)));
  await dialog.getByRole('button',{name:'Upload file',exact:true}).click();
  await dialog.getByRole('button',{name:'Upload file',exact:true,disabled:true}).waitFor();
  await dialog.locator('.file-list strong').filter({hasText:name}).waitFor();
 }
 await dialog.locator('input[type=file]').setInputFiles({name:'synthetic.pdf',mimeType:'application/pdf',buffer:Buffer.from('%PDF-1.7\nSynthetic preview regression test')});
 await dialog.getByRole('button',{name:'Upload file',exact:true}).click();
 await dialog.locator('.file-list strong').filter({hasText:'synthetic.pdf'}).waitFor();
 assert.equal(await dialog.getByRole('button',{name:'Preview photo',exact:true}).count(),2);
 assert.equal(await page.evaluate(()=>window.__previewUrls.size),0);
 passed('Staff uploads PNG, uppercase JPG and PDF; photos load only on demand');

 const rows=dialog.locator('.attachment-item');
 const wide=rows.filter({hasText:'wide.png'}),portrait=rows.filter({hasText:'portrait.JPG'}),pdf=rows.filter({hasText:'synthetic.pdf'});
 const list=await request(`/requests/${ticket.id}/attachments`,null,admin);
 const wideId=list.find(x=>x.originalName==='wide.png').id;
 const jpgId=list.find(x=>x.originalName==='portrait.JPG').id;
 const widePath=`**/api/requests/${ticket.id}/attachments/${wideId}`;
 const jpgPath=`**/api/requests/${ticket.id}/attachments/${jpgId}`;
 let sawAuth=false;
 await page.route(widePath,async route=>{sawAuth=route.request().headers().authorization?.startsWith('Bearer ')===true;await new Promise(resolve=>setTimeout(resolve,500));await route.continue()});
 await wide.getByRole('button',{name:'Preview photo',exact:true}).click();
 await wide.getByRole('status').waitFor();
 const wideImage=wide.getByRole('img');await wideImage.waitFor();
 await page.waitForFunction(()=>document.querySelector('.attachment-preview img')?.naturalWidth>0);
 assert.ok((await wideImage.getAttribute('src')).startsWith('blob:'));
 assert.ok(sawAuth);
 passed('Authenticated PNG preview decodes in the dialog with loading feedback');
 await wide.getByRole('button',{name:'Hide preview',exact:true}).click();
 await page.waitForFunction(()=>window.__previewUrls.size===0);
 await page.unroute(widePath);
 passed('Hiding a preview releases its temporary object URL');

 let failOnce=true;
 await page.route(jpgPath,async route=>{if(failOnce){failOnce=false;await route.fulfill({status:503,contentType:'application/json',body:'{}'})}else await route.continue()});
 await portrait.getByRole('button',{name:'Preview photo',exact:true}).click();
 await portrait.getByRole('alert').waitFor();
 assert.ok(await portrait.getByRole('button',{name:'Download',exact:true}).isVisible());
 await portrait.getByRole('button',{name:'Try again',exact:true}).click();
 await page.waitForFunction(()=>document.querySelector('.attachment-preview img')?.naturalWidth>0);
 await page.unroute(jpgPath);
 passed('Failed preview retains Download and retry displays the real JPEG');

 await page.setViewportSize({width:390,height:844});
 await page.evaluate(()=>{document.documentElement.dataset.theme='dark'});
 const imageBox=await portrait.getByRole('img').boundingBox();
 const dialogBox=await dialog.boundingBox();
 assert.ok(imageBox&&dialogBox&&imageBox.width<=dialogBox.width&&imageBox.height<=844*.45+1);
 assert.ok(dialogBox.width<=390);
 const evidence=process.env.PREVIEW_EVIDENCE_DIR;
 if(evidence){await fs.mkdir(evidence,{recursive:true});await portrait.getByRole('img').scrollIntoViewIfNeeded();await page.screenshot({path:evidence+'/photo-preview-mobile-dark.png'})}
 passed('Portrait preview stays within the dialog on a 390px dark-mode screen');
 await portrait.getByRole('button',{name:'Hide preview',exact:true}).click();

 await page.setViewportSize({width:1280,height:900});
 await page.evaluate(()=>{document.documentElement.dataset.theme='light'});
 let wrongType=true;
 await page.route(widePath,async route=>{if(wrongType){wrongType=false;await route.fulfill({status:200,contentType:'text/html',body:'<b>Not an image</b>'})}else await route.continue()});
 await wide.getByRole('button',{name:'Preview photo',exact:true}).click();
 await wide.getByRole('alert').waitFor();
 assert.equal(await wide.getByRole('img').count(),0);
 await wide.getByRole('button',{name:'Try again',exact:true}).click();
 await wideImage.waitFor();await page.waitForFunction(()=>document.querySelector('.attachment-preview img')?.naturalWidth>0);
 await page.unroute(widePath);
 if(evidence){await wideImage.scrollIntoViewIfNeeded();await page.screenshot({path:evidence+'/photo-preview-desktop-light.png'})}
 passed('Unexpected content types are refused; retry restores a valid image');
 await dialog.getByRole('button',{name:'Close dialog',exact:true}).click();
 await page.waitForFunction(()=>window.__previewUrls.size===0);
 passed('Closing the Files dialog releases active preview URLs');

 await page.getByRole('button',{name:`Files for request ${ticket.id}`,exact:true}).click();
 let corrupt=true;
 await page.route(widePath,async route=>{if(corrupt){corrupt=false;await route.fulfill({status:200,contentType:'image/png',body:'corrupt image bytes'})}else await route.continue()});
 await wide.getByRole('button',{name:'Preview photo',exact:true}).click();
 await wide.getByRole('alert').waitFor();
 await wide.getByRole('button',{name:'Try again',exact:true}).click();
 await page.waitForFunction(()=>document.querySelector('.attachment-preview img')?.naturalWidth>0);
 await page.unroute(widePath);
 await wide.getByRole('button',{name:'Hide preview',exact:true}).click();
 passed('Image decode failure shows a recoverable error');

 const downloading=page.waitForEvent('download');
 await pdf.getByRole('button',{name:'Download',exact:true}).click();
 const download=await downloading;
 assert.equal(download.suggestedFilename(),'synthetic.pdf');
 assert.equal((await fs.readFile(await download.path())).toString(),'%PDF-1.7\nSynthetic preview regression test');
 assert.equal(await pdf.getByRole('button',{name:'Preview photo',exact:true}).count(),0);
 passed('PDF download still preserves the original bytes and filename');
 assert.equal((await fetch(api+`/api/requests/${ticket.id}/attachments/${wideId}`)).status,401);
 passed('Anonymous attachment access remains unauthorized');
 assert.deepEqual(failures,[]);
 console.log(JSON.stringify({passed:checks.length,checks,browserErrors:failures},null,2));
}finally{await browser.close()}
