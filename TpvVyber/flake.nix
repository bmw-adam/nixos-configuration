{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = inputs@{ self, nixpkgs, flake-utils, ... }:
  
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs {
          inherit system;
        };

        dotnet-sdk = pkgs.dotnetCorePackages.dotnet_8.sdk;
        dotnet-runtime = pkgs.dotnetCorePackages.aspnetcore_8_0-bin;

        genDeps = pkgs.writeShellScriptBin "genDeps" ''
          set -e

          # Find flake root by walking up until flake.nix is found
          dir="$PWD"
          while [ "$dir" != "/" ]; do
            if [ -f "$dir/flake.nix" ]; then
              cd "$dir"
              break
            fi
            dir=$(dirname "$dir")
          done

          if [ ! -f flake.nix ]; then
            echo "Error: flake.nix not found in any parent directory." >&2
            exit 1
          fi

          echo "Restoring NuGet packages..."
          dotnet restore ./TpvVyber/TpvVyber.sln --packages ./.nuget-packages
          echo "Generating deps/deps.json..."
          nuget-to-json ./.nuget-packages ./TpvVyber/deps/excluded_list > ./TpvVyber/deps/deps.json
          echo "Dependencies JSON generated at ./TpvVyber/deps/deps.json"
          rm -rf ./.nuget-packages
          echo "Cleaned up temporary NuGet packages."
        '';
      in {
        packages.default = pkgs.buildDotnetModule {
          inherit dotnet-sdk dotnet-runtime;
          pname = "TpvVyber";
          version = "0.1.0";
          src = ./TpvVyber;
          projectFile = "TpvVyber/TpvVyber.csproj";

          nugetDeps = ./TpvVyber/deps/deps.json;
          packNupkg = true;
          selfContainedBuild = true;
          runtimeMicrosoftDependencies = false;

          nativeBuildInputs = [ pkgs.makeWrapper pkgs.wasm-tools ];
          buildInputs = [ pkgs.wasm-tools ];
          doCheck = true;

          buildPhase = ''
            dotnet publish --configuration Release --no-restore --output $PWD/publish ./TpvVyber/TpvVyber.csproj
          '';


          installPhase = ''
            mkdir -p $out/bin
            cp -r $PWD/publish/* $out/bin/

            makeWrapper ${dotnet-runtime}/bin/dotnet $out/bin/TpvVyber \
              --chdir $out/bin \
              --add-flags "exec $out/bin/TpvVyber.dll" \
              --prefix LD_LIBRARY_PATH : "${pkgs.lib.makeLibraryPath [
                pkgs.stdenv.cc.cc.lib
                pkgs.openssl
              ]}" \
              --set ASPNETCORE_URLS "http://localhost:1234;https://localhost:1235"
          '';
        };

        devShells.default = pkgs.mkShell {
          packages = [
            dotnet-sdk
            pkgs.omnisharp-roslyn
            pkgs.nuget-to-json
            pkgs.openssl
            genDeps
          ];

          shellHook = ''
            export DOTNET_ROOT=${pkgs.dotnet-sdk_8}
            mkdir -p .nuget-packages
            export NUGET_PACKAGES="$PWD/.nuget-packages"
          '';
        };
      }
    );
}
