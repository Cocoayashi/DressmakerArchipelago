from __future__ import annotations

from typing import TYPE_CHECKING

from rule_builder.rules import Has, HasAll, Rule, HasFromList

from .items import PATTERN_ITEMS, FABRIC_ITEMS

from . import options

if TYPE_CHECKING:
    from .world import DressmakerWorld

def set_all_rules(world: DressmakerWorld) -> None:

    # prevents a location from unlocking itself. The most important logic, honestly
    set_location_rules(world)
    # how player can win.
    set_completion_condition(world)

def set_location_rules(world: DressmakerWorld) -> None:
    for fabric_item_name in FABRIC_ITEMS:
        fabric_location_name = fabric_item_name.replace("Fabric: ", "Use fabric: ")
        world.set_rule(world.get_location(fabric_location_name), Has(fabric_item_name))
    for pattern_item_name in PATTERN_ITEMS:
        pattern_location_name = pattern_item_name.replace("Pattern: ", "Use pattern: ")
        world.set_rule(world.get_location(pattern_location_name), Has(pattern_item_name))

def set_completion_condition(world: DressmakerWorld) -> None:
    goal_location = world.get_location("Dressmaker goal reached")
    if world.options.goal_choice == options.GoalChoice.option_pattern:
        patterns_needed = (int)(len(PATTERN_ITEMS.values()) * (world.options.goal_percentage/100))
        world.set_rule(goal_location, HasFromList(*PATTERN_ITEMS, count=patterns_needed))
    if world.options.goal_choice == options.GoalChoice.option_fabric:
        fabrics_needed = (int)(len(FABRIC_ITEMS.values()) * (world.options.goal_percentage/100))
        world.set_rule(goal_location, HasFromList(*FABRIC_ITEMS, count=fabrics_needed))
    world.set_completion_rule(Has("Victory"))
