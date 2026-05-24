# Справочник tileset’ов для сцен

Справочник для переиспользования тайлов при проверке/кастомизации CP-событий Harvey Overhaul.

**Источники:** TMX audit-карт в `tmpMap/sve/maps/`, `tmpMap/vanilla/maps/` (см. [`maps-and-tilesets-inventory.md`](maps-and-tilesets-inventory.md)).
**Не менялось:** карты, события, tileset-файлы.

Легенда:
- **Local tile ID** — индекс внутри tileset (0-based).
- **Global ID** = `firstgid` + local_id на конкретной карте.
- **needs visual check** — тип тайла (стул/кровать/окно) не определён без PNG/XNB; указан путь к image source.
- **Сиденье:** в SDV-сценах обычно `showFrame`/animate, не tile `Sit`; NPC ставится на соседний проходимый тайл.

---

## Сводка tileset’ов

| Image source | Tileset name(s) | Tiles | Columns | Maps (firstgid) | Локальный файл |
|--------------|-----------------|------:|--------:|-----------------|----------------|
| `townInterior.png` | 1 | 2176 | 32 | ArchaeologyHouse:65, Hospital:1, Saloon:1 | `…Injury\tmpMap\vanilla\tilesets\townInterior.xnb` |
| `townInterior_2.png` | v15_TownInterior_2 | 1312 | 32 | ArchaeologyHouse:10777, Hospital:2241, Saloon:2177 | `…jury\tmpMap\vanilla\tilesets\townInterior_2.xnb` |
| `paths.png` | paths | 64 | 4 | ArchaeologyHouse:1, Beach:1, BusStop:1976, Custom_AdventurerSummit:1976, Desert:1, Forest:1, Hospital:2177, Mine:1, Mountain:3096, Saloon:3489, SkullCave:1, Town:10680, Woods:1976 | `…verhaulInjury\tmpMap\vanilla\tilesets\paths.xnb` |
| `mine.png` | untitled tile sheet | 288 | 16 | Mine:65 | `…ulInjury\tmpMap\vanilla\tilesets\Mines\mine.xnb` |
| `Mines\mine_desert` | untitled tile sheet | 384 | 16 | SkullCave:65 | `…ardew Valley\Content\Maps\Mines\mine_desert.xnb` |
| `spring_outdoorsTileSheet` | untitled tile sheet | 1975 | 25 | BusStop:1, Custom_AdventurerSummit:1, Forest:20363, Mountain:1, Town:1, Woods:1 | `…p\vanilla\tilesets\spring_outdoorsTileSheet.xnb` |
| `spring_outdoorsTileSheet2.PNG` | v16_Outdoors2 | 1120 | 16 | BusStop:4344, Custom_AdventurerSummit:4344, Forest:9296, Mountain:1976, Town:11746, Woods:11659 | `…\vanilla\tilesets\spring_outdoorsTileSheet2.xnb` |
| `desert_festival_tilesheet.png` | zdesert_festival_tilesheet | 1024 | 32 | Desert:6516 | `…lley\Content\Maps\desert_festival_tilesheet.xnb` |
| `DesertTiles.png` | desert-new | 368 | 16 | Desert:6148 | `…lInjury\tmpMap\vanilla\tilesets\DesertTiles.xnb` |
| `DesertTiles_Extended.png` | desert-extended | 33 | 3 | Desert:433 | `…ew Valley\Content\Maps\DesertTiles_Extended.xnb` |
| `spring_beach.png` | zspring_beach | 527 | 17 | Beach:65, Custom_AdventurerSummit:11864, Forest:2369, Town:10744 | `…Injury\tmpMap\vanilla\tilesets\spring_beach.xnb` |
| `spring_island_tilesheet_1.png` | zspring_island_tilesheet_1 | 1280 | 32 | Forest:23713, Town:14366, Woods:12779 | `…\vanilla\tilesets\spring_island_tilesheet_1.xnb` |
| `spring_RedBaneberry_Tilesheet.png` | zspring_RedBaneberry_Tilesheet | 15 | 5 | Woods:10744 | `…\sve\tilesets\spring_RedBaneberry_Tilesheet.png` |
| `spring_Shadows.png` | v16_Shadows | 475 | 19 | Beach:592, Custom_AdventurerSummit:13291, Forest:22338, Mountain:3160, Town:11271, Woods:14059 | `…jury\tmpMap\vanilla\tilesets\spring_Shadows.xnb` |
| `spring_town.png` | zspring_town | 2304 | 32 | Beach:1067, BusStop:2040, Custom_AdventurerSummit:2040, Forest:65, Mountain:18610, Town:8376, Woods:8440 | `…lInjury\tmpMap\vanilla\tilesets\spring_town.xnb` |
| `spring_Waterfalls` | v16_Waterfalls | 900 | 36 | Custom_AdventurerSummit:12391, Forest:22813, Mountain:3635, Town:12866, Woods:10759 | `…y\tmpMap\vanilla\tilesets\spring_Waterfalls.xnb` |
| `VanillaFurniture.png` | zVanillaFurniture | 2976 | 32 | ArchaeologyHouse:2801 | `…Expanded\Assets\Tilesheets\VanillaFurniture.png` |
| `VanillaWallsAndFloors.png` | zVanillaWallsAndFloors | 560 | 16 | ArchaeologyHouse:2241 | `…ded\Assets\Tilesheets\VanillaWallsAndFloors.png` |
| `winter_outdoorsTileSheet.png` | winter_outdoorsTileSheet | 1975 | 25 | Mountain:10935 | `…p\vanilla\tilesets\winter_outdoorsTileSheet.xnb` |
| `zGrandpasFarm_CanopyShadow.png` | zGrandpasFarm_CanopyShadow | 3686 | 38 | Forest:12991 | `…Map\sve\tilesets\zGrandpasFarm_CanopyShadow.png` |
| `zGrandpasFarm_UnderCanopyShadow.png` | zGrandpasFarm_UnderCanopyShadow | 3686 | 38 | Forest:16677 | `…ve\tilesets\zGrandpasFarm_UnderCanopyShadow.png` |
| `spring_SVE_Tilesheet2.png` | zspring_SVE_Tilesheet2 | 5700 | 76 | BusStop:5464, Custom_AdventurerSummit:13766, Forest:24993, Mountain:12910, Town:15646 | `tmpMap\sve\tilesets\spring_SVE_Tilesheet2.png` |
| `spring_z_extras.png` | zspring_z_extras | 6400 | 25 | ArchaeologyHouse:5777, Custom_AdventurerSummit:5464, Desert:466, Forest:2896, Mountain:4535, Town:1976, Woods:2040 | `tmpMap\sve\tilesets\spring_z_extras.png` |
| `z_SVEbuildingShadow.png` | z_SVEbuildingShadow | 600 | 24 | Desert:5692, Forest:12391, Town:13766 | `tmpMap\sve\tilesets\z_SVEbuildingShadow.png` |

---

## Tileset: `1` (`townInterior.png`)

### Общая информация

