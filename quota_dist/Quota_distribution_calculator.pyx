#!python
#cython: language_level=3

"""Seperated out the quota distribution calculations to speed up compilation"""

import numpy as np
import pandas as pd


def calculate_distributions(
        base_dist_array, int quota_num, previous_quotas,
        previous_probabilities):
    """Calculates the distributions for a given quota number"""
    previous_quotas = np.atleast_1d(previous_quotas)
    previous_probabilities = np.atleast_1d(previous_probabilities)
    quota_counts_split = pd.DataFrame(dtype=int)
    # Floors the quota array
    current_quota_shape = np.array(
        base_dist_array * (1 + (quota_num ** 2) / 16), dtype=int)
    cdef int prev_quota_index, prev_quota_val
    for prev_quota_index, prev_quota_val in enumerate(previous_quotas):
        part_quota_values = current_quota_shape + prev_quota_val
        part_quota_counts = pd.DataFrame(
            np.bincount(part_quota_values), dtype=int)
        quota_counts_split = pd.concat(
            [quota_counts_split, part_quota_counts], axis=1)
    quota_counts_split.fillna(value=0, inplace=True)
    quota_counts_split_array = np.array(
        quota_counts_split, dtype=float)
    quota_counts_split_array *= previous_probabilities
    quota_counts = np.sum(quota_counts_split_array, axis=1)
    quota_probabilities = quota_counts / quota_counts.sum()
    quota_values = np.nonzero(quota_probabilities)[0]
    non_zero_probabilities = quota_probabilities[quota_values]
    return quota_values, non_zero_probabilities
