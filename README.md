# TPV - SELECT
## NIXOS CONFIG
* Hetzner Deployment
```bash
nix run --extra-experimental-features 'nix-command flakes' github:nix-community/nixos-anywhere -- --flake .#hetzner-cloud -i ~/.ssh/id_ed25519_mpbox root@78.46.206.4
```

Then
```bash
ssh-keygen -R 78.46.206.4
ssh -i ~/.ssh/id_ed25519_mpbox root@78.46.206.4
```

Checkout the [flake.nix](flake.nix) for examples tested on different hosters.
