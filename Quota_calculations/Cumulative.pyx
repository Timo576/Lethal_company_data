import numpy as np
cimport numpy as cnp
from libc.stdlib cimport malloc, free
# noinspection PyUnresolvedReferences
from cython.parallel import prange

cnp.import_array()

# noinspection PyTypeChecker
cpdef calculate_distributions(
        double[::1] base_dist_array,
        double quota_num,
        double[:, ::1] previous_quota_probs,
        long single_offset, long cumulative_offset):
    """Calculates the distributions for a given quota number"""
    cdef long indexer
    cdef double current_multiple = (1 + (quota_num * quota_num) / 16)
    # Set values in current_quota_shape to be the rounded and multiplied
    # values of change in quota
    # Subtract the lowest value and keep it, then count duplicate values
    cdef long current_quota_offset = <long> (
            current_multiple * base_dist_array[0])
    cdef long highest_change = <long> (
            current_multiple * base_dist_array[10000000 - 1])
    cdef long current_array_shape = highest_change - current_quota_offset + 1
    cdef long * current_quota_shape = <long *> malloc(
        current_array_shape * sizeof(long))
    if not current_quota_shape:
        raise MemoryError()
    for indexer in prange(current_array_shape, nogil=True):
        current_quota_shape[indexer] = 0
    for indexer in prange(10000000, nogil=True):
        current_quota_shape[<long> (current_multiple * base_dist_array[
            indexer]) - current_quota_offset] += 1

    cdef long new_s_offset = single_offset + current_quota_offset
    cdef long new_c_offset = cumulative_offset + current_quota_offset


    cdef long prev_quota_len = previous_quota_probs.shape[0]
    cdef long cum_len = previous_quota_probs.shape[1]

    cdef long longest = (previous_quotas[prev_quota_len - 1] +
                        current_array_shape - previous_quotas[0])
    cdef double * counts = <double *> malloc(longest * sizeof(double))
    if not counts:
        raise MemoryError()
    for indexer in prange(longest, nogil=True):
        counts[indexer] = 0
    cdef double quota_counts_sum = 0
    cdef long long * quota_count = <long long *>malloc(
        longest * sizeof(long long))
    if not quota_count:
        raise MemoryError()

    cdef long prev_quota_idx
    cdef long long prev_quota
    cdef double prev_prob
    cdef double val_count

    for prev_quota_idx in prange(prev_quota_len, nogil=True):
        prev_quota = previous_quotas[prev_quota_idx]
        prev_prob = previous_probabilities[prev_quota_idx]
        for indexer in prange(longest):
            quota_count[indexer] = 0
        for indexer in prange(current_array_shape):
            # Adds the number of quotas which will be a certain amount to the
            # array quota_count at the index of that amount
            quota_count[indexer + prev_quota - previous_quotas[0]
                        ] = current_quota_shape[indexer]
        for indexer in prange(longest):
            val_count = (quota_count[indexer] * prev_prob)
            counts[indexer] += val_count
            quota_counts_sum += val_count
    free(current_quota_shape)
    free(quota_count)
    cdef cnp.ndarray[cnp.float64_t, ] quota_probs = np.empty(
        (longest,), dtype=float)
    for indexer in range(longest):
        quota_probs[indexer] = counts[indexer] / quota_counts_sum
    free(counts)
    cdef cnp.ndarray[long long, ]quota_values = np.indices(
        (longest,), dtype=np.longlong)[0]
    quota_values += current_quota_offset + previous_quotas[0]
    return quota_values, quota_probs
