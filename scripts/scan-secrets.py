from pathlib import Path
import re,sys
root=Path(__file__).resolve().parents[1]
patterns=[r'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----',r'gh[pousr]_[A-Za-z0-9]{30,}',r'AKIA[A-Z0-9]{16}']
findings=[]
for p in root.rglob('*'):
 if not p.is_file() or any(x in {'.git','node_modules','bin','obj','dist','TestResults','evidence','uploads'} for x in p.relative_to(root).parts):continue
 if p.name=='.env':continue
 if p.suffix.lower() in {'.pdf','.png','.jpg','.zip'}:continue
 text=p.read_text(encoding='utf-8',errors='ignore')
 if any(re.search(pattern,text) for pattern in patterns):findings.append(str(p.relative_to(root)))
print('Pattern-based secret scan: '+('FAIL: '+', '.join(findings) if findings else 'no findings'))
sys.exit(bool(findings))
