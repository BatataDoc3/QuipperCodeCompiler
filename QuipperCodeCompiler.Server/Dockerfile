FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

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



RUN apt-get update && curl -sSL https://get.haskellstack.org/ | sh

RUN stack setup --resolver lts-14.27

RUN ln -s $(stack path --compiler-bin --resolver lts-14.27)/ghc /usr/local/bin/ghc && \
    ln -s $(stack path --compiler-bin --resolver lts-14.27)/ghci /usr/local/bin/ghci

RUN stack install --resolver lts-14.27 cabal-install

RUN export PATH="$(stack path --compiler-bin --resolver lts-14.27):/root/.local/bin:$PATH" && \
    cabal update && \
    cabal v1-install random-1.1 --allow-newer && \
    cabal v1-install fixedprec --allow-newer && \
    cabal v1-install quipper-language --allow-newer

ENV STACK_BIN_PATH="/root/.stack/programs/x86_64-linux/ghc-tinfo6-8.6.5/bin"
ENV PATH="/root/.local/bin:${STACK_BIN_PATH}:${PATH}:/root/.cabal/bin"

COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "QuipperCodeCompiler.Server.dll"]