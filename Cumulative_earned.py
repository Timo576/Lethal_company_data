import numpy as np
import math
import pandas as pd


def find_min_quota(quota_num, prev_quota):
    return math.floor(
        100 * (1 - 0.000008) * (1 + quota_num ** 2 / 16)) + prev_quota


prev_quota = 130
quota_list = [130]
money_made_list = [14847]
money_made = 14847
for quota_num in range(1, 50):
    prev_quota = find_min_quota(quota_num, prev_quota)
    money_made_list.append(money_made)
    quota_list.append(prev_quota)
cumulative_sum_quota = pd.Series(quota_list).cumsum()
cumulative_sum_money = pd.Series(money_made_list).cumsum()
# print(cumulative_sum_quota > cumulative_sum_money)
# print(cumulative_sum_quota)
print(pd.Series(quota_list))
