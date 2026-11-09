from __future__ import annotations

from typing import TYPE_CHECKING

from BaseClasses import Item, ItemClassification

from .locations import DRESSMAKER_DATA

if TYPE_CHECKING:
    from .world import DressmakerWorld


class DressmakerItem(Item):
    game = "Dressmaker"

FILLER_ITEMS = {"Gold": 50}

PATTERN_ITEMS = {}
DEFAULT_PATTERNS = {}
PATTERNS_BY_TYPE = {"Bodice": [],"Skirt": [],  "Sleeve": [], "Collar": []}
next_id = 1000
for pattern in DRESSMAKER_DATA["patternInfos"]:
    if pattern['name'] in ['No Collar', 'No Sleeves']:
        DEFAULT_PATTERNS[f"Pattern: {pattern['prettyName']}/{pattern['name']}"] = next_id
        next_id += 1
        continue
    PATTERN_ITEMS[f"Pattern: {pattern['prettyName']}/{pattern['name']}"] = next_id
    PATTERNS_BY_TYPE[pattern['PatternType']].append(f"Pattern: {pattern['prettyName']}/{pattern['name']}")
    next_id += 1

FABRIC_ITEMS = {}
FABRIC_BLACK_ITEMS = {}
next_id = 2000
for fabric in DRESSMAKER_DATA['fabricInfos']:
    if fabric['isBlack']:
        FABRIC_BLACK_ITEMS[f"Fabric: {fabric['prettyName']}/{fabric['name']}"] = next_id
    else:
        FABRIC_ITEMS[f"Fabric: {fabric['prettyName']}/{fabric['name']}"] = next_id
    next_id += 1

ITEM_NAME_TO_ID = (
    FILLER_ITEMS
    | FABRIC_ITEMS
    | PATTERN_ITEMS
    | FABRIC_BLACK_ITEMS
    | DEFAULT_PATTERNS
)

def get_random_filler_item_name(world: DressmakerWorld) -> str:
    return "Gold"


def create_item_with_correct_classification(world: DressmakerWorld, name: str) -> DressmakerItem:
    if name in FILLER_ITEMS:
        classification = ItemClassification.filler
    else:
        classification = ItemClassification.progression

    return DressmakerItem(name, classification, ITEM_NAME_TO_ID[name], world.player)


def create_all_items(world: DressmakerWorld) -> None:

    itempool: list[Item] = []

    starting_fabrics = world.random.sample(list(FABRIC_ITEMS), world.options.starting_fabrics-4)
    starting_black_fabrics = world.random.sample(list(FABRIC_BLACK_ITEMS), 3)


    for fabric_name in FABRIC_BLACK_ITEMS:
        if fabric_name not in starting_black_fabrics:
            itempool.append(world.create_item(fabric_name))
        else:
            world.push_precollected(world.create_item(fabric_name))

    for fabric_name in FABRIC_ITEMS:
        if fabric_name in starting_fabrics or "Burlap" in fabric_name:
            world.push_precollected(world.create_item(fabric_name))
        else:
            itempool.append(world.create_item(fabric_name))
    starting_pattern_counts = {"Bodice": world.options.starting_bodices.value,
                               "Skirt": world.options.starting_skirts.value,
                               "Sleeve": world.options.starting_sleeves.value,
                               "Collar": world.options.starting_collars.value}
    starting_patterns = []
    for patternType, qty in starting_pattern_counts.items():
        starting_patterns += world.random.sample(PATTERNS_BY_TYPE[patternType], qty)

    for pattern in starting_patterns:
        world.push_precollected(create_item_with_correct_classification(world, pattern))

    for pattern_name in PATTERN_ITEMS:
        if  pattern_name not in starting_patterns:
            itempool.append(world.create_item(pattern_name))

    for pattern_name in DEFAULT_PATTERNS:
        world.push_precollected(create_item_with_correct_classification(world, pattern_name))
    number_of_items = len(itempool)

    number_of_unfilled_locations = len(world.multiworld.get_unfilled_locations(world.player))

    needed_number_of_filler_items = number_of_unfilled_locations - number_of_items

    itempool += [world.create_filler() for _ in range(needed_number_of_filler_items)]

    world.multiworld.itempool += itempool
