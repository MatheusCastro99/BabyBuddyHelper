"""Generates Cub's Lottie animation (Resources/Raw/cub.json).

Cub is drawn from the shapes of the original companion SVG, plus two arms.
All animations live in one file as back-to-back segments that start and end
on the rest pose; CubView plays one segment at a time. If a segment's frames
change here, update the matching table in UI/Controls/CubView.cs.

Run from the repo root:  python tools/cub/generate_cub.py
Preview:                 python -m http.server 8766, then open
                         http://localhost:8766/tools/cub/preview.html
The build never runs this script; the generated JSON is committed.
"""
import json
import os

FPS = 60

# name: (first frame, length in frames)
SEGMENTS = {
    "wave": (0, 100),
    "clap": (120, 90),
    "dance": (230, 120),
    "nod": (370, 56),
}
TOTAL_FRAMES = 430

BODY = "#F9C87B"
LIMB = "#F2B968"
EAR = "#E8A857"
CREAM = "#FCEBCB"
BROWN = "#8A5A34"
EYE = "#23415D"
CHEEK = "#F4A3A3"


def rgba(hex_color):
    h = hex_color.lstrip("#")
    return [round(int(h[i:i + 2], 16) / 255, 4) for i in (0, 2, 4)] + [1]


def static(value):
    return {"a": 0, "k": value}


class Track:
    """One animated property: a rest value plus keyframes per segment."""

    def __init__(self, rest, kind="scalar"):
        self.rest = rest
        self.kind = kind  # scalar | vector (scale) | spatial (position) | path
        self.keys = {}

    def seg(self, name, keys):
        """keys: (frame within the segment, value). The segment returns to rest at its end."""
        self.keys[name] = keys
        return self

    def _key(self, frame, value, last):
        if self.kind == "path":
            key = {"t": frame, "s": [value]}
        else:
            key = {"t": frame, "s": value if isinstance(value, list) else [value]}
        if not last:
            if self.kind in ("spatial", "path"):
                key["i"] = {"x": 0.4, "y": 1}
                key["o"] = {"x": 0.6, "y": 0}
            else:
                n = len(key["s"])
                key["i"] = {"x": [0.4] * n, "y": [1] * n}
                key["o"] = {"x": [0.6] * n, "y": [0] * n}
        return key

    def build(self):
        if not self.keys:
            return static(self.rest)
        flat = []
        for name, (start, length) in SEGMENTS.items():
            keys = self.keys.get(name)
            if not keys:
                continue
            if keys[0][0] > 0:
                flat.append((start, self.rest))
            flat += [(start + t, v) for t, v in keys]
            if keys[-1][0] < length:
                flat.append((start + length, self.rest))
        return {"a": 1, "k": [self._key(t, v, i == len(flat) - 1) for i, (t, v) in enumerate(flat)]}


def prop(value):
    return value.build() if isinstance(value, Track) else static(value)


def transform(anchor=(0, 0), scale=None):
    return {"ty": "tr", "p": static(list(anchor)), "a": static(list(anchor)),
            "s": prop(scale if scale is not None else [100, 100]), "r": static(0), "o": static(100)}


def fill(color, opacity=100):
    return {"ty": "fl", "c": static(rgba(color)), "o": static(opacity), "r": 1}


def stroke(color, width):
    return {"ty": "st", "c": static(rgba(color)), "o": static(100), "w": static(width), "lc": 2, "lj": 2}


def group(name, items, anchor=(0, 0), scale=None):
    return {"ty": "gr", "nm": name, "it": items + [transform(anchor, scale)]}


def ellipse(name, center, rx, ry, color, opacity=100):
    shape = {"ty": "el", "d": 1, "p": prop(center), "s": static([rx * 2, ry * 2])}
    return group(name, [shape, fill(color, opacity)])


def quad(p0, c, p2):
    """A quadratic curve (as in the SVG) expressed as a Lottie bezier."""
    c1 = [p0[0] + 2 / 3 * (c[0] - p0[0]), p0[1] + 2 / 3 * (c[1] - p0[1])]
    c2 = [p2[0] + 2 / 3 * (c[0] - p2[0]), p2[1] + 2 / 3 * (c[1] - p2[1])]
    return {"c": False, "v": [list(p0), list(p2)],
            "o": [[c1[0] - p0[0], c1[1] - p0[1]], [0, 0]],
            "i": [[0, 0], [c2[0] - p2[0], c2[1] - p2[1]]]}


