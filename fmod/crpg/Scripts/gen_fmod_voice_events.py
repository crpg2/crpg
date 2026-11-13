#!/usr/bin/env python3
"""Generate FMOD Studio metadata XMLs for cRPG voice events directly.
Run: python3 gen_fmod_voice_events.py
Close FMOD Studio before running, then reopen it after.
"""
import os, re, struct, uuid, shutil
from pathlib import Path

PROJ = Path(__file__).parent.parent
META = PROJ / 'Metadata'
ASSETS = PROJ / 'Assets'

# Fixed GUIDs from template event
BANK          = '{7d4941b2-09af-4406-8ebb-1cd2d87d250e}'
TAGS          = ['{97dcd2c4-d869-445a-8b51-99206300c848}','{f2c9ecb2-e085-4ed8-8605-6cad262776ad}']
MIX_OUT       = '{e905d906-e187-494b-a49f-fa97761544ea}'
P_DVL         = '{e47d056c-67ef-4cb2-aef0-933107bae5cd}'
P_OCC         = '{db217dc4-cc8f-4328-ac6e-67b2fb3398c7}'
P_ISPLAYER    = '{f096d7a3-0fbe-4b70-b3b3-5f7a82f9993b}'
FX_SPAT       = '{d07c5f8b-8daf-02ad-049c-7960de23f3ad}'
FX_DIST       = '{a86ec1e2-fbc3-0db9-338c-d34e8b8e8d35}'
FX_CLEAN      = '{b0448cae-b3f8-0801-3047-0bbf1b67dedd}'
MR_SLOWCLEAN  = '{e3aaacc9-faaa-4bc3-8b20-5a32b6359a28}'
MR_SLOW       = '{d0bfeae1-3376-480e-9f76-99d0e6ef83c8}'
ASSET_FOLDER  = '{b6479c9e-7ccd-4767-8111-a7d2a8925708}'

# Existing folder path → GUID
FOLDERS = {
    'voice':                         '{c499a118-789a-4d4d-b56c-f667bcfa8a7e}',
    'voice/crpg':                    '{3f3eb82a-be15-43a1-92d6-2591d9207fd9}',
    'voice/crpg/attack':             '{52345d94-1c97-439a-9269-670f4dc6edd1}',
    'voice/crpg/defend':             '{6b90ed57-a53e-4e1d-8ab7-82a794a2352b}',
    'voice/crpg/incoming':           '{758e933c-416f-49cb-b968-7c8ec8495adb}',
    'voice/crpg/quick':              '{64c34b33-2239-4541-a9b5-a5e76628d889}',
    'voice/crpg/formation':          '{df73dbd4-6bcb-452d-9f82-4ca96be0ce4e}',
    'voice/crpg/equipment':          '{323814b9-5639-4fee-9c42-a1e84cc484d0}',
    'voice/crpg/self':               '{493cfde2-da3b-4ea3-9899-8ba360366808}',
    'voice/crpg/self/attack':        '{c61da2a5-1b0e-4531-a0d0-40d641ae241b}',
    'voice/crpg/self/attack/male':   '{329b1884-7796-4bf1-ad18-0672419c2134}',
    'voice/crpg/self/attack/female': '{e96d5a63-445a-4c18-85db-053dfb4c018b}',
    'voice/crpg/self/defend':        '{3c530066-fe8f-4441-a933-73e739eab269}',
    'voice/crpg/self/equipment':     '{d8da1c85-7b06-40f8-8bab-3ab69f4e844d}',
}

def ng():
    return '{' + str(uuid.uuid4()).upper() + '}'

def ogg_duration(path):
    try:
        with open(path, 'rb') as f:
            head = f.read(4096)
            idx = head.find(b'\x01vorbis')
            if idx == -1: return 2.0
            sr = struct.unpack('<I', head[idx+12:idx+16])[0]
            f.seek(max(0, os.path.getsize(path) - 65536))
            tail = f.read()
            p = tail.rfind(b'OggS')
            if p == -1: return 2.0
            gran = struct.unpack('<q', tail[p+6:p+14])[0]
            return gran / sr if sr > 0 and gran > 0 else 2.0
    except Exception:
        return 2.0

