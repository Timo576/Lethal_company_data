CC=cl
CYTHON_FLAGS = -Wextra -X language_level=3 -X boundscheck=False \
-X wraparound=False -X cdivision=True -X initializedcheck=False \
-X c_api_binop_methods=True -E extra_compile_args=['/openmp'] \
-E CYTHON_ATOMICS=1 -a
FILENAME = Quota_distribution_calculator
CYTHON_SRC = $(FILENAME).pyx
C_SRC = $(FILENAME).c
TARGET = $(FILENAME).pyd


CFLAGS=/W4 $(INCLUDE_DIRS) /LD /O2
LDFLAGS = /link
LIB_PATHS = /LIBPATH:"C:\Users\twlln\AppData\Local\Programs\Python\Python312\libs" \
	/LIBPATH:"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VC\Tools\MSVC\14.38.33130\lib\x64" \
	/LIBPATH:"C:\Program Files (x86)\Windows Kits\10\Lib\10.0.22621.0\um\x64" \
	/LIBPATH:"C:\Program Files (x86)\Windows Kits\10\Lib\10.0.22621.0\ucrt\x64"

INCLUDE_DIRS = /I "C:\Users\twlln\AppData\Local\Programs\Python\Python312\include" \
	/I "C:\Program Files (x86)\Windows Kits\10\Include\10.0.22621.0\ucrt" \
	/I "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VC\Tools\MSVC\14.38.33130\include" \
	/I "C:\Program Files (x86)\Windows Kits\10\Include\10.0.22621.0\shared" \
	/I "C:\Users\twlln\Documents\Lethal\.venv\Lib\site-packages\numpy\core\include"

build: $(TARGET) clean

run: build
	python.exe Quota_distribution_interpreter.py

$(TARGET): $(C_SRC)
	$(CC) $(CFLAGS) $(C_SRC) $(LDFLAGS) $(LIB_PATHS) /OUT:$(TARGET)

$(C_SRC): $(CYTHON_SRC)
	cython.exe -3 $(CYTHON_FLAGS) $(CYTHON_SRC)


.PHONY: clean

clean:
	del /Q /F *.exp *.lib *.obj
