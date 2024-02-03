"""Graphs the enemy spawn rates against time"""
import numpy as np
import matplotlib.pyplot as plt


def spawn_lower_bounds(spawn_value, prob_range, days2dead, min_spawn):
    """Calculates the spawn range for a given day"""
    potential_lower_bound = np.floor(
        spawn_value + abs(days2dead - 3) / 1.6 - prob_range)
    lower_bound = np.where(
        potential_lower_bound < min_spawn, min_spawn, potential_lower_bound)
    lower_error = spawn_value - lower_bound
    return lower_error


def spawn_upper_bounds(spawn_value, prob_range):
    """Calculates the spawn range for a given day"""
    potential_upper_bound = np.floor(spawn_value + prob_range)
    upper_bound = np.where(
        potential_upper_bound > 20, 20, potential_upper_bound)
    upper_error = upper_bound - spawn_value
    return upper_error


def main():
    """Try to make the curves"""


if __name__ == "__main__":
    main()