def ensure_folder(path):
    if path in FOLDERS:
        return FOLDERS[path]
    parent_path = '/'.join(path.split('/')[:-1])
    parent_guid = ensure_folder(parent_path)
    g = ng()
    FOLDERS[path] = g
    name = path.split('/')[-1]
    xml = f'''<?xml version="1.0" encoding="UTF-8"?>
<objects serializationModel="Studio.02.02.00">
\t<object class="EventFolder" id="{g}">
\t\t<property name="name">
\t\t\t<value>{name}</value>
\t\t</property>
\t\t<relationship name="folder">
\t\t\t<destination>{parent_guid}</destination>
\t\t</relationship>
\t</object>
</objects>
'''
    (META / 'EventFolder' / f'{g}.xml').write_text(xml, encoding='utf-8')
    return g

def ap_xml(g, pos, val, curve=None):
    s = f'\t<object class="AutomationPoint" id="{g}">\n'
    s += f'\t\t<property name="position"><value>{pos}</value></property>\n'
    s += f'\t\t<property name="value"><value>{val}</value></property>\n'
    if curve is not None:
        s += f'\t\t<property name="curveShape"><value>{curve}</value></property>\n'
    return s + '\t</object>\n'

def make_event(name, folder_guid, audio_file_guids, max_len):
    G = {k: ng() for k in [
        'evt','mixer','mastertrack','mixerinput','autoprops','markertrack',
        'timeline','pxdvl','pxocc','userprop','mixermaster',
        'multisound','effchain_in','panner_in','effchain_master','panner_master',
        'fader_in','fx_spat','fx_dist','fx_clean','ms_slowclean','ms_slow','fader_master',
        'auto_dist','auto_clean','auto_slowclean','auto_slow',
        'curve_dist','curve_clean','curve_slowclean','curve_slow',
    ]}
    # AutomationPoints
    apt_dist  = [ng(), ng(), ng()]   # 3 points for dist send
    apt_clean = [ng(), ng(), ng()]   # 3 points for clean send
    apt_sc    = [ng(), ng()]         # 2 points for slow clean
    apt_sl    = [ng(), ng()]         # 2 points for slow

    single_guids = [ng() for _ in audio_file_guids]

    sounds_rel = ''.join(f'\t\t\t<destination>{g}</destination>\n' for g in single_guids)
    single_objs = ''.join(
        f'\t<object class="SingleSound" id="{sg}">\n'
        f'\t\t<relationship name="audioFile"><destination>{ag}</destination></relationship>\n'
        f'\t</object>\n'
        for sg, ag in zip(single_guids, audio_file_guids)
    )
    tag_rels = ''.join(f'\t\t\t<destination>{t}</destination>\n' for t in TAGS)

    xml = f'''<?xml version="1.0" encoding="UTF-8"?>
<objects serializationModel="Studio.02.02.00">
\t<object class="Event" id="{G['evt']}">
\t\t<property name="name"><value>{name}</value></property>
\t\t<property name="outputFormat"><value>2</value></property>
\t\t<relationship name="folder"><destination>{folder_guid}</destination></relationship>
\t\t<relationship name="tags">
{tag_rels}\t\t</relationship>
\t\t<relationship name="mixer"><destination>{G['mixer']}</destination></relationship>
\t\t<relationship name="masterTrack"><destination>{G['mastertrack']}</destination></relationship>
\t\t<relationship name="mixerInput"><destination>{G['mixerinput']}</destination></relationship>
\t\t<relationship name="automatableProperties"><destination>{G['autoprops']}</destination></relationship>
\t\t<relationship name="markerTracks"><destination>{G['markertrack']}</destination></relationship>
\t\t<relationship name="timeline"><destination>{G['timeline']}</destination></relationship>
\t\t<relationship name="parameters">
\t\t\t<destination>{G['pxdvl']}</destination>
\t\t\t<destination>{G['pxocc']}</destination>
\t\t</relationship>
\t\t<relationship name="userProperties"><destination>{G['userprop']}</destination></relationship>
\t\t<relationship name="banks"><destination>{BANK}</destination></relationship>
\t</object>
\t<object class="EventMixer" id="{G['mixer']}">
\t\t<relationship name="masterBus"><destination>{G['mixermaster']}</destination></relationship>
\t</object>
\t<object class="MasterTrack" id="{G['mastertrack']}">
\t\t<relationship name="modules"><destination>{G['multisound']}</destination></relationship>
\t\t<relationship name="mixerGroup"><destination>{G['mixermaster']}</destination></relationship>
\t</object>
\t<object class="MixerInput" id="{G['mixerinput']}">
\t\t<relationship name="effectChain"><destination>{G['effchain_in']}</destination></relationship>
\t\t<relationship name="panner"><destination>{G['panner_in']}</destination></relationship>
\t\t<relationship name="output"><destination>{MIX_OUT}</destination></relationship>
\t</object>
\t<object class="EventAutomatableProperties" id="{G['autoprops']}">
\t\t<property name="voiceStealing"><value>2</value></property>
\t\t<property name="priority"><value>4</value></property>
\t\t<property name="maximumDistance"><value>450</value></property>
\t</object>
\t<object class="MarkerTrack" id="{G['markertrack']}" />
\t<object class="Timeline" id="{G['timeline']}">
\t\t<relationship name="modules"><destination>{G['multisound']}</destination></relationship>
\t</object>
\t<object class="ParameterProxy" id="{G['pxdvl']}">
\t\t<relationship name="preset"><destination>{P_DVL}</destination></relationship>
\t</object>
\t<object class="ParameterProxy" id="{G['pxocc']}">
\t\t<relationship name="preset"><destination>{P_OCC}</destination></relationship>
\t</object>
\t<object class="UserProperty" id="{G['userprop']}">
\t\t<property name="key"><value>priorityMultiplier</value></property>
\t\t<property name="value"><value>1.3</value></property>
\t</object>
\t<object class="EventMixerMaster" id="{G['mixermaster']}">
\t\t<property name="volume"><value>-80</value></property>
\t\t<relationship name="effectChain"><destination>{G['effchain_master']}</destination></relationship>
\t\t<relationship name="panner"><destination>{G['panner_master']}</destination></relationship>
\t\t<relationship name="mixer"><destination>{G['mixer']}</destination></relationship>
\t</object>
\t<object class="MultiSound" id="{G['multisound']}">
\t\t<property name="length"><value>{max_len:.4f}</value></property>
\t\t<relationship name="sounds">
{sounds_rel}\t\t</relationship>
\t</object>
\t<object class="MixerBusEffectChain" id="{G['effchain_in']}">
\t\t<relationship name="effects"><destination>{G['fader_in']}</destination></relationship>
\t</object>
\t<object class="MixerBusPanner" id="{G['panner_in']}" />
\t<object class="MixerBusEffectChain" id="{G['effchain_master']}">
\t\t<relationship name="effects">
\t\t\t<destination>{G['fx_spat']}</destination>
\t\t\t<destination>{G['fx_dist']}</destination>
\t\t\t<destination>{G['fx_clean']}</destination>
\t\t\t<destination>{G['ms_slowclean']}</destination>
\t\t\t<destination>{G['ms_slow']}</destination>
\t\t\t<destination>{G['fader_master']}</destination>
\t\t</relationship>
\t</object>
\t<object class="MixerBusPanner" id="{G['panner_master']}" />
{single_objs}\t<object class="MixerBusFader" id="{G['fader_in']}" />
\t<object class="ProxyEffect" id="{G['fx_spat']}">
\t\t<property name="inputFormat"><value>1</value></property>
\t\t<relationship name="preset"><destination>{FX_SPAT}</destination></relationship>
\t</object>
\t<object class="ProxyEffect" id="{G['fx_dist']}">
\t\t<property name="inputFormat"><value>2</value></property>
\t\t<relationship name="automators"><destination>{G['auto_dist']}</destination></relationship>
\t\t<relationship name="preset"><destination>{FX_DIST}</destination></relationship>
\t</object>
\t<object class="ProxyEffect" id="{G['fx_clean']}">
\t\t<property name="inputFormat"><value>2</value></property>
\t\t<relationship name="automators"><destination>{G['auto_clean']}</destination></relationship>
\t\t<relationship name="preset"><destination>{FX_CLEAN}</destination></relationship>
\t</object>
\t<object class="MixerSend" id="{G['ms_slowclean']}">
\t\t<property name="inputFormat"><value>2</value></property>
\t\t<relationship name="automators"><destination>{G['auto_slowclean']}</destination></relationship>
\t\t<relationship name="mixerReturn"><destination>{MR_SLOWCLEAN}</destination></relationship>
\t</object>
\t<object class="MixerSend" id="{G['ms_slow']}">
\t\t<property name="inputFormat"><value>2</value></property>
\t\t<relationship name="automators"><destination>{G['auto_slow']}</destination></relationship>
\t\t<relationship name="mixerReturn"><destination>{MR_SLOW}</destination></relationship>
\t</object>
\t<object class="MixerBusFader" id="{G['fader_master']}" />
\t<object class="Automator" id="{G['auto_dist']}">
\t\t<property name="nameOfPropertyBeingAutomated"><value>level</value></property>
\t\t<relationship name="automationCurves"><destination>{G['curve_dist']}</destination></relationship>
\t</object>
\t<object class="Automator" id="{G['auto_clean']}">
\t\t<property name="nameOfPropertyBeingAutomated"><value>level</value></property>
\t\t<relationship name="automationCurves"><destination>{G['curve_clean']}</destination></relationship>
\t</object>
\t<object class="Automator" id="{G['auto_slowclean']}">
\t\t<property name="nameOfPropertyBeingAutomated"><value>level</value></property>
\t\t<relationship name="automationCurves"><destination>{G['curve_slowclean']}</destination></relationship>
\t</object>
\t<object class="Automator" id="{G['auto_slow']}">
\t\t<property name="nameOfPropertyBeingAutomated"><value>level</value></property>
\t\t<relationship name="automationCurves"><destination>{G['curve_slow']}</destination></relationship>
\t</object>
\t<object class="AutomationCurve" id="{G['curve_dist']}">
\t\t<relationship name="parameter"><destination>{P_DVL}</destination></relationship>
\t\t<relationship name="automationPoints">
\t\t\t<destination>{apt_dist[0]}</destination>
\t\t\t<destination>{apt_dist[1]}</destination>
\t\t\t<destination>{apt_dist[2]}</destination>
\t\t</relationship>
\t</object>
\t<object class="AutomationCurve" id="{G['curve_clean']}">
\t\t<relationship name="parameter"><destination>{P_DVL}</destination></relationship>
\t\t<relationship name="automationPoints">
\t\t\t<destination>{apt_clean[0]}</destination>
\t\t\t<destination>{apt_clean[1]}</destination>
\t\t\t<destination>{apt_clean[2]}</destination>
\t\t</relationship>
\t</object>
\t<object class="AutomationCurve" id="{G['curve_slowclean']}">
\t\t<relationship name="parameter"><destination>{P_ISPLAYER}</destination></relationship>
\t\t<relationship name="automationPoints">
\t\t\t<destination>{apt_sc[0]}</destination>
\t\t\t<destination>{apt_sc[1]}</destination>
\t\t</relationship>
\t</object>
\t<object class="AutomationCurve" id="{G['curve_slow']}">
\t\t<relationship name="parameter"><destination>{P_ISPLAYER}</destination></relationship>
\t\t<relationship name="automationPoints">
\t\t\t<destination>{apt_sl[0]}</destination>
\t\t\t<destination>{apt_sl[1]}</destination>
\t\t</relationship>
\t</object>
''' + \
    ap_xml(apt_dist[0],  0,   -80, -0.794435918) + \
    ap_xml(apt_dist[1],  500, -80) + \
    ap_xml(apt_dist[2],  100, -24, -0.100460663) + \
    ap_xml(apt_clean[0], 0,   0) + \
    ap_xml(apt_clean[1], 2,   -6,  -0.0862192065) + \
    ap_xml(apt_clean[2], 150, -80,  0.265698314) + \
    ap_xml(apt_sc[0],    0,   -80) + \
    ap_xml(apt_sc[1],    1,   0) + \
    ap_xml(apt_sl[0],    0,   0) + \
    ap_xml(apt_sl[1],    1,   -80) + \
    '</objects>\n'

    return G['evt'], xml

