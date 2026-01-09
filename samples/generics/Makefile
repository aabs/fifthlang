# Makefile for Fifth generics sample
# Assumes 'fifth' compiler is installed and available on PATH

FIFTH = fifth
SOURCE = test_map_debug.5th
DIR_OUTPUT = build
OUTPUT = $(DIR_OUTPUT)/test_map_debug.dll

.PHONY: all build run clean

# Default to compile-only to satisfy CI/build checks
all: build

prepare:
	mkdir -p $(DIR_OUTPUT)

build: clean prepare
	$(FIFTH) --command build --source $(SOURCE) --output $(OUTPUT)

run: build
	dotnet $(OUTPUT)

clean:
	rm -rf $(DIR_OUTPUT)
	# rm -rf build_debug_roslyn