def line(p0, p1):
    return {"c": False, "v": [list(p0), list(p1)], "i": [[0, 0], [0, 0]], "o": [[0, 0], [0, 0]]}


def path(name, value, color, width):
    return group(name, [{"ty": "sh", "ks": prop(value)}, stroke(color, width)])


_layers = []


def layer(name, shapes, anchor=(0, 0), rotation=0, position=None, parent=None, null=False):
    """A layer whose anchor rests on the same canvas point, so shapes keep the SVG coordinates."""
    ax, ay = anchor
    index = len(_layers) + 1
    result = {"ddd": 0, "ind": index, "ty": 3 if null else 4, "nm": name, "sr": 1, "ao": 0, "bm": 0,
              "ip": 0, "op": TOTAL_FRAMES, "st": 0,
              "ks": {"o": static(100), "r": prop(rotation),
                     "p": prop(position if position is not None else [ax, ay, 0]),
                     "a": static([ax, ay, 0]),
                     "s": static([100, 100, 100])}}
    if not null:
        result["shapes"] = shapes
    if parent is not None:
        result["parent"] = parent
    _layers.append(result)
    return index


# ---------------------------------------------------------------- motion

BASE = (100, 188)
GROUND, HOP, DIP = [100, 188, 0], [100, 183, 0], [100, 185, 0]

# Whole cub: hop and sway from the ground.
root_position = Track(GROUND, "spatial") \
    .seg("clap", [(0, GROUND), (10, GROUND), (17, DIP), (24, GROUND), (31, DIP), (38, GROUND),
                  (45, DIP), (52, GROUND)]) \
    .seg("dance", [(0, GROUND), (14, HOP), (28, GROUND), (42, HOP), (56, GROUND), (70, HOP),
                   (84, GROUND), (98, HOP), (112, GROUND)])
root_rotation = Track(0) \
    .seg("dance", [(0, 0), (14, -6), (42, 6), (70, -6), (98, 6), (114, 0)])

NECK, NOD_DOWN, NOD_UP = [100, 128, 0], [100, 134, 0], [100, 127, 0]
head_rotation = Track(0) \
    .seg("wave", [(0, 0), (16, -6), (72, -6), (92, 0)]) \
    .seg("dance", [(0, 0), (14, 5), (42, -5), (70, 5), (98, -5), (114, 0)])
head_position = Track(NECK, "spatial") \
    .seg("nod", [(0, NECK), (10, NOD_DOWN), (22, NOD_UP), (34, NOD_DOWN), (50, NECK)])

OPEN, SHUT = [100, 100], [100, 22]
eye_scale = Track(OPEN, "vector") \
    .seg("wave", [(78, OPEN), (83, SHUT), (88, OPEN)]) \
    .seg("clap", [(8, OPEN), (14, SHUT), (54, SHUT), (60, OPEN)]) \
    .seg("dance", [(100, OPEN), (105, SHUT), (110, OPEN)]) \
    .seg("nod", [(4, OPEN), (9, SHUT), (38, SHUT), (44, OPEN)])

# Arms hang from the shoulders, drawn pointing straight down; positive rotation swings
# toward the viewer's left. Left and right are the viewer's.
ARM_REST = 12
left_arm = Track(ARM_REST) \
    .seg("wave", [(0, ARM_REST), (16, 148), (27, 116), (38, 148), (49, 116), (60, 148), (72, 136),
                  (92, ARM_REST)]) \
    .seg("clap", [(0, ARM_REST), (10, -42), (17, -70), (24, -44), (31, -70), (38, -44), (45, -70),
                  (56, -44), (76, ARM_REST)]) \
    .seg("dance", [(0, ARM_REST), (14, 152), (42, 120), (70, 152), (98, 120), (114, ARM_REST)])
right_arm = Track(-ARM_REST) \
    .seg("clap", [(0, -ARM_REST), (10, 42), (17, 70), (24, 44), (31, 70), (38, 44), (45, 70),
                  (56, 44), (76, -ARM_REST)]) \
    .seg("dance", [(0, -ARM_REST), (14, -120), (42, -152), (70, -120), (98, -152), (114, -ARM_REST)])

