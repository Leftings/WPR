# Use the official Ubuntu image as the base
FROM ubuntu:20.04

# Set non-interactive mode for apt-get
ARG DEBIAN_FRONTEND=noninteractive

# Install dependencies: curl, tar, sudo, ca-certificates
RUN apt-get update -y && \
    apt-get install -y \
    curl \
    sudo \
    ca-certificates \
    tar \
    lsb-release \
    git

# Install Node.js (needed by the GitHub Actions Runner)
RUN curl -fsSL https://deb.nodesource.com/setup_16.x | sudo -E bash - && \
    sudo apt-get install -y nodejs

# Set the working directory to /home/runner
WORKDIR /home/runner

# Download GitHub Actions runner
ARG RUNNER_VERSION="2.320.0"
RUN curl -o actions-runner-linux-x64-${RUNNER_VERSION}.tar.gz -L https://github.com/actions/runner/releases/download/v${RUNNER_VERSION}/actions-runner-linux-x64-${RUNNER_VERSION}.tar.gz

# Extract the downloaded runner
RUN tar xzf actions-runner-linux-x64-${RUNNER_VERSION}.tar.gz

# Expose necessary port (optional, depending on your needs)
EXPOSE 8080

# Copy the configuration script to setup runner at runtime
COPY ./config.sh /home/runner/config.sh

# Set the entry point to run the GitHub Actions Runner
ENTRYPOINT ["/home/runner/config.sh"]
