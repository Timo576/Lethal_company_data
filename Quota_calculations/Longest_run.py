import numpy as np
import math
import pandas as pd
from icecream import ic


def find_min_quota(quota_num, prev_quota):
    return math.floor(
        100 * (1 + 0.5030365) * (1 + quota_num ** 2 / 16)) + prev_quota

gift_gold_bar = 210 + 14
gift_fancy_lamp = 128 + 14
nutcrackers = 90 * 10
apparatus = 80
prev_quota = 130
quota_list = [130]
money_made_list = [
    3 * (150 * 8 + nutcrackers + 16 * gift_gold_bar + apparatus)]
money_made = 3 * (nutcrackers + 37 * gift_fancy_lamp + apparatus)
for quota_num in range(1, 50):
    prev_quota = find_min_quota(quota_num, prev_quota)
    money_made_list.append(money_made)
    quota_list.append(prev_quota)
cumulative_sum_quota = pd.Series(quota_list).cumsum()
cumulative_sum_money = pd.Series(money_made_list).cumsum()
print(cumulative_sum_quota > cumulative_sum_money)
# ic(money_made_list)
print(pd.Series(quota_list))
