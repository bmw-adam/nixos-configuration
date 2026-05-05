#! /usr/bin/env nix-shell
#! nix-shell -i bash -p pkgs.openssh

temp=$(mktemp -d)
trap "rm -rf $temp" EXIT

mkdir -p "$temp/etc/sops/age"
cp ~/.config/sops/age/cetus.txt "$temp/etc/sops/age/cetus.txt"
chmod 600 "$temp/etc/sops/age/cetus.txt"
# --option pure-eval false 
nix run --extra-experimental-features 'nix-command flakes' github:nix-community/nixos-anywhere -- --extra-files "$temp" --flake .#server1 -i ~/.ssh/id_ed25519_mpbox root@178.104.211.77
ssh-keygen -R 178.104.211.77