- **Normalized key:** `towninterior`
- **Tile size:** 16×16 px
- **Image size:** 512×1088 px
- **Columns:** 32
- **Tile count:** 2176
- **Local tile ID range:** 0–2175
- **Image source (открыть для visual check):** `C:\Users\Admin\HarveyOverhaulInjury\tmpMap\vanilla\tilesets\townInterior.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| ArchaeologyHouse | 65 | global = 65 + local_id |
| Hospital | 1 | global = 1 + local_id |
| Saloon | 1 | global = 1 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 871 | (18,2) on Hospital; gid=872; local=871, global=872 | bed_exam: Message "Hospital.1" | map object (Hospital) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 42 | (16,3) on Hospital; gid=43; local=42, global=43 | bed_exam: Message "Hospital.6" | map object (Hospital) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 42 | (18,3) on Hospital; gid=43; local=42, global=43 | bed_exam: Message "Hospital.1" | map object (Hospital) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 939 | (1,4) on Hospital; gid=940; local=939, global=940 | bed_exam: Message "Hospital.2" | map object (Hospital) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 495 | (3,4) on Hospital; gid=496; local=495, global=496 | bed_exam: Message "Hospital.3" | map object (Hospital) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 838 | (9,5) on Hospital; gid=839; local=838, global=839 | door: Door Harvey | map object (Hospital) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 838 | (5,9) on Hospital; gid=839; local=838, global=839 | door: Door | map object (Hospital) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 42 | (7,9) on Hospital; gid=43; local=42, global=43 | bed_exam: Message "Hospital.4" | map object (Hospital) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1005 | (5,16) on Hospital; gid=1006; local=1005, global=1006 | bed_exam: HospitalShop | map object (Hospital) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 971 | (7,16) on Hospital; gid=972; local=971, global=972 | bed_exam: HospitalShop | map object (Hospital) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 887 | (18,4) on Saloon; gid=888; local=887, global=888 | decorative: Message "Saloon.1" | map object (Saloon) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 351 | (16,6) on Saloon; gid=352; local=351, global=352 | decorative: Message "Saloon.2" | map object (Saloon) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 120 | (11,9) on Saloon; gid=121; local=120, global=121 | door: Door | map object (Saloon) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 120 | (20,9) on Saloon; gid=121; local=120, global=121 | door: Door Gus | map object (Saloon) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 824 | (3,16) on Saloon; gid=825; local=824, global=825 | door: Door | map object (Saloon) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 825 | (4,16) on Saloon; gid=826; local=825, global=826 | door: Door | map object (Saloon) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 1443 | (35,17) on Saloon; gid=1444; local=1443, global=1444 | warp: Arcade_Minecart | map object (Saloon) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 643 | (9,4) on ArchaeologyHouse; gid=708; local=643, global=708 | shelf_cabinet: Notes 4 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (10,4) on ArchaeologyHouse; gid=769; local=704, global=769 | shelf_cabinet: Notes 5 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (11,4) on ArchaeologyHouse; gid=769; local=704, global=769 | shelf_cabinet: Notes 6 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (12,4) on ArchaeologyHouse; gid=769; local=704, global=769 | shelf_cabinet: Notes 7 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 644 | (13,4) on ArchaeologyHouse; gid=709; local=644, global=709 | shelf_cabinet: Notes 8 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 643 | (15,4) on ArchaeologyHouse; gid=708; local=643, global=708 | shelf_cabinet: Notes 9 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (16,4) on ArchaeologyHouse; gid=769; local=704, global=769 | shelf_cabinet: Notes 10 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (17,4) on ArchaeologyHouse; gid=769; local=704, global=769 | shelf_cabinet: Notes 11 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (19,4) on ArchaeologyHouse; gid=769; local=704, global=769 | shelf_cabinet: Notes 12 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (20,4) on ArchaeologyHouse; gid=769; local=704, global=769 | shelf_cabinet: Notes 13 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 644 | (21,4) on ArchaeologyHouse; gid=709; local=644, global=709 | shelf_cabinet: Notes 14 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 768 | (9,7) on ArchaeologyHouse; gid=833; local=768, global=833 | shelf_cabinet: Notes 20 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 768 | (9,8) on ArchaeologyHouse; gid=833; local=768, global=833 | shelf_cabinet: Notes 19 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (11,8) on ArchaeologyHouse; gid=769; local=704, global=769 | shelf_cabinet: Notes 15 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (12,8) on ArchaeologyHouse; gid=769; local=704, global=769 | shelf_cabinet: Notes 16 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 644 | (13,8) on ArchaeologyHouse; gid=709; local=644, global=709 | shelf_cabinet: Notes 17 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 643 | (15,8) on ArchaeologyHouse; gid=708; local=643, global=708 | decorative: Message "ArchaeologyHouse.1" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (16,8) on ArchaeologyHouse; gid=769; local=704, global=769 | decorative: Message "ArchaeologyHouse.2" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (17,8) on ArchaeologyHouse; gid=769; local=704, global=769 | decorative: Message "ArchaeologyHouse.3" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (18,8) on ArchaeologyHouse; gid=769; local=704, global=769 | decorative: Message "ArchaeologyHouse.4" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (19,8) on ArchaeologyHouse; gid=769; local=704, global=769 | decorative: Message "ArchaeologyHouse.5" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 768 | (21,8) on ArchaeologyHouse; gid=833; local=768, global=833 | decorative: Message "ArchaeologyHouse.6" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 950 | (4,9) on ArchaeologyHouse; gid=1015; local=950, global=1015 | decorative: Rearrange | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 950 | (5,9) on ArchaeologyHouse; gid=1015; local=950, global=1015 | shelf_cabinet: Gunther | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 950 | (6,9) on ArchaeologyHouse; gid=1015; local=950, global=1015 | shelf_cabinet: DropBox GuntherBox | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 768 | (9,9) on ArchaeologyHouse; gid=833; local=768, global=833 | shelf_cabinet: Notes 18 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 544 | (20,9) on ArchaeologyHouse; gid=609; local=544, global=609 | decorative: Message "ArchaeologyHouse.7" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 800 | (21,9) on ArchaeologyHouse; gid=865; local=800, global=865 | decorative: Message "ArchaeologyHouse.8" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 768 | (9,10) on ArchaeologyHouse; gid=833; local=768, global=833 | shelf_cabinet: Notes 3 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 864 | (21,10) on ArchaeologyHouse; gid=929; local=864, global=929 | decorative: Message "ArchaeologyHouse.9" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 800 | (9,11) on ArchaeologyHouse; gid=865; local=800, global=865 | shelf_cabinet: Notes 2 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 832 | (9,12) on ArchaeologyHouse; gid=897; local=832, global=897 | shelf_cabinet: Notes 1 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 864 | (9,13) on ArchaeologyHouse; gid=929; local=864, global=929 | shelf_cabinet: Notes 0 | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 768 | (6,15) on ArchaeologyHouse; gid=833; local=768, global=833 | decorative: Message "ArchaeologyHouseSVE.12" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 800 | (6,16) on ArchaeologyHouse; gid=865; local=800, global=865 | decorative: Message "ArchaeologyHouseSVE.13" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 643 | (21,16) on ArchaeologyHouse; gid=708; local=643, global=708 | decorative: Message "ArchaeologyHouseSVE.1" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 704 | (22,16) on ArchaeologyHouse; gid=769; local=704, global=769 | decorative: Message "ArchaeologyHouseSVE.2" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 644 | (23,16) on ArchaeologyHouse; gid=709; local=644, global=709 | decorative: Message "ArchaeologyHouseSVE.3" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 832 | (6,17) on ArchaeologyHouse; gid=897; local=832, global=897 | decorative: Message "ArchaeologyHouseSVE.14" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 864 | (6,18) on ArchaeologyHouse; gid=929; local=864, global=929 | decorative: Message "ArchaeologyHouseSVE.14" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |

### Рекомендации по повторному использованию

- **Клиника / Hospital / Saloon / ArchaeologyHouse:** двери, стойки, кушетки, шкафы — Buildings-слой; NPC ставить на соседний проходимый Back-тайл.
- **Сидение в сцене:** `faceDirection farmer 2` → `showFrame farmer 107`; `showFrame` не перемещает actor — final tile через `move`/`warp` до showFrame; рядом с Buildings-мебелью.
- **Не ставить персонажей** на тайлы с Buildings≠0 (двери, кушетки, стойки).

---

## Tileset: `v15_TownInterior_2` (`townInterior_2.png`)

### Общая информация

- **Normalized key:** `towninterior_2`
- **Tile size:** 16×16 px
- **Image size:** 512×656 px
- **Columns:** 32
- **Tile count:** 1312
- **Local tile ID range:** 0–1311
- **Image source (открыть для visual check):** `C:\Users\Admin\HarveyOverhaulInjury\tmpMap\vanilla\tilesets\townInterior_2.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| ArchaeologyHouse | 10777 | global = 10777 + local_id |
| Hospital | 2241 | global = 2241 + local_id |
| Saloon | 2177 | global = 2177 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 46 | (18,16) on Saloon; gid=2223; local=46, global=2223 | counter_shop: DropBox GusFridge | map object (Saloon) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |

### Рекомендации по повторному использованию

- **Клиника / Hospital / Saloon / ArchaeologyHouse:** двери, стойки, кушетки, шкафы — Buildings-слой; NPC ставить на соседний проходимый Back-тайл.
- **Сидение в сцене:** `faceDirection farmer 2` → `showFrame farmer 107`; `showFrame` не перемещает actor — final tile через `move`/`warp` до showFrame; рядом с Buildings-мебелью.
- **Не ставить персонажей** на тайлы с Buildings≠0 (двери, кушетки, стойки).

---

## Tileset: `paths` (`paths.png`)

### Общая информация

