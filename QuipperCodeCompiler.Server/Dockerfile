# --- Stage 1: Build the .NET application ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /out

# --- Stage 2: Final runtime image ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# 1. Install System Dependencies
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    build-essential \
    pkg-config \
    zlib1g-dev \
    libgmp-dev \
    libncurses-dev \
    libtinfo6 \
    libffi-dev \
    curl \
    xz-utils \
    git \
    make \
    ca-certificates \
    gnupg \
    netbase \
    dnsutils \
    ghostscript \
    --fix-missing && \
    rm -rf /var/lib/apt/lists/*



# 2. Install Stack
RUN apt-get update && curl -sSL https://get.haskellstack.org/ | sh

# 3. Setup GHC 8.6.5 via Stack
RUN stack setup --resolver lts-14.27

# 4. Create symlinks for GHC
RUN ln -s $(stack path --compiler-bin --resolver lts-14.27)/ghc /usr/local/bin/ghc && \
    ln -s $(stack path --compiler-bin --resolver lts-14.27)/ghci /usr/local/bin/ghci

# 5. Install Cabal (matching the GHC 8.6.5 era)
RUN stack install --resolver lts-14.27 cabal-install

# 6. Install Quipper Language
# Fixed: We must force an older 'random' version because newsynth 0.4.1.0 
# is incompatible with random >= 1.2 (missing Read StdGen instance).
RUN export PATH="$(stack path --compiler-bin --resolver lts-14.27):/root/.local/bin:$PATH" && \
    cabal update && \
    cabal v1-install random-1.1 --allow-newer && \
    cabal v1-install fixedprec --allow-newer && \
    cabal v1-install quipper-language --allow-newer

# 7. Final Path Configuration
ENV STACK_BIN_PATH="/root/.stack/programs/x86_64-linux/ghc-tinfo6-8.6.5/bin"
ENV PATH="/root/.local/bin:${STACK_BIN_PATH}:${PATH}:/root/.cabal/bin"

# Copy published application from the build stage
COPY --from=build /out .

# Set environment variables for the ASP.NET app
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "QuipperCodeCompiler.Server.dll"]