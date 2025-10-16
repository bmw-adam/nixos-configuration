{
  description = "Gasos box node";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
    disko.url = "github:nix-community/disko";
    disko.inputs.nixpkgs.follows = "nixpkgs";
    nixos-facter-modules.url = "github:numtide/nixos-facter-modules";
    kubenixconfig.url = "github:bmw-adam/kubenix";
  };

  outputs =
    {
      nixpkgs,
      disko,
      nixos-facter-modules,
      kubenixconfig,
      ...
    }:
    {
      nixosConfigurations.server1 = nixpkgs.lib.nixosSystem {
        system = "x86_64-linux";
        specialArgs = {
          inherit kubenixconfig;
          defaultPassword = builtins.getEnv "DEF_PASS";
          k3stoken = builtins.getEnv "TOKEN";
          pub_key = builtins.getEnv "SSH_PUB";
        };

        modules = [
          disko.nixosModules.disko
          ./src/configuration/configuration.nix
          ./servers/server1.nix
        ];
      };
      # nixosConfigurations.hetzner-cloud = nixpkgs.lib.nixosSystem {
      #   system = "x86_64-linux";
      #   modules = [
      #     disko.nixosModules.disko
      #     ./src/configuration/configuration.nix
      #   ];
      # };
      # tested with 2GB/2CPU droplet, 1GB droplets do not have enough RAM for kexec
      # nixosConfigurations.digitalocean = nixpkgs.lib.nixosSystem {
      #   system = "x86_64-linux";
      #   modules = [
      #     ./src/digitalocean.nix
      #     disko.nixosModules.disko
      #     { disko.devices.disk.disk1.device = "/dev/vda"; }
      #     ./src/configuration/configuration.nix
      #   ];
      # };
      # nixosConfigurations.hetzner-cloud-aarch64 = nixpkgs.lib.nixosSystem {
      #   system = "aarch64-linux";
      #   modules = [
      #     disko.nixosModules.disko
      #     ./src/configuration/configuration.nix
      #   ];
      # };

      # Use this for all other targets
      # nixos-anywhere --flake .#generic --generate-hardware-config nixos-generate-config ./hardware-configuration.nix <hostname>
      # nixosConfigurations.generic = nixpkgs.lib.nixosSystem {
      #   system = "x86_64-linux";
      #   modules = [
      #     disko.nixosModules.disko
      #     ./src/configuration/configuration.nix
      #     ./src/configuration/hardware-configuration.nix
      #   ];
      # };

      # Slightly experimental: Like generic, but with nixos-facter (https://github.com/numtide/nixos-facter)
      # nixos-anywhere --flake .#generic-nixos-facter --generate-hardware-config nixos-facter facter.json <hostname>
      # nixosConfigurations.generic-nixos-facter = nixpkgs.lib.nixosSystem {
      #   system = "x86_64-linux";
      #   modules = [
      #     disko.nixosModules.disko
      #     ./src/configuration/configuration.nix
      #     nixos-facter-modules.nixosModules.facter
      #     {
      #       config.facter.reportPath =
      #         if builtins.pathExists ./facter.json then
      #           ./facter.json
      #         else
      #           throw "Have you forgotten to run nixos-anywhere with `--generate-hardware-config nixos-facter ./facter.json`?";
      #     }
      #   ];
      # };
    };
}