- **Normalized key:** `paths`
- **Tile size:** 16×16 px
- **Image size:** 64×256 px
- **Columns:** 4
- **Tile count:** 64
- **Local tile ID range:** 0–63
- **Image source (открыть для visual check):** `C:\Users\Admin\HarveyOverhaulInjury\tmpMap\vanilla\tilesets\paths.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| ArchaeologyHouse | 1 | global = 1 + local_id |
| Beach | 1 | global = 1 + local_id |
| BusStop | 1976 | global = 1976 + local_id |
| Custom_AdventurerSummit | 1976 | global = 1976 + local_id |
| Desert | 1 | global = 1 + local_id |
| Forest | 1 | global = 1 + local_id |
| Hospital | 2177 | global = 2177 + local_id |
| Mine | 1 | global = 1 + local_id |
| Mountain | 3096 | global = 3096 + local_id |
| Saloon | 3489 | global = 3489 + local_id |
| SkullCave | 1 | global = 1 + local_id |
| Town | 10680 | global = 10680 + local_id |
| Woods | 1976 | global = 1976 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 0 | gid 1 на Forest (firstgid=1) | path: PathType=NE | tileset property (Forest TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1 | gid 2 на Forest (firstgid=1) | path: PathType=SE | tileset property (Forest TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 2 | gid 3 на Forest (firstgid=1) | path: PathType=WS | tileset property (Forest TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 3 | gid 4 на Forest (firstgid=1) | path: PathType=WN | tileset property (Forest TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 4 | gid 5 на Forest (firstgid=1) | path: PathType=Crossroad | tileset property (Forest TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 7 | gid 8 на Forest (firstgid=1) | path: PathType=End | tileset property (Forest TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 0 | gid 1976 на Custom_AdventurerSummit (firstgid=1976) | path: PathType=NE | tileset property (Custom_AdventurerSummit TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1 | gid 1977 на Custom_AdventurerSummit (firstgid=1976) | path: PathType=SE | tileset property (Custom_AdventurerSummit TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 2 | gid 1978 на Custom_AdventurerSummit (firstgid=1976) | path: PathType=WS | tileset property (Custom_AdventurerSummit TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 3 | gid 1979 на Custom_AdventurerSummit (firstgid=1976) | path: PathType=WN | tileset property (Custom_AdventurerSummit TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 4 | gid 1980 на Custom_AdventurerSummit (firstgid=1976) | path: PathType=Crossroad | tileset property (Custom_AdventurerSummit TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 7 | gid 1983 на Custom_AdventurerSummit (firstgid=1976) | path: PathType=End | tileset property (Custom_AdventurerSummit TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 0 | gid 3096 на Mountain (firstgid=3096) | path: PathType=NE | tileset property (Mountain TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1 | gid 3097 на Mountain (firstgid=3096) | path: PathType=SE | tileset property (Mountain TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 2 | gid 3098 на Mountain (firstgid=3096) | path: PathType=WS | tileset property (Mountain TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 3 | gid 3099 на Mountain (firstgid=3096) | path: PathType=WN | tileset property (Mountain TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 4 | gid 3100 на Mountain (firstgid=3096) | path: PathType=Crossroad | tileset property (Mountain TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 7 | gid 3103 на Mountain (firstgid=3096) | path: PathType=End | tileset property (Mountain TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 0 | gid 10680 на Town (firstgid=10680) | path: PathType=NE | tileset property (Town TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1 | gid 10681 на Town (firstgid=10680) | path: PathType=SE | tileset property (Town TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 2 | gid 10682 на Town (firstgid=10680) | path: PathType=WS | tileset property (Town TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 3 | gid 10683 на Town (firstgid=10680) | path: PathType=WN | tileset property (Town TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 4 | gid 10684 на Town (firstgid=10680) | path: PathType=Crossroad | tileset property (Town TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 7 | gid 10687 на Town (firstgid=10680) | path: PathType=End | tileset property (Town TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 0 | gid 1 на Desert (firstgid=1) | path: PathType=NE | tileset property (Desert TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 3 | gid 4 на Desert (firstgid=1) | path: PathType=WN | tileset property (Desert TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 0 | gid 1 на Beach (firstgid=1) | path: PathType=NE | tileset property (Beach TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1 | gid 2 на Beach (firstgid=1) | path: PathType=SE | tileset property (Beach TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 2 | gid 3 на Beach (firstgid=1) | path: PathType=WS | tileset property (Beach TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 3 | gid 4 на Beach (firstgid=1) | path: PathType=WN | tileset property (Beach TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 4 | gid 5 на Beach (firstgid=1) | path: PathType=Crossroad | tileset property (Beach TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 7 | gid 8 на Beach (firstgid=1) | path: PathType=End | tileset property (Beach TMX) | Paths | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |

### Рекомендации по повторному использованию

- **Paths-слой:** NPC-маршруты; для событий не заменять path-тайлы без проверки schedule.

---

## Tileset: `untitled tile sheet` (`mine.png`)

### Общая информация

- **Normalized key:** `mine`
- **Tile size:** 16×16 px
- **Image size:** 256×288 px
- **Columns:** 16
- **Tile count:** 288
- **Local tile ID range:** 0–287
- **Image source (открыть для visual check):** `C:\Users\Admin\HarveyOverhaulInjury\tmpMap\vanilla\tilesets\Mines\mine.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Mine | 65 | global = 65 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 155 | (67,17) on Mine; gid=220; local=155, global=220 | warp: LoadMap Mountain 103 17 | map object (Mine) | Back objectgroup | object overlay | осторожно | needs visual check | Action активен в runtime |
| 112 | (17,3) on Mine; gid=177; local=112, global=177 | warp: MineElevator | map object (Mine) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 195 | (11,10) on Mine; gid=260; local=195, global=260 | warp: MinecartTransport | map object (Mine) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 224 | (12,10) on Mine; gid=289; local=224, global=289 | warp: MinecartTransport | map object (Mine) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |

### Рекомендации по повторному использованию

- **Mine / SkullCave:** пол и стены mine sheet; лифт/шахта — object Action, не декор.

---

## Tileset: `untitled tile sheet` (`Mines\mine_desert`)

### Общая информация

