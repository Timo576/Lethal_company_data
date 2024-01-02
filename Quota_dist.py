"""Python for lethal company science"""

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import gc
import timeit
from icecream import ic
from scipy.interpolate import CubicHermiteSpline as spline


def save_sample_curve() -> None:
    """Save an array for quota 2"""
    sample_length: int = 10000000
    times: list[float] = [0, 0.11723505, 0.88036245, 1]
    values: list[float] = [-0.50302887, -0.13017726, 0.15344214, 0.5030365]
    derivatives: list[float] = [7.455404, 0.5548811, 0.5221589, 7.0514693]
    r_spline: spline = spline(times, values, derivatives)
    # noinspection PyTypeChecker
    r_values: np.ndarray[float] = r_spline(np.linspace(0, 1, sample_length))
    base_dist_array: np.ndarray[float] = 100 * (1 + r_values)
    np.save(f"quota_test_monte", base_dist_array)


def calculate_distributions(
        base_dist_array: np.ndarray[float],
        quota_num: int, previous_quota: int) -> pd.Series:
    """Calculates the distributions for a given quota number"""
    # noinspection PyTypeChecker
    # Floors the quota array
    quota_array: np.ndarray[int] = np.array(
        base_dist_array * (1 + (quota_num ** 2) / 16), dtype=int)
    true_quota_series: pd.Series = pd.Series(quota_array + previous_quota)
    quota_dist: pd.Series = true_quota_series.value_counts(
        sort=False, normalize=True)
    return quota_dist


def calculate_many_quota(
        base_dist_array: np.ndarray[float],
        quota_record_list: list[np.ndarray], upper_quota: int) -> None:
    """Calculates and save the distributions for multiple quotas"""
    quota_num: int
    print("1")
    for quota_num in range(2, upper_quota):
        # noinspection PyTypeChecker
        # Floors the quota array
        quota_array: np.ndarray[int] = np.array(
            base_dist_array * (1 + (quota_num ** 2) / 16), dtype=int)
        #
        print("2")
        last_quota_data: np.ndarray = quota_record_list[-1]
        # noinspection PyTypeChecker
        prev_quota_array: np.ndarray[int] = last_quota_data[:, 1]
        print("3")
        # noinspection PyTypeChecker
        prev_prob_array: np.ndarray[float] = last_quota_data[:, 0]
        print("4")
        current_quota_df: pd.DataFrame[int] = pd.DataFrame(
            np.tile(quota_array, (len(prev_quota_array), 1)), dtype=int)
        print("5")
        true_quota_df: pd.DataFrame[int] = current_quota_df.add(
            prev_quota_array, axis=0)
        print("6")
        gc.collect()
        quota_counts = np.bincount(true_quota_df)
        quota_values = np.nonzero(quota_counts)[0]
        quota_dist = np.vstack((
            quota_values, quota_counts[quota_values])).T
        quota_dist[:, 0] *= (
                prev_prob_array / quota_dist[:, 0].sum())
        print("8")
        #
        # for prev_quota, prev_prob in quota_record_list[-1].items():
        #     true_quota_series = pd.Series(quota_array + prev_quota)
        #     quota_dist = true_quota_series.value_counts(sort=False,
        #                                                 normalize=True)
        #     part_quota_dist = quota_dist * prev_prob
        #     part_quota_list.append(part_quota_dist)
        # quota_dist = pd.DataFrame(part_quota_list).sum()
        quota_record_list.append(quota_dist)
        print(f"Quota {quota_num} done")
    pd.to_pickle(quota_record_list, "quota_record_list2.pkl")


# noinspection PyTypeChecker
def find_averages(quota_record_list: list[pd.Series]) -> None:
    """Finds the mean, median and mode for a quotas distribution"""
    quota_num: int
    quota_dist: pd.Series
    for quota_num, quota_dist in enumerate(quota_record_list):
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
        plt.savefig(f"quota_{quota_num + 1}_dist2.pdf")
        plt.clf()


def main():
    """Doobee do. Quota num is the num completed plus 1"""
    # save_sample_curve()
    base_dist_array: np.ndarray[float] = np.load("quota_test_monte.npy")
    quota_record_list: list[pd.Series] = [
        calculate_distributions(base_dist_array, 1, 130)]
    calculate_many_quota(base_dist_array, quota_record_list, 3)
    quota_record_list: list[pd.Series] = pd.read_pickle(
        "quota_record_list2.pkl")
    quota_record_list.insert(0, pd.Series({130: 1}))
    plot_bar_dist(quota_record_list)
    find_averages(quota_record_list)


if __name__ == '__main__':
    main()
