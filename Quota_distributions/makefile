#define NPY_NO_DEPRECATED_API NPY_1_7_API_VERSION
CC=cl
CYTHON_FLAGS = -X language_level=3 -X boundscheck=False -X wraparound=False \
-X cdivision=True -X initializedcheck=False -a
CYTHON_SRC = Quota_distribution_calculator.pyx
CFLAGS=/W4 $(INCLUDE_DIRS) /LD /O2
C_SRC = Quota_distribution_calculator.c
TARGET = Quota_distribution_calculator.pyd
LDFLAGS = /link
LIBPATHS = /LIBPATH:"C:\Users\twlln\AppData\Local\Programs\Python\Python312\libs" \
	/LIBPATH:"C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Tools\MSVC\14.38.33130\lib\x64" \
	/LIBPATH:"C:\Program Files (x86)\Windows Kits\10\Lib\10.0.22621.0\um\x64" \
	/LIBPATH:"C:\Program Files (x86)\Windows Kits\10\Lib\10.0.22621.0\ucrt\x64"

INCLUDE_DIRS = /I C:\Users\twlln\AppData\Local\Programs\Python\Python312\include \
	/I "C:\Program Files (x86)\Windows Kits\10\Include\10.0.22621.0\ucrt" \
	/I "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Tools\MSVC\14.38.33130\include" \
	/I "C:\Program Files (x86)\Windows Kits\10\Include\10.0.22621.0\shared" \
	/I "C:\Users\twlln\Documents\Lethal\.venv\Lib\site-packages\numpy\core\include"

build: $(TARGET) clean

run: build
	python Quota_distribution_interpreter.py

$(TARGET): $(C_SRC)
	$(CC) $(CFLAGS) $(C_SRC) $(LDFLAGS) $(LIBPATHS) /OUT:$(TARGET)

$(C_SRC): $(CYTHON_SRC)
	cython -3 $(CYTHON_FLAGS) $(CYTHON_SRC)


.PHONY: clean

clean:
	del /Q /F *.exp *.lib *.obj
