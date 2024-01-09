ifeq ($(OS), Windows_NT)
CC=gcc
CFLAGS=$(INC_PARAMS) -O3 -Wall

INC=
INC_PARAMS=$(INC:%=-I%)

ODIR=obj
_OBJ = quota_dist.o
OBJ = $(patsubst %,$(ODIR)/%,$(_OBJ))

PY = quota_dist.py QuotaDist.py


$(ODIR)/%.o: %.c $(INC)
	$(CC) -c -o $@ $< $(CFLAGS)

build: $(OBJ) $(PY) setup.py
	python setup.py build_ext --inplace
	$(CC) -o quota_dist $(OBJ) $(CFLAGS)

.PHONY: clean

clean:
	rm -f $(ODIR)/*.o
endif
