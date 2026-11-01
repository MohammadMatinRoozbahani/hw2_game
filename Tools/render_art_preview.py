                                                                             
                                                                                     
                                                                                 
   
import bpy, json, math, sys
from pathlib import Path
from mathutils import Vector, Matrix, Euler

args=sys.argv[sys.argv.index('--')+1:]
DATA=json.loads(Path(args[0]).read_text())
OUT=Path(args[1]);OUT.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
materials={}
for item in DATA['materials']:
    mat=bpy.data.materials.new(item['name']);mat.use_nodes=True
    bsdf=mat.node_tree.nodes.get('Principled BSDF');c=item['color'];rgb=(c['x'],c['y'],c['z'],1)
    bsdf.inputs['Base Color'].default_value=rgb
    bsdf.inputs['Metallic'].default_value=item['metallic'];bsdf.inputs['Roughness'].default_value=1-item['smoothness']
    bsdf.inputs['Coat Weight'].default_value=.15
    if item['glow']:
        bsdf.inputs['Emission Color'].default_value=rgb;bsdf.inputs['Emission Strength'].default_value=item['glow']
    materials[item['name']]=mat

meshes={}
def get_mesh(kind):
    if kind in meshes:return meshes[kind]
    if kind=='RoundBox':
        bpy.ops.mesh.primitive_cube_add(size=1)
        obj=bpy.context.object;mod=obj.modifiers.new('Rounded toy corners','BEVEL');mod.width=.17;mod.segments=4
        bpy.context.view_layer.objects.active=obj;bpy.ops.object.modifier_apply(modifier=mod.name)
    elif kind=='Sphere':bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=12,radius=.5)
    elif kind=='Cylinder':bpy.ops.mesh.primitive_cylinder_add(vertices=20,radius=.5,depth=1)
    elif kind=='Cone':bpy.ops.mesh.primitive_cone_add(vertices=24,radius1=.5,radius2=0,depth=1)
    elif kind=='Torus':bpy.ops.mesh.primitive_torus_add(major_segments=40,minor_segments=8,major_radius=.46,minor_radius=.04)
    else:bpy.ops.mesh.primitive_cube_add(size=1)
    obj=bpy.context.object
                                                                       
    if kind=='Torus':
        obj.rotation_euler[0]=math.pi/2;bpy.ops.object.transform_apply(location=False,rotation=True,scale=False)
                                                                                            
    mesh=obj.data;mesh.name='Shared '+kind
    for poly in mesh.polygons:poly.use_smooth=kind in ('RoundBox','Sphere','Torus')
    meshes[kind]=mesh;bpy.data.objects.remove(obj,do_unlink=True)
    return mesh

def recipe(parts,offset=(0,0,0),prefix=''):
    created=[]
    for n,p in enumerate(parts):
        mesh=get_mesh(p['shape']);obj=bpy.data.objects.new(prefix+str(n),mesh);bpy.context.collection.objects.link(obj)
        a=p['position'];s=p['scale'];r=p['rotation']
                                                                        
        obj.location=(a['x']+offset[0],a['z']+offset[2],a['y']+offset[1])
        obj.scale=(s['x'],s['z'],s['y'])
        basis=Matrix(((1,0,0),(0,0,1),(0,1,0)))
        rotate=Euler(tuple(math.radians(r[k]) for k in ('x','y','z')),'ZXY').to_matrix()
        obj.rotation_mode='QUATERNION';obj.rotation_quaternion=(basis @ rotate @ basis).to_quaternion()
        obj.data=obj.data.copy();obj.data.materials.clear();obj.data.materials.append(materials[p['material']])
        created.append(obj)
    return created

root=bpy.data.objects.new('Unity Y-up to Blender Z-up',None);bpy.context.collection.objects.link(root)
root.rotation_euler[0]=0

def add(parts,offset=(0,0,0),prefix=''):
    result=recipe(parts,offset,prefix)
    for obj in result:obj.parent=root
    return result

car=add(DATA['car'],prefix='Mochi ')
road_objects=[]
for i,z in enumerate((-24,24,72,120)):
    road_objects+=add(DATA['road'+str(i%3)],(0,0,z),'Block '+str(i)+' ')
backdrop=add(DATA['backdrop'],prefix='Backdrop ')
wheel=add(DATA['ferris'],(-31,13.5,118),'Ferris ')
traffic=add(DATA['traffic0'],(-3.2,0,27),'Traffic ')+add(DATA['traffic1'],(3.2,0,47),'Bunny ')+add(DATA['traffic2'],(0,0,94),'Buddy ')
                                                                                
