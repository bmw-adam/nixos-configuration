{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs =
    inputs@{
      self,
      nixpkgs,
      flake-utils,
      ...
    }:
    flake-utils.lib.eachDefaultSystem (
      system:
      let
        nswagConsoleTool = pkgs.stdenv.mkDerivation {
          pname = " NSwag.ConsoleCore";
          version = "14.6.3";

          src = pkgs.fetchurl {
            url = "https://www.nuget.org/api/v2/package/NSwag.ConsoleCore/14.6.3";
            sha256 = "sha256-4oGTSG3/uwPprJSb+WGX2WtXEsKKLbSiBQ1GMnEJRcM=";
          };

          unpackPhase = ''
            mkdir source
            unzip $src -d source
          '';

          installPhase = ''
            mkdir -p $out/lib
            mkdir -p $out/bin

            # Extract prebuilt DLL content
            cp -r source/tools/net9.0/any/* $out/lib/

            # Create launcher script
            cat > $out/bin/nswag <<EOF
            #!${pkgs.bash}/bin/bash
            exec ${pkgs.dotnetCorePackages.dotnet_9.sdk}/bin/dotnet $out/lib/dotnet-nswag.dll "\$@"
            EOF

            chmod +x $out/bin/nswag
          '';

          nativeBuildInputs = [
            pkgs.unzip
            pkgs.bash
            pkgs.dotnetCorePackages.dotnet_9.sdk
          ];

          meta = {
            homepage = "https://github.com/blazorlore/blazor-formatter#readme";
            license = pkgs.lib.licenses.mit;
            platforms = pkgs.lib.platforms.linux;
          };
        };

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

          # Use the WASM-aware SDK
          dotnet-sdk = pkgs.dotnet-sdk-wasm;

          pname = "TpvVyber-Client-Assets";
          version = "0.1.0";
          src = ./TpvVyber/TpvVyber.Client;
          projectFile = "TpvVyber.Client.csproj";
          nugetDeps = ./TpvVyber/TpvVyber.Client/deps.client.json;

          nativeBuildInputs = [ pkgs.wasm-tools nswagConsoleTool ];
          buildInputs = [ pkgs.wasm-tools nswagConsoleTool ];

          # Disable the workload resolver so Nix's preinstalled workloads are used
          dotnetBuildFlags = [
            "/p:WasmEnableWorkloadResolver=false"
            "/p:EnableWorkloadResolver=false"
          ];

          configurePhase = ''
            runHook preConfigure
            echo "--- Executing Custom Configure Phase ---"
            dotnet restore "TpvVyber.Client.csproj" \
              -r browser-wasm \
              --no-cache \
              /p:WasmEnableWorkloadResolver=false \
              /p:EnableWorkloadResolver=false
            runHook postConfigure
          '';

          buildPhase = ''
            echo "--- Building Blazor WASM Client Assets ---"
            dotnet publish \
              --configuration Release \
              --no-restore \
              --output $PWD/publish \
              /p:RuntimeIdentifier=browser-wasm \
              /p:WasmEnableWorkloadResolver=false \
              /p:EnableWorkloadResolver=false
          '';

          installPhase = ''
            mkdir -p $out
            cp -r $PWD/publish/* $out/
          '';
        };
      in
      {
        # Expose the client package
        packages.client = clientDrv;

        # --- Server Package Definition ---
        packages.server = pkgs.buildDotnetModule {
          inherit dotnet-sdk dotnet-runtime;
          pname = "TpvVyber";
          version = "0.1.0";

          buildInputs = [ nswagConsoleTool];

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

            # 3. Wrap the executable so it runs from $out/bin
            makeWrapper ${dotnet-runtime}/bin/dotnet $out/bin/TpvVyber \
              --run "cd $out/bin && exec  ${dotnet-runtime}/bin/dotnet $out/bin/TpvVyber.dll" \
              --prefix LD_LIBRARY_PATH : "${
                pkgs.lib.makeLibraryPath [
                  pkgs.stdenv.cc.cc.lib
                  pkgs.openssl
                  pkgs.wasm-tools
                ]
              }" \
              --set ASPNETCORE_URLS "http://localhost:1234;https://localhost:1235"          
          '';
        };

        packages.default = self.packages.${system}.server;

        devShells.default = pkgs.mkShell {
          packages = [
            pkgs.dotnet-sdk-wasm
            pkgs.nuget-to-json
            genDeps
            nswagConsoleTool
          ];
          shellHook = ''
            export DOTNET_ROOT=${pkgs.dotnet-sdk-wasm}
            mkdir -p .nuget-packages
            export NUGET_PACKAGES="$PWD/.nuget-packages"
          '';
        };
      }
    );
}
