import timeit

import numpy as np
cimport numpy as cnp
from libc.stdlib cimport malloc, free
# noinspection PyUnresolvedReferences
from cython.parallel import prange

cnp.import_array()

# noinspection PyTypeChecker
cpdef calculate_distributions(
        double[::1] base_dist_array,
        unsigned char quota_num,
        long long[::1] previous_quotas,
        long long[::1] cumulative_quotas,
        double[::1] previous_probabilities,longest=None):
    """Calculates the distributions for a given quota number.
    Quota info is stored in a 2D array"""
    # pre = timeit.default_timer()
    # print(f"Overhead-np {pre}")

    cdef double current_multiple = (1 + (quota_num * quota_num) / 16)

    cdef long * current_quota_shape = <long *> malloc(10000000 * sizeof(long))
    if not current_quota_shape:
        raise MemoryError()
    cdef int curr_quota_idx
    for curr_quota_idx in prange(10000000, nogil=True):
        current_quota_shape[curr_quota_idx] = <long> (
                current_multiple * base_dist_array[curr_quota_idx])

    cdef int prev_quota_len = previous_quotas.shape[0]
    cdef int cumulative_len = cumulative_quotas.shape[0]


    cdef int largest_quota_idx = (previous_quotas[prev_quota_len - 1] +
                        current_quota_shape[10000000 - 1] + 1)
    cdef int largest_sum_idx = (cumulative_quotas[cumulative_len - 1] +
                        current_quota_shape[10000000 - 1] + 1)
    cdef int quota_arr_size = largest_quota_idx * largest_sum_idx

    cdef double * counts = <double *> malloc(quota_arr_size * sizeof(double))
    if not counts:
        raise MemoryError()
    cdef int counts_idx
    for counts_idx in prange(quota_arr_size, nogil=True):
        counts[counts_idx] = 0

    cdef double quota_counts_sum = 0
    cdef long long * quota_count = <long long *>malloc(
        quota_arr_size * sizeof(long long))
    if not quota_count:
        raise MemoryError()

    cdef int prev_paired_idx
    cdef long long prev_quota
    cdef int counts_quota_idx
    cdef double val_count

    # loop = timeit.default_timer()
    # print(f"Overhead-top {loop-pre}")
    for prev_paired_idx in prange(prev_quota_len, nogil=True):
        prev_quota = previous_quotas[prev_paired_idx]
        prev_sum = cumulative_quotas[prev_paired_idx]

        for counts_idx in prange(quota_arr_size):
            quota_count[counts_idx] = 0

        for curr_quota_idx in prange(10000000):
            quota_count[(prev_quota + current_quota_shape[curr_quota_idx]) *
                        largest_sum_idx +
                        prev_sum + current_quota_shape[curr_quota_idx]] += 1
        # combine loops
        for counts_quota_idx in prange(quota_arr_size):
            val_count = (quota_count[counts_quota_idx] *
                                       previous_probabilities[prev_paired_idx])
            counts[counts_quota_idx] += val_count
            quota_counts_sum += val_count

    # pre2 = timeit.default_timer()
    # print(f"Loop {pre2-loop}")

    free(current_quota_shape)
    free(quota_count)

    ?????????????????
    cdef cnp.ndarray[cnp.float64_t, ] quota_probs = np.empty(
        (quota_arr_size,), dtype=float)
    for counts_idx in range(quota_arr_size):
        quota_probs[counts_idx] = counts[counts_idx] / quota_counts_sum
    free(counts)
    cdef cnp.ndarray[long long, ] quota_values = np.nonzero(quota_probs)[0]
    cdef cnp.ndarray[long long, ] quota_sums = np.nonzero(quota_probs)[1]
    return quota_values, quota_probs[quota_probs != 0]
