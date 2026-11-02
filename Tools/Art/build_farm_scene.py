"""Original primitive diorama. Re-run only before editing the generated scene section manually."""
from pathlib import Path
import math,re,uuid,random
ROOT=Path(__file__).resolve().parents[2]; r=ROOT/'Assets/_Project'; scene=r/'Scenes/Levels/MVP_Farm.unity'
def guid(path):return re.search(r'^guid: (\w+)',Path(str(path)+'.meta').read_text(),re.M)[1]
def meta(path,kind):
 p=Path(str(path)+'.meta')
 if not p.exists():p.write_text(f'fileFormatVersion: 2\nguid: {uuid.uuid4().hex}\n{kind}:\n  externalObjects: {{}}\n')
colors={'Meadow':'8EAF64','Earth':'8B6546','Cream':'F7E8C6','Barn':'BF5D48','Roof':'485E60','Wood':'B78756','Leaves':'5D874F','LeavesLight':'A2B866','Path':'DCC49A','Soil':'72503D','Gold':'E8B44F','Teal':'548F86','Glass':'A5D5D1','Dark':'353E41','Pink':'E6AC9C'}
mats={}
for name,hex in colors.items():
 p=r/f'Art/Materials/{name}.mat';rgb=[int(hex[i:i+2],16)/255 for i in (0,2,4)]
 p.write_text(f'''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!21 &2100000
Material:
  serializedVersion: 8
  m_ObjectHideFlags: 0
  m_Name: {name}
  m_Shader: {{fileID: 4800000, guid: 933532a4fcc9baf4fa0491de14d08ed7, type: 3}}
  m_ValidKeywords: []
  m_InvalidKeywords: []
  m_LightmapFlags: 4
  m_CustomRenderQueue: -1
  m_SavedProperties:
    serializedVersion: 3
    m_TexEnvs: []
    m_Ints: []
    m_Floats:
    - _Surface: 0
    - _Smoothness: 0.12
    m_Colors:
    - _BaseColor: {{r: {rgb[0]}, g: {rgb[1]}, b: {rgb[2]}, a: 1}}
''');meta(p,'NativeFormatImporter');mats[name]=guid(p)
blocks=[];transforms=[];childmap={};index=2000000
# All decoration has no collision except architectural footprints; NavMesh sees those footprints.
def part(name,pos,scale,color,shape='cube',angle=0,axis='z',solid=False,parent=1999991):
 global index
 n=index;index+=10
 if parent==1999991:transforms.append(n+1)
 else:childmap.setdefault(parent,[]).append(n+1)
 q=[0.,0.,0.];q['xyz'.index(axis)]=math.sin(math.radians(angle)/2);w=math.cos(math.radians(angle)/2)
 mesh={'cube':10202,'sphere':10207,'cylinder':10206}[shape]
 comps=[n+1,n+2,n+3]+([n+4] if solid else [])
 b=f'''--- !u!1 &{n}
GameObject:
  m_ObjectHideFlags: 0
  serializedVersion: 6
  m_Component:
'''+''.join(f'  - component: {{fileID: {c}}}\n' for c in comps)+f'''  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{n+1}
Transform:
  m_GameObject: {{fileID: {n}}}
  serializedVersion: 2
  m_LocalRotation: {{x: {q[0]}, y: {q[1]}, z: {q[2]}, w: {w}}}
  m_LocalPosition: {{x: {pos[0]}, y: {pos[1]}, z: {pos[2]}}}
  m_LocalScale: {{x: {scale[0]}, y: {scale[1]}, z: {scale[2]}}}
  m_Children: []
  m_Father: {{fileID: {parent}}}
--- !u!33 &{n+2}
MeshFilter:
  m_GameObject: {{fileID: {n}}}
  m_Mesh: {{fileID: {mesh}, guid: 0000000000000000e000000000000000, type: 0}}
--- !u!23 &{n+3}
MeshRenderer:
  m_GameObject: {{fileID: {n}}}
  m_Enabled: 1
  m_CastShadows: 1
  m_ReceiveShadows: 1
  m_Materials:
  - {{fileID: 2100000, guid: {mats[color]}, type: 2}}
'''
 if solid:b+=f'''--- !u!65 &{n+4}
BoxCollider:
  m_GameObject: {{fileID: {n}}}
  m_IsTrigger: 0
  m_Enabled: 1
  serializedVersion: 3
  m_Size: {{x: 1, y: 1, z: 1}}
  m_Center: {{x: 0, y: 0, z: 0}}
'''
 blocks.append(b)
 return n

