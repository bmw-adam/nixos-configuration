{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    kubenix.url = "github:hall/kubenix";
    flake-utils.url = "github:numtide/flake-utils";
    tpvsel = {
      url = "path:../TpvVyber";
      inputs.nixpkgs.follows = "nixpkgs";
      flake = true;
    };
  };

  outputs = { self, nixpkgs, kubenix, flake-utils, tpvsel, ... }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs {
          inherit system;
        };

        myK8sManifest = (kubenix.evalModules.${system} {
          specialArgs = {
            inherit pkgs tpvsel;
          };
          modules = [
            ./kubernetes/default.nix
          ];
        }).config.kubernetes.result;

        k8sJsonPackage = pkgs.stdenv.mkDerivation {
          pname = "k8s-json";
          version = "1.0";
          src = ./.;

          buildPhase = ''
            mkdir -p $out
            cp ${myK8sManifest} $out/kube.json
          '';

          installPhase = ''
            echo "Installation complete."
          '';
        };
      in
      {
        packages.default = k8sJsonPackage;
        defaultPackage = k8sJsonPackage;

        devShells.default = pkgs.mkShell {
          shellHook = ''
            echo
            echo "---------------------------------------------------------"
            echo "Welcome to the development shell for your K8s manifests!"
            echo "The generated files are available in the K8S_MANIFEST_DIR environment variable."
            echo "Try running: ls \$K8S_MANIFEST_DIR"
            echo "---------------------------------------------------------"
            echo
          '';

          K8S_MANIFEST_DIR = "${k8sJsonPackage}";
        };
      }
    );
}
