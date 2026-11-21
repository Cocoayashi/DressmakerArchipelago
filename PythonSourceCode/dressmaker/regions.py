from __future__ import annotations

from typing import TYPE_CHECKING

from BaseClasses import LocationProgressType, Region

from rule_builder.rules import Has

from .locations import DRESSMAKER_DATA

if TYPE_CHECKING:
    from .world import DressmakerWorld


def create_and_connect_regions(world: DressmakerWorld) -> None:
    previous_region = Region("workshop", world.player, world.multiworld)
    world.multiworld.regions += [previous_region]
    previous_name = "workshop"
    for quests in DRESSMAKER_DATA['storyQuestInfos']:
        if quests == "Complete Final Commission":
            quest_name = "FinalQuest"
        else:
            quest_name = quests.split("/")[-1]
        region_name = "Region: " + quests['name']
        quest_region = Region(region_name, world.player, world.multiworld)
        previous_region = world.get_region(previous_name)
        world.multiworld.regions += [quest_region]
        previous_region.connect(quest_region, previous_name+" to "+ region_name, Has(quest_name))
        previous_name = "Region: " + quests['name']
    region_name = "Region: "+DRESSMAKER_DATA['finalQuestInfo']['name']
    final_region = Region(region_name, world.player, world.multiworld)
    world.multiworld.regions += [final_region]
    previous_region = world.get_region(previous_name)
    previous_region.connect(final_region, previous_name+" to "+region_name)

