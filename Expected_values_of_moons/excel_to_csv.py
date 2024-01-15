import pandas as pd


weight_list = []
for moon in range(8):
    if moon == 0:
        weight_df = pd.read_csv("scrap.csv",
                                usecols=[0, 1])
        weight_df.set_index(weight_df.columns[0], inplace=True)
        weight_df.dropna(axis=0, inplace=True, how="all")
        first_weight = weight_df
    else:
        start_col = moon * 3
        weight_df = pd.read_csv("scrap.csv",
                                usecols=[start_col, start_col + 1])
        weight_df.set_index(weight_df.columns[0], inplace=True)
        weight_df.dropna(axis=0, inplace=True, how="all")
        weight_list.append(weight_df)
# noinspection PyUnboundLocalVariable
full_weight_df = first_weight.join(weight_list)
full_weight_df.fillna(value=0, inplace=True)
full_weight_df.to_csv("Moon_scrap_rarities_v47.csv")
