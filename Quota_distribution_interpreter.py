"""Python for lethal company science"""


import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import typing as tp
import gc
import timeit
from icecream import ic
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
        upper_quota: int) -> None:
    """Calculates and save the distributions for multiple quotas"""
    quota_num: int
    quota_distributions: list[tuple[int_array, float_array]] = [
        calculate_distributions(base_dist_array, 1, 130, 1.0)]
    print("Quota 1 done")
    for quota_num in range(2, upper_quota + 1):
        prev_quotas: int_array
        prev_probabilities: float_array
        prev_quotas, prev_probabilities = quota_distributions[-1]
        quota_distributions.append(calculate_distributions(
            base_dist_array, quota_num, prev_quotas, prev_probabilities))
        print(f"Quota {quota_num} done")
    pd.to_pickle(quota_distributions, "saved_arrays/quota_record_list2.pkl")


def find_averages(quota_record_list: list[pd.Series]) -> None:
    """Finds the mean, median and mode for a quotas distribution"""
    quota_num: int
    quota_dist: pd.Series
    for quota_num, quota_dist in enumerate(quota_record_list):
        # noinspection PyTypeChecker
        mode_quota: int = quota_dist.idxmax()
        cumulative_sum: pd.Series = quota_dist.cumsum()
        # noinspection PyUnresolvedReferences
        median_quota: int = (cumulative_sum >= 0.5).idxmax()
        mean_quota: float = quota_dist.dot(quota_dist.index)
        print(f"Quota {quota_num + 1} mean: {mean_quota},"
              f"median: {median_quota:.0f}, mode: {mode_quota:.0f}")


def plot_bar_dist(quota_record_list: list[pd.Series]) -> None:
    """Plots the distribution of a quota"""
    quota_num: int
    quota_dist: pd.Series
    for quota_num, quota_dist in enumerate(quota_record_list):
        plt.bar(quota_dist.index, quota_dist)
        plt.title(f"Quota {quota_num + 1} Distribution")
        plt.xlabel("Quota")
        plt.ylabel("Probability")
        plt.savefig(f"graphs/quota_{quota_num + 1}_dist2.pdf")
        plt.clf()


def many_plots_and_averages():
    """Doobee do. Quota num is the num completed plus 1"""
    save_sample_curve()
    base_dist_array: float_array = np.load("saved_arrays/quota_test_monte.npy")
    save_multiple_distributions(base_dist_array, 37)
    quota_record_list_series: list[pd.Series] = pd.read_pickle(
        "saved_arrays/quota_record_list2.pkl")
    quota_record_list_series.insert(0, pd.Series({130: 1}))
    plot_bar_dist(quota_record_list_series)
    find_averages(quota_record_list_series)


if __name__ == '__main__':
    many_plots_and_averages()
