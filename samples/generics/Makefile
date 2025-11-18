# Makefile for Fifth generics sample
# Assumes 'fifth' compiler is installed and available on PATH

FIFTH = fifth
SOURCE = main.5th
OUTPUT = main

.PHONY: all build run clean

# Default to compile-only to satisfy CI/build checks
all: build

build:
	$(FIFTH) --command build --source $(SOURCE) --output $(OUTPUT)

run: build
	$(FIFTH) --command run --source $(SOURCE) --output $(OUTPUT)

clean:
	rm -f $(OUTPUT)