def star(x,z):
    verts=[]
    for side in (-.12,.12):
        for i in range(10):
            a=math.pi/2+i*math.pi/5;r=.5 if i%2==0 else .245
            verts.append((x+math.cos(a)*r,1.08+math.sin(a)*r,z+side))
    verts.extend([(x,1.08,z-.12),(x,1.08,z+.12)])
    faces=[]
    for i in range(10):
        j=(i+1)%10;faces.extend([(20,j,i),(21,i+10,j+10),(i,j,j+10,i+10)])
    mesh=bpy.data.meshes.new('Lucky star');mesh.from_pydata([(v[0],v[2],v[1]) for v in verts],[],[tuple(reversed(f)) for f in faces]);mesh.materials.append(materials['CoinGlow'])
    obj=bpy.data.objects.new('Collectible star',mesh);bpy.context.collection.objects.link(obj);obj.parent=root
    add([{'shape':'Torus','material':'CoinGlow','position':{'x':x,'y':1.08,'z':z},'scale':{'x':1.20,'y':1.20,'z':1.20},'rotation':{'x':0,'y':0,'z':0}}],prefix='Coin rim ')
    return obj
stars=[star(0,z) for z in (9,12,15,18,22,27,31,36)]+[star(-3.2,z) for z in (46,50,54)]

                                                                               
import random
random.seed(402531387)
for cx,cy,cz,mat,pattern in [(-15,34,90,'PinkGlow',0),(15,39,110,'WarmGlow',1)]:
    for i in range(100):
        a=i*math.pi*2/100
        if pattern==0:
            vx=16*math.sin(a)**3*.57;vy=(13*math.cos(a)-5*math.cos(2*a)-2*math.cos(3*a)-math.cos(4*a))*.57
        else:
            r=7+3*math.cos(a*5);vx=math.cos(a)*r;vy=math.sin(a)*r
        add([{'shape':'Sphere','material':mat,'position':{'x':cx+vx*.65,'y':cy+vy*.65,'z':cz},'scale':{'x':.19,'y':.19,'z':.19},'rotation':{'x':0,'y':0,'z':0}}],prefix='Firework ')
for i in range(85):
    x=random.uniform(-180,180);y=random.uniform(30,105);z=random.uniform(220,290)
    add([{'shape':'Sphere','material':'WarmGlow','position':{'x':x,'y':y,'z':z},'scale':{'x':.22,'y':.22,'z':.22},'rotation':{'x':0,'y':0,'z':0}}],prefix='Starfield ')

scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=48;scene.cycles.use_denoising=True
scene.world.use_nodes=True;world=scene.world.node_tree.nodes.get('Background');world.inputs[0].default_value=(.075,.09,.18,1);world.inputs[1].default_value=.6
scene.view_settings.view_transform='AgX'
try:scene.view_settings.look='AgX - Medium High Contrast'
except:pass
scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG'

def pos(p):return (p[0],p[2],p[1])
def sun(name,rotation,energy,color):
    data=bpy.data.lights.new(name,'SUN');data.energy=energy;data.angle=.18;data.color=color
    obj=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(obj);obj.rotation_euler=rotation
sun('Warm moon key',(math.radians(28),math.radians(-24),math.radians(-18)),2.1,(1,.88,.78))
sun('Blue fill',(math.radians(60),math.radians(15),math.radians(150)),.7,(.55,.69,1))
                                                              
for x in (-7.1,7.1):
    data=bpy.data.lights.new('Warm festival practical','POINT');data.energy=190;data.color=(1,.67,.33);data.shadow_soft_size=1
    obj=bpy.data.objects.new('Warm festival practical',data);bpy.context.collection.objects.link(obj);obj.location=pos((x,5.2,12))
cam_data=bpy.data.cameras.new('Art review camera');cam=bpy.data.objects.new('Art review camera',cam_data);bpy.context.collection.objects.link(cam);scene.camera=cam

def camera(location,target,lens):
    cam.location=pos(location);direction=Vector(pos(target))-cam.location;cam.rotation_euler=direction.to_track_quat('-Z','Y').to_euler();cam_data.lens=lens
                                                                                 
scene.use_nodes=True;nodes=scene.node_tree.nodes;nodes.clear();rl=nodes.new('CompositorNodeRLayers');gl=nodes.new('CompositorNodeGlare');gl.glare_type='FOG_GLOW';gl.quality='MEDIUM';gl.threshold=1.5;gl.size=7
out=nodes.new('CompositorNodeComposite');scene.node_tree.links.new(rl.outputs['Image'],gl.inputs['Image']);scene.node_tree.links.new(gl.outputs['Image'],out.inputs['Image'])

camera(tuple(DATA['camera'][k] for k in ('x','y','z')),tuple(DATA['look'][k] for k in ('x','y','z')),36/(2*(1600/900)*math.tan(math.radians(61)/2)))
scene.render.resolution_x=1600;scene.render.resolution_y=900;scene.render.filepath=str(OUT/'Moonpop_world_preview.png');bpy.ops.render.render(write_still=True)
                                                                             
for obj in traffic+stars:obj.hide_render=True
camera((5,3.7,6),(0,1.05,0),52)
scene.render.resolution_x=1200;scene.render.resolution_y=900;scene.render.filepath=str(OUT/'Moonpop_car_preview.png');bpy.ops.render.render(write_still=True)
print('ART REVIEW COMPLETE - these are Blender renders of actual placement data, not Unity screenshots.')
