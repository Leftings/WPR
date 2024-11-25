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
    git \
    wget

# Install Node.js (needed by the GitHub Actions Runner)
RUN curl -fsSL https://deb.nodesource.com/setup_16.x | sudo -E bash - && \
    sudo apt-get install -y nodejs

# Set the working directory to /home/runner
WORKDIR /home/runner

# Download the GitHub Actions runner package
ARG RUNNER_VERSION="2.320.0"
RUN curl -o actions-runner-linux-x64-${RUNNER_VERSION}.tar.gz -L https://github.com/actions/runner/releases/download/v${RUNNER_VERSION}/actions-runner-linux-x64-${RUNNER_VERSION}.tar.gz

# Extract the runner package
RUN tar xzf actions-runner-linux-x64-${RUNNER_VERSION}.tar.gz

# Set up the runner
# Ensure that the config.sh script will be executed at runtime
COPY ./config.sh /home/runner/config.sh

# Expose any necessary ports (optional)
EXPOSE 8080

# Default entry point to run the GitHub Actions Runner
ENTRYPOINT ["bash", "/home/runner/config.sh"]
