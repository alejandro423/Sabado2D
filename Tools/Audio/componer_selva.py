"""Composición original para FLUXLAB: flauta suave, marimba y acordes ambientales.
Requiere numpy. Genera un WAV estéreo que enlaza sus colas al principio del bucle.
"""
from pathlib import Path
import wave
import numpy as np

sr = 22050
beat = 60 / 86
length = round(64 * beat * sr)
mix = np.zeros((length, 2), dtype=np.float64)

def note(midi, start, beats, gain, voice='marimba', pan=0):
    count = round((beats * beat + 1.5) * sr)
    t = np.arange(count) / sr
    f = 440 * 2 ** ((midi - 69) / 12)
    phase = 2 * np.pi * f * t
    if voice == 'pad':
        signal = (np.sin(phase) + .16*np.sin(phase*2))
        envelope = np.minimum(t/.7, 1) * np.minimum(np.maximum(beats*beat+1.5-t, 0)/1.5, 1)
    elif voice == 'flute':
        phase += .018*np.sin(2*np.pi*4.6*t)
        signal = np.sin(phase) + .12*np.sin(phase*2) + .035*np.sin(phase*3)
        envelope = np.minimum(t/.09, 1)*np.minimum(np.maximum(beats*beat+.25-t, 0)/.35, 1)
    else:
        signal = np.sin(phase)*np.exp(-t*2.8) + .24*np.sin(phase*3.99)*np.exp(-t*8)
        envelope = np.minimum(t/.008, 1)*np.exp(-t*.7)
    signal *= envelope * gain
    indices = (round(start * beat * sr) + np.arange(count)) % length
    np.add.at(mix[:, 0], indices, signal*np.sqrt((1-pan)/2))
    np.add.at(mix[:, 1], indices, signal*np.sqrt((1+pan)/2))

chords = [(45,57,60,64),(41,53,57,60),(48,55,60,64),(43,55,59,62)]
melody = [(76,0,1),(79,1.5,.5),(81,2,1.5),(79,4,1),(76,5.5,1),(72,7,.75),
          (76,8,1.5),(74,10,1),(72,11.5,1.5),(69,14,1.5),
          (72,16,1),(76,17.5,1),(79,19,1.5),(76,22,1),
          (74,24,1.5),(72,26,1),(71,28,1),(69,30,1.5)]
for bar in range(16):
    chord = chords[(bar//2)%4]
    if bar % 2 == 0:
        for midi in chord: note(midi,bar*4,7,.043,'pad')
    for step in range(4):
        note(chord[1+step%3]+12,bar*4+step,.8,.095,'marimba',(-.3 if step%2 else .3))
for offset in (0,32):
    for midi,start,duration in melody:
        note(midi,start+offset,duration,.075,'flute',-.12)
# Circular stereo echoes preserve a continuous tail at the loop boundary.
mix += .14*np.roll(mix,round(beat*.75*sr),axis=0)[:,::-1]
peak = float(np.max(np.abs(mix)))
mix *= .65/max(peak,1e-9)
pcm = np.round(mix*32767).astype('<i2')
dest = Path(__file__).resolve().parents[2]/'Assets/Resources/Sonido/Musica/SelvaAmbiente.wav'
dest.parent.mkdir(parents=True,exist_ok=True)
with wave.open(str(dest),'wb') as audio:
    audio.setnchannels(2); audio.setsampwidth(2); audio.setframerate(sr)
    audio.writeframes(pcm.tobytes())
assert peak > 0 and np.isfinite(mix).all()
print(f'Música original: {length/sr:.2f} s, estéreo, pico {np.max(np.abs(mix)):.2f}')
