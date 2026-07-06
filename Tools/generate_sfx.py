#!/usr/bin/env python3
"""Generate retro dungeon SFX as 16-bit mono WAV files for The Forgotten Fort."""
import math
import random
import struct
import wave
from pathlib import Path

SAMPLE_RATE = 44100
OUT = Path(__file__).resolve().parent.parent / "Assets" / "Audio" / "SFX"
OUT.mkdir(parents=True, exist_ok=True)


def clamp(x, lo=-1.0, hi=1.0):
    return max(lo, min(hi, x))


def envelope(length, attack=0.01, release=0.08):
    n = max(1, length)
    env = [0.0] * n
    a = int(attack * SAMPLE_RATE)
    r = int(release * SAMPLE_RATE)
    for i in range(n):
        if i < a:
            env[i] = i / max(1, a)
        elif i > n - r:
            env[i] = max(0.0, (n - i) / max(1, r))
        else:
            env[i] = 1.0
    return env


def mix(*tracks):
    length = max(len(t) for t in tracks)
    out = [0.0] * length
    for t in tracks:
        for i, v in enumerate(t):
            out[i] += v
    return [clamp(v * 0.85) for v in out]


def sine(freq, duration, amp=0.5, phase=0.0):
    n = int(duration * SAMPLE_RATE)
    return [amp * math.sin(2 * math.pi * freq * i / SAMPLE_RATE + phase) for i in range(n)]


def noise(duration, amp=0.3, seed=0):
  random.seed(seed)
  n = int(duration * SAMPLE_RATE)
  return [amp * (random.random() * 2 - 1) for _ in range(n)]


def apply_env(signal, attack=0.005, release=0.05):
    env = envelope(len(signal), attack, release)
    return [s * e for s, e in zip(signal, env)]


def lowpass(signal, alpha=0.15):
    if not signal:
        return signal
    out = [signal[0]]
    for i in range(1, len(signal)):
        out.append(out[-1] + alpha * (signal[i] - out[-1]))
    return out


def write_wav(name, signal):
    path = OUT / f"{name}.wav"
    data = []
    for s in signal:
        data.append(struct.pack("<h", int(clamp(s) * 32767)))
    with wave.open(str(path), "w") as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2)
        wf.setframerate(SAMPLE_RATE)
        wf.writeframes(b"".join(data))
    print(f"  {path.name} ({len(signal) / SAMPLE_RATE:.2f}s)")


def make_footstep(loud=False):
    thump = apply_env(lowpass(sine(90 if not loud else 110, 0.09, 0.55), 0.2), 0.002, 0.06)
    grit = apply_env(lowpass(noise(0.05, 0.18 if not loud else 0.28, seed=7), 0.12), 0.001, 0.03)
    return mix(thump, grit)


def make_key_pickup():
    notes = [660, 880, 1175]
    parts = []
    for i, f in enumerate(notes):
        tone = apply_env(sine(f, 0.12, 0.35), 0.005, 0.08)
        parts.append([0.0] * int(i * 0.04 * SAMPLE_RATE) + tone)
    return mix(*parts)


def make_door_open():
    n = int(0.35 * SAMPLE_RATE)
    creak = []
    for i in range(n):
        freq = 180 + i * (120 / max(1, n))
        creak.append(0.22 * math.sin(2 * math.pi * freq * i / SAMPLE_RATE))
    creak = apply_env(lowpass(creak, 0.08), 0.01, 0.12)
    click = apply_env(sine(420, 0.05, 0.25), 0.001, 0.02)
    return mix(creak, [0.0] * (len(creak) - len(click)) + click)


def make_door_locked():
    return apply_env(mix(
        sine(140, 0.08, 0.45),
        noise(0.06, 0.2, seed=3),
    ), 0.002, 0.05)


def make_hurt():
    buzz = []
    for i in range(int(0.18 * SAMPLE_RATE)):
        t = i / SAMPLE_RATE
        buzz.append(0.4 * math.sin(2 * math.pi * (220 - t * 400) * t))
    return apply_env(lowpass(buzz, 0.1), 0.002, 0.08)


def make_caught():
  parts = []
  freqs = [440, 330, 220, 165]
  for i, f in enumerate(freqs):
      tone = apply_env(sine(f, 0.18, 0.4), 0.01, 0.1)
      parts.append([0.0] * int(i * 0.12 * SAMPLE_RATE) + tone)
  alarm = apply_env(noise(0.35, 0.12, seed=11), 0.02, 0.2)
  return mix(*parts, alarm)


def make_win():
    melody = [523, 659, 784, 1047]
    parts = []
    for i, f in enumerate(melody):
        tone = apply_env(sine(f, 0.22, 0.38), 0.01, 0.12)
        parts.append([0.0] * int(i * 0.14 * SAMPLE_RATE) + tone)
    shimmer = apply_env(sine(1568, 0.5, 0.12), 0.2, 0.25)
    return mix(*parts, [0.0] * int(0.42 * SAMPLE_RATE) + shimmer)


def make_puzzle_solve():
    parts = []
    for i, f in enumerate([392, 494, 587, 740]):
        tone = apply_env(sine(f, 0.1, 0.28), 0.004, 0.06)
        parts.append([0.0] * int(i * 0.05 * SAMPLE_RATE) + tone)
    sparkle = apply_env(lowpass(noise(0.25, 0.15, seed=21), 0.2), 0.01, 0.2)
    return mix(*parts, sparkle)


def make_guard_alert():
    a = apply_env(sine(520, 0.09, 0.35), 0.005, 0.04)
    b = apply_env(sine(780, 0.09, 0.35), 0.005, 0.04)
    gap = int(0.11 * SAMPLE_RATE)
    return mix(a + [0.0] * gap + b)


def make_menu_click():
    return apply_env(mix(sine(900, 0.04, 0.2), noise(0.02, 0.08, seed=1)), 0.001, 0.02)


def main():
    print(f"Writing SFX to {OUT}")
    write_wav("footstep", make_footstep(False))
    write_wav("footstep_sprint", make_footstep(True))
    write_wav("key_pickup", make_key_pickup())
    write_wav("door_open", make_door_open())
    write_wav("door_locked", make_door_locked())
    write_wav("hurt", make_hurt())
    write_wav("caught", make_caught())
    write_wav("win", make_win())
    write_wav("puzzle_solve", make_puzzle_solve())
    write_wav("guard_alert", make_guard_alert())
    write_wav("menu_click", make_menu_click())
    print("Done.")


if __name__ == "__main__":
    main()