def empty(name,pos,parent=1999991,script=None):
 global index
 n=index;index+=10
 if parent==1999991:transforms.append(n+1)
 else:childmap.setdefault(parent,[]).append(n+1)
 b=f"""--- !u!1 &{n}
GameObject:
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {n+1}}}
"""+(f"  - component: {{fileID: {n+5}}}\n" if script else '')+f"""  m_Name: {name}
  m_IsActive: 1
--- !u!4 &{n+1}
Transform:
  m_GameObject: {{fileID: {n}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: {pos[0]}, y: {pos[1]}, z: {pos[2]}}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_Children: []
  m_Father: {{fileID: {parent}}}
"""
 if script:b+=f"""--- !u!114 &{n+5}
MonoBehaviour:
  m_GameObject: {{fileID: {n}}}
  m_Enabled: 1
  m_Script: {{fileID: 11500000, guid: {script}, type: 3}}
"""
 blocks.append(b);return n

part('Island soil edge',(3,-.48,0),(19,.65,11),'Earth')
part('Market lane',(3,.015,-3.9),(18.5,.035,1.25),'Path')
# Barn, facing the player, with pitched roof and cream trim.
part('Barn',(-3,1.05,2.3),(2.8,2.1,2.3),'Barn',solid=True)
for side in (-1,1):
 part('Barn roof',(-3+side*.78,2.42,2.3),(1.9,.18,2.8),'Roof',angle=-side*32)
 part('Barn corner',(-3+side*1.32,1.05,1.12),(.12,2.1,.12),'Cream')
part('Barn lintel',(-3,1.95,1.1),(2.8,.14,.12),'Cream')
part('Barn door',(-3,.78,1.1),(1.5,1.55,.1),'Wood')
for side in (-1,1):
 part('Door cross brace',(-3,.78,1.02),(.09,1.95,.08),'Cream',angle=side*43)
part('Loft window',(-3,2.23,1.12),(.5,.4,.08),'Glass')
# Coop / storage shed.
part('Hen house',(4,.75,2.5),(1.8,1.5,1.65),'Gold',solid=True)
for side in (-1,1):part('Coop roof',(4+side*.5,1.85,2.5),(1.25,.16,2),'Roof',angle=-side*30)
part('Hen door',(4,.45,1.64),(.6,.8,.08),'Dark')
part('Hen ramp',(4,.14,1.05),(.7,.16,1.1),'Wood',angle=12,axis='x')
# Crates near the barn.
for i in range(3):
 x=-4.7+i*.55
 part('Harvest crate',(x,.25,.8),(.46,.5,.46),'Wood')
 for side in (-1,1):part('Crate band',(x,.25,.8+side*.24),(.48,.06,.04),'Cream')
# Fence back and sides, leave front clear for the truck.
for x in range(-6,13,2):
 part('Fence post',(x,.5,4.9),(.16,1,.16),'Cream')
 if x<12:
  for y in (.35,.73):part('Fence rail',(x+1,y,4.9),(2,.12,.12),'Cream')
for x in (-6.1,12.1):
 for z in (-2,0,2,4):
  part('Fence post',(x,.5,z),(.16,1,.16),'Cream')
  if z<4:
   for y in (.35,.73):part('Fence rail',(x,y,z+1),(.12,.12,2),'Cream')
# Trees are deliberately outside the useful play area.
for x,z,h in [(-5,3.7,1.8),(5.2,4,1.9),(-5.2,-1.5,1.6),(11.2,-1,1.6)]:
 part('Tree trunk',(x,h*.45,z),(.24,h,.24),'Wood',solid=True)
 for dx,dy,dz,size,col in [(0,0,0,1.5,'Leaves'),(-.4,.15,.15,1.1,'LeavesLight'),(.35,.45,0,1.15,'Leaves')]:
  part('Tree crown',(x+dx,h+dy,z+dz),(size,size*.9,size),col,'sphere')
