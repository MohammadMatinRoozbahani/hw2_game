                                                                                   
from pathlib import Path
import hashlib
import json
import re
import sys
import wave
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
EDITOR_VERSION = '6000.6.5f1'
PACKAGE_VERSIONS = {'com.unity.inputsystem': '1.20.1', 'com.unity.postprocessing': '3.5.4', 'com.unity.ugui': '2.6.0'}


def main():
    errors = []
    version_file = (ROOT / 'ProjectSettings/ProjectVersion.txt').read_text()
    if f'm_EditorVersion: {EDITOR_VERSION}\n' not in version_file:
        errors.append(f'Expected Unity {EDITOR_VERSION}')
    dependencies = json.loads((ROOT / 'Packages/manifest.json').read_text())['dependencies']
    for name, version in PACKAGE_VERSIONS.items():
        if dependencies.get(name) != version:
            errors.append(f'Expected {name} {version}')
    files = list((ROOT / 'Assets').rglob('*'))
    guids = {}
    for p in files:
        if p.suffix == '.meta':
            match = re.search(r'^guid: ([a-f0-9]{32})$', p.read_text(), re.M)
            if not match:
                errors.append(f'Invalid meta GUID: {p.relative_to(ROOT)}')
            elif match[1] in guids:
                errors.append(f'Duplicate GUID: {p.relative_to(ROOT)}')
            else:
                guids[match[1]] = p
        elif not p.with_name(p.name + '.meta').is_file():
            errors.append(f'Missing meta: {p.relative_to(ROOT)}')
    for rel in ['Assets/Moonpop/Scenes/Moonpop.unity', 'Assets/Moonpop/Resources/GameSettings.asset', 'ProjectSettings/EditorBuildSettings.asset']:
        p = ROOT / rel
        if not p.is_file():
            errors.append(f'Missing file: {rel}')
            continue
        for value in re.findall(r'guid: ([a-f0-9]{32})', p.read_text()):
            if value not in guids:
                errors.append(f'Unresolved project GUID in {rel}: {value}')
    actions = json.loads((ROOT / 'Assets/Moonpop/Resources/Input/DrivingControls.inputactions').read_text())
    maps = {m['name']: m for m in actions['maps']}
    if set(maps) != {'Gameplay', 'UI', 'System'}:
        errors.append('Expected Gameplay, UI and System input maps')
    ids = set()
    for m in maps.values():
        names = {a['name'] for a in m['actions']}
        for b in m['bindings']:
            if b['action'] not in names:
                errors.append(f'Invalid binding action: {b["action"]}')
        for item in [m] + m['actions'] + m['bindings']:
            if item['id'] in ids:
                errors.append('Duplicate input GUID')
            ids.add(item['id'])
    source = '\n'.join(p.read_text() for p in (ROOT / 'Assets/Moonpop/Scripts').rglob('*.cs'))
    if re.search(r'\bInput\.(?:GetKey|GetAxis|GetButton|mousePosition)', source):
        errors.append('Legacy Input API found')
    if 'activeInputHandler: 1' not in (ROOT / 'ProjectSettings/ProjectSettings.asset').read_text():
        errors.append('Active Input Handling is not New Input System only')
    for clip in ['MidnightCircuit', 'Engine', 'Coin', 'Fuel', 'Crash', 'Lane', 'Click', 'Firework', 'Nitro', 'NearMiss', 'Badge']:
        p = ROOT / f'Assets/Moonpop/Resources/Audio/{clip}.wav'
        try:
            with wave.open(str(p)) as w:
                assert w.getnframes() > 0 and w.getsampwidth() == 2
        except Exception as exc:
            errors.append(f'Invalid audio {clip}: {exc}')
    own_assemblies = set()
    for p in (ROOT / 'Assets').rglob('*.asmdef'):
        own_assemblies.add(json.loads(p.read_text())['name'])
    external = {'Unity.InputSystem', 'Unity.ugui', 'Unity.Postprocessing.Runtime'}
    for p in (ROOT / 'Assets').rglob('*.asmdef'):
        for ref in json.loads(p.read_text()).get('references', []):
            if ref not in own_assemblies | external:
                errors.append(f'Unresolved assembly {ref}')
    ET.parse(ROOT / 'Assets/Moonpop/link.xml')
    gitignore_hash = hashlib.sha256((ROOT / '.gitignore').read_bytes()).hexdigest()
    print('Target Editor:', EDITOR_VERSION)
    print('Asset GUIDs:', len(guids))
    print('Input maps:', len(maps), '| unique input IDs:', len(ids))
    print('C# files:', len(list((ROOT / 'Assets').rglob('*.cs'))))
    print('Original .gitignore SHA-256:', gitignore_hash)
    if errors:
        for error in errors:
            print('FAIL:', error)
        return 1
    print('PASS: scene references, metadata, input, audio, assembly references and linker configuration')
    print('Unity Editor execution, shader compilation and player builds are separate checks.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
