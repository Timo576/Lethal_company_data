import numpy as np
cimport numpy as cnp
import timeit

cnp.import_array()

# noinspection PyTypeChecker
cpdef calculate_distributions(
        cnp.ndarray[cnp.float64_t, ] base_dist_array,
        unsigned char quota_num,
        cnp.ndarray[cnp.int64_t, ] previous_quotas,
        cnp.ndarray[cnp.float64_t, ] previous_probabilities):
    """Calculates the distributions for a given quota number"""
    cdef cnp.ndarray[cnp.int64_t, ] current_quota_shape = np.array(
        base_dist_array * (1 + (quota_num ** 2) / 16), dtype=np.int64)
    cdef int longest = previous_quotas[previous_quotas.shape[0] - 1] + \
                       current_quota_shape[
                           current_quota_shape.shape[0] - 1] + 1
    cdef cnp.ndarray[cnp.float64_t, ndim=2] counts = np.zeros(
        (previous_quotas.shape[0], longest), dtype=np.float64)
    cdef int prev_quota_val
    cdef cnp.ndarray[cnp.int64_t, ] part_quota_values = np.empty(
        (len(base_dist_array),), dtype=np.int64)
    cdef cnp.ndarray[cnp.int64_t, ] quota_count
    cdef int loop_n = 0
    for prev_quota_val in previous_quotas:
        part_quota_values = current_quota_shape + prev_quota_val
        quota_count = np.bincount(part_quota_values)
        counts[loop_n, :len(quota_count)] = quota_count
        loop_n += 1
    counts = counts.T
    counts *= previous_probabilities
    cdef cnp.ndarray[cnp.float64_t, ] quota_counts = np.sum(counts, axis=1)
    cdef cnp.ndarray[
            cnp.float64_t, ] quota_probs = quota_counts / quota_counts.sum()
    cdef cnp.ndarray[Py_ssize_t, ] quota_values = np.nonzero(quota_probs)[0]
    return quota_values, quota_probs[quota_values]