# Flower beds / tufts in deterministic positions, no gameplay colliders.
rng=random.Random(47)
for i in range(28):
 x=rng.choice([-1,1])*rng.uniform(4.6,5.7);z=rng.uniform(-2.8,4.2)
 part('Meadow tuft',(x,.08,z),(.18,.16,.18),'Leaves','sphere')
 if i%3==0:part('Wildflower',(x,.22,z),(.13,.13,.13),'Cream' if i%2 else 'Pink','sphere')
# Raised garden frame around the existing food plot.
for z in (-1.6,1.0):part('Garden timber',(4,.18,z),(2.85,.22,.13),'Wood')
for x in (2.65,5.35):part('Garden timber',(x,.18,-.3),(.13,.22,2.7),'Wood')
for i in range(5):part('Soil furrow',(4,.309,-1.25+i*.47),(2.25,.022,.05),'Earth')
# Stepping stones toward the barn.
for i in range(4):part('Stepping stone',(-2.7,.04,-1.7+i*.6),(.55,.08,.4),'Path',angle=i*9,axis='y')
# New production district, a separate editable hierarchy.
mill=empty('Windmill',(8,0,2),script=guid(r/'Scripts/Production/ProductionBuilding.cs'))
part('Mill tower',(0,1.5,0),(2.2,3,2.2),'Cream',solid=True,parent=mill+1)
part('Mill cap',(0,3.15,0),(2.5,.35,2.5),'Roof',parent=mill+1)
part('Mill door',(0,.6,-1.12),(.7,1.2,.06),'Wood',parent=mill+1)
rotor=empty('Mill rotor',(0,2.5,-1.3),parent=mill+1)
part('Axle',(0,0,0),(.35,.35,.35),'Wood','sphere',parent=rotor+1)
for angle in (0,90):part('Windmill sail',(0,0,0),(.23,4,.1),'Wood',angle=angle,parent=rotor+1)
for angle in (0,90,180,270):
 a=math.radians(angle)
 part('Sail cloth',(-math.sin(a)*1.1,math.cos(a)*1.1,-.07),(.55,1.1,.06),'Cream',angle=angle,parent=rotor+1)
grain=empty('Wheat plot',(7.5,0,-1.2),script=guid(r/'Scripts/Farming/GrainPlot.cs'))
part('Wheat soil',(0,.15,0),(2.5,.3,2.2),'Soil',solid=True,parent=grain+1)
for x in (-1.3,1.3):part('Wheat border',(x,.18,0),(.12,.22,2.4),'Wood',parent=grain+1)
for z in (-1.15,1.15):part('Wheat border',(0,.18,z),(2.7,.22,.12),'Wood',parent=grain+1)
wheat=empty('Wheat',(0,.3,0),parent=grain+1)
for row in range(3):
 for col in range(5):
  x=-.9+col*.45;z=-.65+row*.65
  part('Wheat stalk',(x,.22,z),(.04,.44,.04),'Leaves',parent=wheat+1)
  part('Wheat ear',(x,.5,z),(.12,.26,.12),'Gold','sphere',parent=wheat+1)
for i in range(4):part('Mill path',(8,.035,-2.5+i*.7),(.7,.07,.45),'Path')
# Dairy shed and a dedicated cattle feed patch.
dairy=empty('Cheese maker',(10.8,0,2.6),script=guid(r/'Scripts/Production/ProductionBuilding.cs'))
blocks[-1]+='  kind: 1\n'
part('Dairy walls',(0,.9,0),(1.8,1.8,1.8),'Teal',solid=True,parent=dairy+1)
for side in (-1,1):
 part('Dairy roof',(side*.5,2.08,0),(1.3,.14,2.1),'Roof',angle=-side*30,parent=dairy+1)
