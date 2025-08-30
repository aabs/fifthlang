# Makefile

ANTLR_VERSION ?= 4.13.1
IMAGE_NAME ?= antlr-cli
ANTLR_FLAGS ?=  -Dlanguage=CSharp -visitor -listener -o grammar -lib . -Xexact-output-dir
GRAMMAR_DIR ?= $(CURDIR)/grammar
ARGS ?= Fifth.g4

.PHONY: build run clean

build:
	docker build --platform=linux/arm64 -t $(IMAGE_NAME) --build-arg ANTLR_VERSION=$(ANTLR_VERSION) ./tools/docker

run:
	docker run --rm -v $(GRAMMAR_DIR):/workspace -w /workspace $(IMAGE_NAME) $(ANTLR_FLAGS) $(ARGS)

clean:
	-docker rmi $(IMAGE_NAME)
