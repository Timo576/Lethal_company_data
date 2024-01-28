import numpy as np
from scipy.interpolate import PPoly
from scipy.interpolate import BPoly
from scipy.interpolate import CubicHermiteSpline as spline
from scipy.interpolate._cubic import prepare_input
import matplotlib.pyplot as plt
from matplotlib.widgets import Slider
from icecream import ic


class Hermite_Y(PPoly):
    """Piecewise-cubic interpolator matching values and first derivatives.

    The result is represented as a `PPoly` instance.

    Parameters
    ----------
    x : array_like, shape (n,)
        1-D array containing values of the independent variable.
        Values must be real, finite and in strictly increasing order.
    y : array_like
        Array containing values of the dependent variable. It can have
        arbitrary number of dimensions, but the length along ``axis``
        (see below) must match the length of ``x``. Values must be finite.
    dydx : array_like
        Array containing derivatives of the dependent variable. It can have
        arbitrary number of dimensions, but the length along ``axis``
        (see below) must match the length of ``x``. Values must be finite.
    axis : int, optional
        Axis along which `y` is assumed to be varying. Meaning that for
        ``x[i]`` the corresponding values are ``np.take(y, i, axis=axis)``.
        Default is 0.
    extrapolate : {bool, 'periodic', None}, optional
        If bool, determines whether to extrapolate to out-of-bounds points
        based on first and last intervals, or to return NaNs. If 'periodic',
        periodic extrapolation is used. If None (default), it is set to True.

    Attributes
    ----------
    x : ndarray, shape (n,)
        Breakpoints. The same ``x`` which was passed to the constructor.
    c : ndarray, shape (4, n-1, ...)
        Coefficients of the polynomials on each segment. The trailing
        dimensions match the dimensions of `y`, excluding ``axis``.
        For example, if `y` is 1-D, then ``c[k, i]`` is a coefficient for
        ``(x-x[i])**(3-k)`` on the segment between ``x[i]`` and ``x[i+1]``.
    axis : int
        Interpolation axis. The same axis which was passed to the
        constructor.

    Methods
    -------
    __call__
    derivative
    antiderivative
    integrate
    roots

    See Also
    --------
    Akima1DInterpolator : Akima 1D interpolator.
    PchipInterpolator : PCHIP 1-D monotonic cubic interpolator.
    CubicSpline : Cubic spline data interpolator.
    PPoly : Piecewise polynomial in terms of coefficients and breakpoints

    Notes
    -----
    If you want to create a higher-order spline matching higher-order
    derivatives, use `BPoly.from_derivatives`.

    References
    ----------
    .. [1] `Cubic Hermite spline
            <https://en.wikipedia.org/wiki/Cubic_Hermite_spline>`_
            on Wikipedia.
    """

    def __init__(self, x, y, dydx, w1, w2, axis=0, extrapolate=None):
        if extrapolate is None:
            extrapolate = True

        x, dx, y, axis, dydx = prepare_input(x, y, axis, dydx)

        b = (x[:-1] + x[1:]) * w2 * dydx[:-1]
        cy = x[1:] * w1 * dydx[1:]

        c = np.empty((4, len(x) - 1) + y.shape[1:], dtype=float)
        c[0] = cy+b+2*y[:-1]-2*y[1:]
        c[1] = -cy+(3*y[1:]-3*y[:-1])-2*b
        c[2] = b
        c[3] = y[:-1]
        ic(c)

        super().__init__(c, x, extrapolate=extrapolate)
        self.axis = axis