part('Dairy door',(-.4,.6,-.92),(.55,1.2,.06),'Cream',parent=dairy+1)
part('Dairy window',(.45,1.15,-.92),(.45,.5,.06),'Glass',parent=dairy+1)
part('Cheese sign',(.45,.55,-.94),(.45,.2,.08),'Gold',parent=dairy+1)
cattle=part('Cow feed',(0,.15,2),(2,.3,2),'Soil',solid=True)
blocks[-1]=blocks[-1].replace(f'  - component: {{fileID: {cattle+4}}}',f'  - component: {{fileID: {cattle+4}}}\n  - component: {{fileID: {cattle+5}}}')
blocks[-1]+=f"""--- !u!114 &{cattle+5}
MonoBehaviour:
  m_GameObject: {{fileID: {cattle}}}
  m_Enabled: 1
  m_Script: {{fileID: 11500000, guid: {guid(r/'Scripts/Farming/FoodPlot.cs')}, type: 3}}
  kind: 1
  growthSeconds: 8
"""
for z in (.95,3.05):part('Pasture border',(0,.18,z),(2.2,.22,.12),'Wood')
for x in (-1.05,1.05):part('Pasture border',(x,.18,2),(.12,.22,2.2),'Wood')

# Populate child lists for nested art groups.
for i,b in enumerate(blocks):
 for parent,children in childmap.items():
  marker=f'--- !u!4 &{parent}\n'
  if marker in b:
   start=b.index(marker);end=b.find('--- !u!',start+5)
   if end<0:end=len(b)
   section=b[start:end].replace('  m_Children: []','  m_Children:\n'+'\n'.join(f'  - {{fileID: {c}}}' for c in children))
   b=b[:start]+section+b[end:]
 blocks[i]=b

# Group lets the scene stay editable.
group='''--- !u!1 &1999990
GameObject:
  serializedVersion: 6
  m_Component:
  - component: {fileID: 1999991}
  m_Name: Meadow - Art
  m_IsActive: 1
--- !u!4 &1999991
Transform:
  m_GameObject: {fileID: 1999990}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Children:
'''+''.join(f'  - {{fileID: {n}}}\n' for n in transforms)+'  m_Father: {fileID: 0}\n'
s=scene.read_text()
# Keep the gameplay collider aligned with its generated garden frame.
a=s.index('--- !u!4 &910021');b=s.index('--- !u!',a+5)
s=s[:a]+re.sub(r'm_LocalPosition: \{[^}]+\}', 'm_LocalPosition: {x: 4, y: 0.15, z: -0.3}', s[a:b])+s[b:]
# Idempotently remove generated art and obsolete test cube.
sections=re.split(r'(?=^--- !u!)',s,flags=re.M)
s=''.join(section for section in sections if not (m:=re.match(r'--- !u!\d+ &(\d+)',section)) or not (1999990<=int(m[1])<3000000 or 910010<=int(m[1])<=910015))
s=re.sub(r'^  - \{fileID: (?:1999991|910011)\}\n','',s,flags=re.M)
s=s.replace('m_LocalScale: {x: 10, y: 0.3, z: 10}','m_LocalScale: {x: 19, y: 0.3, z: 11}')
s=s.replace('m_LocalScale: {x: 13, y: 0.3, z: 11}', 'm_LocalScale: {x: 19, y: 0.3, z: 11}').replace('m_LocalPosition: {x: 0, y: -0.15, z: 0}', 'm_LocalPosition: {x: 3, y: -0.15, z: 0}')
s=s.replace('m_LocalPosition: {x: 0, y: 12, z: -12}','m_LocalPosition: {x: 0, y: 14, z: -14}').replace('field of view: 60','field of view: 48')
s=s.replace('m_ClearFlags: 1','m_ClearFlags: 2').replace('m_BackGroundColor: {r: 0.19215687, g: 0.3019608, b: 0.4745098, a: 0}','m_BackGroundColor: {r: 0.73, g: 0.81, b: 0.76, a: 1}')
s=s.replace('m_ColorTemperature: 5000','m_ColorTemperature: 5500')
a=s.index('--- !u!23 &910003');b=s.index('--- !u!',a+5)
s=s[:a]+re.sub(r'guid: \w+',f'guid: {mats["Meadow"]}',s[a:b])+s[b:]
s=s.replace('--- !u!1660057539',group+''.join(blocks)+'--- !u!1660057539')+'  - {fileID: 1999991}\n'
scene.write_text(s)
print(f'Created {len(blocks)} original scene pieces and {len(mats)} materials')
