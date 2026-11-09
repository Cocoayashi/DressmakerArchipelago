from __future__ import annotations

from typing import TYPE_CHECKING

from BaseClasses import LocationProgressType, Location

import json

import pkgutil

DRESSMAKER_DATA = json.loads(pkgutil.get_data(__name__, "data/dressmaker_dump.json"))

from . import items

if TYPE_CHECKING:
    from .world import DressmakerWorld

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

PIGEON_QUEST_UNLOCKED_BY = {
    "RoseMouse": "PrudenceModest",
    "RoseTombRaider": "PrudenceModest",
    "PrudenceHaughtyAux": "PriyaArtShow",
    "PrudenceSickNieceAux": "PriyaArtRetreat",
    "PriyaBrotherlyWoes": "MarieSpringFete",
    "PriyaNewAesthetic": "MarieGoat",
    "PriyaUnusualCommission": "PrudenceSecretGalleryVisit",
    "MariePoeticMuse": "PrudenceDukeWedding",
    "MarieVistor": "PrudenceModest",
    "EdithRomanceReader": "MarieGoat",
    "EdithGraveVisit": "PrudenceSpringFete",
    "EdithPigeonCat": "PriyaDukeWedding",
    "FionaFakeInterview": "FionaMarketFair",
    "FionaFlowerSale": "FionaHedgeMaze",
    "FionaBaking": "FionaPetShow",
    "AliceSelafie": "MarieGoat",
    "AliceKroq": "MarieSpringFete",
    "AliceFamily": "VivianCoronation",
    "VivianPet": "AliceHedgeMaze",
    "VivianStain": "AliceSeance",
    "DorothyAliens": "PrudenceSpringFete",
    "DorothyDetective": "VivianSeance",
    "AdoraTranslate": "FionaPetShow",
    "AdoraEtiquette": "FionaMarketFair",
}

_story_names = {quest["name"] for quest in DRESSMAKER_DATA["storyQuestInfos"]}
_pigeon_names = {quest["name"] for quest in DRESSMAKER_DATA["sideQuestInfos"]}
assert set(PIGEON_QUEST_UNLOCKED_BY) == _pigeon_names, \
    f"PIGEON_QUEST_UNLOCKED_BY doesn't match the dump's pigeon quests: {set(PIGEON_QUEST_UNLOCKED_BY) ^ _pigeon_names}"
assert set(PIGEON_QUEST_UNLOCKED_BY.values()) <= _story_names, \
    f"Unknown story quests in PIGEON_QUEST_UNLOCKED_BY: {set(PIGEON_QUEST_UNLOCKED_BY.values()) - _story_names}"

PATTERN_LOCATIONS = {}
next_id = 3000
for pattern in DRESSMAKER_DATA["patternInfos"]:
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

LOCATION_NAME_TO_ID = (  STORY_QUEST_LOCATIONS
                        | SIDE_QUEST_LOCATIONS
                        | PATTERN_LOCATIONS
                        | REPUTATION_LOCATIONS
                        | CHARACTER_FRIENDSHIP_LOCATIONS
                        | FABRIC_LOCATIONS )
NO_STORY_LOCATION_NAME_TO_ID = ( PATTERN_LOCATIONS
                        | CHARACTER_FRIENDSHIP_LOCATIONS
                        | FABRIC_LOCATIONS )

class DressmakerLocation(Location):
    game = "Dressmaker"


def get_location_names_with_ids(location_names: list[str]) -> dict[str, int | None]:
    return {location_name: LOCATION_NAME_TO_ID[location_name] for location_name in location_names}


def create_all_locations(world: DressmakerWorld) -> None:
    create_regular_locations(world)
    create_events(world)


def create_regular_locations(world: DressmakerWorld) -> None:
    workshop = world.get_region("workshop")
    count = 0
    for quest_location, quest_id in STORY_QUEST_LOCATIONS.items():
        if quest_location == "Complete Final Commission":
            quest_name = "FinalQuest"
        else:
            quest_name = quest_location.split("/")[-1]
        region = world.get_region("Region: " + quest_name)
        region_location = get_location_names_with_ids([quest_location])
        region.add_locations(region_location, DressmakerLocation)
        location = world.get_location(quest_location)
        count+=1
        if count < 10:
            location.progress_type = LocationProgressType.PRIORITY
        if count > 30:
            location.progress_type = LocationProgressType.EXCLUDED


    create_pigeon_quest_locations(world)
    region = world.get_region("workshop")

    for reputation_name, quest_id in REPUTATION_LOCATIONS.items():
        region_location = get_location_names_with_ids([reputation_name])
        region.add_locations(region_location, DressmakerLocation)
        location = world.get_location(reputation_name)
        if ' 4' in reputation_name or ' 5' in reputation_name:
            location.progress_type = LocationProgressType.EXCLUDED
    workshop_locations = get_location_names_with_ids(NO_STORY_LOCATION_NAME_TO_ID
    )
    workshop.add_locations(workshop_locations, DressmakerLocation)


def create_pigeon_quest_locations(world: DressmakerWorld) -> None:
     for pigeon_quest in DRESSMAKER_DATA["sideQuestInfos"]:
        location_name = f"Complete Pigeon Quest: {pigeon_quest['prettyName']}/{pigeon_quest['name']}"
        story_region = world.get_region("Region: " + PIGEON_QUEST_UNLOCKED_BY[pigeon_quest["name"]])
        story_region.add_locations({location_name: SIDE_QUEST_LOCATIONS[location_name]}, DressmakerLocation)

def create_events(world: DressmakerWorld) -> None:
    workshop = world.get_region("workshop")

    workshop.add_event(
        "Dressmaker goal reached", "Victory", location_type=DressmakerLocation, item_type=items.DressmakerItem
    )

