"""Modified distribution code to include the average cumulative sums
 using 64 bits"""

import numpy as np


def unpack_int64(full_dist_arr):
    """Unpacks the 64 bit ints to a cumulative sum and a current sum.
    e.g. for 16-bits cumulative sum = 17, current = 2
    packed = 00010001 00000010 where split_loc is
    after the first 8 bits"""
    split_loc = 2 ** 32
    cumulative = full_dist_arr // split_loc
    current = full_dist_arr % split_loc
    return cumulative, current

# adjust calculator