- **Normalized key:** `mines/mine_desert`
- **Tile size:** 16×16 px
- **Image size:** 256×384 px
- **Columns:** 16
- **Tile count:** 384
- **Local tile ID range:** 0–383
- **Image source (открыть для visual check):** `D:\Games\Steam\steamapps\common\Stardew Valley\Content\Maps\Mines\mine_desert.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| SkullCave | 65 | global = 65 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 286 | (3,3) on SkullCave; gid=351; local=286, global=351 | door: SkullDoor | map object (SkullCave) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |

### Рекомендации по повторному использованию

- **Mine / SkullCave:** пол и стены mine sheet; лифт/шахта — object Action, не декор.

---

## Tileset: `untitled tile sheet` (`spring_outdoorsTileSheet`)

### Общая информация

- **Normalized key:** `spring_outdoorstilesheet`
- **Tile size:** 16×16 px
- **Image size:** 400×1264 px
- **Columns:** 25
- **Tile count:** 1975
- **Local tile ID range:** 0–1974
- **Image source (открыть для visual check):** `C:\Users\Admin\HarveyOverhaulInjury\tmpMap\vanilla\tilesets\spring_outdoorsTileSheet.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| BusStop | 1 | global = 1 + local_id |
| Custom_AdventurerSummit | 1 | global = 1 + local_id |
| Forest | 20363 | global = 20363 + local_id |
| Mountain | 1 | global = 1 + local_id |
| Town | 1 | global = 1 + local_id |
| Woods | 1 | global = 1 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 209 | gid 210 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 234 | gid 235 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 285 | gid 286 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 308 | gid 309 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 309 | gid 310 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 310 | gid 311 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 333 | gid 334 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 334 | gid 335 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 335 | gid 336 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 336 | gid 337 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 628 | gid 629 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 629 | gid 630 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 653 | gid 654 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 734 | gid 735 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 759 | gid 760 на Woods (firstgid=1) | passability: Water=t | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 779 | gid 780 на Woods (firstgid=1) | passability: Passable=T, Type=Wood | tileset property (Woods TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 780 | gid 781 на Woods (firstgid=1) | passability: Passable=T, Type=Wood | tileset property (Woods TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 781 | gid 782 на Woods (firstgid=1) | passability: Passable=T, Type=Wood | tileset property (Woods TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 782 | gid 783 на Woods (firstgid=1) | passability: Passable=T, Type=Wood | tileset property (Woods TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 809 | gid 810 на Woods (firstgid=1) | passability: Passable=T, Type=Wood | tileset property (Woods TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 834 | gid 835 на Woods (firstgid=1) | passability: Passable=T, Type=Wood | tileset property (Woods TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 859 | gid 860 на Woods (firstgid=1) | passability: Passable=T, Type=Wood | tileset property (Woods TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 884 | gid 885 на Woods (firstgid=1) | passability: Passable=T, Type=Wood | tileset property (Woods TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 1186 | gid 1187 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1187 | gid 1188 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1188 | gid 1189 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1211 | gid 1212 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1212 | gid 1213 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1213 | gid 1214 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1227 | gid 1228 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1228 | gid 1229 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1229 | gid 1230 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1230 | gid 1231 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1231 | gid 1232 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1237 | gid 1238 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1238 | gid 1239 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1239 | gid 1240 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1240 | gid 1241 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1241 | gid 1242 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1242 | gid 1243 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1243 | gid 1244 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1244 | gid 1245 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1245 | gid 1246 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1246 | gid 1247 на Woods (firstgid=1) | passability: Passable=F, Water=T | tileset property (Woods TMX) | Back (или декор Front) | Passable=F (Back блокирует) | нет на тайле | нет | коллизия Back; не ставить NPC |
| 1247 | gid 1248 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1248 | gid 1249 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1249 | gid 1250 на Woods (firstgid=1) | passability: Passable=F, Water=T | tileset property (Woods TMX) | Back (или декор Front) | Passable=F (Back блокирует) | нет на тайле | нет | коллизия Back; не ставить NPC |
| 1252 | gid 1253 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1253 | gid 1254 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1254 | gid 1255 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1255 | gid 1256 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1256 | gid 1257 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1263 | gid 1264 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1264 | gid 1265 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1265 | gid 1266 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1266 | gid 1267 на Woods (firstgid=1) | passability: Passable=F, Water=T | tileset property (Woods TMX) | Back (или декор Front) | Passable=F (Back блокирует) | нет на тайле | нет | коллизия Back; не ставить NPC |
| 1267 | gid 1268 на Woods (firstgid=1) | passability: Passable=F, Water=T | tileset property (Woods TMX) | Back (или декор Front) | Passable=F (Back блокирует) | нет на тайле | нет | коллизия Back; не ставить NPC |
| 1268 | gid 1269 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1269 | gid 1270 на Woods (firstgid=1) | passability: Water=T, asdf=F | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1270 | gid 1271 на Woods (firstgid=1) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| … | … | ещё 565 записей | … | … | … | … | … | … |

### Рекомендации по повторному использованию

- **Лес / гроза / outdoor:** Passable=T Type=Wood — возможные мостики/настилы (**needs visual check**).
- **Front/AlwaysFront:** декор, тени, кусты — персонаж может визуально перекрыться.

---

## Tileset: `v16_Outdoors2` (`spring_outdoorsTileSheet2.PNG`)

### Общая информация

- **Normalized key:** `spring_outdoorstilesheet2`
- **Tile size:** 16×16 px
- **Image size:** 256×1120 px
- **Columns:** 16
- **Tile count:** 1120
- **Local tile ID range:** 0–1119
- **Image source (открыть для visual check):** `C:\Users\Admin\HarveyOverhaulInjury\tmpMap\vanilla\tilesets\spring_outdoorsTileSheet2.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| BusStop | 4344 | global = 4344 + local_id |
| Custom_AdventurerSummit | 4344 | global = 4344 + local_id |
| Forest | 9296 | global = 9296 + local_id |
| Mountain | 1976 | global = 1976 + local_id |
| Town | 11746 | global = 11746 + local_id |
| Woods | 11659 | global = 11659 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 816 | gid 12475 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 817 | gid 12476 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 820 | gid 12479 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 821 | gid 12480 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 822 | gid 12481 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 832 | gid 12491 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 833 | gid 12492 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 834 | gid 12493 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 835 | gid 12494 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 836 | gid 12495 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 837 | gid 12496 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 838 | gid 12497 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 848 | gid 12507 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 849 | gid 12508 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 850 | gid 12509 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 851 | gid 12510 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 852 | gid 12511 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 853 | gid 12512 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 854 | gid 12513 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 864 | gid 12523 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 865 | gid 12524 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 866 | gid 12525 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 867 | gid 12526 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 868 | gid 12527 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 869 | gid 12528 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 870 | gid 12529 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 880 | gid 12539 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 881 | gid 12540 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 882 | gid 12541 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 883 | gid 12542 на Woods (firstgid=11659) | passability: Water=T | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 816 | gid 10112 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 817 | gid 10113 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 820 | gid 10116 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 821 | gid 10117 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 822 | gid 10118 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 832 | gid 10128 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 833 | gid 10129 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 834 | gid 10130 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 835 | gid 10131 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 836 | gid 10132 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 837 | gid 10133 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 838 | gid 10134 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 848 | gid 10144 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 849 | gid 10145 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 850 | gid 10146 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 851 | gid 10147 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 852 | gid 10148 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 853 | gid 10149 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 854 | gid 10150 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 864 | gid 10160 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 865 | gid 10161 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 866 | gid 10162 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 867 | gid 10163 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 868 | gid 10164 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 869 | gid 10165 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 870 | gid 10166 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 880 | gid 10176 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 881 | gid 10177 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 882 | gid 10178 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 883 | gid 10179 на Forest (firstgid=9296) | passability: Water=T | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| … | … | ещё 93 записей | … | … | … | … | … | … |

### Рекомендации по повторному использованию

- **Лес / гроза / outdoor:** Passable=T Type=Wood — возможные мостики/настилы (**needs visual check**).
- **Front/AlwaysFront:** декор, тени, кусты — персонаж может визуально перекрыться.

---

## Tileset: `zdesert_festival_tilesheet` (`desert_festival_tilesheet.png`)

### Общая информация

- **Normalized key:** `desert_festival_tilesheet`
- **Tile size:** 16×16 px
- **Image size:** 512×512 px
- **Columns:** 32
- **Tile count:** 1024
- **Local tile ID range:** 0–1023
- **Image source (открыть для visual check):** `D:\Games\Steam\steamapps\common\Stardew Valley\Content\Maps\desert_festival_tilesheet.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Desert | 6516 | global = 6516 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 145 | gid 6661 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 174 | gid 6690 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 181 | gid 6697 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 182 | gid 6698 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 183 | gid 6699 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 186 | gid 6702 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 187 | gid 6703 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 206 | gid 6722 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 212 | gid 6728 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 213 | gid 6729 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 214 | gid 6730 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 215 | gid 6731 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 218 | gid 6734 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 219 | gid 6735 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 238 | gid 6754 на Desert (firstgid=6516) | terrain: Type=Stone | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 244 | gid 6760 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 245 | gid 6761 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 270 | gid 6786 на Desert (firstgid=6516) | terrain: Type=Stone | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 276 | gid 6792 на Desert (firstgid=6516) | terrain: Type=Stone | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 302 | gid 6818 на Desert (firstgid=6516) | terrain: Type=Stone | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 303 | gid 6819 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 304 | gid 6820 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 305 | gid 6821 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 306 | gid 6822 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 308 | gid 6824 на Desert (firstgid=6516) | terrain: Type=Stone | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 335 | gid 6851 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 336 | gid 6852 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 337 | gid 6853 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 338 | gid 6854 на Desert (firstgid=6516) | terrain: Diggable=T, Spawnable=T, Type=Dirt | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 340 | gid 6856 на Desert (firstgid=6516) | terrain: Type=Stone | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |

### Рекомендации по повторному использованию

- **needs visual check** — откройте image source и сверьте с картой в Tiled/игре.

---

## Tileset: `desert-new` (`DesertTiles.png`)

### Общая информация

- **Normalized key:** `deserttiles`
- **Tile size:** 16×16 px
- **Image size:** 256×368 px
- **Columns:** 16
- **Tile count:** 368
- **Local tile ID range:** 0–367
- **Image source (открыть для visual check):** `C:\Users\Admin\HarveyOverhaulInjury\tmpMap\vanilla\tilesets\DesertTiles.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Desert | 6148 | global = 6148 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 91 | gid 6239 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 92 | gid 6240 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 93 | gid 6241 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 94 | gid 6242 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 95 | gid 6243 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 107 | gid 6255 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 108 | gid 6256 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 109 | gid 6257 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 110 | gid 6258 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 111 | gid 6259 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 123 | gid 6271 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 124 | gid 6272 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 125 | gid 6273 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 126 | gid 6274 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 127 | gid 6275 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 139 | gid 6287 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 140 | gid 6288 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 141 | gid 6289 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 155 | gid 6303 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 156 | gid 6304 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 171 | gid 6319 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 172 | gid 6320 на Desert (firstgid=6148) | passability: Water=T | tileset property (Desert TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 206 | (18,27) on Desert; gid=271; local=206, global=6354 | warp: DesertBus | map object (Desert) | Back objectgroup | object overlay | осторожно | needs visual check | Action активен в runtime |
| 355 | (20,14) on Desert; gid=420; local=355, global=6503 | door: LockedDoorWarp 4 9 SandyHouse 900 2350 | map object (Desert) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 19 | (51,89) on Desert; gid=84; local=19, global=6167 | decorative: SandDragon | map object (Desert) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 20 | (52,89) on Desert; gid=85; local=20, global=6168 | decorative: SandDragon | map object (Desert) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 148 | (7,107) on Desert; gid=213; local=148, global=6296 | decorative: Message "Desert.7" | map object (Desert) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |

### Рекомендации по повторному использованию

- **Desert storm:** площадка у автобуса; Light property на карте, не на tile.

---

## Tileset: `desert-extended` (`DesertTiles_Extended.png`)

### Общая информация

- **Normalized key:** `deserttiles_extended`
- **Tile size:** 16×16 px
- **Image size:** 48×176 px
- **Columns:** 3
- **Tile count:** 33
- **Local tile ID range:** 0–32
- **Image source (открыть для visual check):** `D:\Games\Steam\steamapps\common\Stardew Valley\Content\Maps\DesertTiles_Extended.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Desert | 433 | global = 433 + local_id |

### Полезные тайлы

_В TMX нет tile properties / object Action для этого sheet. **needs visual check** — откройте image source._

### Рекомендации по повторному использованию

- **Desert storm:** площадка у автобуса; Light property на карте, не на tile.

---

## Tileset: `zspring_beach` (`spring_beach.png`)

### Общая информация

- **Normalized key:** `spring_beach`
- **Tile size:** 16×16 px
- **Image size:** 272×496 px
- **Columns:** 17
- **Tile count:** 527
- **Local tile ID range:** 0–526
- **Image source (открыть для visual check):** `C:\Users\Admin\HarveyOverhaulInjury\tmpMap\vanilla\tilesets\spring_beach.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Beach | 65 | global = 65 + local_id |
| Custom_AdventurerSummit | 11864 | global = 11864 + local_id |
| Forest | 2369 | global = 2369 + local_id |
| Town | 10744 | global = 10744 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 173 | (33,102) on Forest; gid=2542; local=173, global=2542 | decorative: Message "Forest.41" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 174 | (34,102) on Forest; gid=2543; local=174, global=2543 | decorative: Message "Forest.41" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 190 | (33,103) on Forest; gid=2559; local=190, global=2559 | decorative: Message "Forest.41" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 191 | (34,103) on Forest; gid=2560; local=191, global=2560 | decorative: Message "Forest.41" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 7 | gid 72 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 8 | gid 73 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 75 | gid 140 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 89 | gid 154 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 90 | gid 155 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 91 | gid 156 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 92 | gid 157 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 93 | gid 158 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 107 | gid 172 на Beach (firstgid=65) | passability: Type=Dirt, Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 108 | gid 173 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 109 | gid 174 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 110 | gid 175 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 124 | gid 189 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 125 | gid 190 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 126 | gid 191 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 127 | gid 192 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 128 | gid 193 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 129 | gid 194 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 130 | gid 195 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 131 | gid 196 на Beach (firstgid=65) | passability: Passable=F, Water=T | tileset property (Beach TMX) | Back (или декор Front) | Passable=F (Back блокирует) | нет на тайле | нет | коллизия Back; не ставить NPC |
| 141 | gid 206 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 142 | gid 207 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 143 | gid 208 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 144 | gid 209 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 145 | gid 210 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 146 | gid 211 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 147 | gid 212 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 148 | gid 213 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 158 | gid 223 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 159 | gid 224 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 160 | gid 225 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 161 | gid 226 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 162 | gid 227 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 163 | gid 228 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 164 | gid 229 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 165 | gid 230 на Beach (firstgid=65) | passability: Passable=F, Water=T | tileset property (Beach TMX) | Back (или декор Front) | Passable=F (Back блокирует) | нет на тайле | нет | коллизия Back; не ставить NPC |
| 175 | gid 240 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 176 | gid 241 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 177 | gid 242 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 178 | gid 243 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 179 | gid 244 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 180 | gid 245 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 181 | gid 246 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 185 | gid 250 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 192 | gid 257 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 193 | gid 258 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 194 | gid 259 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 195 | gid 260 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 196 | gid 261 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 197 | gid 262 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 198 | gid 263 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 203 | gid 268 на Beach (firstgid=65) | passability: Water=T | tileset property (Beach TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 209 | gid 274 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 210 | gid 275 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 211 | gid 276 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 212 | gid 277 на Beach (firstgid=65) | passability: Passable=T, Type=Dirt | tileset property (Beach TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| … | … | ещё 78 записей | … | … | … | … | … | … |

### Рекомендации по повторному использованию

- **Лес / гроза / outdoor:** Passable=T Type=Wood — возможные мостики/настилы (**needs visual check**).
- **Front/AlwaysFront:** декор, тени, кусты — персонаж может визуально перекрыться.
- **Beach / pier:** пирс E4 — проверить Passable и Front на (39,23).

---

## Tileset: `zspring_island_tilesheet_1` (`spring_island_tilesheet_1.png`)

### Общая информация

- **Normalized key:** `spring_island_tilesheet_1`
- **Tile size:** 16×16 px
- **Image size:** 512×640 px
- **Columns:** 32
- **Tile count:** 1280
- **Local tile ID range:** 0–1279
- **Image source (открыть для visual check):** `C:\Users\Admin\HarveyOverhaulInjury\tmpMap\vanilla\tilesets\spring_island_tilesheet_1.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Forest | 23713 | global = 23713 + local_id |
| Town | 14366 | global = 14366 + local_id |
| Woods | 12779 | global = 12779 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 305 | gid 13084 на Woods (firstgid=12779) | terrain: Type=Grass | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 306 | gid 13085 на Woods (firstgid=12779) | terrain: Type=Grass | tileset property (Woods TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 305 | gid 24018 на Forest (firstgid=23713) | terrain: Type=Grass | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 306 | gid 24019 на Forest (firstgid=23713) | terrain: Type=Grass | tileset property (Forest TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 305 | gid 14671 на Town (firstgid=14366) | terrain: Type=Grass | tileset property (Town TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 306 | gid 14672 на Town (firstgid=14366) | terrain: Type=Grass | tileset property (Town TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |

### Рекомендации по повторному использованию

- **Лес / гроза / outdoor:** Passable=T Type=Wood — возможные мостики/настилы (**needs visual check**).
- **Front/AlwaysFront:** декор, тени, кусты — персонаж может визуально перекрыться.

---

## Tileset: `zspring_RedBaneberry_Tilesheet` (`spring_RedBaneberry_Tilesheet.png`)

### Общая информация

- **Normalized key:** `spring_redbaneberry_tilesheet`
- **Tile size:** 16×16 px
- **Image size:** 80×48 px
- **Columns:** 5
- **Tile count:** 15
- **Local tile ID range:** 0–14
- **Image source (открыть для visual check):** `tmpMap\sve\tilesets\spring_RedBaneberry_Tilesheet.png`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Woods | 10744 | global = 10744 + local_id |

### Полезные тайлы

_В TMX нет tile properties / object Action для этого sheet. **needs visual check** — откройте image source._

### Рекомендации по повторному использованию

- **Лес / гроза / outdoor:** Passable=T Type=Wood — возможные мостики/настилы (**needs visual check**).
- **Front/AlwaysFront:** декор, тени, кусты — персонаж может визуально перекрыться.

---

## Tileset: `v16_Shadows` (`spring_Shadows.png`)

### Общая информация

- **Normalized key:** `spring_shadows`
- **Tile size:** 16×16 px
- **Image size:** 304×400 px
- **Columns:** 19
- **Tile count:** 475
- **Local tile ID range:** 0–474
- **Image source (открыть для visual check):** `C:\Users\Admin\HarveyOverhaulInjury\tmpMap\vanilla\tilesets\spring_Shadows.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Beach | 592 | global = 592 + local_id |
| Custom_AdventurerSummit | 13291 | global = 13291 + local_id |
| Forest | 22338 | global = 22338 + local_id |
| Mountain | 3160 | global = 3160 + local_id |
| Town | 11271 | global = 11271 + local_id |
| Woods | 14059 | global = 14059 + local_id |

### Полезные тайлы

_В TMX нет tile properties / object Action для этого sheet. **needs visual check** — откройте image source._

### Рекомендации по повторному использованию

- **Лес / гроза / outdoor:** Passable=T Type=Wood — возможные мостики/настилы (**needs visual check**).
- **Front/AlwaysFront:** декор, тени, кусты — персонаж может визуально перекрыться.
- **Только визуал:** тени/canopy — AlwaysFront; не использовать для коллизий.

---

## Tileset: `zspring_town` (`spring_town.png`)

### Общая информация

- **Normalized key:** `spring_town`
- **Tile size:** 16×16 px
- **Image size:** 512×1152 px
- **Columns:** 32
- **Tile count:** 2304
- **Local tile ID range:** 0–2303
- **Image source (открыть для visual check):** `C:\Users\Admin\HarveyOverhaulInjury\tmpMap\vanilla\tilesets\spring_town.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Beach | 1067 | global = 1067 + local_id |
| BusStop | 2040 | global = 2040 + local_id |
| Custom_AdventurerSummit | 2040 | global = 2040 + local_id |
| Forest | 65 | global = 65 + local_id |
| Mountain | 18610 | global = 18610 + local_id |
| Town | 8376 | global = 8376 + local_id |
| Woods | 8440 | global = 8440 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 852 | (13,58) on Woods; gid=9292; local=852, global=9292 | decorative: Message "SecretWoods.1" | map object (Woods) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 851 | (12,58) on Woods; gid=9291; local=851, global=9291 | decorative: Message "SecretWoods.1" | map object (Woods) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 850 | (11,58) on Woods; gid=9290; local=850, global=9290 | decorative: Message "SecretWoods.1" | map object (Woods) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 914 | (14,58) on Woods; gid=9354; local=914, global=9354 | decorative: Message "SecretWoods.1" | map object (Woods) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 915 | (15,58) on Woods; gid=9355; local=915, global=9355 | decorative: Message "SecretWoods.1" | map object (Woods) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 321 | (62,4) on Custom_AdventurerSummit; gid=2361; local=321, global=2361 | decorative: Message "Summit.1" | map object (Custom_AdventurerSummit) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 159 | gid 8535 на Town (firstgid=8376) | passability: Passable=T | tileset property (Town TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 191 | gid 8567 на Town (firstgid=8376) | passability: Passable=T | tileset property (Town TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 223 | gid 8599 на Town (firstgid=8376) | passability: Passable=T | tileset property (Town TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 905 | gid 9281 на Town (firstgid=8376) | passability: Passable=F | tileset property (Town TMX) | Back (или декор Front) | Passable=F (Back блокирует) | нет на тайле | нет | коллизия Back; не ставить NPC |
| 916 | gid 9292 на Town (firstgid=8376) | passability: Water=T | tileset property (Town TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 917 | gid 9293 на Town (firstgid=8376) | passability: Water=T | tileset property (Town TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 918 | gid 9294 на Town (firstgid=8376) | passability: Water=T | tileset property (Town TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 919 | gid 9295 на Town (firstgid=8376) | passability: Water=T | tileset property (Town TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 950 | gid 9326 на Town (firstgid=8376) | passability: Water=T | tileset property (Town TMX) | Back (или декор Front) | — | needs visual check | needs visual check | сверить Buildings/Front на целевой карте |
| 1260 | (19,13) on Town; gid=9636; local=1260, global=9636 | decorative: Message "PelicanTown.2" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1261 | (20,13) on Town; gid=9637; local=1261, global=9637 | decorative: Message "PelicanTown.2" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1262 | (21,13) on Town; gid=9638; local=1262, global=9638 | decorative: Message "PelicanTown.2" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1263 | (22,13) on Town; gid=9639; local=1263, global=9639 | decorative: Message "PelicanTown.2" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1541 | (52,19) on Town; gid=9917; local=1541, global=9917 | warp: WarpCommunityCenter | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1542 | (53,19) on Town; gid=9918; local=1542, global=9918 | warp: WarpCommunityCenter | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 944 | (59,45) on Town; gid=9320; local=944, global=9320 | decorative: Message "PelicanTown.44" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 88 | (66,45) on Town; gid=8464; local=88, global=8464 | decorative: Message "PelicanTown.43" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 976 | (59,46) on Town; gid=9352; local=976, global=9352 | decorative: Message "PelicanTown.44" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 55 | (86,49) on Town; gid=8431; local=55, global=8431 | decorative: Message "PelicanTown.19" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1925 | (95,50) on Town; gid=10301; local=1925, global=10301 | door: LockedDoorWarp 13 29 JojaMart 1000 2200 | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 1926 | (96,50) on Town; gid=10302; local=1926, global=10302 | door: LockedDoorWarp 14 29 JojaMart 1000 2200 | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 1970 | (91,51) on Town; gid=10346; local=1970, global=10346 | decorative: Message "PelicanTown.17" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 41 | (67,52) on Town; gid=8417; local=41, global=8417 | decorative: Message "PelicanTown.45" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 579 | (36,55) on Town; gid=8955; local=579, global=8955 | door: LockedDoorWarp 10 19 Hospital 900 1500 | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 618 | (43,56) on Town; gid=8994; local=618, global=8994 | door: LockedDoorWarp 6 29 SeedShop 900 2100 | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 619 | (44,56) on Town; gid=8995; local=619, global=8995 | door: LockedDoorWarp 6 29 SeedShop 900 2100 | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 620 | (45,56) on Town; gid=8996; local=620, global=8996 | decorative: Message "Town.2" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 88 | (62,56) on Town; gid=8464; local=88, global=8464 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 87 | (63,56) on Town; gid=8463; local=87, global=8463 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 885 | (18,61) on Town; gid=9261; local=885, global=9261 | decorative: Message "PelicanTown.52" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 855 | (19,61) on Town; gid=9231; local=855, global=9231 | decorative: Message "PelicanTown.52" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 854 | (17,62) on Town; gid=9230; local=854, global=9230 | decorative: Message "PelicanTown.52" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 339 | (57,63) on Town; gid=8715; local=339, global=8715 | door: LockedDoorWarp 9 24 JoshHouse 800 2000 | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 2008 | (53,68) on Town; gid=10384; local=2008, global=10384 | decorative: NPCSpeechMessageNoRadius Dusty dustyPets | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 753 | (72,68) on Town; gid=9129; local=753, global=9129 | door: LockedDoorWarp 12 9 Trailer 900 2000 | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 596 | (45,70) on Town; gid=8972; local=596, global=8972 | door: LockedDoorWarp 14 24 Saloon 1200 2400 | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 626 | (43,71) on Town; gid=9002; local=626, global=9002 | decorative: Message "PelicanTown.47" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 627 | (44,71) on Town; gid=9003; local=627, global=9003 | decorative: Message "PelicanTown.47" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 983 | (33,77) on Town; gid=9359; local=983, global=9359 | decorative: Message "PelicanTown.46" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 984 | (34,77) on Town; gid=9360; local=984, global=9360 | decorative: Message "PelicanTown.46" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 985 | (35,77) on Town; gid=9361; local=985, global=9361 | decorative: Message "PelicanTown.46" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1015 | (33,78) on Town; gid=9391; local=1015, global=9391 | decorative: Message "PelicanTown.46" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1016 | (34,78) on Town; gid=9392; local=1016, global=9392 | decorative: Message "PelicanTown.46" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1017 | (35,78) on Town; gid=9393; local=1017, global=9393 | decorative: Message "PelicanTown.46" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1079 | (65,80) on Town; gid=9455; local=1079, global=9455 | decorative: Message "PelicanTown.21" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1080 | (66,80) on Town; gid=9456; local=1080, global=9456 | decorative: Message "PelicanTown.21" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1081 | (67,80) on Town; gid=9457; local=1081, global=9457 | decorative: Message "PelicanTown.21" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1082 | (68,80) on Town; gid=9458; local=1082, global=9458 | decorative: Message "PelicanTown.21" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 126 | (100,80) on Town; gid=8502; local=126, global=8502 | decorative: Message "PelicanTown.13" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 127 | (101,80) on Town; gid=8503; local=127, global=8503 | decorative: Message "PelicanTown.13" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 55 | (21,81) on Town; gid=8431; local=55, global=8431 | decorative: Message "PelicanTown.5" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1111 | (65,81) on Town; gid=9487; local=1111, global=9487 | decorative: Message "PelicanTown.21" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1112 | (66,81) on Town; gid=9488; local=1112, global=9488 | decorative: Message "PelicanTown.21" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1113 | (67,81) on Town; gid=9489; local=1113, global=9489 | decorative: Message "PelicanTown.21" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| … | … | ещё 32 записей | … | … | … | … | … | … |

### Рекомендации по повторному использованию

- **Лес / гроза / outdoor:** Passable=T Type=Wood — возможные мостики/настилы (**needs visual check**).
- **Front/AlwaysFront:** декор, тени, кусты — персонаж может визуально перекрыться.
- **Town / Forest декор:** здания, скамейки, машины — часто Front/Buildings; rescue truck = temporaryAnimatedSprite, не tile.

---

## Tileset: `v16_Waterfalls` (`spring_Waterfalls`)

### Общая информация

- **Normalized key:** `spring_waterfalls`
- **Tile size:** 16×16 px
- **Image size:** 576×400 px
- **Columns:** 36
- **Tile count:** 900
- **Local tile ID range:** 0–899
- **Image source (открыть для visual check):** `C:\Users\Admin\HarveyOverhaulInjury\tmpMap\vanilla\tilesets\spring_Waterfalls.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Custom_AdventurerSummit | 12391 | global = 12391 + local_id |
| Forest | 22813 | global = 22813 + local_id |
| Mountain | 3635 | global = 3635 + local_id |
| Town | 12866 | global = 12866 + local_id |
| Woods | 10759 | global = 10759 + local_id |

### Полезные тайлы

_В TMX нет tile properties / object Action для этого sheet. **needs visual check** — откройте image source._

### Рекомендации по повторному использованию

- **Лес / гроза / outdoor:** Passable=T Type=Wood — возможные мостики/настилы (**needs visual check**).
- **Front/AlwaysFront:** декор, тени, кусты — персонаж может визуально перекрыться.

---

## Tileset: `zVanillaFurniture` (`VanillaFurniture.png`)

### Общая информация

- **Normalized key:** `vanillafurniture`
- **Tile size:** 16×16 px
- **Image size:** 512×1488 px
- **Columns:** 32
- **Tile count:** 2976
- **Local tile ID range:** 0–2975
- **Image source (открыть для visual check):** `D:\Games\Steam\steamapps\common\Stardew Valley\Mods\Stardew Valley Expanded\[CP] Stardew Valley Expanded\Assets\Tilesheets\VanillaFurniture.png`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| ArchaeologyHouse | 2801 | global = 2801 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 1359 | (27,4) on ArchaeologyHouse; gid=4160; local=1359, global=4160 | decorative: Message "ArchaeologyHouseSVE.9" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1344 | (39,4) on ArchaeologyHouse; gid=4145; local=1344, global=4145 | decorative: Message "ArchaeologyHouseSVE.8" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1345 | (40,4) on ArchaeologyHouse; gid=4146; local=1345, global=4146 | decorative: Message "ArchaeologyHouseSVE.8" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1346 | (41,4) on ArchaeologyHouse; gid=4147; local=1346, global=4147 | decorative: Message "ArchaeologyHouseSVE.8" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1330 | (45,4) on ArchaeologyHouse; gid=4131; local=1330, global=4131 | decorative: Message "ArchaeologyHouseSVE.6" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1336 | (46,4) on ArchaeologyHouse; gid=4137; local=1336, global=4137 | decorative: Message "ArchaeologyHouseSVE.5" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1332 | (47,4) on ArchaeologyHouse; gid=4133; local=1332, global=4133 | decorative: Message "ArchaeologyHouseSVE.7" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1333 | (38,17) on ArchaeologyHouse; gid=4134; local=1333, global=4134 | decorative: Message "ArchaeologyHouseSVE.4" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1334 | (39,17) on ArchaeologyHouse; gid=4135; local=1334, global=4135 | decorative: Message "ArchaeologyHouseSVE.4" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1335 | (40,17) on ArchaeologyHouse; gid=4136; local=1335, global=4136 | decorative: Message "ArchaeologyHouseSVE.4" | map object (ArchaeologyHouse) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |

### Рекомендации по повторному использованию

- **needs visual check** — откройте image source и сверьте с картой в Tiled/игре.

---

## Tileset: `zVanillaWallsAndFloors` (`VanillaWallsAndFloors.png`)

### Общая информация

- **Normalized key:** `vanillawallsandfloors`
- **Tile size:** 16×16 px
- **Image size:** 256×560 px
- **Columns:** 16
- **Tile count:** 560
- **Local tile ID range:** 0–559
- **Image source (открыть для visual check):** `D:\Games\Steam\steamapps\common\Stardew Valley\Mods\Stardew Valley Expanded\[CP] Stardew Valley Expanded\Assets\Tilesheets\VanillaWallsAndFloors.png`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| ArchaeologyHouse | 2241 | global = 2241 + local_id |

### Полезные тайлы

_В TMX нет tile properties / object Action для этого sheet. **needs visual check** — откройте image source._

### Рекомендации по повторному использованию

- **needs visual check** — откройте image source и сверьте с картой в Tiled/игре.

---

## Tileset: `winter_outdoorsTileSheet` (`winter_outdoorsTileSheet.png`)

### Общая информация

- **Normalized key:** `winter_outdoorstilesheet`
- **Tile size:** 16×16 px
- **Image size:** 400×1264 px
- **Columns:** 25
- **Tile count:** 1975
- **Local tile ID range:** 0–1974
- **Image source (открыть для visual check):** `C:\Users\Admin\HarveyOverhaulInjury\tmpMap\vanilla\tilesets\winter_outdoorsTileSheet.xnb`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Mountain | 10935 | global = 10935 + local_id |

### Полезные тайлы

_В TMX нет tile properties / object Action для этого sheet. **needs visual check** — откройте image source._

### Рекомендации по повторному использованию

- **Лес / гроза / outdoor:** Passable=T Type=Wood — возможные мостики/настилы (**needs visual check**).
- **Front/AlwaysFront:** декор, тени, кусты — персонаж может визуально перекрыться.

---

## Tileset: `zGrandpasFarm_CanopyShadow` (`zGrandpasFarm_CanopyShadow.png`)

### Общая информация

- **Normalized key:** `zgrandpasfarm_canopyshadow`
- **Tile size:** 16×16 px
- **Image size:** 616×1552 px
- **Columns:** 38
- **Tile count:** 3686
- **Local tile ID range:** 0–3685
- **Image source (открыть для visual check):** `tmpMap\sve\tilesets\zGrandpasFarm_CanopyShadow.png`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Forest | 12991 | global = 12991 + local_id |

### Полезные тайлы

_В TMX нет tile properties / object Action для этого sheet. **needs visual check** — откройте image source._

### Рекомендации по повторному использованию

- **Только визуал:** тени/canopy — AlwaysFront; не использовать для коллизий.

---

## Tileset: `zGrandpasFarm_UnderCanopyShadow` (`zGrandpasFarm_UnderCanopyShadow.png`)

### Общая информация

- **Normalized key:** `zgrandpasfarm_undercanopyshadow`
- **Tile size:** 16×16 px
- **Image size:** 616×1552 px
- **Columns:** 38
- **Tile count:** 3686
- **Local tile ID range:** 0–3685
- **Image source (открыть для visual check):** `tmpMap\sve\tilesets\zGrandpasFarm_UnderCanopyShadow.png`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Forest | 16677 | global = 16677 + local_id |

### Полезные тайлы

_В TMX нет tile properties / object Action для этого sheet. **needs visual check** — откройте image source._

### Рекомендации по повторному использованию

- **Только визуал:** тени/canopy — AlwaysFront; не использовать для коллизий.

---

## Tileset: `zspring_SVE_Tilesheet2` (`spring_SVE_Tilesheet2.png`)

### Общая информация

- **Normalized key:** `spring_sve_tilesheet2`
- **Tile size:** 16×16 px
- **Image size:** 1216×1200 px
- **Columns:** 76
- **Tile count:** 5700
- **Local tile ID range:** 0–5699
- **Image source (открыть для visual check):** `tmpMap\sve\tilesets\spring_SVE_Tilesheet2.png`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| BusStop | 5464 | global = 5464 + local_id |
| Custom_AdventurerSummit | 13766 | global = 13766 + local_id |
| Forest | 24993 | global = 24993 + local_id |
| Mountain | 12910 | global = 12910 + local_id |
| Town | 15646 | global = 15646 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 2147461896 | (2,21) on Forest; gid=2147486889; local=2147461896, global=2147486889 | decorative: Message "Forest.12" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 2147461897 | (3,21) on Forest; gid=2147486890; local=2147461897, global=2147486890 | decorative: Message "Forest.12" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |

### Рекомендации по повторному использованию

- **Лес / гроза / outdoor:** Passable=T Type=Wood — возможные мостики/настилы (**needs visual check**).
- **Front/AlwaysFront:** декор, тени, кусты — персонаж может визуально перекрыться.
- **SVE-локации:** уникальные тайлы Summit, BusStop patches — только при наличии sheet в игре.

---

## Tileset: `zspring_z_extras` (`spring_z_extras.png`)

### Общая информация

- **Normalized key:** `spring_z_extras`
- **Tile size:** 16×16 px
- **Image size:** 400×4096 px
- **Columns:** 25
- **Tile count:** 6400
- **Local tile ID range:** 0–6399
- **Image source (открыть для visual check):** `tmpMap\sve\tilesets\spring_z_extras.png`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| ArchaeologyHouse | 5777 | global = 5777 + local_id |
| Custom_AdventurerSummit | 5464 | global = 5464 + local_id |
| Desert | 466 | global = 466 + local_id |
| Forest | 2896 | global = 2896 + local_id |
| Mountain | 4535 | global = 4535 + local_id |
| Town | 1976 | global = 1976 + local_id |
| Woods | 2040 | global = 2040 + local_id |

### Полезные тайлы

| Local tile ID | Global ID / firstgid context | Что это | Где использовать | Слой | Passability | NPC рядом | Сиденье | Риски |
|---------------|-------------------------------|---------|------------------|------|-------------|-----------|---------|-------|
| 486 | (92,6) on Forest; gid=3382; local=486, global=3382 | decorative: Message "Forest.55" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 489 | (93,6) on Forest; gid=3385; local=489, global=3385 | decorative: Message "Forest.56" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 482 | (99,6) on Forest; gid=3378; local=482, global=3378 | decorative: Message "Forest.60" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 483 | (100,6) on Forest; gid=3379; local=483, global=3379 | decorative: Message "Forest.60" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 484 | (101,6) on Forest; gid=3380; local=484, global=3380 | decorative: Message "Forest.60" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 479 | (79,14) on Forest; gid=3375; local=479, global=3375 | decorative: Message "Forest.5" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 480 | (80,14) on Forest; gid=3376; local=480, global=3376 | decorative: Message "Forest.5" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 345 | (2,20) on Forest; gid=3241; local=345, global=3241 | decorative: Message "Forest.12" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 346 | (3,20) on Forest; gid=3242; local=346, global=3242 | decorative: Message "Forest.12" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 347 | (4,20) on Forest; gid=3243; local=347, global=3243 | decorative: Message "Forest.12" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 483 | (100,29) on Forest; gid=3379; local=483, global=3379 | decorative: Message "Forest.22" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 482 | (100,30) on Forest; gid=3378; local=482, global=3378 | decorative: Message "Forest.22" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 491 | (83,64) on Forest; gid=3387; local=491, global=3387 | decorative: Message "Forest.59" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 479 | (56,66) on Forest; gid=3375; local=479, global=3375 | decorative: Message "Forest.30" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 480 | (57,66) on Forest; gid=3376; local=480, global=3376 | decorative: Message "Forest.30" | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 794 | (62,66) on Forest; gid=3690; local=794, global=3690 | door: LockedDoorWarp 12 22 Custom_AndyHouse 800 2200 | map object (Forest) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 1783 | gid 7247 на Custom_AdventurerSummit (firstgid=5464) | passability: Passable=T, Type=Wood | tileset property (Custom_AdventurerSummit TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 1784 | gid 7248 на Custom_AdventurerSummit (firstgid=5464) | passability: Passable=T, Type=Wood | tileset property (Custom_AdventurerSummit TMX) | Back (или декор Front) | Passable=T | можно на тайле | needs visual check | Passable=T — проверить Buildings-слой на карте |
| 490 | (26,19) on Custom_AdventurerSummit; gid=5954; local=490, global=5954 | decorative: Message "AdventurerSummit.7" | map object (Custom_AdventurerSummit) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 6350 | (53,30) on Custom_AdventurerSummit; gid=11814; local=6350, global=11814 | decorative: Message "AdventurerSummit.1" | map object (Custom_AdventurerSummit) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 6351 | (54,30) on Custom_AdventurerSummit; gid=11815; local=6351, global=11815 | decorative: Message "AdventurerSummit.1" | map object (Custom_AdventurerSummit) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 6352 | (55,30) on Custom_AdventurerSummit; gid=11816; local=6352, global=11816 | decorative: Message "AdventurerSummit.1" | map object (Custom_AdventurerSummit) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 6375 | (53,31) on Custom_AdventurerSummit; gid=11839; local=6375, global=11839 | decorative: Message "AdventurerSummit.1" | map object (Custom_AdventurerSummit) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 6376 | (54,31) on Custom_AdventurerSummit; gid=11840; local=6376, global=11840 | decorative: Message "AdventurerSummit.1" | map object (Custom_AdventurerSummit) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 6377 | (55,31) on Custom_AdventurerSummit; gid=11841; local=6377, global=11841 | decorative: Message "AdventurerSummit.1" | map object (Custom_AdventurerSummit) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 481 | (36,13) on Town; gid=2457; local=481, global=2457 | decorative: Message "PelicanTown.50" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 481 | (40,47) on Town; gid=2457; local=481, global=2457 | decorative: Message "PelicanTown.41" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 479 | (45,47) on Town; gid=2455; local=479, global=2455 | decorative: Message "PelicanTown.40" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 480 | (46,47) on Town; gid=2456; local=480, global=2456 | decorative: Message "PelicanTown.40" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 1218 | (59,51) on Town; gid=3194; local=1218, global=3194 | door: LockedDoorWarp 20 41 Custom_JenkinsHouse 900 2200 | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | нет | не блокировать дверь/warp |
| 199 | (52,56) on Town; gid=2175; local=199, global=2175 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 47 | (53,56) on Town; gid=2023; local=47, global=2023 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 39 | (54,56) on Town; gid=2015; local=39, global=2015 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 274 | (55,56) on Town; gid=2250; local=274, global=2250 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 196 | (52,57) on Town; gid=2172; local=196, global=2172 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 46 | (53,57) on Town; gid=2022; local=46, global=2022 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 40 | (54,57) on Town; gid=2016; local=40, global=2016 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 273 | (55,57) on Town; gid=2249; local=273, global=2249 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 198 | (52,58) on Town; gid=2174; local=198, global=2174 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 49 | (53,58) on Town; gid=2025; local=49, global=2025 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 43 | (54,58) on Town; gid=2019; local=43, global=2019 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 272 | (55,58) on Town; gid=2248; local=272, global=2248 | decorative: Message "PelicanTown.23" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 499 | (94,76) on Town; gid=2475; local=499, global=2475 | decorative: Message "PelicanTown.14" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 498 | (97,76) on Town; gid=2474; local=498, global=2474 | decorative: Message "PelicanTown.14" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 490 | (98,76) on Town; gid=2466; local=490, global=2466 | decorative: Message "PelicanTown.29" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 482 | (102,76) on Town; gid=2458; local=482, global=2458 | decorative: Message "PelicanTown.28" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 483 | (103,76) on Town; gid=2459; local=483, global=2459 | decorative: Message "PelicanTown.28" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 476 | (42,85) on Town; gid=2452; local=476, global=2452 | decorative: Message "PelicanTown.56" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 496 | (105,99) on Town; gid=2472; local=496, global=2472 | decorative: Message "PelicanTown.32" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 497 | (106,99) on Town; gid=2473; local=497, global=2473 | decorative: Message "PelicanTown.32" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 40 | (96,101) on Town; gid=2016; local=40, global=2016 | decorative: Message "PelicanTown.12" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 43 | (97,101) on Town; gid=2019; local=43, global=2019 | decorative: Message "PelicanTown.12" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 41 | (100,101) on Town; gid=2017; local=41, global=2017 | decorative: Message "PelicanTown.12" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 274 | (104,101) on Town; gid=2250; local=274, global=2250 | decorative: Message "PelicanTown.12" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 273 | (105,101) on Town; gid=2249; local=273, global=2249 | decorative: Message "PelicanTown.12" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 272 | (106,101) on Town; gid=2248; local=272, global=2248 | decorative: Message "PelicanTown.12" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 271 | (107,101) on Town; gid=2247; local=271, global=2247 | decorative: Message "PelicanTown.12" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 138 | (96,102) on Town; gid=2114; local=138, global=2114 | decorative: Message "PelicanTown.12" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 142 | (97,102) on Town; gid=2118; local=142, global=2118 | decorative: Message "PelicanTown.12" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| 493 | (98,102) on Town; gid=2469; local=493, global=2469 | decorative: Message "PelicanTown.12" | map object (Town) | Buildings objectgroup | Buildings≠0 → непроходимо | рядом, не на тайле | needs visual check | Action активен в runtime |
| … | … | ещё 23 записей | … | … | … | … | … | … |

### Рекомендации по повторному использованию

- **Лес / гроза / outdoor:** Passable=T Type=Wood — возможные мостики/настилы (**needs visual check**).
- **Front/AlwaysFront:** декор, тени, кусты — персонаж может визуально перекрыться.
- **SVE-локации:** уникальные тайлы Summit, BusStop patches — только при наличии sheet в игре.

---

## Tileset: `z_SVEbuildingShadow` (`z_SVEbuildingShadow.png`)

### Общая информация

- **Normalized key:** `z_svebuildingshadow`
- **Tile size:** 16×16 px
- **Image size:** 384×400 px
- **Columns:** 24
- **Tile count:** 600
- **Local tile ID range:** 0–599
- **Image source (открыть для visual check):** `tmpMap\sve\tilesets\z_SVEbuildingShadow.png`

**firstgid на картах audit:**

| Map | firstgid | Global ID formula |
|-----|----------|-------------------|
| Desert | 5692 | global = 5692 + local_id |
| Forest | 12391 | global = 12391 + local_id |
| Town | 13766 | global = 13766 + local_id |

### Полезные тайлы

_В TMX нет tile properties / object Action для этого sheet. **needs visual check** — откройте image source._

### Рекомендации по повторному использованию

- **Только визуал:** тени/canopy — AlwaysFront; не использовать для коллизий.

---

## Приложение: тайлы на координатах событий (audit)

Извлечено из layer GID на coords событий. **needs visual check** для визуального типа (стул/кровать/лава).

| Map | X | Y | Layer | GID | Tileset | Local ID | Global ID |
|-----|---|---|-------|-----|---------|----------|-----------|
| Mine | 17 | 7 | Back | 203 | untitled tile sheet | 138 | 203 |
| Mine | 17 | 10 | Back | 220 | untitled tile sheet | 155 | 220 |
| Mine | 15 | 5 | Back | 203 | untitled tile sheet | 138 | 203 |
| Mine | 18 | 14 | Back | 203 | untitled tile sheet | 138 | 203 |
|  |  |  | Front | 279 | untitled tile sheet | 214 | 279 |
| Hospital | 4 | 6 | Back | 1010 | 1 | 1009 | 1010 |
| Hospital | 5 | 9 | Back | 1010 | 1 | 1009 | 1010 |
|  |  |  | Buildings | 839 | 1 | 838 | 839 |
| Hospital | 6 | 10 | Back | 1011 | 1 | 1010 | 1011 |
|  |  |  | Front | 3099 | v15_TownInterior_2 | 858 | 3099 |
| Hospital | 10 | 19 | Back | 93 | 1 | 92 | 93 |
| Hospital | 20 | 5 | Back | 1006 | 1 | 1005 | 1006 |
|  |  |  | Buildings | 1100 | 1 | 1099 | 1100 |
|  |  |  | Paths | 2185 | p | 8 | 2185 |
| Hospital | 3 | 15 | Back | 1010 | 1 | 1009 | 1010 |
|  |  |  | Front | 1004 | 1 | 1003 | 1004 |
| Hospital | 9 | 5 | Back | 1010 | 1 | 1009 | 1010 |
|  |  |  | Buildings | 839 | 1 | 838 | 839 |
| Hospital | 10 | 5 | Back | 1010 | 1 | 1009 | 1010 |
|  |  |  | Buildings | 839 | 1 | 838 | 839 |
| SkullCave | 5 | 5 | Back | 220 | untitled tile sheet | 155 | 220 |
| SkullCave | 7 | 7 | Back | 219 | untitled tile sheet | 154 | 219 |
| Woods | 27 | 18 | Back | 1093 | untitled tile sheet | 1092 | 1093 |
|  |  |  | AlwaysFront | 993 | untitled tile sheet | 992 | 993 |
| Woods | 40 | 20 | Back | 407 | untitled tile sheet | 406 | 407 |
| Forest | 23 | 13 | Back | 10743 | outdoors | 327 | 20690 |
| Forest | 48 | 14 | Back | 10668 | outdoors | 252 | 20615 |
| Forest | 50 | 13 | Back | 10791 | outdoors | 375 | 20738 |
| Forest | 66 | 16 | Back | 10617 | outdoors | 201 | 20564 |
| Forest | 67 | 12 | Back | 10667 | outdoors | 251 | 20614 |
| Custom_AdventurerSummit | 41 | 27 | Back | 153 | outdoors | 152 | 153 |
| Custom_AdventurerSummit | 32 | 42 | Back | 339 | outdoors | 338 | 339 |
| Mountain | 79 | 1 | Back | 339 | outdoors | 338 | 339 |
| Mountain | 44 | 21 | Back | 705 | outdoors | 704 | 705 |
| Town | 39 | 73 | Back | 624 | Landscape | 623 | 624 |
| Town | 28 | 67 | Back | 9300 | Town | 924 | 9300 |
| Town | 26 | 22 | Back | 9113 | Town | 737 | 9113 |
| Town | 35 | 88 | Back | 382 | Landscape | 381 | 382 |
| Town | 72 | 22 | Back | 228 | Landscape | 227 | 228 |
| Saloon | 14 | 23 | Back | 640 | 1 | 639 | 640 |
| Desert | 15 | 23 | Back | 98 | desert-new | 33 | 6181 |
| Desert | 17 | 26 | Back | 239 | desert-new | 174 | 6322 |
|  |  |  | Buildings | 239 | desert-new | 174 | 6322 |
| BusStop | 19 | 23 | Back | 611 | outdoors | 610 | 611 |
|  |  |  | AlwaysFront | 5469 | zspring_SVE_Tilesheet2 | 5 | 5469 |
| BusStop | 27 | 23 | Back | 207 | outdoors | 206 | 207 |
| BusStop | 20 | 23 | Back | 227 | outdoors | 226 | 227 |
| BusStop | 26 | 22 | Back | 202 | outdoors | 201 | 202 |
| BusStop | 5 | 9 | Back | 1055 | outdoors | 1054 | 1055 |
|  |  |  | Buildings | 103 | outdoors | 102 | 103 |
| Beach | 39 | 23 | Back | 107 | untitled tile sheet | 42 | 107 |
| ArchaeologyHouse | 18 | 9 | Back | 605 | untitled tile sheet | 540 | 605 |
|  |  |  | Front | 690 | untitled tile sheet | 625 | 690 |
| ArchaeologyHouse | 3 | 15 | Back | 1062 | untitled tile sheet | 997 | 1062 |
|  |  |  | Front | 230 | untitled tile sheet | 165 | 230 |

## Ограничения

- `townInterior` / `townInterior_2` в SVE Hospital TMX **без embedded tile properties** — мебель видна только как Buildings GID + object Action.
- Saloon — vanilla TMX; SVE `.tbin` может отличаться.
- Seasonal sheets (`spring_*`, `fall_*`) в runtime подменяются сезоном — firstgid сохраняется, текстуры меняются.
- Для полного каталога стульев/кровать **needs visual check** на `Content/Maps/townInterior.xnb` + Tiled.