def main():
    # Parse EVENT_GROUPS from existing JS
    js = (Path(__file__).parent / 'VC_EventGen.js').read_text(encoding='utf-8')
    m = re.search(r'var EVENT_GROUPS\s*=\s*(\{.*?\});', js, re.DOTALL)
    if not m:
        print('ERROR: Could not parse EVENT_GROUPS from VC_EventGen.js'); return
    # Convert JS object to JSON
    raw = m.group(1)
    raw = re.sub(r',\s*\}', '}', raw)   # trailing commas
    import json
    event_groups = json.loads(raw)

    # Clear unsaved Metadata so stale broken events are gone
    unsaved_meta = PROJ / '.unsaved' / 'Metadata'
    if unsaved_meta.exists():
        shutil.rmtree(unsaved_meta)
        print('Cleared .unsaved/Metadata/')

    (META / 'AudioFile').mkdir(parents=True, exist_ok=True)
    (META / 'EventFolder').mkdir(parents=True, exist_ok=True)
    (META / 'Event').mkdir(parents=True, exist_ok=True)

    created = 0
    errors = []

    for group_key, file_paths in event_groups.items():
        parts = group_key.split('/')
        event_name  = parts[-1]
        folder_path = '/'.join(parts[:-1])

        # Create any missing folders
        folder_guid = ensure_folder(folder_path)

        # Create AudioFile XMLs and collect GUIDs + durations
        af_guids = []
        max_len  = 0.0
        for rel_path in file_paths:
            phys = ASSETS / rel_path
            dur  = ogg_duration(phys) if phys.exists() else 2.0
            if dur > max_len:
                max_len = dur
            ag = ng()
            af_guids.append(ag)
            af_xml = f'''<?xml version="1.0" encoding="UTF-8"?>
<objects serializationModel="Studio.02.02.00">
\t<object class="AudioFile" id="{ag}">
\t\t<property name="assetPath">
\t\t\t<value>{rel_path}</value>
\t\t</property>
\t\t<property name="frequencyInKHz">
\t\t\t<value>44.0999985</value>
\t\t</property>
\t\t<property name="channelCount">
\t\t\t<value>2</value>
\t\t</property>
\t\t<property name="length">
\t\t\t<value>{dur:.10f}</value>
\t\t</property>
\t\t<relationship name="masterAssetFolder">
\t\t\t<destination>{ASSET_FOLDER}</destination>
\t\t</relationship>
\t</object>
</objects>
'''
            (META / 'AudioFile' / f'{ag}.xml').write_text(af_xml, encoding='utf-8')

        # Create Event XML
        try:
            event_guid, event_xml = make_event(event_name, folder_guid, af_guids, max_len)
            (META / 'Event' / f'{event_guid}.xml').write_text(event_xml, encoding='utf-8')
            created += 1
        except Exception as e:
            errors.append(f'{group_key}: {e}')

    print(f'Done. Created {created} events, {len(errors)} errors.')
    if errors:
        for e in errors[:10]:
            print(' ', e)

if __name__ == '__main__':
    main()