TAIL_REST = ((142, 150), (178, 142), (166, 116))
TAIL_OUT = ((142, 150), (184, 150), (176, 122))
TAIL_IN = ((142, 150), (172, 136), (158, 112))


def tail_keys(times):
    poses = [TAIL_REST] + [TAIL_OUT, TAIL_IN] * len(times)
    return list(zip([0] + times, poses))


tail_curve = Track(quad(*TAIL_REST), "path")
tail_tuft = Track(list(TAIL_REST[2]), "spatial")
for name, keys in {"wave": tail_keys([24, 48, 72]),
                   "clap": tail_keys([17, 31, 45]),
                   "dance": tail_keys([14, 42, 70, 98])}.items():
    tail_curve.seg(name, [(t, quad(*pose)) for t, pose in keys])
    tail_tuft.seg(name, [(t, list(pose[2])) for t, pose in keys])

# ---------------------------------------------------------------- drawing
# Layers and groups are listed top-most first.

root = layer("root", [], anchor=BASE, position=root_position, rotation=root_rotation, null=True)


def eye(name, cx):
    return group(name, [
        ellipse("glint", [cx + 1.5, 84.5], 1.3, 1.3, "#FFFFFF"),
        ellipse("pupil", [cx, 86], 4.5, 4.5, EYE),
    ], anchor=(cx, 86), scale=eye_scale)


layer("head", [
    ellipse("cheek right", [125, 100], 5.5, 5.5, CHEEK, 55),
    ellipse("cheek left", [75, 100], 5.5, 5.5, CHEEK, 55),
    path("mouth right", quad((100, 101), (100, 109), (109, 108)), BROWN, 2.5),
    path("mouth left", quad((100, 101), (100, 109), (91, 108)), BROWN, 2.5),
    ellipse("nose", [100, 97], 6.5, 4.5, BROWN),
    eye("eye right", 116),
    eye("eye left", 84),
    ellipse("muzzle", [100, 102], 25, 18, CREAM),
    ellipse("face", [100, 88], 44, 44, BODY),
    ellipse("ear right inner", [138, 52], 9, 9, CREAM),
    ellipse("ear left inner", [62, 52], 9, 9, CREAM),
    ellipse("ear right", [138, 52], 18, 18, EAR),
    ellipse("ear left", [62, 52], 18, 18, EAR),
], anchor=(100, 128), rotation=head_rotation, position=head_position, parent=root)

layer("feet", [
    ellipse("pad right", [126, 180], 8, 5, CREAM),
    ellipse("pad left", [74, 180], 8, 5, CREAM),
    ellipse("foot right", [126, 177], 15, 12, BODY),
    ellipse("foot left", [74, 177], 15, 12, BODY),
], parent=root)


def arm(name, sx, rotation):
    sy, length = 134, 30
    layer(name, [
        ellipse("pad", [sx, sy + length + 1], 4.5, 4, CREAM),
        path("limb", line((sx, sy), (sx, sy + length)), LIMB, 16),
    ], anchor=(sx, sy), rotation=rotation, parent=root)


arm("arm left", 64, left_arm)
arm("arm right", 136, right_arm)

layer("body", [
    ellipse("belly", [100, 155], 34, 20, CREAM),
    ellipse("torso", [100, 148], 52, 36, BODY),
], parent=root)

layer("tail", [
    ellipse("tuft", tail_tuft, 8, 8, BROWN),
    path("curl", tail_curve, EAR, 10),
], parent=root)

animation = {"v": "5.7.4", "fr": FPS, "ip": 0, "op": TOTAL_FRAMES, "w": 200, "h": 200, "nm": "Cub",
             "ddd": 0, "assets": [], "layers": _layers,
             "markers": [{"tm": start, "cm": name, "dr": length} for name, (start, length) in SEGMENTS.items()]}

output = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Resources", "Raw", "cub.json")
with open(output, "w", newline="\n") as handle:
    json.dump(animation, handle, separators=(",", ":"))

print("wrote", os.path.normpath(output))
for name, (start, length) in SEGMENTS.items():
    print(f"  {name}: frames {start}-{start + length} ({length / FPS:.2f}s)")
