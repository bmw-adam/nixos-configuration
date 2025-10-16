{
  modulesPath,
  lib,
  pkgs,
  defaultPassword,
  pub_key,
  ...
} @ args:
{
  imports = [
    (modulesPath + "/installer/scan/not-detected.nix")
    (modulesPath + "/profiles/qemu-guest.nix")
    ./disk-part/disk-config.nix
  ];
  boot.loader.grub = {
    # no need to set devices, disko will add all devices that have a EF02 partition to the list already
    # devices = [ ];
    efiSupport = true;
    efiInstallAsRemovable = true;
  };
  services.openssh.enable = true;

  users.users.root.openssh.authorizedKeys.keys =
  [
    pub_key
  ] ++ (args.extraPublicKeys or []); # this is used for unit-testing this module and can be removed if not needed

  users.users.root.initialPassword = defaultPassword;

  system.stateVersion = "24.05";
}
