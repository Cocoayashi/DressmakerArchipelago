from dataclasses import dataclass

from Options import Choice, OptionGroup, PerGameCommonOptions, Range, Toggle

# In this file, we define the options the player can pick.
# The most common types of options are Toggle, Range and Choice.

# Options will be in the game's template yaml.
# They will be represented by checkboxes, sliders etc. on the game's options page on the website.
# (Note: Options can also be made invisible from either of these places by overriding Option.visibility.
#  APQuest doesn't have an example of this, but this can be used for secret / hidden / advanced options.)

# For further reading on options, you can also read the Options API Document:
# https://github.com/ArchipelagoMW/Archipelago/blob/main/docs/options%20api.md


# The first type of Option we'll discuss is the Toggle.
# A toggle is an option that can either be on or off. This will be represented by a checkbox on the website.
# The default for a toggle is "off".
# If you want a toggle to be on by default, you can use the "DefaultOnToggle" class instead of the "Toggle" class.
class GoalChoice(Choice):
    """
    Goal choices in order of how hard and long they take:
    Pattern percentage: use a certain % of patterns in dresses to win. There are 155 patterns.
    Fabric percentage: use a certain % of fabrics in dresses to win There are 363 fabrics.
    Story: Complete the final 53rd commission, does not include pigeon quests as location. There are 54 commissions.
    Complete EVERY commission: There's 78 of these bad boys, choose this one if you
    REALLY want another 35+ hour playthrough. This one adds 24 pigeon commissions.
    """

    # The docstring of an option is used as the description on the website and in the template yaml.

    # You'll also want to set a display name, which will determine what the option is called on the website.
    display_name = "Win Condition"
    option_pattern = 0
    option_fabric = 1
    option_main_story = 2
    option_all_commissions = 3

    default = option_pattern


class PatternGoalPercentage(Range):
    """
    Percentage of patterns needed to be used in order to win. (if patterns was your goal)
    """

    display_name = "Pattern Percentage Completion Requirement"

    range_start = 0
    range_end = 100
    default = 50

class FabricGoalPercentage(Range):
    """
    Percentage of fabrics needed to be used in order to win. (if fabric was your goal)
    """

    display_name = "Fabric Percentage Completion Requirement"

    range_start = 0
    range_end = 100
    default = 50

class StartingBodices(Range):
    """
    How many bodice patterns you want to start with.
    """

    display_name = "Starting Amount of Bodices"

    range_start = 3
    range_end = 30
    default = 6

class StartingSkirts(Range):
    """
    How many skirt patterns you want to start with.
    """

    display_name = "Starting Amount of Skirts"

    range_start = 3
    range_end = 30
    default = 7

class StartingSleeves(Range):
    """
    How many sleeve patterns you want to start with.
    """

    display_name = "Starting Amount of Sleeves"

    range_start = 3
    range_end = 15
    default = 4

class StartingCollars(Range):
    """
    How many collars you want to start with.
    """

    display_name = "Starting Amount of Collars"

    range_start = 3
    range_end = 10
    default = 3

class StartingFabrics(Range):
    """
    How many fabrics you want to start unlocked with.
    """

    display_name = "Starting Amount of fabrics"

    range_start = 23
    range_end = 100
    default = 23

# We must now define a dataclass inheriting from PerGameCommonOptions that we put all our options in.
# This is in the format "option_name_in_snake_case: OptionClassName".
@dataclass
class DressmakerOptions(PerGameCommonOptions):
    goal_choice: GoalChoice
    fabric_goal_percentage: FabricGoalPercentage
    pattern_goal_percentage: PatternGoalPercentage
    starting_bodices: StartingBodices
    starting_skirts: StartingSkirts
    starting_sleeves: StartingSleeves
    starting_collars: StartingCollars
    starting_fabrics: StartingFabrics


# If we want to group our options by similar type, we can do so as well. This looks nice on the website.
option_groups = [
    OptionGroup(
        "Goal Options",
        [GoalChoice, FabricGoalPercentage, PatternGoalPercentage],
    ),
    OptionGroup(
        "Starting Options",
        [StartingBodices, StartingSkirts, StartingSleeves, StartingCollars, StartingFabrics],
    ),
]

