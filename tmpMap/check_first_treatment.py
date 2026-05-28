import json
from pathlib import Path

p = Path(
    r"D:\Games\Steam\steamapps\common\Stardew Valley\Mods\HarveyOverhaul\HarveyOverhaul [CP]\assets\Code\events.json"
)
text = p.read_text(encoding="utf-8")
lines = [ln for ln in text.splitlines() if not ln.strip().startswith("//")]
data = json.loads("\n".join(lines))

for ch in data["Changes"]:
    if ch.get("Target") != "Data/Events/Hospital":
        continue
    for k, v in ch.get("Entries", {}).items():
        if not k.startswith("HarveyMod_FirstTreatment/"):
            continue
        print("found key")
        print("comma glitch:", '",\n' in v or ",\n        pause" in v)
        parts = [x.strip() for x in v.split("/") if x.strip()]
        for i, part in enumerate(parts):
            if "волновался" in part:
                print("speak idx", i)
                print("  prev:", parts[i - 1] if i else None)
                print("  cmd:", part[:100])
                print("  next:", parts[i + 1] if i + 1 < len(parts) else None)
        print("remove topicHarveyNeedsFirstTreatment:", "removeConversationTopic topicHarveyNeedsFirstTreatment" in v)
        print("setSkipActions:", "setSkipActions" in v)
