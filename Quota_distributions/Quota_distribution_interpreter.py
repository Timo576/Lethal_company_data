"""Calculate the distributions for Lethal company"""

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.ticker as tick
import typing as tp
import glob
import timeit
from scipy.interpolate import CubicHermiteSpline as spline
# noinspection PyUnresolvedReferences
from Quota_distribution_calculator import calculate_distributions


int_array: tp.TypeAlias = np.ndarray[tp.Any, np.dtype[int]]
float_array: tp.TypeAlias = np.ndarray[tp.Any, np.dtype[float]]


def save_sample_curve() -> None:
    """Save an array for quota 2"""
    sample_length: int = 10000000
    times: list[float] = [0, 0.11723505, 0.88036245, 1]
    values: list[float] = [-0.50302887, -0.13017726, 0.15344214, 0.5030365]
    derivatives: list[float] = [7.455404, 0.5548811, 0.5221589, 7.0514693]
    r_spline: spline = spline(times, values, derivatives)
    r_values: np.ndarray[tp.Any, float]
    r_values = r_spline(np.linspace(0, 1, sample_length))
    base_dist_array: float_array = 100 * (1 + r_values)
    np.save(f"saved_arrays/quota_test_monte", base_dist_array)


def save_multiple_distributions(
        base_dist_array: float_array,
        starting_quota: int, upper_quota: int) -> None:
    """Calculates and save the distributions for multiple quotas"""
    quota_num: int
    quota_distribution: tuple[int_array, float_array]
    quota_distribution = (calculate_distributions(
        base_dist_array, 1, np.array([130], dtype=np.longlong),
        np.array([1.0])))
    pd.to_pickle(quota_distribution, f"saved_arrays/quota_1.pkl")
    print("Quota 1 done")
    for quota_num in range(starting_quota, upper_quota + 1):  # quota_num is +1
        pickled_quota: tuple[int_array, float_array] = pd.read_pickle(
            f"saved_arrays/quota_{quota_num - 1}.pkl")
        prev_quotas: int_array
        prev_probabilities: float_array
        prev_quotas, prev_probabilities = pickled_quota
        quota_distribution = (calculate_distributions(
            base_dist_array, quota_num, prev_quotas, prev_probabilities))
        pd.to_pickle(quota_distribution,
                     f"saved_arrays/quota_{quota_num}.pkl")


def find_stats(
        quota_record_list: list[tuple[int_array, float_array]]) -> None:
    """Finds the mean, median and mode for a quotas distribution"""
    quota_num: int
    quota_dist_info: tuple[int_array, float_array]
    for quota_num, quota_dist_info in enumerate(quota_record_list):
        mode_quota: int = quota_dist_info[0][quota_dist_info[1].argmax()]
        cumulative_sum: pd.Series = pd.Series(quota_dist_info[1]).cumsum()
        median_quota: int = quota_dist_info[0][(cumulative_sum >= 0.5)][0]
        mean_quota: float = np.average(quota_dist_info[0],
                                       weights=quota_dist_info[1])
        min_quota: int = quota_dist_info[0][0]
        max_quota: int = quota_dist_info[0][-1]
        print(f"Quota {quota_num + 1} mean: {mean_quota},"
              f"median: {median_quota:.0f}, mode: {mode_quota:.0f}, "
              f"min: {min_quota}, max: {max_quota}")


def plot_bar_dist(
        quota_record_list: list[tuple[int_array, float_array]]) -> None:
    """Plots the distribution of a quota"""
    quota_num: int
    quota_dist_info: tuple[int_array, float_array]
    for quota_num, quota_dist_info in enumerate(quota_record_list):
        plt.bar(quota_dist_info[0], quota_dist_info[1])
        plt.title(f"Quota {quota_num + 1} Distribution")
        plt.xlabel("Quota")
        plt.ylabel("Probability")
        ax = plt.gca()
        if quota_num == 0:
            ax.xaxis.set_major_locator(tick.MultipleLocator(base=1))
        else:
            ax.xaxis.set_major_locator(tick.MaxNLocator(integer=True))
        plt.savefig(f"graphs/quota_{quota_num + 1}_dist.pdf")
        plt.clf()


def extract_int_from_path(path):
    """Extracts the int from the path quota_*.pkl"""
    try:
        return int(path.split("_")[-1].split(".")[0])
    except ValueError:
        return float('inf')


def process_quota_distributions() -> None:
    """Processes the quota distributions"""
    quota_record_list_series: list[tuple[int_array, float_array]] = []
    for quota_file in sorted(glob.glob("saved_arrays/quota_*.pkl"),
                             key=extract_int_from_path):
        quota_record_list_series.append(pd.read_pickle(quota_file))
        # Unsure if this is right, could just use tuples
    quota_record_list_series.insert(0, (np.array([130]), np.array([1.0])))
    plot_bar_dist(quota_record_list_series)
    find_stats(quota_record_list_series)


def many_plots_and_averages():
    """Doobee do. Quota num is the num completed plus 1"""
    # save_sample_curve()
<<<<<<< HEAD:Quota_distributions/Quota_distribution_interpreter.py
    pre = timeit.default_timer()
    base_dist_array: float_array = np.load(
        "saved_arrays/quota_test_monte.npy")
<<<<<<< Updated upstream:Quota_distribution_interpreter.py
    save_multiple_distributions(base_dist_array, 18, 37)
=======
    save_multiple_distributions(base_dist_array, 10, 10)
    print(f"Time taken: {timeit.default_timer() - pre:.2f} seconds")
=======
    base_dist_array: float_array = np.load(
        "saved_arrays/quota_test_monte.npy")
    save_multiple_distributions(base_dist_array, 18, 37)
>>>>>>> cf49b979d2b6922fb45987bbed938fed21a1a5cb:Quota_distribution_interpreter.py
>>>>>>> Stashed changes:Quota_distributions/Quota_distribution_interpreter.py
    # process_quota_distributions()


if __name__ == '__main__':
    many_plots_and_averages()