class Hermite_X(PPoly):
    """Piecewise-cubic interpolator matching values and first derivatives.

    The result is represented as a `PPoly` instance.

    Parameters
    ----------
    x : array_like, shape (n,)
        1-D array containing values of the independent variable.
        Values must be real, finite and in strictly increasing order.
    y : array_like
        Array containing values of the dependent variable. It can have
        arbitrary number of dimensions, but the length along ``axis``
        (see below) must match the length of ``x``. Values must be finite.
    dydx : array_like
        Array containing derivatives of the dependent variable. It can have
        arbitrary number of dimensions, but the length along ``axis``
        (see below) must match the length of ``x``. Values must be finite.
    axis : int, optional
        Axis along which `y` is assumed to be varying. Meaning that for
        ``x[i]`` the corresponding values are ``np.take(y, i, axis=axis)``.
        Default is 0.
    extrapolate : {bool, 'periodic', None}, optional
        If bool, determines whether to extrapolate to out-of-bounds points
        based on first and last intervals, or to return NaNs. If 'periodic',
        periodic extrapolation is used. If None (default), it is set to True.

    Attributes
    ----------
    x : ndarray, shape (n,)
        Breakpoints. The same ``x`` which was passed to the constructor.
    c : ndarray, shape (4, n-1, ...)
        Coefficients of the polynomials on each segment. The trailing
        dimensions match the dimensions of `y`, excluding ``axis``.
        For example, if `y` is 1-D, then ``c[k, i]`` is a coefficient for
        ``(x-x[i])**(3-k)`` on the segment between ``x[i]`` and ``x[i+1]``.
    axis : int
        Interpolation axis. The same axis which was passed to the
        constructor.

    Methods
    -------
    __call__
    derivative
    antiderivative
    integrate
    roots

    See Also
    --------
    Akima1DInterpolator : Akima 1D interpolator.
    PchipInterpolator : PCHIP 1-D monotonic cubic interpolator.
    CubicSpline : Cubic spline data interpolator.
    PPoly : Piecewise polynomial in terms of coefficients and breakpoints

    Notes
    -----
    If you want to create a higher-order spline matching higher-order
    derivatives, use `BPoly.from_derivatives`.

    References
    ----------
    .. [1] `Cubic Hermite spline
            <https://en.wikipedia.org/wiki/Cubic_Hermite_spline>`_
            on Wikipedia.
    """

    def __init__(self, x, y, dydx, w1, w2, axis=0, extrapolate=None):
        if extrapolate is None:
            extrapolate = True

        x, dx, y, axis, dydx = prepare_input(x, y, axis, dydx)

        c = np.empty((4, len(x) - 1) + y.shape[1:], dtype=float)

        b = (x[:-1] + x[1:]) * w2
        d = (x[1:] - x[:-1])

        c[0] = b + x[1:] * w1 - 2 * d
        c[1] = (3 * d - 2 * b - w1 * x[1:])
        c[2] = b
        c[3] = x[:-1]

        super().__init__(c, x, extrapolate=extrapolate)
        self.axis = axis


times: list[float] = [0, 0.11723505, 0.88036245, 1]
values: list[float] = [-0.50302887, -0.13017726, 0.15344214, 0.5030365]
derivatives: list[float] = [7.455404, 0.5548811, 0.5221589, 7.0514693]
# spline1 = CubicHermiteSpline(times, values, derivatives)
# x= np.linspace(0, 1, 1000)
# plt.plot(x, spline1(x))
# plt.show()

turret_keyframe1 = (0, 2.0207, 0.3546, 0.3546, 0, 1, 0.9235)
turret_keyframe2 = (0.6991, 2.9655, 7.0642, 7.0642, 3, 0.2517, 0.6898)
turret_keyframe3 = (1.0002, 6.5672, 57.2522, 57.2522, 3, 0.1665, 0)
time, value, in_tangent, out_tangent, weighted_mode, in_weight, out_weight = (
    [], [], [], [], [], [], [])
for keyframe in turret_keyframe1, turret_keyframe2, turret_keyframe3:
    time.append(keyframe[0])
    value.append(keyframe[1])
    in_tangent.append(keyframe[2])
    in_weight.append(keyframe[5])
    out_weight.append(keyframe[6])

in_weight = np.array(in_weight)
out_weight = np.array(out_weight)
spline_y = Hermite_Y(time, value, in_tangent, w1=in_weight[1:],
                     w2=out_weight[:-1])
spline_x = Hermite_X(time, value, in_tangent, w1=in_weight[1:],
                     w2=out_weight[:-1])
ts = np.linspace(0, 1, 1000)
plt.plot(spline_x(ts), spline_y(ts))
# plt.plot(ts, spline_y(ts))
# plt.plot(ts, spline_x(ts))
plt.grid(True)
plt.show()
