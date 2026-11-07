from __future__ import annotations

from typing import TYPE_CHECKING

from BaseClasses import ItemClassification, Location

import json

import pkgutil

DRESSMAKER_DATA = json.loads(pkgutil.get_data(__name__, "data/dressmaker_dump.json"))

from . import items

if TYPE_CHECKING:
    from .world import DressmakerWorld

# Even if a location doesn't exist on specific options, it must be present in this lookup.
STORY_QUEST_LOCATIONS = {}
next_id = 1000
for story_quest in DRESSMAKER_DATA["storyQuestInfos"]:
    STORY_QUEST_LOCATIONS[f"Complete Story Quest: {story_quest['prettyName']}/{story_quest['name']}"] = next_id
    next_id += 1
SIDE_QUEST_LOCATIONS = {}
next_id = 2000
for pigeon_quest in DRESSMAKER_DATA["sideQuestInfos"]:
    SIDE_QUEST_LOCATIONS[f"Complete Pigeon Quest: {pigeon_quest['prettyName']}/{pigeon_quest['name']}"] = next_id
    next_id += 1
PATTERN_LOCATIONS = {}
next_id = 3000
for pattern in DRESSMAKER_DATA["patternInfos"]:
    if pattern['prettyName'] in ["No Collar", "No Sleeves"]:
        continue
    PATTERN_LOCATIONS[f"Use pattern: {pattern['prettyName']}/{pattern['name']}"] = next_id
    next_id += 1
REPUTATION_LOCATIONS = {}
next_id = 4000
for reputation_level in range(1,51):
    REPUTATION_LOCATIONS[f"Reach reputation level: {reputation_level}"] = next_id
    next_id += 1
CHARACTER_FRIENDSHIP_LOCATIONS = {}
next_id = 5000
for character in DRESSMAKER_DATA["characterNames"]:
    for friendship_star in range(1,6):
        CHARACTER_FRIENDSHIP_LOCATIONS[f"Reach friendship: {friendship_star} with {character}"] = next_id
        next_id += 1
FABRIC_LOCATIONS = {}
next_id = 6000
for fabric in DRESSMAKER_DATA["fabricInfos"]:
    FABRIC_LOCATIONS[f"Use fabric: {fabric['prettyName']}/{fabric['name']}"] = next_id
    next_id += 1
next_id = 8000
STORY_QUEST_LOCATIONS["Complete Final Commission"] = next_id

LOCATION_NAME_TO_ID = ( STORY_QUEST_LOCATIONS
                        | SIDE_QUEST_LOCATIONS
                        | PATTERN_LOCATIONS
                        | REPUTATION_LOCATIONS
                        | CHARACTER_FRIENDSHIP_LOCATIONS
                        | FABRIC_LOCATIONS )

# Each Location instance must correctly report the "game" it belongs to.
# To make this simple, it is common practice to subclass the basic Location class and override the "game" field.
class DressmakerLocation(Location):
    game = "Dressmaker"


def get_location_names_with_ids(location_names: list[str]) -> dict[str, int | None]:
    return {location_name: LOCATION_NAME_TO_ID[location_name] for location_name in location_names}


def create_all_locations(world: DressmakerWorld) -> None:
    create_regular_locations(world)
    create_events(world)


def create_regular_locations(world: DressmakerWorld) -> None:
    # Finally, we need to put the Locations ("checks") into their regions.
    # Once again, before we do anything, we can grab our regions we created by using world.get_region()
    workshop = world.get_region("workshop")

    # You also need to pass your overridden Location class.
    workshop_locations = get_location_names_with_ids(LOCATION_NAME_TO_ID
    )
    workshop.add_locations(workshop_locations, DressmakerLocation)


def create_events(world: DressmakerWorld) -> None:
    workshop = world.get_region("workshop")

    # The Victory event is a completely optional abstraction - This will be discussed more in set_rules().
    workshop.add_event(
        "Dressmaker goal reached", "Victory", location_type=DressmakerLocation, item_type=items.DressmakerItem
    )

