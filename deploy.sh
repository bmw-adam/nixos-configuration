#! /usr/bin/env nix-shell
#! nix-shell -i bash -p pkgs.openssh



nix run --extra-experimental-features 'nix-command flakes' github:nix-community/nixos-anywhere -- --flake .#server1 -i ~/.ssh/id_ed25519_mpbox root@78.46.206.4
ssh-keygen -R 78.46.206.4
