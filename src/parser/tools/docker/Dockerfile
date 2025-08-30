# Dockerfile
FROM --platform=linux/arm64 openjdk:21-slim

ARG ANTLR_VERSION=4.13.1

WORKDIR /workspace

RUN apt-get update && \
    apt-get install -y curl && \
    curl -L -o /usr/local/lib/antlr-${ANTLR_VERSION}-complete.jar https://www.antlr.org/download/antlr-${ANTLR_VERSION}-complete.jar && \
    ln -s /usr/local/lib/antlr-${ANTLR_VERSION}-complete.jar /usr/local/lib/antlr4.jar && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

ENV CLASSPATH=".:/usr/local/lib/antlr4.jar:$CLASSPATH"

ENTRYPOINT ["java", "-jar", "/usr/local/lib/antlr4.jar"]
