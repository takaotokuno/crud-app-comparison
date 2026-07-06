FROM mcr.microsoft.com/devcontainers/dotnet:1-9.0-bookworm

USER root

# The base image may contain an old Yarn apt repository.
# Remove it because apt update can fail with:
# NO_PUBKEY 62D54FD4003F6525
RUN rm -f /etc/apt/sources.list.d/yarn.list \
    /etc/apt/sources.list.d/yarn*.list \
    /etc/apt/trusted.gpg.d/yarn*.gpg \
    /usr/share/keyrings/yarn*.gpg || true

USER vscode