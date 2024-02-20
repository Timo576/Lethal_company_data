"""Graphs the enemy spawn rates against time"""
import numpy as np
import matplotlib.pyplot as plt
from icecream import ic
from Parametric import UnityX, UnityY, get_keyframe_data


def spawn_bounds(spawn_value, prob_range, days2dead, min_spawn):
    """Calculates the spawn range for a given day"""
    potential_lower_bound = np.floor(
        spawn_value + abs(days2dead - 3) / 1.6 - prob_range)
    lower_bound = np.where(
        potential_lower_bound < min_spawn, min_spawn, potential_lower_bound)
    potential_upper_bound = np.floor(spawn_value + prob_range)
    upper_bound = np.where(
        potential_upper_bound > 20, 20, potential_upper_bound)
    return lower_bound, upper_bound


def main():
    """Try to make the curves"""
    x_vals, y_vals, dydx_in, dydx_out, wins, wouts, c_dat = get_keyframe_data()
    x_spline = UnityX(x_vals, wins, wouts)
    y_spline = UnityY(x_vals, y_vals, dydx_in, dydx_out, wins, wouts, c_dat)
    times = np.linspace(np.min(x_vals), np.max(x_vals), 1000)
    deviation, min_spawn = 4, 0
    max_power = 4
    first_spawn = 1 / 18
    # color_list = ["#440154", "#003F5C", "#21918C", "#FDE725"]
    # alpha = 0.7
    color_list = ["#0000FF", "#80FF80", "#FFFF00", "#FF0000"]
    alpha = 0.5
    fig, ax = plt.subplots()
    # ax.vlines(x=first_spawn, ymin=0, ymax=20,
    #           color="grey", linestyle="--", label="First spawn")
    ax.hlines(y=max_power, xmin=0, xmax=1,
              color="red", linestyle="--", label="Max power")
    for day in [3, 2, 1, 0]:
        spawn_min, spawn_max = spawn_bounds(
            y_spline(times), deviation, day, min_spawn)
        ax.fill_between(x_spline(times), spawn_min, spawn_max,
                        alpha=alpha, label=f"Days to deadline {day}",
                        color=color_list[day])
    ax.grid(True)
    ax.legend(loc=2)
    ax.set(xlabel="Time of day", ylabel="Spawn attempts",
           title="Experimentation indoor spawns", xticks=np.linspace(0, 1, 7),
           xticklabels=["6am", "9am", "12pm", "3pm", "6pm", "9pm", "12am"])
    plt.show()


if __name__ == "__main__":
    main()
