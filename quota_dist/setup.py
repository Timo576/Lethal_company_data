from setuptools import setup
from Cython.Build import cythonize

setup(
    ext_modules=cythonize("Quota_distribution_calculator.pyx",
                          compiler_directives={"language_level": 3,
                                               "boundscheck": False}))
