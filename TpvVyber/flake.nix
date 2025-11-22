{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = inputs@{ self, nixpkgs, flake-utils, ... }:

    flake-utils.lib.eachDefaultSystem (system:
      let
        # --- Overlay Definition ---
        dotnetWasmOverlay = final: prev: {
          dotnet-sdk-wasm = prev.dotnetCorePackages.combinePackages [
            prev.dotnetCorePackages.dotnet_9.sdk
            prev.wasm-tools
          ];
        };

        pkgs = import nixpkgs {
          inherit system;
          overlays = [ dotnetWasmOverlay ];
        };

        dotnet-sdk = pkgs.dotnetCorePackages.dotnet_9.sdk;
        dotnet-runtime = pkgs.dotnetCorePackages.aspnetcore_9_0-bin;

        genDeps = pkgs.writeShellScriptBin "genDeps" ''
          set -e
          dir="$PWD"
          while [ "$dir" != "/" ]; do
            if [ -f "$dir/flake.nix" ]; then
              cd "$dir"
              break
            fi
            dir=$(dirname "$dir")
          done

          if [ ! -f flake.nix ]; then
            echo "Error: flake.nix not found." >&2
            exit 1
          fi

          echo "Restoring Server..."
          dotnet restore ./TpvVyber/TpvVyber/TpvVyber.csproj --packages ./.nuget-packages
          nuget-to-json ./.nuget-packages > ./TpvVyber/TpvVyber/deps.server.json
          rm -rf ./.nuget-packages

          echo "Restoring Client (Dual RID)..."
          # Restoring for both WASM (target) and Linux (host tools) to ensure 'deps.json' is complete
          dotnet restore ./TpvVyber/TpvVyber.Client/TpvVyber.Client.csproj \
            -r browser-wasm \
            --packages ./.nuget-packages

          echo "Generating Client deps.json..."
          nuget-to-json ./.nuget-packages > ./TpvVyber/TpvVyber.Client/deps.client.json
          
          rm -rf ./.nuget-packages
          echo "Done."
        '';

        # --- Client Derivation (Defined here to be used by Server) ---
        clientDrv = pkgs.buildDotnetModule {
          inherit dotnet-runtime;
          # Use specialized WASM SDK
          dotnet-sdk = pkgs.dotnet-sdk-wasm;
          
          pname = "TpvVyber-Client-Assets";
          version = "0.1.0";
          src = ./TpvVyber/TpvVyber.Client;
          projectFile = "TpvVyber.Client.csproj";
          nugetDeps = ./TpvVyber/TpvVyber.Client/deps.client.json; 
          
          nativeBuildInputs = [ pkgs.wasm-tools ];
          buildInputs = [ pkgs.wasm-tools ];

          configurePhase = ''
            runHook preConfigure
            echo "--- Executing Custom Configure Phase ---"
            # Restore specifically for browser-wasm
            dotnet restore "TpvVyber.Client.csproj" -r browser-wasm --no-cache
            runHook postConfigure
          '';

          buildPhase = ''
            echo "--- Building Blazor WASM Client Assets ---"
            dotnet publish \
              --configuration Release \
              --no-restore \
              --output $PWD/publish \
              /p:RuntimeIdentifier=browser-wasm
          '';

          installPhase = ''
            mkdir -p $out
            cp -r $PWD/publish/* $out/
          '';
        };

      in {
        # Expose the client package
        packages.client = clientDrv;

        # --- Server Package Definition ---
        packages.server = pkgs.buildDotnetModule {
          inherit dotnet-sdk dotnet-runtime;
          pname = "TpvVyber";
          version = "0.1.0";
          
          src = ./TpvVyber;
          projectFile = "TpvVyber/TpvVyber.csproj"; 
          nugetDeps = ./TpvVyber/TpvVyber/deps.server.json; 
          
          selfContainedBuild = false;
          # We still need the C# compilation to know about Client types (if any), 
          # but we stop the WASM build process.
          
          buildPhase = ''
            runHook preBuild
            echo "Publish - Custom"
            dotnet publish ./TpvVyber/TpvVyber.csproj \
              -c Release \
              -o publish \
              --no-restore \
              /p:BlazorWebAssemblyBuildServer=false \
              /p:BlazorWebAssemblyClusterPublish=false
            runHook postBuild
          '';

          configurePhase = ''
            runHook preConfigure
            echo "--- Custom Restoring Server ---"
            dotnet restore "$projectFile"
            runHook postConfigure
          '';

          installPhase = ''
            mkdir -p $out/bin
            
            # 1. Copy Server Artifacts
            ls -al
            cp -r $PWD/publish/* $out/bin/
            
            # 2. Inject Pre-Built Client Assets (DLLs, _framework, wasm)
            # This puts the client artifacts exactly where the Server expects them to be served from.
            echo "--- Injecting Client Artifacts ---"
            mkdir -p $out/bin/wwwroot
            cp -r ${clientDrv}/wwwroot/* $out/bin/wwwroot/

            # 3. Wrap the executable
            makeWrapper ${dotnet-runtime}/bin/dotnet $out/bin/TpvVyber \
              --add-flags "exec $out/bin/TpvVyber.dll" \
              --prefix LD_LIBRARY_PATH : "${pkgs.lib.makeLibraryPath [ pkgs.stdenv.cc.cc.lib pkgs.openssl ]}" \
              --set ASPNETCORE_URLS "http://localhost:1234;https://localhost:1235"
          '';
        };

        packages.default = self.packages.${system}.server;

        devShells.default = pkgs.mkShell {
          packages = [ pkgs.dotnet-sdk-wasm pkgs.nuget-to-json genDeps ];
          shellHook = ''
            export DOTNET_ROOT=${pkgs.dotnet-sdk-wasm}
            mkdir -p .nuget-packages
            export NUGET_PACKAGES="$PWD/.nuget-packages"
          '';
        };
      }
    );
}
