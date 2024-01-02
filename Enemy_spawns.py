"""Graphs the enemy spawn rates against time"""

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import math
import timeit
from icecream import ic
from scipy.interpolate import CubicHermiteSpline as spline
from scipy.interpolate import BPoly


class Keyframe:
    """Class to store keyframe data for Unity animation curves"""

    def __init__(self, time, value, in_tangent, out_tangent,
                 weighted_mode, in_weight, out_weight):
        self.time = time
        self.value = value
        self.in_tangent = in_tangent
        self.out_tangent = out_tangent
        self.weighted_mode = weighted_mode
        self.in_weight = in_weight
        self.out_weight = out_weight


def plotter(spawn_spline):
    """Plots the spawn_spline"""
    times_to_plot = np.linspace(0, 1, 1000)
    triple_hours = np.linspace(0, 1, 7)
    day_colours = ["red", "purple", "blue", "green"]
    spawn_rate_values = spawn_spline(times_to_plot)
    keyframe_values = spawn_spline(spawn_spline.x)
    fig, ax = plt.subplots()
    ax.set_facecolor("lightgray")
    ax.plot(spawn_spline.x, keyframe_values, "o")
    ax.plot(times_to_plot, spawn_rate_values, "cornflowerblue")
    upper_bounds = spawn_upper_bounds(spawn_spline(triple_hours), 4)
    for day_to_deadline in range(3, -1, -1):
        lower_bounds = spawn_lower_bounds(
            spawn_spline(triple_hours), 4, day_to_deadline, 0)
        ax.errorbar(triple_hours, spawn_spline(triple_hours),
                    yerr=[lower_bounds, upper_bounds],
                    ecolor=day_colours[day_to_deadline], fmt="none",
                    capsize=(day_to_deadline + 0.5) * 5, capthick=1.5,
                    elinewidth=1.5,
                    label=f"{day_to_deadline} days to the deadline")
    ax.set(xticks=triple_hours, xlabel="Time", ylabel="Spawn Rate",
           xticklabels=["6am", "9am", "12pm", "3pm", "6pm", "9pm", "12am"])
    plt.grid(True)
    plt.legend()
    plt.show()


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


def bezier_curve(keyframes):
    """Takes as inputs the keyframes with times values derivatives, weights
    and weighted_modes provided in unity and returns a Bézier curve"""
    bezier_coefficient1_list = []
    bezier_coefficient2_list = []
    bezier_coefficient3_list = []
    bezier_coefficient4_list = []
    breakpoints = [keyframe.time for keyframe in keyframes]
    cubic_beziers = [list(cubic_beziers) for cubic_beziers in
                     zip(keyframes[:-1], keyframes[1:])]
    for keyframe_pair in cubic_beziers:
        for keyframe in keyframe_pair:
            if keyframe.weighted_mode == 0:
                keyframe.in_weight = 1 / 3
                keyframe.out_weight = 1 / 3
            elif keyframe.weighted_mode == 1:
                keyframe.in_weight = 1 / 3
            elif keyframe.weighted_mode == 2:
                keyframe.out_weight = 1 / 3
            elif keyframe.weighted_mode != 3:
                return ValueError("Invalid weighted mode")
        bezier_coefficient1_list.append(keyframe_pair[0].value)
        bezier_coefficient2_list.append(keyframe_pair[0].value +
            keyframe_pair[0].out_tangent * keyframe_pair[0].out_weight)
        bezier_coefficient3_list.append(keyframe_pair[1].value +
            keyframe_pair[1].in_tangent * keyframe_pair[1].in_weight)
        bezier_coefficient4_list.append(keyframe_pair[1].value)
    bezier_coefficients_list = np.vstack((bezier_coefficient1_list,
                                          bezier_coefficient2_list,
                                          bezier_coefficient3_list,
                                          bezier_coefficient4_list))
    ic(bezier_coefficients_list)
    return BPoly(bezier_coefficients_list, breakpoints)


def main():
    """Main function, exp."""
    # times = [0, 0.3851, 0.6767, 0.9998]
    # values = [2.2707, -0.0064, -7.0217, -14.8181]
    # derivatives = [7.5001, -2.7671, -27.2869, 0]
    # spawn_spline = spline(times, values, derivatives)
    # plotter(spawn_spline)

    # times = [0, 0.11723505, 0.88036245, 1]
    # values = [-0.50302887, -0.13017726, 0.15344214, 0.5030365]
    # derivatives = [7.455404, 0.5548811, 0.5221589, 7.0514693]

    turret_keyframe1 = Keyframe(0, 2.0207, 0.3546, 0.3546, 0, 0, 0.9235)
    turret_keyframe2 = Keyframe(0.6991, 2.9655, 7.0642, 7.0642, 3, 0.2517,
                                0.6898)
    turret_keyframe3 = Keyframe(1.0002, 6.5672, 57.2522, 57.2522, 3, 0.1665, 0)
    turret_keyframes = [turret_keyframe1, turret_keyframe2, turret_keyframe3]
    turret_spline = bezier_curve(turret_keyframes)
    plt.plot(np.linspace(0, 1, 1000), turret_spline(np.linspace(0, 1, 1000)))
    plt.plot(turret_spline.x, turret_spline(turret_spline.x), "o")
    plt.show()


if __name__ == "__main__":
    main()
