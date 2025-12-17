{
  modulesPath,
  lib,
  pkgs,
  config,
  ...
}@args:
let
  defaultPasswordPath = config.sops.secrets.password.path;
in
{
  imports = [
    (modulesPath + "/installer/scan/not-detected.nix")
    (modulesPath + "/profiles/qemu-guest.nix")
    ./disk-part/disk-config.nix
  ];

  boot.loader.grub = {
    enable = true;
    efiSupport = false;
    # devices = ["/dev/sda"];
    # efiInstallAsRemovable = true;
  };

  users.users.root = {
    hashedPasswordFile = defaultPasswordPath;
    openssh.authorizedKeys.keys = [
      "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIE0/XzWuuJp5E+dGUNGZagJSbb/9ePjkzc7RRDFA5z/9"
    ];
  };

  services.openssh.enable = true;

  system.stateVersion = "24.05";
}
