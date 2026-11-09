from dataclasses import dataclass

from Options import Choice, OptionGroup, PerGameCommonOptions, Range, Toggle


class GoalChoice(Choice):
    """
    Goal choices in order of how hard and long they take:
    Pattern percentage: use a certain % of patterns in dresses to win. There are 157 patterns.
    Fabric percentage: use a certain % of fabrics in dresses to win. There are 363 fabrics.
    Story: Complete the final 54th commission.
    Complete EVERY commission: There's 78 of these bad boys, choose this one if you
    REALLY want another 35+ hour playthrough. This one adds 24 pigeon commissions.
    """

    display_name = "Win Condition"
    option_pattern = 0
    option_fabric = 1
    option_main_story = 2
    option_all_commissions = 3

    default = option_pattern


class GoalPercentage(Range):
    """
    Percentage of patterns or fabrics needed to be used in order to win.
    (if either patterns or fabrics were your goal)
    """

    display_name = "Pattern or Fabric % Completion Requirement"

    range_start = 1
    range_end = 100
    default = 75

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
    How many collar patterns you want to start with.
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

    range_start = 24
    range_end = 100
    default = 30

@dataclass
class DressmakerOptions(PerGameCommonOptions):
    goal_choice: GoalChoice
    goal_percentage: GoalPercentage
    starting_bodices: StartingBodices
    starting_skirts: StartingSkirts
    starting_sleeves: StartingSleeves
    starting_collars: StartingCollars
    starting_fabrics: StartingFabrics


option_groups = [
    OptionGroup(
        "Goal Options",
        [GoalChoice, GoalPercentage],
    ),
    OptionGroup(
        "Starting Options",
        [StartingBodices, StartingSkirts, StartingSleeves, StartingCollars, StartingFabrics],
    ),
]

