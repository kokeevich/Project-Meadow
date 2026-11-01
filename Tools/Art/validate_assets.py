"""Check persisted scene hierarchy, original materials and script metadata."""
from pathlib import Path
import re
root=Path(__file__).resolve().parents[2]/'Assets'
guids={}
for p in root.rglob('*.meta'):
 m=re.search(r'^guid: (\w+)',p.read_text(),re.M)
 if m:
  assert m[1] not in guids, f'Duplicate GUID: {p}'
  guids[m[1]]=p
for p in (root/'_Project').rglob('*.cs'):
 assert Path(str(p)+'.meta').is_file(),p
scene=(root/'_Project/Scenes/Levels/MVP_Farm.unity').read_text()
sections=list(re.finditer(r'^--- !u!(\d+) &(\d+)\n(.*?)(?=^--- !u!|\Z)',scene,re.M|re.S))
ids={m[2] for m in sections}
assert len(ids)==len(sections),'Duplicate scene object ID'
for reference in re.findall(r'\{fileID: (\d+)\}',scene):
 assert reference=='0' or reference in ids, f'Missing local scene reference: {reference}'
for m in sections:
 if m[1]=='23':
  for guid in re.findall(r'guid: (\w+)',m[3]):
   assert guid in guids,f'Missing scene material: {guid}'
assert 'Clickable Prototype Cube' not in scene
for label in ['Meadow - Art','Barn','Hen house','Tree crown','Market lane','Food Plot']:
 assert f'm_Name: {label}\n' in scene,label
print(f'PASS: {len(ids)} scene objects, material references, unique GUIDs, script metadata and art landmarks')
