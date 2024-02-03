"""Gets and plots unity keyframe data"""
import numpy as np
from scipy.interpolate import BPoly
import matplotlib.pyplot as plt


class UnityY(BPoly):
    """Creates a y spline from unity keyframes using the Bernstein basis"""

    def __init__(self, x, y, dydx_in, dydx_out, win, wout, axis=0,
                 extrapolate=None):
        if extrapolate is None:
            extrapolate = True
        diff = np.diff(x)
        c = np.empty((4, len(x) - 1), dtype=float)
        c[0] = y[:-1]
        c[1] = y[:-1] + diff * wout[:-1] * dydx_out[:-1]
        c[2] = y[1:] - diff * win[1:] * dydx_in[1:]
        c[3] = y[1:]
        super().__init__(c, x, extrapolate=extrapolate)
        self.axis = axis


class UnityX(BPoly):
    """Creates an x spline from unity keyframes using the Bernstein basis"""

    def __init__(self, x, win, wout, axis=0, extrapolate=None):
        if extrapolate is None:
            extrapolate = True
        c = np.empty((4, len(x) - 1), dtype=float)
        diff = np.diff(x)
        c[0] = x[:-1]
        c[1] = x[:-1] + diff * wout[:-1]
        c[2] = x[1:] - diff * win[1:]
        c[3] = x[1:]
        super().__init__(c, x, extrapolate=extrapolate)
        self.axis = axis


def get_keyframe_data():
    """Regex parser that outputs keyframe data"""
    # TODO: BS SyntaxWarning, outside curve data
    data_dtype = [("time", float), ("value", float), ("inSlope", float),
                  ("outSlope", float), ("weightedMode", int),
                  ("inWeight", float), ("outWeight", float)]
    keyframes = np.fromregex("Keyframes.txt", "(?<=time): (-?\d+\.*\d*)\n"
                                              " {6}value: (-?\d+\.*\d*)\n"
                                              " {6}inSlope: (-?\d+\.*\d*)\n"
                                              " {6}outSlope: (-?\d+\.*\d*)\n"
                                              " {6}tangentMode: \d\n"
                                              " {6}weightedMode: (\d)\n"
                                              " {6}inWeight: (-?\d+\.*\d*)\n"
                                              " {6}outWeight: (-?\d+\.*\d*)",
                             data_dtype)
    curve_data = np.fromregex("Keyframes.txt", "(?<= m_PreInfinity): (\d)\n"
                                               " {4} m_PostInfinity: (\d)\n"
                                               " {4}m_RotationOrder: (\d)",
                              [("pre", int), ("post", int), ("rot", int)])
    for idx, weighted_mode in enumerate(keyframes["weightedMode"]):
        if weighted_mode == 0:
            keyframes["inWeight"][idx] = 1 / 3
            keyframes["outWeight"][idx] = 1 / 3
        elif weighted_mode == 1:
            keyframes["outWeight"][idx] = 1 / 3
        elif weighted_mode == 2:
            keyframes["outWeight"][idx] = 1 / 3
        elif weighted_mode == 3:
            continue
        else:
            raise ValueError("Invalid weighted mode")
    return keyframes["time"], keyframes["value"], keyframes["inSlope"], \
        keyframes["outSlope"], keyframes["inWeight"], keyframes["outWeight"]


def plot_keyframes():
    """Gets and plots unity keyframe data"""
    x_values, y_values, dydx_in, dydx_out, wins, wouts = get_keyframe_data()
    x_spline = UnityX(x_values, wins, wouts)
    y_spline = UnityY(x_values, y_values, dydx_in, dydx_out, wins, wouts)
    times = np.linspace(0, 1, 1000)
    fig, ax = plt.subplots()
    ax.plot(x_spline(times), y_spline(times))
    ax.grid(True)
    ax.set(xlabel="Input values", ylabel="Output values")
    plt.show()
    # plt.savefig()
    # plt.clf()


if __name__ == '__main__':
    plot_keyframes()
