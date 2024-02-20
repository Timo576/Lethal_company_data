"""Calculates the next single quota"""

import numpy as np
import pandas as pd
from scipy.interpolate import CubicHermiteSpline as spline
from Quota_working_calc import calculate_distributions


def main():
    """Main"""
    # base_dist_array = np.load("saved_arrays/quota_test_monte.npy")
    # dist_list = []
    # for i in range(1, 11):
    #     quotas, probs = calculate_distributions(
    #         base_dist_array, i, np.array([0], dtype=np.longlong),
    #         np.array([1.0]))
    #     min_q, max_q = quotas[0], quotas[-1]
    #     cumulative_sum = pd.Series(probs).cumsum()
    #     median_quota = quotas[(cumulative_sum >= 0.5)][0]
    #     lq = quotas[(cumulative_sum >= 0.25)][0]
    #     uq = quotas[(cumulative_sum >= 0.75)][0]
    #     dist_list.append([min_q, lq, median_quota, uq, max_q])
    # for q in dist_list:
    #     print(q)

    times = [0, 0.11723505, 0.88036245, 1]
    values = [-0.50302887, -0.13017726, 0.15344214, 0.5030365]
    derivatives = [7.455404, 0.5548811, 0.5221589, 7.0514693]
    r_spline = spline(times, values, derivatives)
    print(r_spline([0, 0.25, 0.5, 0.75, 1]))


if __name__ == "__main__":
    main()
