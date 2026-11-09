from __future__ import annotations

from typing import TYPE_CHECKING

from rule_builder.rules import Has, HasFromList, CanReachLocation

from . import items

from . import locations

from . import options

if TYPE_CHECKING:
    from .world import DressmakerWorld

def set_all_rules(world: DressmakerWorld) -> None:

    # prevents a location from unlocking itself. The most important logic, honestly
    set_location_rules(world)
    # how player can win.
    set_completion_condition(world)

def set_location_rules(world: DressmakerWorld) -> None:
    for fabric_item_name in items.FABRIC_ITEMS:
        fabric_location_name = fabric_item_name.replace("Fabric: ", "Use fabric: ")
        world.set_rule(world.get_location(fabric_location_name), Has(fabric_item_name))
    for fabric_item_name in items.FABRIC_BLACK_ITEMS:
        fabric_location_name = fabric_item_name.replace("Fabric: ", "Use fabric: ")
        world.set_rule(world.get_location(fabric_location_name), Has(fabric_item_name))
    for pattern_item_name in items.PATTERN_ITEMS:
        pattern_location_name = pattern_item_name.replace("Pattern: ", "Use pattern: ")
        world.set_rule(world.get_location(pattern_location_name), Has(pattern_item_name))



def set_completion_condition(world: DressmakerWorld) -> None:
    goal_location = world.get_location("Dressmaker goal reached")
    if world.options.goal_choice == options.GoalChoice.option_pattern:
        total_patterns =  len(items.PATTERN_ITEMS.values())+ len(items.DEFAULT_PATTERNS)
        patterns_needed = (int)(total_patterns * (world.options.goal_percentage/100))
        world.set_rule(goal_location, HasFromList(*items.PATTERN_ITEMS, *items.DEFAULT_PATTERNS, count=patterns_needed))
    if world.options.goal_choice == options.GoalChoice.option_fabric:
        total_fabrics = len(items.FABRIC_ITEMS) + len(items.FABRIC_BLACK_ITEMS)
        fabrics_needed = (int)(total_fabrics * (world.options.goal_percentage/100))
        world.set_rule(goal_location, HasFromList(*items.FABRIC_ITEMS, *items.FABRIC_BLACK_ITEMS, count=fabrics_needed))
    if world.options.goal_choice == options.GoalChoice.option_main_story:
        world.set_rule(goal_location, CanReachLocation("Complete Final Commission"))
    if world.options.goal_choice == options.GoalChoice.option_all_commissions:
        all_commissions = ( locations.STORY_QUEST_LOCATIONS | locations.SIDE_QUEST_LOCATIONS )
        #Temporary placeholder. As temporary as duct tape haha
        world.set_rule(goal_location, CanReachLocation(all_commissions))
    world.set_completion_rule(Has("Victory"))
