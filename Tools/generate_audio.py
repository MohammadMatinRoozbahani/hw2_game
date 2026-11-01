                                                                                
                                                                    
   
from pathlib import Path
import wave
import numpy as np

OUT = Path(__file__).resolve().parents[1] / 'Assets/Moonpop/Resources/Audio'
SR = 22050
RNG = np.random.default_rng(402531387)


def write(name, signal, peak=.85):
    signal = np.asarray(signal)
    signal = signal / max(np.max(np.abs(signal)), .001) * peak
    pcm = (np.clip(signal, -1, 1) * 32767).astype('<i2')
    OUT.mkdir(parents=True, exist_ok=True)
    with wave.open(str(OUT / (name + '.wav')), 'wb') as f:
        f.setnchannels(1 if signal.ndim == 1 else 2)
        f.setsampwidth(2)
        f.setframerate(SR)
        f.writeframes(pcm.tobytes())


def time(seconds):
    return np.arange(int(SR * seconds)) / SR


def note(midi):
    return 440 * 2 ** ((midi - 69) / 12)


def add(target, sound, at):
    start = round(at * SR)
    n = min(len(sound), len(target) - start)
    if n > 0:
        target[start:start+n] += sound[:n]


def soundtrack():
                                                                             
    beat = 60 / 118
    n = round(64 * beat * SR)
    lead = np.zeros(n)
    rhythm = np.zeros(n)
    chords = [(60,64,67),(57,60,64),(53,57,60),(55,59,62)]
    melody = [0,4,7,9,7,4,2,4, 7,9,12,11,9,7,4,2]
    for bar in range(16):
        chord = chords[(bar//2)%4]
        for step in range(8):
            at = (bar*4+step*.5)*beat
            midi = 72 + melody[(step+bar*3)%len(melody)]
            t = time(.48)
            f = note(midi)
            pluck = (np.sin(2*np.pi*f*t)*np.exp(-t*10) + .24*np.sin(2*np.pi*f*2.01*t)*np.exp(-t*18) + .09*np.sin(2*np.pi*f*3*t)*np.exp(-t*26))
            pluck *= np.minimum(t/.003,1)
            if step%2==0 or bar%2:
                add(lead,pluck*.11,at)
            tb = time(.24)
            bass = note(chord[0]-24+(12 if step==6 else 0))
            add(rhythm,(np.sin(2*np.pi*bass*tb)+.15*np.sin(4*np.pi*bass*tb))*np.exp(-tb*13)*np.minimum(tb/.007,1)*.18,at)
            th = time(.06)
            add(rhythm,RNG.normal(size=len(th))*np.exp(-th*85)*np.minimum(th/.002,1)*.012,at)
        for b in range(4):
            at = (bar*4+b)*beat
            tk = time(.19)
            kick = np.sin(2*np.pi*(57*tk+2*(1-np.exp(-tk*35))))*np.exp(-tk*23)
            add(rhythm,kick*.18,at)
            if b in (1,3):
                tc = time(.08)
                add(rhythm,RNG.normal(size=len(tc))*np.exp(-tc*45)*.028,at)
        for off in (0,2):
            tp = time(beat*1.8)
            arp = sum(np.sin(2*np.pi*note(pitch+12)*tp)*np.exp(-tp*6) for pitch in chord)
            add(lead,arp*np.minimum(tp/.008,1)*.023,(bar*4+off)*beat)
    left = rhythm+lead+np.roll(lead,round(beat*.75*SR))*.16
    right = rhythm+lead+np.roll(lead,round(beat*.5*SR))*.18
    write('MidnightCircuit',np.column_stack((left,right)),.70)


def effects():
    t = time(2)
    engine = sum(np.sin(2*np.pi*68*k*t)/k**1.35 for k in range(1, 9))
    engine *= .86+.14*np.sin(2*np.pi*34*t)
    write('Engine', engine, .64)
    t = time(.28)
    coin = np.sin(2*np.pi*1046*t)*np.exp(-t*20)
    coin += np.sin(2*np.pi*1568*t)*np.maximum(0,1-np.exp(-t*70))*np.exp(-t*14)
    write('Coin', coin*np.minimum(t/.002,1), .6)
    result = np.zeros(round(.7*SR))
    for i, pitch in enumerate((72, 76, 79, 84)):
        t = time(.3)
        add(result,np.sin(2*np.pi*note(pitch)*t)*np.exp(-t*13)*np.minimum(t/.004,1),i*.105)
    write('Fuel',result,.68)
    t = time(.8)
    noise = RNG.normal(size=len(t))
    crash = noise*np.exp(-t*11)*.6+np.sin(2*np.pi*(70*t+1.2*(1-np.exp(-t*10))))*np.exp(-t*8)
    write('Crash',crash*np.minimum(t/.002,1),.8)
    t = time(.14)
    write('Lane',RNG.normal(size=len(t))*np.sin(np.pi*t/.14)**2*.2,.25)
    t = time(.09)
    write('Click',np.sin(2*np.pi*(900*t-1800*t*t))*np.exp(-t*55)*np.minimum(t/.002,1),.5)
    t = time(1)
    fire = RNG.normal(size=len(t))*np.exp(-t*8)*.4+np.sin(2*np.pi*45*t)*np.exp(-t*6)
    write('Firework',fire*np.minimum(t/.003,1),.7)
    t = time(.65)
    whoosh = RNG.normal(size=len(t))*np.sin(np.pi*t/.65)**2*.25
    whoosh += np.sin(2*np.pi*(180*t+220*t*t))*np.sin(np.pi*t/.65)**2*.2
    write('Nitro',whoosh,.55)
    t = time(.22)
    write('NearMiss',(np.sin(2*np.pi*note(79)*t)+np.sin(2*np.pi*note(84)*t)*.6)*np.exp(-t*20)*np.minimum(t/.004,1),.42)
    reward = np.zeros(round(1.1*SR))
    for i,pitch in enumerate((72,76,79,84,88)):
        t=time(.4)
        add(reward,np.sin(2*np.pi*note(pitch)*t)*np.exp(-t*11)*np.minimum(t/.004,1),i*.13)
    write('Badge',reward,.62)


if __name__ == '__main__':
    soundtrack()
    effects()
    print('Generated 11 original WAV files in', OUT)
