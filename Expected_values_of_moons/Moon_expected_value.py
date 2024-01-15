"""Starting to calculate the average value of scrap on moons"""

import numpy as np
import pandas as pd
from icecream import ic

REND_MAX_GIFT = 138
VOW_MAX_GIFT = 97
MOONS = ["Experimentation", "Assurance", "Vow", "Offense", "March",
         "Rend", "Dine", "Titan"]


def load_scrap_data(moon, version=47):
    """Function to load data from csv files. Loads data for all versions
    and moons then resolves to one dataframe overwriting old item rarities.
    Adds a column for the specific moon which will be used and
    separates the row for giftboxes."""
    moons = MOONS
    scrap_values = pd.read_csv(
        "scrap_values.csv", index_col=0)
    rarities_list = [pd.read_csv("Moon_scrap_rarities_v40.csv",
                                 index_col=0)]
    if version >= 45:
        rarities_list.append(pd.read_csv("Moon_scrap_rarities_v45.csv",
                                         index_col=0, names=moons))
    if version >= 47:
        rarities_list.append(pd.read_csv("Moon_scrap_rarities_v47.csv",
                                         index_col=0, names=moons))
    moon_scrap_rarities = pd.concat(rarities_list, axis=0)
    moon_scrap_rarities = moon_scrap_rarities[
        ~moon_scrap_rarities.index.duplicated(keep='last')]
    moon_scrap_info = pd.merge(scrap_values, moon_scrap_rarities,
                               left_index=True, right_index=True)
    moon_scrap_info["rarity"] = moon_scrap_info[moon]
    transpose = moon_scrap_info.T
    gift_info = transpose.pop("Giftbox")
    moon_scrap_info = transpose.T
    return moon_scrap_info, gift_info


def calculate_average_value(moon_scrap_info, average_scrap_num,
                            gift_info, gift_manip=False, version=47):
    """Function to calculate the average value of scrap on moons"""
    if version != 47:
        print("Version not added")
        return
    moon_scrap_info["average_value"] = (moon_scrap_info["Max value (c)"] +
                                        moon_scrap_info["Min value (c)"]) / 2
    scrap_average_value_no_gift = np.average(moon_scrap_info["average_value"],
                                             weights=moon_scrap_info["rarity"])
    moon_scrap_info.loc["Gift"] = gift_info
    moon_scrap_info.loc["Gift", "Min value (c)"] = np.min(
        moon_scrap_info["Min value (c)"]) + 10
    moon_scrap_info.loc["Gift", "Max value (c)"] = np.max(
        moon_scrap_info["Max value (c)"]) + 14
    if not gift_manip:
        moon_scrap_info.loc[
            "Gift", "average_value"] = scrap_average_value_no_gift + 12
    elif gift_manip == "rend":
        moon_scrap_info.loc["Gift", "average_value"] = REND_MAX_GIFT
    elif gift_manip == "vow":
        moon_scrap_info.loc["Gift", "average_value"] = VOW_MAX_GIFT
    else:
        print("Gift manip not added")
        return
    scrap_average_value = np.average(moon_scrap_info["average_value"],
                                     weights=moon_scrap_info["rarity"])
    moon_average_value = average_scrap_num * scrap_average_value
    return moon_average_value


def calculate_average_weight(moon_scrap_info, average_scrap_num):
    """Function to calculate the average weight of scrap on moons"""
    scrap_average_weight = np.average(moon_scrap_info["Weight (lb)"],
                                      weights=moon_scrap_info["rarity"])
    moon_average_weight = average_scrap_num * scrap_average_weight
    return moon_average_weight


def moon_average_values():
    """Function to calculate the average value of scrap on moons"""
    version = 47
    print("Without accounting for beehives, shotguns or mansions")
    print(f"In version {version}")
    for moon in MOONS:
        # min and max scrap don't include apparatus
        # also Random.Next is exclusive
        min_scrap, max_scrap = pd.read_csv("min_max.csv")[moon]
        average_scrap_num = max_scrap - min_scrap / 2
        moon_scrap_info, gift_info = load_scrap_data(moon)
        moon_average_value = calculate_average_value(
            moon_scrap_info, average_scrap_num, gift_info) + 80
        moon_average_weight = calculate_average_weight(
            moon_scrap_info, average_scrap_num) + 31
        print(f"The average value of scrap on {moon} is: "
              f"{moon_average_value:.2f}")
        print(f"The average weight of scrap on {moon} is: "
              f"{moon_average_weight:.2f}")


if __name__ == '__main__':
    moon_average_values()
    # TODO: beehives and shotguns, two hand, weight ratio,
    #  conduction, sell value version, mansion
