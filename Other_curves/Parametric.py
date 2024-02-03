"""Gets and plots unity keyframe data"""
import numpy as np
from scipy.interpolate import BPoly
import matplotlib.pyplot as plt


class UnityY(BPoly):
    """Creates a y spline from unity keyframes using the Bernstein basis"""

    def __init__(self, t, y, dydx_in, dydx_out, win, wout, clipping_info,
                 axis=0, extrapolate=None):
        # t should be strictly increasing
        if extrapolate is None:
            extrapolate = False
        diff = np.diff(t)
        c = np.empty((4, len(t) - 1), dtype=float)
        c[0] = y[:-1]
        c[1] = y[:-1] + diff * wout[:-1] * dydx_out[:-1]
        c[2] = y[1:] - diff * win[1:] * dydx_in[1:]
        c[3] = y[1:]
        super().__init__(c, t, extrapolate=extrapolate)
        self.axis = axis
        self.clipping = clipping_info
        self.t = t

    def _evaluate(self, t, nu, extrapolate, out):
        if self.clipping["pre"] == 2 and self.clipping["post"] == 2:
            np.clip(t, self.t[0], self.t[-1], out=t)  # Base case
        else:
            if self.clipping["pre"] == 0:
                while np.any(t < self.t[0]):
                    t[np.where(t < self.t[0])] += self.t[-1]
            elif self.clipping["pre"] == 1:
                while np.any(t < self.t[0]):
                    overflows = np.where(t < self.t[0])
                    t[overflows] = 2 * self.t[0] - t[overflows]
            elif self.clipping["pre"] == 2:
                np.clip(t, a_min=self.t[0], a_max=None, out=t)
            if self.clipping["post"] == 0:
                while np.any(t > self.t[-1]):
                    t[np.where(t > self.t[-1])] += self.t[-1]
            elif self.clipping["post"] == 1:
                while np.any(t > self.t[-1]):
                    overflows = np.where(t > self.t[-1])
                    t[overflows] = 2 * self.t[-1] - t[overflows]
            elif self.clipping["post"] == 2:
                np.clip(t, a_min=None, a_max=self.t[-1], out=t)
        super()._evaluate(t, nu, extrapolate, out)


class UnityX(BPoly):
    """Creates an x spline from unity keyframes using the Bernstein basis"""

    def __init__(self, t, win, wout, axis=0, extrapolate=None):
        # t should be strictly increasing
        if extrapolate is None:
            extrapolate = False
        c = np.empty((4, len(t) - 1), dtype=float)
        diff = np.diff(t)
        c[0] = t[:-1]
        c[1] = t[:-1] + diff * wout[:-1]
        c[2] = t[1:] - diff * win[1:]
        c[3] = t[1:]
        super().__init__(c, t, extrapolate=extrapolate)
        self.axis = axis


def get_keyframe_data():
    """Regex parser that outputs keyframe data"""
    data_dtype = [("time", float), ("value", float), ("inSlope", float),
                  ("outSlope", float), ("weightedMode", int),
                  ("inWeight", float), ("outWeight", float)]
    data_regex = (r"(?<=time): (-?\d+\.*\d*)\n"
                  r" *value: (-?\d+\.*\d*)\n"
                  r" *inSlope: (-?\d+\.*\d*)\n"
                  r" *outSlope: (-?\d+\.*\d*)\n"
                  r" *tangentMode: \d\n"
                  r" *weightedMode: (\d)\n"
                  r" *inWeight: (-?\d+\.*\d*)\n"
                  r" *outWeight: (-?\d+\.*\d*)")
    keyframes = np.fromregex("Keyframes.txt", data_regex,
                             data_dtype)
    curve_data = np.fromregex("Keyframes.txt", r"(?<=m_PreInfinity): (\d)\n"
                                               r" *m_PostInfinity: (\d)\n"
                                               r" *m_RotationOrder: (\d)",
                              [("pre", int), ("post", int), ("rot", int)])
    # Wrapping enum: 0=Loop, 1=PingPong, 2=Clamp. Rotation: Unknown (Always 4)
    for idx, weighted_mode in enumerate(keyframes["weightedMode"]):
        if weighted_mode == 0:  # Neither
            keyframes["inWeight"][idx] = 1 / 3
            keyframes["outWeight"][idx] = 1 / 3
        elif weighted_mode == 1:  # In
            keyframes["outWeight"][idx] = 1 / 3
        elif weighted_mode == 2:  # Out
            keyframes["outWeight"][idx] = 1 / 3
        elif weighted_mode == 3:  # Both
            continue
        else:
            raise ValueError("Invalid weighted mode")
    return keyframes["time"], keyframes["value"], keyframes["inSlope"], \
        keyframes["outSlope"], keyframes["inWeight"], keyframes["outWeight"], \
        curve_data


def plot_keyframes():
    """Gets and plots unity keyframe data"""
    x_vals, y_vals, dydx_in, dydx_out, wins, wouts, c_dat = get_keyframe_data()
    x_spline = UnityX(x_vals, wins, wouts)
    y_spline = UnityY(x_vals, y_vals, dydx_in, dydx_out, wins, wouts, c_dat)
    times = np.linspace(np.min(x_vals), np.max(x_vals), 1000)
    fig, ax = plt.subplots()
    ax.plot(x_spline(times), y_spline(times))
    ax.grid(True)
    ax.set(xlabel="Input values", ylabel="Output values")
    plt.show()
    # plt.savefig()
    # plt.clf()


if __name__ == '__main__':
    plot_keyframes()
